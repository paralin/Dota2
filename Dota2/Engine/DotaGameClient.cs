using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Dota2.Engine.Data;
using Dota2.GC.Dota.Internal;
using Dota2.Utils;
using SteamKit2;
using SteamKit2.Internal;

namespace Dota2.Engine
{
    /// <summary>
    /// A client capable of connecting to Source 1 servers.
    /// </summary>
    public class DotaGameClient : IDisposable
    {
        /// <summary>
        /// The DOTA Gc handler.
        /// </summary>
        internal DotaGCHandler DotaGc;

        /// <summary>
        /// Callback manager.
        /// </summary>
        internal CallbackManager Callbacks;

        /// <summary>
        /// List of registered callbacks for cleanup later.
        /// </summary>
        private readonly List<CallbackBase> _registeredCallbacks;

        /// <summary>
        /// A queue of available game connect tokens.
        /// </summary>
        private readonly Queue<byte[]> _gameConnectTokens;

        /// <summary>
        /// Public IP address according to steam.
        /// </summary>
        private IPAddress publicIP;

        /// <summary>
        /// Retreived app ownership ticket.
        /// </summary>
        private byte[] _appOwnershipTicket = null;

        /// <summary>
        /// Auth ticket for next connect.
        /// </summary>
        private byte[] _authTicket = null;

        /// <summary>
        /// Connect details.
        /// </summary>
        private DOTAConnectDetails _connectDetails = null;

        /// <summary>
        /// The current connect attempt ID.
        /// </summary>
        private uint _connectAttempt = 0;

        /// <summary>
        /// Subscribe to get text debug logging.
        /// </summary>
        public event EventHandler<LogEventArgs> OnLog;

        /// <summary>
        /// Create a game client attached to an existing DOTA gc handler.
        /// </summary>
        /// <param name="gc">existing GC handler</param>
        /// <param name="cb">callback manager</param>
        public DotaGameClient(DotaGCHandler gc, CallbackManager cb)
        {
            Callbacks = cb;
            _registeredCallbacks = new List<CallbackBase>();
            _gameConnectTokens = new Queue<byte[]>(11);
            this.DotaGc = gc;
            RegisterCallbacks();
            if (DotaGc.Ready) FetchAppTicket();
            if (DotaGc.SteamClient.IsConnected) CheckPublicIP();
        }

        /// <summary>
        /// Registers all internal handlers.
        /// </summary>
        private void RegisterCallbacks()
        {
            var gc = DotaGc;
            var cbs = _registeredCallbacks;
            cbs.Add(
                new Callback<SteamApps.AppOwnershipTicketCallback>(cb =>
                {
                    if (cb.AppID != (uint)DotaGc.GameID || cb.Result != EResult.OK || cb.Ticket == null) return;
                    Log("Received app ownership ticket with " + _appOwnershipTicket.Length + " length.");
                    _appOwnershipTicket = cb.Ticket;
                }, Callbacks)
            );
            cbs.Add(
                new Callback<Dota2.DotaGCHandler.GCWelcomeCallback>(callback => FetchAppTicket(), Callbacks)
            );
            cbs.Add(
                new Callback<SteamApps.GameConnectTokensCallback>(cb =>
                {
                    Log("Received " + cb.Tokens.Count + " game connect tokens, keeping a total of " + cb.TokensToKeep +
                        ", currently have " + _gameConnectTokens.Count + ".");
                    foreach(var tok in cb.Tokens) _gameConnectTokens.Enqueue(tok);
                    var rem = (int)cb.TokensToKeep - _gameConnectTokens.Count;
                    while (rem > 0)
                    {
                        _gameConnectTokens.Dequeue();
                        rem--;
                    }
                }, Callbacks)
            );
            cbs.Add(
                new Callback<SteamUser.LoggedOnCallback>(cb =>
                {
                    if (cb.Result == EResult.OK)
                    {
                        publicIP = cb.PublicIP;
                    }
                }, Callbacks)
            );
            cbs.Add(
                new Callback<Dota2.DotaGCHandler.AuthListAck>(cb =>
                {
                    if (_connectDetails == null || cb.authAck.ticket_crc[0] != _connectDetails.AuthTicketCRC) return;
                    Log("Received ack for auth ticket, proceeding with connection.");
                    BeginServerSession();
                }, Callbacks)
            );
            cbs.Add(
                new Callback<Dota2.DotaGCHandler.BeginSessionResponse>(cb =>
                {
                    if (cb.response.Result != EResult.OK)
                    {
                        Log("Result for session start is " + cb.response.Result + ", not ok!");
                        Disconnect();
                        return;
                    }

                    _connectDetails.SteamworksSessionId = cb.response.SessionId;
                }, Callbacks)
            );
        }

