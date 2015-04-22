using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Dota2.Datagram.Internal;
using ProtoBuf;

namespace Dota2.Datagram
{
    /// <summary>
    /// Implementation of a client to connect to Steam Datagram servers.
    /// </summary>
    public class DatagramClient
    {
        private UdpClient client;

        /// <summary>
        /// Create a new DatagramClient with a datagram server to connect to.
        /// </summary>
        public DatagramClient()
        {
            client = new UdpClient();
        }

        /// <summary>
        /// Connect the UDPClient to an endpoint.
        /// </summary>
        /// <param name="endpoint"></param>
        public void Connect(IPEndPoint endpoint)
        {
            client.Connect(endpoint);
        }

        /// <summary>
        /// Mostly for testing, wait for one complete response.
        /// </summary>
        public byte[] WaitOneResponse()
        {
            var end = new IPEndPoint(IPAddress.Any, 0);
            return client.Receive(ref end);
        }

        /// <summary>
        /// Format a packet to be sent to the server.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="body"></param>
        public void Send(uint type, byte[] body)
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(type);
                    bw.Write(body.Length);
                    bw.Write(body);
                    client.Send(ms.ToArray(), (int)ms.Length);
                }
            }
        }

        /// <summary>
        /// Serialize a protobuf message and send it to the server.
        /// </summary>
        /// <typeparam name="T">The protobuf type.</typeparam>
        /// <param name="mid">The EMsg.</param>
        /// <param name="msg">The protobuf message.</param>
        public void SendProtobuf<T>(ESteamDatagramMsgID mid, T msg)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, msg);
                Send((uint)mid, ms.ToArray());
            }
        }

        #region Messages

        /// <summary>
        /// Send a datagram server ping request
        /// </summary>
        public void SendPing()
        {
            var body = new CMsgSteamDatagramRouterPingRequest()
            {
                client_timestamp = (uint) (DateTime.UtcNow - new DateTime(0)).TotalMilliseconds,
            };

            body.request_latency_service_ids.Add(3);

            SendProtobuf(ESteamDatagramMsgID.k_ESteamDatagramMsg_RouterPingRequest, body);
        }

        #endregion
    }
}
