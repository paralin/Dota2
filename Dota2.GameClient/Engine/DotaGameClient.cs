using System;
using System.Collections.Generic;
using System.Net;
using Dota2.GameClient.Engine.Control;
using Dota2.GameClient.Engine.Data;
using Dota2.GameClient.Engine.Game;
using Dota2.GameClient.Engine.Game.Data;
using Dota2.GameClient.Engine.Game.Entities;
using Dota2.GameClient.Engine.Session;
using Dota2.GameClient.Utils;
using Dota2.GC;
using Dota2.GC.Dota.Internal;
using SteamKit2;
using SteamKit2.Internal;

namespace Dota2.GameClient.Engine
{
    /// <summary>
    ///     A client capable of connecting to Source 1 servers.
    /// </summary>
    public partial class DotaGameClient : IDisposable
    {
        /// <summary>
        ///     A queue of available game connect tokens.
        /// </summary>
        private readonly Queue<byte[]> _gameConnectTokens;

        /// <summary>
        ///     List of registered callbacks for cleanup later.
        /// </summary>
        private readonly List<CallbackBase> _registeredCallbacks;

        /// <summary>
        ///     Retreived app ownership ticket.
        /// </summary>
        private byte[] _appOwnershipTicket;

        /// <summary>
        ///     Auth ticket for next connect.
        /// </summary>
        private byte[] _authTicket;

        /// <summary>
        ///     The current connect attempt ID.
        /// </summary>
        private uint _connectAttempt;

        /// <summary>
        /// Is there a pending connect request
        /// </summary>
        private bool _waitingForAuthTicket;

        /// <summary>
        /// Connect lobby
        /// </summary>
        private CSODOTALobby _connectLobby = null;

        /// <summary>
        ///     Connect details.
        /// </summary>
        private DOTAConnectDetails _connectDetails;

        /// <summary>
        ///     Callback manager.
        /// </summary>
        internal CallbackManager Callbacks;

        /// <summary>
        ///     The DOTA Gc handler.
        /// </summary>
        internal DotaGCHandler DotaGc;

        /// <summary>
        ///     Public IP address according to steam.
        /// </summary>
        private IPAddress publicIP;

        /// <summary>
        /// Controllers supplied by the user
        /// </summary>
        private List<IDotaGameController> Controllers;

        /// <summary>
        /// Entity builder.
        /// </summary>
        private DotaEntityPool.Builder EntityBuilder;

        /// <summary>
        ///     Create a game client attached to an existing DOTA gc handler.
        /// </summary>
        /// <param name="gc">existing GC handler</param>
        /// <param name="cb">callback manager</param>
        /// <param name="publicIp">optionally help out by specifying the public ip address</param>
        public DotaGameClient(DotaGCHandler gc, CallbackManager cb, IPAddress publicIp = null)
        {
            Callbacks = cb;
            _registeredCallbacks = new List<CallbackBase>();
            _gameConnectTokens = new Queue<byte[]>(11);
            DotaGc = gc;
            RegisterCallbacks();
            if (DotaGc.SteamClient.IsConnected) FetchAppTicket();
            if (publicIp != null) publicIP = publicIp;
            else if (DotaGc.SteamClient.IsConnected) CheckPublicIP();
            EntityBuilder = DotaEntitySet.Associate(new DotaEntityPool.Builder());
            Controllers = new List<IDotaGameController>(1);
        }

        /// <summary>
        ///     The game session.
        /// </summary>
        public DotaGameSession Session { get; private set; }

        /// <summary>
        ///     Dispose of the game client.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
            foreach (var cb in _registeredCallbacks)
                Callbacks.Unregister(cb);
            _registeredCallbacks.Clear();
        }

