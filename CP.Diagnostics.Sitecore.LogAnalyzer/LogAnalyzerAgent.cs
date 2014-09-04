using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace CP.Diagnostics.Sitecore.LogAnalyzer
{
	public class LogAnalyzerAgent
	{
		public string ClientName { get; set; }
		public string LogDirectory { get; set; }
		public string LogFilesRegEx { get; set; }
		public int LookBackHours { get; set; }
		public string EmailIds { get; set; }
		public string LogLevel { get; set; }
		public string SmtpHost { get; set; }
		public bool AsAttachement { get; set; }
		public string FromAddress { get; set; }

		public LogAnalyzerAgent() { }

		public void Run()
		{
			// Read parameters from Task Configuration
			ReadConfig();

			// Get Log Entries
			var logEntries = LogFilesAnalyzer.Analyze().ToList();

			// if no entries are found return.
			if (!logEntries.Any())
				return;

			// build error list string
			var errorList = new StringBuilder();
			
			logEntries.ForEach(logEntry =>
				{
					errorList.AppendLine("File Name: " + logEntry.FileName);
					errorList.AppendLine("Event Source: " + logEntry.EventSource);
					errorList.AppendLine("Date: " + logEntry.DateTime);
					errorList.AppendLine("Level: " + logEntry.LogLevel);
					errorList.AppendLine("Text: " + logEntry.Text);
					errorList.AppendLine("");
				});

			// Send Email
			SendEmail("Found " + logEntries.Count + " errors. The list of errors found are below", errorList.ToString());
		}

		private void ReadConfig()
		{
			if (!string.IsNullOrEmpty(LogDirectory))
				LogFilesAnalyzer.LogDirectory = LogDirectory;
			if (!string.IsNullOrEmpty(LogFilesRegEx))
				LogFilesAnalyzer.LogFileRegex = LogFilesRegEx;
			if (LookBackHours != 0)
				LogFilesAnalyzer.LookBackHours = LookBackHours;
			if (!string.IsNullOrEmpty(LogLevel))
			{
				try
				{
					LogFilesAnalyzer.LogLevel = (LogLevel) Enum.Parse(typeof (LogLevel), LogLevel, true);
				}
				catch { }
			}
		}

		private void SendEmail(string errorCountString, string errorListString)
		{
			var mail = new MailMessage
				{
					From = new MailAddress(FromAddress),
					Subject = ClientName + ": Logs",
					Priority = MailPriority.High,
					Body = AsAttachement ? errorCountString : errorCountString + Environment.NewLine + errorListString
				};
			EmailIds.Split(';').ToList().ForEach(e => mail.To.Add(e.Trim()));

			if (AsAttachement)
			{
				var memoryStream = new MemoryStream(Encoding.Default.GetBytes(errorListString));
				mail.Attachments.Add(new Attachment(memoryStream, ClientName + "LogEntries_" + DateTime.Now.Ticks + ".txt", MediaTypeNames.Text.Plain));
			}

			var emailClient = string.IsNullOrEmpty(SmtpHost) ? new SmtpClient() : new SmtpClient(SmtpHost);
			try
			{
				emailClient.Send(mail);
			}
			catch (Exception)
			{
				// Fallback to send errors as message body
				mail = new MailMessage
				{
					From = new MailAddress(FromAddress),
					Subject = ClientName + ": Log entries in the last " + LookBackHours + " hours",
					Priority = MailPriority.High,
					Body = errorCountString + Environment.NewLine + errorListString
				};
				EmailIds.Split(';').ToList().ForEach(e => mail.To.Add(e.Trim()));

				emailClient.Send(mail);
			}
		}
	}
}
