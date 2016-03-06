using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace Dota2.Datagram.Config.Model
{
    /// <summary>
    /// Steam Datagram network config
    /// </summary>
    public class NetworkConfig
    {
        /// <summary>
        /// Revision of the config
        /// </summary>
        public uint revision { get; set; }

        /// <summary>
        /// Datacenter definitions
        /// </summary>
        public Dictionary<string, NetworkDatacenter> data_centers { get; set; }

        /// <summary>
        /// Routing cluster definitions
        /// </summary>
        public Dictionary<string, NetworkRoutingCluster> routing_clusters { get; set; }
    }

    /// <summary>
    /// Steam Datagram datacenter definition
    /// </summary>
    public class NetworkDatacenter
    {
        /// <summary>
        /// Latitude of the datacenter
        /// </summary>
        public double lat { get; set; }

        /// <summary>
        /// Longitude of the datacenter
        /// </summary>
        public double lon { get; set; }

        /// <summary>
        /// IP address ranges of the datacenter
        /// </summary>
        public string[] address_ranges { get; set; }
    }

    /// <summary>
    /// Routing cluster definition
    /// </summary>
    public class NetworkRoutingCluster
    {
        /// <summary>
        /// Address range of the cluster
        /// </summary>
        public string[] addresses { get; set; }
    }
}