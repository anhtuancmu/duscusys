// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublicIPAddressReader.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.LoadBalancing.Common
{
    using System;
    using System.IO;
    using System.Net;

    using ExitGames.Logging;

    public static class PublicIPAddressReader
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public static bool TryParsePublicIpAddress(string publicIpAddressFromSettings, out IPAddress ipAddress)
        {
            if (string.IsNullOrEmpty(publicIpAddressFromSettings))
            {
                return TryLookupPublicIpAddress(out ipAddress);
            }

            if (IPAddress.TryParse(publicIpAddressFromSettings, out ipAddress))
            {
                return true;
            }

            IPHostEntry hostEntry = Dns.GetHostEntry(publicIpAddressFromSettings);
            if (hostEntry.AddressList.Length > 0)
            {
                foreach (var entry in hostEntry.AddressList)
                {
                    if (entry.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ipAddress = entry;
                        return true;
                    }
                }
            }

            if (TryLookupPublicIpAddress(out ipAddress))
            {
                log.WarnFormat("cannot resolve '{0}', using public IP {1} instead", publicIpAddressFromSettings, ipAddress);
                return true;
            }

            return false;
        }

        private static bool TryLookupPublicIpAddress(out IPAddress ipAddress)
        {
            ipAddress = null;
            WebResponse response = null;
            Stream stream = null;

            try
            {
                WebRequest request = WebRequest.Create("http://automation.whatismyip.com/n09230945.asp");
                response = request.GetResponse();
                stream = response.GetResponseStream();

                if (stream == null)
                {
                    log.Error("Failed to lookup public ip address: No web response received");
                    return false;
                }

                string address;
                using (var reader = new StreamReader(stream))
                {
                    address = reader.ReadToEnd();
                }

                if (IPAddress.TryParse(address, out ipAddress) == false)
                {
                    log.ErrorFormat("Failed to lookup public ip address: Parse address failed - Response = {0}", address);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to lookup public ip address: {0}", ex);
                return false;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }

                if (response != null)
                {
                    response.Close();
                }
            }        
        }
    }
}