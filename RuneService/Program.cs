using System;
using System.Collections.Generic;
using System.ServiceProcess;

namespace RuneService {
    static class Program {

        public static Dictionary<string, object> Args = new Dictionary<string, object>();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args) {
            try {
                for (int i = 0; i < args.Length; i++) {
                    int v;
                    if (i == 0 && int.TryParse(args[i], out v)) {
                        Args["port"] = v;
                    }
                    else if ((args[i] == "-P" || args[i] == "--port") && i < args.Length - 1 && int.TryParse(args[i+1], out v)) {
                        i++;
                        Args["port"] = v;
                    }
                }
                
                RuneService service = new RuneService();

                // Check if attempting to debug?
                // http://einaregilsson.com/run-windows-service-as-a-console-program/
                if (Environment.UserInteractive) {
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
                else {
                    ServiceBase.Run(service);
                }
            }
            catch (Exception e) {
                if (Environment.UserInteractive) {
                    Console.WriteLine(e.GetType() + ": " + e.Message + Environment.NewLine + e.StackTrace);
                    for (var ie = e.InnerException; ie != null; ie = ie.InnerException) {
                        Console.WriteLine(ie.GetType() + ": " + ie.Message);
                    }
                }
            }
        }
    }
}