using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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
        static Dictionary<IPAddress, PhysicalAddress> _ipToMac = new Dictionary<IPAddress, PhysicalAddress>();

        static void Main(string[] args)
        {
            var data = ReturnInterfaceData();
            PopulateArpDict();
           _ipToMac = _ipToMac.Where(d => Utils.IdentifyClass(d.Key) == Utils.IdentifyClass(data.IpAddressInformation.Address)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Console.WriteLine($"DNS Suffix.......................... {data.DnsSuffix}");
            Console.WriteLine(
                $"IpAddress......................... {data.IpAddressInformation.Address}({Utils.IdentifyClass(data.IpAddressInformation.Address)})");
            Console.WriteLine($"Subnet Mask....................... {data.IpAddressInformation.IPv4Mask}");
            Console.ReadLine();
        }


        public static InterfaceData ReturnInterfaceData()
        {
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
                    DnsAddress = data.DnsAddresses,
                    IpAddressInformation =
                        data.UnicastAddresses.FirstOrDefault(d => d.Address.AddressFamily == AddressFamily.InterNetwork)
                };
                return interfaceData;
            }

            return null;
        }

        public static void PopulateArpDict()
        {
            Regex macAddressPattern = new Regex("([A-Fa-f0-9]{2}[-]){5}([A-Fa-f0-9]{2})"); 
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

           
            // strip off all the meta data from each interface arp table. 
            var regexSplit = era.Where(z => macAddressPattern.IsMatch(z)).ToArray();
           
            foreach (var info in regexSplit)
            {
                // strip off all the white spaces.
                var dataEntry = info.Trim().Split().Where(d => !string.IsNullOrEmpty(d)).ToArray();
                string ip = dataEntry[0];
                string mac = dataEntry[1].ToUpper(); // uppercase the mac because the parse function only accepts uppercase.
                IPAddress ipAddress = IPAddress.Parse(ip);
                PhysicalAddress physicalAddress = PhysicalAddress.Parse(mac);

                if (_ipToMac.ContainsKey(ipAddress))
                    continue;
                _ipToMac.Add(ipAddress, physicalAddress);
            }
        }
    }
}
