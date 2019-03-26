using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;


namespace NetScan
{
    class Program
    {
        private static bool onlyPI = false;
        private static CancellationTokenSource cts = new CancellationTokenSource();
        static void Main(string[] args)
        {
            Ping sender = new Ping();
            Stopwatch s = new Stopwatch();
            string ipBase = " ";
            if (args.Length == 0)
            {
                ipBase = "192.168.1.";
            }

            s.Start();

            Console.CancelKeyPress += Console_CancelKeyPress;

            Console.WriteLine("netscanner tool");
            Console.WriteLine(string.Format("version {0}", Assembly.GetExecutingAssembly().GetName().Version));
            Console.WriteLine(string.Format("more from https://github.com/qitas"));

            var onlyPIParams = new string[] { "-o", "/o", "-onlypi", "/onlypi" };
            var helpParams = new string[] { "-h", "/h", "-help", "/help" };
            for (var i = 0; i < args.Count(); i++)
            {
                try
                {
                    PingReply reply = sender.Send(ipBase + i.ToString(), 5);
                    if (reply.Status == IPStatus.Success)
                    {
                        Console.WriteLine("Address: {0}", reply.Address);
                        Console.WriteLine("Hostname: {0}", Dns.GetHostEntry(reply.Address).HostName);
                        Console.WriteLine();
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Couldn't resolve host.");
                    Console.WriteLine();
                }

                if (helpParams.Contains(args[i].ToLowerInvariant()))
                {
                    Console.WriteLine(string.Format("Usage : {0} [IP] [-a|-all] [-h|-help]", System.AppDomain.CurrentDomain.FriendlyName));
                    Console.WriteLine("IP: The IP you want check using 0.0.0.0 format (ex: 192.168.1.1 10.0.0.1). We'll check all the machines on the same network.");
                    Console.WriteLine(string.Format("{0}: Display the parameters help", string.Join("|", helpParams)));
                    Console.WriteLine(string.Format("{0}: List only Raspberry PI machines. By default, all machines will be listed.", string.Join("|", onlyPIParams)));
                    return;
                }
                else if (onlyPIParams.Contains(args[i].ToLowerInvariant()))
                {
                    onlyPI = true;
                }
                else
                {
                    IPAddress paramIP;
                    if (IPAddress.TryParse(args[i], out paramIP))
                    {
                        IPAddressHelper.ScanMac(ScanOutput, cts.Token, paramIP);
                    }
                }
            }
            IPAddressHelper.ScanMac(ScanOutput, cts.Token);
#if DEBUG
            Console.ReadKey(false);
#endif
        }
        static void ScanOutput(IPAddress ip, string mac)
        {
            //All Raspberry PI MAC address start with the same prefix
            var spotted = mac.ToString().ToUpper().StartsWith("B8:27:EB");
            if ((onlyPI && spotted) || !onlyPI)
                Console.WriteLine(string.Format("IP={0} MAC={1}{2}", ip, mac, spotted ? " <- spotted !!!" : ""));
        }
        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            cts.Cancel();
        }
    }
}
