using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NetScan
{
    class IpDetails : INotifyPropertyChanged
    {
        private uint IpWeightValue = 0;
        private IPAddress IpValue;
        private string MacValue;
        private bool PingStateValue;
        private string HostNameValue;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="pingstate"></param>
        public IpDetails(IPAddress ip,string mac, bool pingstate, string hostname)
        {
            IpWeightValue = IpToInt(ip);
            IpValue = ip;
            MacValue = mac;
            PingStateValue = pingstate;
            HostNameValue = hostname;
        }

        /// <summary>
        /// Tools to convert ip to int
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private uint IpToInt(IPAddress ip)
        {

            byte[] bytes = ip.GetAddressBytes();
            Array.Reverse(bytes); // flip big-endian(network order) to little-endian
            uint intAddress = BitConverter.ToUInt32(bytes, 0);

            return intAddress;

        }

        /// <summary>
        /// Ip weight in number
        /// </summary>
        [System.ComponentModel.DisplayName("Ip Weight")]
        public uint IpWeight
        {
            get
            {
                return this.IpWeightValue;
            }

            set
            {
                if (value != this.IpWeightValue)
                {
                    this.IpWeightValue = value;
                    NotifyPropertyChanged();
                }
            }

        }

        /// <summary>
        /// Ip address
        /// </summary>
        [System.ComponentModel.DisplayName("Address")]
        public IPAddress Ip
        {
            get
            {
                return this.IpValue;
            }

            set
            {
                if (value != this.IpValue)
                {
                    this.IpValue = value;
                    IpWeightValue = IpToInt(value);
                    NotifyPropertyChanged();
                }
            }

        }

        /// <summary>
        /// MAC address
        /// </summary>
        [System.ComponentModel.DisplayName("MAC")]
        public string Mac
        {
            get
            {
                return this.MacValue;
            }

            set
            {
                if (value != this.MacValue)
                {
                    this.MacValue = value;
                    NotifyPropertyChanged();
                }
            }

        }

        /// <summary>
        /// Host Name
        /// </summary>
        [System.ComponentModel.DisplayName("Hostname")]
        public string HostName
        {
            get
            {
                return this.HostNameValue;
            }

            set
            {
                if (value != this.HostNameValue)
                {
                    this.HostNameValue = value;
                    NotifyPropertyChanged();
                }
            }

        }

        /// <summary>
        /// Ping State
        /// </summary>
        [System.ComponentModel.DisplayName("Ping State")]
        public bool PingState
        {
            get
            {
                return this.PingStateValue;
            }

            set
            {
                if (value != this.PingStateValue)
                {
                    this.PingStateValue = value;
                    NotifyPropertyChanged();
                }
            }
        }

    }
}
