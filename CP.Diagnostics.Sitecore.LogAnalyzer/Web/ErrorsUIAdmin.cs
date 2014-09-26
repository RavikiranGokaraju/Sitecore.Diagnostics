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
	public class ErrorsUIAdmin : Page
	{
		protected string LogFilesPath;
		protected Literal ErrorCount;
		protected Literal ErrorsList;
		protected TextBox TextBox1;
		protected TextBox TextBox2;
		protected DropDownList DropDownList1;
		protected DropDownList DropDownList2;
		protected Button Button1;

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				SetFields();
				return;
			}
			var filesAnalyzer = GetAnalyzer();
			// Get Log Entries
			var logEntries = filesAnalyzer.Analyze().ToList();

			ErrorCount.Text = logEntries.Count.ToString();
			if (!logEntries.Any())
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

		private void SetFields()
		{
			TextBox1.Text = GetLogFilePath();
			TextBox2.Text = "*log.*.txt";
			DropDownList1.SelectedIndex = 1;
			DropDownList2.SelectedIndex = 1;
		}

		private LogFilesAnalyzer GetAnalyzer()
		{
			return new LogFilesAnalyzer
				{
					LogDirectory = TextBox1.Text,
					LogFileRegex = TextBox2.Text,
					LookBackHours = Convert.ToInt32(DropDownList1.SelectedValue),
					LogLevel = (LogLevel) Enum.Parse(typeof (LogLevel), DropDownList2.SelectedValue, true)
				};;
		}

		private string GetLogFilePath()
		{
			var sitecoreAssm = Assembly.Load("Sitecore.Kernel");
			var t = sitecoreAssm.GetType("Sitecore.Configuration.Settings");
			var methodInfo = t.GetMethod("GetSetting", new[] { typeof(string), typeof(string) });
			if (methodInfo != null)
			{
				var result = methodInfo.Invoke(null, new object[] { "DataFolder", "/data" });
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
