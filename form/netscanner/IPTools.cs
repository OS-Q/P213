using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NetScan
{
    public class IpTools
    {

        /// <summary>
        /// Ping ip address en return boolean
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        public static bool PingIP(IPAddress IP, int timeout)
        {
            bool result = false;
            try
            {
                Ping ping = new Ping();

                PingReply pingReply = ping.Send(IP, timeout);

                if (pingReply.Status == IPStatus.Success)
                    result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Get hotsname by ip address
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static string GetHostName(IPAddress ipAddress)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(ipAddress);
                if (entry != null)
                {
                    return entry.HostName;
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("No hostname found for {0}", ipAddress.ToString());
            }

            return null;
        }

        /// <summary>
        /// Get loca comuter ip address
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetLocalIpAddress()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint.Address;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Can get local ip");
            }

            return null;

        }

        /// <summary>
        /// Get network mask from ip address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static IPAddress GetSubnetMask(IPAddress address)
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (address.Equals(unicastIPAddressInformation.Address))
                        {
                            return unicastIPAddressInformation.IPv4Mask;
                        }
                    }
                }
            }

            Console.WriteLine("Can't find subnetmask for IP address '{0}'", address);
            return null;
        }

        /// <summary>
        /// Get network from ip address and network mask
        /// </summary>
        /// <param name="address"></param>
        /// <param name="subnetMask"></param>
        /// <returns></returns>
        public static IPAddress GetNetworkAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
            {
                Console.WriteLine("Lengths of IP address and subnet mask do not match.");
                return null;
            }

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }

        
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(int DestIP, int SrcIP, [Out] byte[] pMacAddr, ref int PhyAddrLen);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostIPAddress"></param>
        /// <returns></returns>
        public static string GetMacFromIP(IPAddress hostIPAddress)
        {

            try
            {
                byte[] ab = new byte[6];
                int len = ab.Length,
                    r = SendARP((int)hostIPAddress.Address, 0, ab, ref len);
                return BitConverter.ToString(ab, 0, 6);
            }
            catch (Exception) { }

            return null;

        }

    }
}
