using System;
using System.ServiceProcess;

namespace RuneService
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			RuneService service = new RuneService();

			// Check if attempting to debug?
			// http://einaregilsson.com/run-windows-service-as-a-console-program/
			if (Environment.UserInteractive)
			{
				// Apparently OnStart is protected? Like we care.
				System.Reflection.MethodInfo aMeth = typeof(RuneService).GetMethod("OnStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				aMeth.Invoke(service, new object[] { args });
				
				Console.WriteLine("Press any key to stop the service.");
				Console.ReadKey();

				// Not sure the best way to stop it.

				// Like we started it
				//aMeth = typeof(Service).GetMethod("OnStop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				//aMeth.Invoke(service, null);

				// Or perhaps better?
				service.Stop();
			}
			else
			{
				ServiceBase.Run(service);
			}
		}
	}
}
