using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;

namespace ApplicationDevelopmentKit
{
	public class BuildErrorConsoleLogger : Logger
	{
		public override void Initialize(Microsoft.Build.Framework.IEventSource eventSource)
		{
			//Register for the ProjectStarted, TargetStarted, and ProjectFinished events
			eventSource.ErrorRaised += EventSource_ErrorRaised;
		}
		private void EventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
		{
			Console.WriteLine($"\t{e.File}({e.LineNumber},{e.ColumnNumber}) Desc: {e.Message}");
		}
	}
}