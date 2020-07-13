using System;

using Microsoft.Extensions.CommandLineUtils;

namespace ApplicationDevelopmentKit
{
	public class CommandLineOptions
	{
		public ICommand Command { get; set; }

		public static CommandLineOptions Parse(string[] args)
		{
			CommandLineOptions commandLineOpts = new CommandLineOptions();
			CommandLineApplication commandLineApp = new CommandLineApplication {
				Name = "mfcli",
				FullName = "M-Files Command-line Interface"
			};

			RootCommandConfiguration.Configure(commandLineApp, commandLineOpts);
			try {
				int result = commandLineApp.Execute(args);
				if (result != 0)
					return null;
			} catch (Exception ex) {
				Console.WriteLine($"[ERROR] {ex.Message}");
			}

			return commandLineOpts;
		}
	}
}
