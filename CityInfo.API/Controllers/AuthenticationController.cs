using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CityInfo.API.Controllers
{
	[Route("api/authentication")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		public class AuthenticationRequestBody
		{
			public string? UserName { get; set; }
			public string? Password { get; set; }

		}

		public class CityInfoUser
		{
			public int Id { get; set; }

			public string UserName { get; set; }

			public string FirstName { get; set; }

			public string LastName { get; set; }

			public string City { get; set; }

			public CityInfoUser(int id, string userName, string firstName, string lastName, string city)
			{
				Id = id;
				UserName = userName;
				FirstName = firstName;
				LastName = lastName;
				City = city;
			}
		}

		private readonly IConfiguration _configuration;

		public AuthenticationController(IConfiguration configuration)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		[HttpPost("authenticate")]
		public ActionResult<string> Authenticate(AuthenticationRequestBody authenticationRequest)
		{
			// step 1: validate username and password
			var user = ValidateUserCredentials(
				authenticationRequest.UserName,
				authenticationRequest.Password
				);

			if (user == null)
			{
				return Unauthorized();
			}

			// step 2: create a token
			var securityKey = new SymmetricSecurityKey(
				Convert.FromBase64String(_configuration["Authentication:SecretForKey"])
				);

			var signingCredentials = new SigningCredentials(securityKey,
				SecurityAlgorithms.HmacSha256);

			var claimsForToken = new List<Claim>();
			claimsForToken.Add(new Claim("sub", user.Id.ToString()));
			claimsForToken.Add(new Claim("given_name", user.FirstName));
			claimsForToken.Add(new Claim("family_name", user.LastName));
			claimsForToken.Add(new Claim("city", user.City));

			var securityToken = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
				_configuration["Authentication:Issuer"],
				_configuration["Authentication:Audience"],
				claimsForToken,
				DateTime.UtcNow,
				DateTime.UtcNow.AddHours(1),
				signingCredentials
				);

			var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(securityToken);

			return Ok(tokenToReturn);


		}

		private CityInfoUser ValidateUserCredentials(string? userName, string? password)
		{
			// not implementing a user database or table anywhere. 
			// for demo purposes, assuming credentials are valid...

			//returning a CityInfoUser, these values would normally come from the database or table...
			return new CityInfoUser(1, userName ?? "", "Name", "LastName", "City");
		}
	}
}