        /// <summary>
        /// Connect to the game server. Will use existing lobby on default.
        /// </summary>
        /// <param name="lobb"></param>
        public void Connect(CSODOTALobby lobb = null)
        {
            if (_connectDetails != null) Disconnect();

            lobb = lobb ?? DotaGc.Lobby;
            if (lobb == null)
            {
                Log("No lobby so not connecting.");
                return;
            }

            _authTicket = AuthTicket.CreateAuthTicket(_gameConnectTokens.Dequeue(), publicIP);

            var ver = new CMsgAuthTicket()
            {
                gameid = (uint)DotaGc.GameID,
                h_steam_pipe = 327684,
                ticket = _authTicket
            };

            using (var stream = Bitstream.CreateWith(_authTicket))
                ver.ticket_crc = CrcUtils.Compute32(stream);

            _connectDetails = new DOTAConnectDetails()
            {
                AuthTicket = _authTicket,
                ConnectInfo = lobb.connect,
                ConnectID = _connectAttempt++,
                AuthTicketCRC = ver.ticket_crc,
                Name = DotaGc.SteamClient.GetHandler<SteamFriends>().GetPersonaName(),
                PassKey = lobb.pass_key,
                SteamId = DotaGc.SteamClient.SteamID.ConvertToUInt64()
            };

            var msg = new ClientMsgProtobuf<CMsgClientAuthList>(EMsg.ClientAuthList)
            {
                Body =
                {
                    tokens_left = (uint)_gameConnectTokens.Count,
                    app_ids = { (uint)DotaGc.GameID },
                    tickets = { ver },
                    message_sequence = 2 // Second in sequence.
                }
            };

            DotaGc.SteamClient.Send(msg);
            Log("Sent crc ticket auth list, hash: "+ver.ticket_crc+".");
        }

        /// <summary>
        /// Start the connection to the game server after auth ack.
        /// </summary>
        private void BeginServerSession()
        {
            DotaGc.SteamClient.Send(new ClientMsg<MsgClientOGSBeginSession>
            {
                Body =
                {
                    AccountType = (byte) DotaGc.SteamClient.SteamID.AccountType,
                    AccountId = DotaGc.SteamClient.SteamID,
                    AppId = (uint) DotaGc.GameID,
                    TimeStarted =
                        (uint) (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds
                }
            });
        }

        /// <summary>
        /// Disconnect from the game server or cancel connection attempt.
        /// </summary>
        public void Disconnect()
        {
            if (_connectDetails == null) return;
            _connectDetails = null;

        }

        /// <summary>
        /// Check the public IP address.
        /// </summary>
        private void CheckPublicIP()
        {
            Log("GameClient init while steam client already connected. Checking public IP ourselves. This is unreliable.");
            System.Net.WebRequest req = System.Net.WebRequest.Create("http://bot.whatismyipaddress.com/");
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            publicIP = IPAddress.Parse(sr.ReadToEnd().Trim());
            Log("Public IP address resolved to " + publicIP + ".");
        }

        /// <summary>
        /// Log a message.
        /// </summary>
        /// <param name="message"></param>
        internal void Log(string message)
        {
            if (OnLog != null) OnLog(this, new LogEventArgs(message));
        }

        /// <summary>
        /// Request the app ownership ticket from Steam.
        /// </summary>
        private void FetchAppTicket()
        {
            if (_appOwnershipTicket != null) return;
            DotaGc.SteamClient.Send(new ClientMsgProtobuf<CMsgClientGetAppOwnershipTicket>(EMsg.ClientGetAppOwnershipTicket) {Body = {app_id = (uint)DotaGc.GameID} });
            Log("Requested app ownership ticket.");
        }

        /// <summary>
        /// Dispose of the game client.
        /// </summary>
        void IDisposable.Dispose()
        {
            foreach (var cb in _registeredCallbacks)
                Callbacks.Unregister(cb);
            _registeredCallbacks.Clear();
        }

        /// <summary>
        /// Used for the OnLog event.
        /// </summary>
        public class LogEventArgs : EventArgs
        {
            public string Message { get; internal set; }

            /// <summary>
            /// Creates a new log event
            /// </summary>
            /// <param name="msg"></param>
            internal LogEventArgs(string msg)
            {
                Message = msg;
            }
        }
    }
}
