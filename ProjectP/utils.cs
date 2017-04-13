using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProjectP
{
    // just a utilities class for all the functions i'll need.
    static class Utils
    {
        // ip range finder.
        public static void rangeFinder(InterfaceData ipData)
        {
            
            const int totalBits = 8;
            var stuff = ipData.IpAddressInformation.IPv4Mask.GetAddressBytes(); // get the bytes of the mask in array Form.
            var data = stuff.Select(d => Convert.ToString(d, 2)); // convert masks to binary...
            var moreShit = string.Join(" ", data); // join them by a space. probably don't need to join... just count 1s and 0s
            // that will identify host and subnets.
        }

        public static string IdentifyClass(IPAddress ipAddress)
        {
            var seperateOctets = ipAddress.ToString().Split('.');
            int firstOctect = int.Parse(seperateOctets[0]);

            if (firstOctect <= 126)
                return "Class A";
            if (firstOctect <= 191)
                return "Class B";
            return "Class C";
        }
    }
}
