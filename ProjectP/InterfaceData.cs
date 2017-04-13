using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ProjectP
{
    class InterfaceData
    {
        public string DnsSuffix { get; set; }
        public IPAddressCollection DnsAddress { get; set; }
        public UnicastIPAddressInformation IpAddressInformation { get; set; }
    }
}
