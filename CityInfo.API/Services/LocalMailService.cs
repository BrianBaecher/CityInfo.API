namespace CityInfo.API.Services
{
	public class LocalMailService : IMailService
	{
		private string _mailTo = string.Empty;
		private string _mailFrom = string.Empty;

		public LocalMailService(IConfiguration config)
		{
			_mailTo = config["mailSettings:mailToAddress"];
			_mailFrom = config["mailSettings:mailFromAddress"];
		}

		public void Send(string subject, string message)
		{
			// demo, not actually sending a message...
			Console.WriteLine($"Mail from {_mailFrom}, to {_mailTo}, with {nameof(LocalMailService)}");
			Console.WriteLine($"Subject: {subject}");
			Console.WriteLine($"Message: {message}");
		}
	}
}