        /// <summary>
        ///     Registers all internal handlers.
        /// </summary>
        private void RegisterCallbacks()
        {
            var cbs = _registeredCallbacks;
            cbs.Add(
                new Callback<SteamApps.AppOwnershipTicketCallback>(cb =>
                {
                    if (cb.AppID != (uint) DotaGc.GameID || cb.Result != EResult.OK || cb.Ticket == null) return;
                    _appOwnershipTicket = cb.Ticket;
                    Log("Received app ownership ticket with " + _appOwnershipTicket.Length + " length.");
                    if (_waitingForAuthTicket)
                    {
                        _waitingForAuthTicket = false;
                        Connect(_connectLobby);
                    }
                }, Callbacks)
                );
            cbs.Add(
                new Callback<DotaGCHandler.GCWelcomeCallback>(callback => FetchAppTicket(), Callbacks)
                );
            cbs.Add(
                new Callback<SteamApps.GameConnectTokensCallback>(cb =>
                {
                    Log("Received " + cb.Tokens.Count + " game connect tokens, keeping a total of " + cb.TokensToKeep +
                        ", currently have " + _gameConnectTokens.Count + ".");
                    foreach (var tok in cb.Tokens) _gameConnectTokens.Enqueue(tok);
                    var rem = (int) cb.TokensToKeep - _gameConnectTokens.Count;
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
                new Callback<DotaGCHandler.AuthListAck>(cb =>
                {
                    if (_connectDetails == null || cb.authAck.ticket_crc[0] != _connectDetails.AuthTicketCRC) return;
                    Log("Received ack for auth ticket, proceeding with connection.");
                    BeginServerSession();
                }, Callbacks)
                );
            cbs.Add(
                new Callback<DotaGCHandler.BeginSessionResponse>(cb =>
                {
                    if (cb.response.Result != EResult.OK)
                    {
                        Log("Result for session start is " + cb.response.Result + ", not ok!");
                        Disconnect();
                        return;
                    }

                    if (_connectDetails == null)
                    {
                        Log("Received a session create response with no connectDetails, disconnecting...");
                        Disconnect();
                        return;
                    }

                    _connectDetails.SteamworksSessionId = cb.response.SessionId;

                    Log("Session start received with ID " + _connectDetails.SteamworksSessionId +
                        ", starting game session...");
                    Session = new DotaGameSession(_connectDetails, Controllers.ToArray(), EntityBuilder);
                    Session.Callback += (sender, args) => DotaGc.SteamClient.PostCallback(args.msg);
                    Session.Closed += (s, a) => Disconnect();
                    Session.Start();
                }, Callbacks)
                );
        }

        /// <summary>
        ///     Connect to the game server. Will use existing lobby on default.
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

            _connectLobby = lobb;
            if (_appOwnershipTicket == null)
            {
                Log("Waiting for ownership ticket...");
                _waitingForAuthTicket = true;
                FetchAppTicket();
                return;
            }

            _authTicket = AuthTicket.CreateAuthTicket(_gameConnectTokens.Dequeue(), publicIP);

            var ver = new CMsgAuthTicket
            {
                gameid = (uint) DotaGc.GameID,
                h_steam_pipe = 327684,
                ticket = _authTicket
            };

            using (var stream = Bitstream.CreateWith(_authTicket))
                ver.ticket_crc = CrcUtils.Compute32(stream);

            _connectDetails = new DOTAConnectDetails
            {
                AuthTicket = _authTicket,
                ServerAuthTicket = AuthTicket.CreateServerTicket(DotaGc.SteamClient.SteamID, _authTicket, _appOwnershipTicket),
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
                    tokens_left = (uint) _gameConnectTokens.Count,
                    app_ids = {(uint) DotaGc.GameID},
                    tickets = {ver},
                    message_sequence = 2 // Second in sequence.
                }
            };

            DotaGc.SteamClient.Send(msg);
            Log("Sent crc ticket auth list, hash: " + ver.ticket_crc + ".");
        }

        /// <summary>
        ///     Start the connection to the game server after auth ack.
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
        ///     Disconnect from the game server or cancel connection attempt.
        /// </summary>
        public void Disconnect()
        {
            if (_connectDetails == null) return;
            _connectDetails = null;
            Session?.Stop();
            Session = null;
            _waitingForAuthTicket = false;
        }

        /// <summary>
        /// Adds a controller to the client.
        /// </summary>
        /// <param name="cont"></param>
        public void RegisterController(IDotaGameController cont)
        {
            if(Session != null) throw new InvalidOperationException("Controllers must be added before the client connects.");
            if(!Controllers.Contains(cont)) Controllers.Add(cont);
        }

        /// <summary>
        /// Register an entity mapping class in the system.
        /// </summary>
        /// <typeparam name="T">Entity mapping class.</typeparam>
        /// <param name="cname">Table name.</param>
        /// <param name="factory">Factory to build the class.</param>
        public void RegisterEntityMapping<T>(string cname, Func<uint, DotaGameState, MappedEntityClass> factory)
        {
            if(Session != null) throw new InvalidOperationException("Entity mappings must be added before the client connects.");
            EntityBuilder.Associate<T>(cname, factory);
        }


        /// <summary>
        ///     Check the public IP address.
        /// </summary>
        private void CheckPublicIP()
        {
            Log(
                "GameClient init while steam client already connected. Checking public IP ourselves. This is unreliable.");
            using (WebClient req = new WebClient())
            {
                req.Headers["User-Agent"] =
                    "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";
                var addrstr = req.DownloadString("http://bot.whatismyipaddress.com/").Replace("\n", "").Trim();
                publicIP = IPAddress.Parse(addrstr);
                Log("Public IP address resolved to " + publicIP + ".");
            }
        }

        /// <summary>
        ///     Log a message.
        /// </summary>
        /// <param name="message"></param>
        internal void Log(string message)
        {
            DotaGc?.SteamClient?.PostCallback(new LogMessage(message));
        }

        /// <summary>
        ///     Request the app ownership ticket from Steam.
        /// </summary>
        private void FetchAppTicket()
        {
            if (_appOwnershipTicket != null) return;
            DotaGc.SteamClient.Send(
                new ClientMsgProtobuf<CMsgClientGetAppOwnershipTicket>(EMsg.ClientGetAppOwnershipTicket)
                {
                    Body = {app_id = (uint) DotaGc.GameID}
                });
            Log("Requested app ownership ticket.");
        }
    }
}