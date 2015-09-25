using System;
using System.Net;
using Dota2.Base.Data;
using Dota2.Datagram.Config.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dota2.CDN
{
    /// <summary>
    /// Interacts with the DOTA 2 CDN
    /// </summary>
    public static class DotaCDN
    {
        /// <summary>
        /// Gets the CDN Hostname for a CDN type
        /// </summary>
        /// <param name="type">CDN type</param>
        /// <returns></returns>
        public static string GetHostname(CDNType type)
        {
            switch (type)
            {
                case CDNType.LOCAL:
                    return "localhost";
                case CDNType.STANDARD:
                    return "cdn.dota2.com";
                case CDNType.CHINA:
                    return "cdn.dota2.com.cn";
                case CDNType.TEST:
                    return "cdntest.steampowered.com";
            }

            return null;
        }

        /// <summary>
        /// Build the URL to something static in the CDN
        /// </summary>
        /// <returns></returns>
        public static string StaticDataPath(CDNData target, Games game = Games.DOTA2)
        {
            switch (target)
            {
                case CDNData.DATAGRAM_NETWORK_CONFIG:
                    return "/apps/sdr/network_config.json";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Builds the URL to the Steam Datagram network config file.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static Uri DatagramNetworkConfig(CDNType type = CDNType.STANDARD, Games game = Games.DOTA2)
        {
            UriBuilder builder = new UriBuilder("http", GetHostname(type), 80,
                StaticDataPath(CDNData.DATAGRAM_NETWORK_CONFIG, game));
            return builder.Uri;
        }

        /// <summary>
        /// Retreive the network config from the DOTA 2 CDN
        /// </summary>
        /// <returns>Network config on success, null otherwise.</returns>
        public static NetworkConfig GetNetworkConfig(CDNType type = CDNType.STANDARD, Games game = Games.DOTA2)
        {
            using (var wc = new WebClient())
            {
                var str = wc.DownloadString(DatagramNetworkConfig(type, game));
                JObject obj = JObject.Parse(str);
                return obj.ToObject<NetworkConfig>();
            }
        }

        /// <summary>
        /// CDN type
        /// </summary>
        public enum CDNType
        {
            /// <summary>
            /// Local CDN. Presumably developer machines.
            /// </summary>
            LOCAL,

            /// <summary>
            /// Standard, public CDN
            /// </summary>
            STANDARD,

            /// <summary>
            /// CDN specific to China due to regulations.
            /// </summary>
            CHINA,

            /// <summary>
            /// TEST cdn, valve internal.
            /// </summary>
            TEST
        }

        /// <summary>
        /// Known data on the CDN with constant paths
        /// </summary>
        public enum CDNData
        {
            /// <summary>
            /// Steam Datagram network config
            /// </summary>
            DATAGRAM_NETWORK_CONFIG,
        }
    }
}