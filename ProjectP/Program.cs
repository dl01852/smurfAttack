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
        static string _output = "";
        static Dictionary<string,string> _ipToMac = new Dictionary<string, string>();
        static void Main(string[] args)
        {
            // Start a new process that'll run the arp -a command.
            // the addresses in my arp table will be used to ping the victim.
           
            Process cmd = new Process
            {
                StartInfo =
                {
                    FileName = "cmd",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false,
                    Arguments = "/C arp -a" // /C tells the process to exit right after the command.
                }
            };
            cmd.Start();
            var era = cmd
                        .StandardOutput
                        .ReadToEnd()
                        .TrimEnd()
                        .Split(new string[] {"\r\n"}, StringSplitOptions.None);

            // skip the first 3 values as it is just useless meta data that i could give 2 shits about and split on spaces.
            var moreSplitting = era.Skip(3).Select(machine => machine.Split(' '));

            foreach (var info in moreSplitting)
            {
                string ip = "";
                string mac = "";

                /* Note:
                 * arp -a command gets displayed as followed.
                 * 
                 * Internet Address          Physical Address          Type
                 * 192.168.12.45             D3-41-B6-F1-C4-E8         Dynamic.
                 */
                foreach (var dataEntry in info)
                {
                    // I only care about the ip Address and the Mac. skip the white spaces.
                    if (string.IsNullOrEmpty(dataEntry))
                        continue;
                    else if (dataEntry.Contains("-")) // if it contains dashes then it's the mac.
                    {
                        mac = dataEntry;
                        break; // Mac is the last thing i care about, break out and proceed to next entry.
                    }
                    else
                        ip = dataEntry;
                }
                _ipToMac.Add(ip,mac); // add it to the dictionary. 
            }
        
         

            Console.ReadLine();
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
