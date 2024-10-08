using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CityInfo.API;
using CityInfo.API.DbContexts;
using CityInfo.API.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Reflection;

// configuring serilog
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug()
	.WriteTo.Console()
	.WriteTo.File("logs/cityinfo.txt", rollingInterval: RollingInterval.Day)
	.CreateLogger();


var builder = WebApplication.CreateBuilder(args);
//builder.Logging.ClearProviders(); // removes the logger provided by default
//builder.Logging.AddConsole();
builder.Host.UseSerilog(); // using serilog for logging...


// Add services to the container.

//builder.Services.AddControllers(); // generated by template...

builder.Services.AddControllers(options =>
{
	options.ReturnHttpNotAcceptable = true; // reject xml format requests, because we are not formatting for it. returns 406 code.
}).AddNewtonsoftJson().
AddXmlDataContractSerializerFormatters(); // add the ability to respond with xml

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSingleton<FileExtensionContentTypeProvider>(); // to determine extensions of static files...

#if DEBUG
builder.Services.AddTransient<IMailService, LocalMailService>();
#else
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif
builder.Services.AddSingleton<CitiesDataStore>();

builder.Services.AddDbContext<CityInfoContext>(dbContextOptions =>
{
	dbContextOptions.UseSqlite(builder.Configuration["ConnectionStrings:CityInfoDbConnectionString"]);
});

builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>(); // like others, injected through ctors in controllers.

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddProblemDetails();

builder.Services.AddAuthentication("Bearer").AddJwtBearer(options =>
{
	options.TokenValidationParameters = new()
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["Authentication:Issuer"],
		ValidAudience = builder.Configuration["Authentication:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(
			Convert.FromBase64String(builder.Configuration["Authentication:SecretForKey"]))
	};
});


//// adding problem details for error information
//builder.Services.AddProblemDetails(options =>
//{
//	options.CustomizeProblemDetails = ctx =>
//	{
//		ctx.ProblemDetails.Extensions.Add("additionalInfo", "additional info example");
//		ctx.ProblemDetails.Extensions.Add("server", Environment.MachineName);
//	};
//});

// policy example
builder.Services.AddAuthorization(authOptions =>
{
	authOptions.AddPolicy("MustBeFromCity", policy =>
	{
		policy.RequireAuthenticatedUser();
		policy.RequireClaim("city", "City");
	});
});

builder.Services.AddApiVersioning(setupAction =>
{
	setupAction.ReportApiVersions = true;
	setupAction.AssumeDefaultVersionWhenUnspecified = true;
	setupAction.DefaultApiVersion = new ApiVersion(1, 0);
}).
AddMvc()
.AddApiExplorer(setupAction =>
{
	setupAction.SubstituteApiVersionInUrl = true;
});

var apiVersionDescriptionProvider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();


builder.Services.AddSwaggerGen(setupAction =>
{
	foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
	{
		setupAction.SwaggerDoc(
			$"{description.GroupName}",
			new()
			{
				Title = "City Info API",
				Version = description.ApiVersion.ToString(),
				Description = "City api demo..."
			});
	}

	var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

	setupAction.IncludeXmlComments(xmlCommentsFullPath);
	setupAction.AddSecurityDefinition("CityInfoBearerAuth", new()
	{
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
		Scheme = "Bearer",
		Description = "Input valid token to access the API"
	});

	setupAction.AddSecurityRequirement(new()
	{
		{
			new()
		{
			Reference=new Microsoft.OpenApi.Models.OpenApiReference
			{
				Type=Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
				Id="CityInfoApiBearerAuth"
			}
		},
			new List<string>()
		}
	});
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler();
}

// forwarding headers should come before anything else...
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(setupAction =>
	{
		var descriptions = app.DescribeApiVersions();
		foreach (var desc in descriptions)
		{
			setupAction.SwaggerEndpoint(
				$"/swagger/{desc.GroupName}/swagger.json",
				desc.GroupName.ToUpperInvariant());
		}
	});
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
	endpoints.MapControllers();
}); // the warning wants you to just use app.MapControllers(); below... for tutorial we are using the old way, which is app.UseRouting(), and app.UseEndpoints(...)

//app.MapControllers();

app.Run();
