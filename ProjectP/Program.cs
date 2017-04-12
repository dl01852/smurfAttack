using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ProjectP
{
    class Program
    {
        // Tb = 8 always 8...
        // networks = based off subnetMask
        // host = Tb - networks

        // totalSubnets = 2^networks
        // totalHost = 2^hosts

            //ipaddress - 141.165.20.79
            // subnetMask = 255.255.255.128
            // Tb = 8
            // networks = 1
            // hosts = 8 - 1 = 7
            // totalSubnets = 2^1 = 2
            // totalHost = 2^7 = 128 - 2

  
        static void Main(string[] args)
        {
            List<InterfaceData> interfaces = new List<InterfaceData>();
            var network = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(intf => intf.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                               intf.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                .Where(interf => interf.GetIPProperties().GetIPv4Properties().IsDhcpEnabled);
                
            foreach (NetworkInterface ni in network)
            {
                var data = ni.GetIPProperties();
                if (string.IsNullOrEmpty(data.DnsSuffix))
                    continue;
                InterfaceData interfaceData = new InterfaceData()
                {
                    DnsSuffix = data.DnsSuffix,
                    DNSAddress = data.DnsAddresses,
                    IpAddressInformation = data.UnicastAddresses.FirstOrDefault(d => d.Address.AddressFamily == AddressFamily.InterNetwork)

                };
                interfaces.Add(interfaceData);
            }
            foreach (var data in interfaces)
            {
                Console.WriteLine($"DNS Suffix.......................... {data.DnsSuffix}");
                Console.WriteLine($"IpAddress......................... {data.IpAddressInformation.Address}({Utils.IdentifyClass(data.IpAddressInformation.Address)})");
                Console.WriteLine($"Subnet Mask....................... {data.IpAddressInformation.IPv4Mask}");
                Utils.rangeFinder(data);

            }
            Console.ReadLine();
        }
    }
}
