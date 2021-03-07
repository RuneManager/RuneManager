using System;
using System.Diagnostics;

namespace RuneApp.InternalServer {
    public static class NetAclChecker {
        
        private static ProcessStartInfo NetshRead(string args) {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", args);
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            return psi;
        }

        private static ProcessStartInfo NetshWrite(string args) {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", args);
            psi.Verb = "runas";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = true;
            return psi;
        }
        public static void AddAddress(string address) {
            if (!HasAddress(address, Environment.UserDomainName + "\\" + Environment.UserName))
                AddAddress(address, Environment.UserDomainName + "\\" + Environment.UserName);
        }

        public static void AddAddress(string address, string user) {
            var psi = NetshWrite($"http add urlacl url={address} user={user}");
            Process.Start(psi).WaitForExit();
        }

        public static bool HasAddress(string address, string user) {
            var psi = NetshRead($"http show urlacl url={address}");
            var p = Process.Start(psi);
            p.WaitForExit();
            var output = p.StandardOutput.ReadToEnd();

            return output.Contains("Listen: Yes") && output.Contains(user);
        }

        public static void AddFirewall(string name, bool incoming, bool allow, bool tcp, int port) {
            var psi = NetshWrite($"advfirewall firewall add rule name=\"{name}\" dir={(incoming ? "in" : "out")} action={(allow ? "allow" : "block")} protocol={(tcp ? "TCP" : "UDP")} localport={port}");
            Process.Start(psi).WaitForExit();
        }


        public static bool HasFirewall(string name, bool? incoming = null, bool? allow = null, bool? tcp = null, int? port = null) {
            var psi = NetshRead($"advfirewall firewall show rule name=\"{name}\"");
            var p = Process.Start(psi);
            p.WaitForExit();
            var output = p.StandardOutput.ReadToEnd();

            if (output.Contains("No rules match the specified criteria"))
                return false;

            if (incoming.HasValue && !output.Contains(incoming.Value ? "In" : "Out"))
                return false;

            if (allow.HasValue && output.Contains("Allow") != allow)
                return false;

            if (tcp.HasValue && output.Contains("TCP") != tcp)
                return false;

            if (port.HasValue && !output.Contains(port.ToString()))
                return false;

            return true;
        }

    }
}