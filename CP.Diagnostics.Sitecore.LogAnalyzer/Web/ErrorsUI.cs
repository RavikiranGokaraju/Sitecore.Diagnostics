using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CP.Diagnostics.Sitecore.LogAnalyzer.Web
{
	public class ErrorsUI : Page
	{
		protected string LogFilesPath;
		protected Literal ErrorCount;
		protected Literal ErrorsList;

		protected void Page_Load(object sender, EventArgs e)
		{
			var filesAnalyzer = GetAnalyzer();
			// Get Log Entries
			var logEntries = filesAnalyzer.Analyze().ToList();

			ErrorCount.Text = logEntries.Count.ToString();
			if(!logEntries.Any())
				return;

			var errorList = new StringBuilder();
			logEntries.ForEach(logEntry =>
			{
				errorList.AppendLine("<p>");
				errorList.AppendLine("File Name: " + logEntry.FileName);
				errorList.AppendLine("<br/>");
				errorList.AppendLine("Event Source: " + logEntry.EventSource);
				errorList.AppendLine("<br/>");
				errorList.AppendLine("Date: " + logEntry.DateTime);
				errorList.AppendLine("<br/>");
				errorList.AppendLine("Level: " + logEntry.LogLevel);
				errorList.AppendLine("<br/>");
				errorList.AppendLine("Text: " + logEntry.Text);
				errorList.AppendLine("</p>");
				errorList.AppendLine("<br/>");
			});

			ErrorsList.Text = errorList.ToString();
		}

		private LogFilesAnalyzer GetAnalyzer()
		{
			var filesAnalyzer = new LogFilesAnalyzer();
			var logFilePath = GetLogFilePath();
			filesAnalyzer.LogDirectory = logFilePath;
			if (!string.IsNullOrEmpty(Request.QueryString["regex"]))
				filesAnalyzer.LogFileRegex = Request.QueryString["regex"];
			if (!string.IsNullOrEmpty(Request.QueryString["hours"]))
				filesAnalyzer.LookBackHours = Convert.ToInt32(Request.QueryString["hours"]);
			if (!string.IsNullOrEmpty(Request.QueryString["level"]))
			{
				try
				{
					filesAnalyzer.LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), Request.QueryString["level"], true);
				}
				catch
				{
					filesAnalyzer.LogLevel = LogLevel.Error;
				}
			}

			return filesAnalyzer;
		}

		private string GetLogFilePath()
		{
			var sitecoreAssm = Assembly.Load("Sitecore.Kernel");
			var t = sitecoreAssm.GetType("Sitecore.Configuration.Settings");
			var methodInfo = t.GetMethod("GetSetting", new[] {typeof (string), typeof(string)});
			if (methodInfo != null)
			{
				var result = methodInfo.Invoke(null, new object[]{"DataFolder", "/data"});
				if (result != null)
				{
					var path = result.ToString();
					path = path.StartsWith("/") ? System.Web.HttpContext.Current.Server.MapPath(path) : path;
					return Path.Combine(path, "logs");
				}
			}

			return Path.Combine(System.Web.HttpContext.Current.Server.MapPath(null), "/../data/logs");
		}
	}
}
