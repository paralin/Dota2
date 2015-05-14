/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Dota2.Base.Data;
using Dota2.GC.Dota.Internal;
using Dota2.GC.Internal;
using ProtoBuf;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.Internal;

namespace Dota2
{
    /// <summary>
    ///     This handler handles all Dota 2 GC lobbies interaction.
    /// </summary>
    public sealed partial class DotaGCHandler : ClientMsgHandler
    {
        private List<CMsgSerializedSOCache> cache = new List<CMsgSerializedSOCache>();
        private Timer gcConnectTimer;
        private bool running = false;
        internal SteamClient SteamClient;
        private bool ready = false;
        private Games gameId = Games.DOTA2;

        /// <summary>
        /// The Game ID the handler will use.
        /// </summary>
        public Games GameID => gameId;

        /// <summary>
        /// Is the GC ready?
        /// </summary>
        public bool Ready
        {
            get { return ready; }
        }

        internal DotaGCHandler(SteamClient client, Games appId)
        {
            gameId = appId;
            SteamClient = client;
            gcConnectTimer = new Timer(5000);
            gcConnectTimer.Elapsed += (sender, args) =>
            {
                if (!running)
                {
                    gcConnectTimer.Stop();
                    return;
                }
                SayHello();
            };
        }

        /// <summary>
        /// Setup the DOTA 2 GC handler on an existing client.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="appId">Optional, specify the GC to communicate with.</param>
        public static void Bootstrap(SteamClient client, Games appId = Games.DOTA2)
        {
            client.AddHandler(new DotaGCHandler(client, appId));
        }

        /// <summary>
        ///     The current up to date lobby
        /// </summary>
        /// <value>The lobby.</value>
        public CSODOTALobby Lobby { get; private set; }

        /// <summary>
        ///     The current up to date party.
        /// </summary>
        public CSODOTAParty Party { get; private set; }

        /// <summary>
        ///     The active invite to the party.
        /// </summary>
        public CSODOTAPartyInvite PartyInvite { get; private set; }

        /// <summary>
        ///     Last invitation to the game.
        /// </summary>
        public CMsgClientUDSInviteToGame Invitation { get; private set; }

        /// <summary>
        ///     Sends a game coordinator message.
        /// </summary>
        /// <param name="msg">The GC message to send.</param>
        public void Send(IClientGCMsg msg)
        {
            var clientMsg = new ClientMsgProtobuf<CMsgGCClient>(EMsg.ClientToGC);

            clientMsg.Body.msgtype = MsgUtil.MakeGCMsg(msg.MsgType, msg.IsProto);
            clientMsg.Body.appid = (uint)GameID;

            clientMsg.Body.payload = msg.Serialize();

            Client.Send(clientMsg);
        }

        /// <summary>
        /// Old method of starting the DOTA client.
        /// </summary>
        [Obsolete("LaunchDota is deprecated, use Start/Stop instead.")]
        public void LaunchDota()
        {
            Start();
        }

        /// <summary>
        /// Start playing DOTA 2 and automatically request a GC session.
        /// </summary>
        public void Start()
        {
            running = true;
            var playGame = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

            playGame.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = (ulong)GameID
            });

            Client.Send(playGame);

            gcConnectTimer.Stop();
            SayHello();
            gcConnectTimer.Start();
        }

        /// <summary>
        /// Send the hello message requesting a GC session. Do not call this manually!
        /// </summary>
        public void SayHello()
        {
            if (!running) return;
            var clientHello = new ClientGCMsgProtobuf<CMsgClientHello>((uint)EGCBaseClientMsg.k_EMsgGCClientHello);
            Send(clientHello);
        }

        /// <summary>
        /// Stop playing DOTA 2.
        /// </summary>
        public void Stop()
        {
            running = false;
            gcConnectTimer.Stop();

            var playGame = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
            // playGame.Body.games_played left empty
            Client.Send(playGame);
        }

        /// <summary>
        ///     Abandon the current game
        /// </summary>
        public void AbandonGame()
        {
            var abandon = new ClientGCMsgProtobuf<CMsgAbandonCurrentGame>((uint) EDOTAGCMsg.k_EMsgGCAbandonCurrentGame);
            Send(abandon);
        }

        /// <summary>
        ///     Cancel the queue for a match
        /// </summary>
        public void StopQueue()
        {
            var queue = new ClientGCMsgProtobuf<CMsgStopFindingMatch>((uint) EDOTAGCMsg.k_EMsgGCStopFindingMatch);
            Send(queue);
        }

        /// <summary>
        ///     Respond to a party invite
        /// </summary>
        /// <param name="party_id"></param>
        /// <param name="accept"></param>
        public void RespondPartyInvite(ulong party_id, bool accept = true)
        {
            var invite = new ClientGCMsgProtobuf<CMsgPartyInviteResponse>((uint) EGCBaseMsg.k_EMsgGCPartyInviteResponse);
            invite.Body.party_id = party_id;
            invite.Body.accept = accept;
            invite.Body.as_coach = false;
            invite.Body.team_id = 0;
            invite.Body.game_language_enum = 1;
            invite.Body.game_language_name = "english";
            Send(invite);
        }

        /// <summary>
        ///     Join a lobby given a lobby ID
        /// </summary>
        /// <param name="lobbyId"></param>
        /// <param name="pass_key"></param>
        public void JoinLobby(ulong lobbyId, string pass_key = null)
        {
            var joinLobby = new ClientGCMsgProtobuf<CMsgPracticeLobbyJoin>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyJoin);
            joinLobby.Body.lobby_id = lobbyId;
            joinLobby.Body.pass_key = pass_key;
            Send(joinLobby);
        }

        /// <summary>
        ///     Leave a lobby
        /// </summary>
        public void LeaveLobby()
        {
            var leaveLobby =
                new ClientGCMsgProtobuf<CMsgPracticeLobbyLeave>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyLeave);
            Lobby = null;
            Send(leaveLobby);
        }

        /// <summary>
        ///     Leave a party.
        /// </summary>
        public void LeaveParty()
        {
            var leaveParty = new ClientGCMsgProtobuf<CMsgLeaveParty>((uint) EGCBaseMsg.k_EMsgGCLeaveParty);
            Party = null;
            Send(leaveParty);
        }

        /// <summary>
        ///     Respond to a ping()
        /// </summary>
        public void Pong()
        {
            var pingResponse = new ClientGCMsgProtobuf<CMsgGCClientPing>((uint) EGCBaseClientMsg.k_EMsgGCPingResponse);
            Send(pingResponse);
        }

        /// <summary>
        ///     Joins a broadcast channel in the lobby
        /// </summary>
        /// <param name="channel">The channel slot to join. Valid channel values range from 0 to 5.</param>
        public void JoinBroadcastChannel(uint channel = 0)
        {
            var joinChannel =
                new ClientGCMsgProtobuf<CMsgPracticeLobbyJoinBroadcastChannel>(
                    (uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyJoinBroadcastChannel);
            joinChannel.Body.channel = channel;
            Send(joinChannel);
        }

        /// <summary>
        ///     Join a team
        /// </summary>
        /// <param name="channel">The channel slot to join. Valid channel values range from 0 to 5.</param>
        public void JoinCoachSlot(DOTA_GC_TEAM team = DOTA_GC_TEAM.DOTA_GC_TEAM_GOOD_GUYS)
        {
            var joinChannel =
                new ClientGCMsgProtobuf<CMsgPracticeLobbySetCoach>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbySetCoach)
                {
                    Body = {team = team}
                };
            Send(joinChannel);
        }

        /// <summary>
        /// Requests a subscription refresh for a specific cache ID.
        /// </summary>
        /// <param name="type">the type of the cache</param>
        /// <param name="id">the cache soid</param>
        public void RequestSubscriptionRefresh(uint type, ulong id)
        {
            var refresh =
                new ClientGCMsgProtobuf<CMsgSOCacheSubscriptionRefresh>((uint) ESOMsg.k_ESOMsg_CacheSubscriptionRefresh);
            refresh.Body.owner_soid = new CMsgSOIDOwner
            {
                id = id,
                type = type
            };
            Send(refresh);
        }

        /// <summary>
        /// Send a request for player information.
        /// </summary>
        /// <param name="ids">DOTA 2 profile ids.</param>
        public void RequestPlayerInfo(IEnumerable<UInt32> ids)
        {
            var req = new ClientGCMsgProtobuf<CMsgGCPlayerInfoRequest>((uint) EDOTAGCMsg.k_EMsgGCPlayerInfoRequest);
            req.Body.account_ids.AddRange(ids);
            Send(req);
        }

        /// <summary>
        /// Requests the entire pro team list.
        /// </summary>
        public void RequestProTeamList()
        {
            var req = new ClientGCMsgProtobuf<CMsgDOTAProTeamListRequest>((uint) EDOTAGCMsg.k_EMsgGCProTeamListRequest);
            Send(req);
        }

        /// <summary>
        /// Switches team in a GC Lobby.
        /// </summary>
        /// <param name="team">target team</param>
        /// <param name="slot">slot on the team</param>
        public void JoinTeam(DOTA_GC_TEAM team, uint slot = 1)
        {
            var joinSlot =
                new ClientGCMsgProtobuf<CMsgPracticeLobbySetTeamSlot>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbySetTeamSlot);
            joinSlot.Body.team = team;
            joinSlot.Body.slot = slot;
            Send(joinSlot);
        }

        /// <summary>
        ///     Start the game
        /// </summary>
        public void LaunchLobby()
        {
            var start =
                new ClientGCMsgProtobuf<CMsgPracticeLobbyLaunch>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyLaunch);
            Send(start);
        }

        /// <summary>
        ///     Create a practice or tournament or custom lobby.
        /// </summary>
        /// <param name="pass_key">Password for the lobby.</param>
        /// <param name="details">Lobby options.</param>
        /// <param name="tournament_game">Is this a tournament game?</param>
        /// <param name="tournament">Tournament ID</param>
        /// <param name="tournament_game_id">Tournament game ID</param>
        public void CreateLobby(string pass_key, CMsgPracticeLobbySetDetails details, bool tournament_game = false,
            uint tournament = 0, uint tournament_game_id = 0)
        {
            var create = new ClientGCMsgProtobuf<CMsgPracticeLobbyCreate>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyCreate);
            create.Body.pass_key = pass_key;
            create.Body.tournament_game_id = tournament_game_id;
            create.Body.tournament_game = tournament_game;
            create.Body.tournament_id = tournament;
            create.Body.lobby_details = details;
            create.Body.lobby_details.pass_key = pass_key;
            Send(create);
        }

        /// <summary>
        ///     Invite someone to the party.
        /// </summary>
        /// <param name="steam_id">Steam ID</param>
        public void InviteToParty(ulong steam_id)
        {
            var invite = new ClientGCMsgProtobuf<CMsgInviteToParty>((uint) EGCBaseMsg.k_EMsgGCInviteToParty);
            invite.Body.steam_id = steam_id;
            Send(invite);
        }

        /// <summary>
        ///     Send the chat invite message.
        /// </summary>
        /// <param name="steam_id"></param>
        public void InviteToPartyUDS(ulong steam_id, ulong party_id)
        {
            var invite = new ClientMsgProtobuf<CMsgClientUDSInviteToGame>(EMsg.ClientUDSInviteToGame);
            invite.Body.connect_string = "+invite " + party_id;
            invite.Body.steam_id_dest = steam_id;
            invite.Body.steam_id_src = 0;
            Client.Send(invite);
        }

        /// <summary>
        /// Sets the team details on the team the bot is sitting on.
        /// </summary>
        /// <param name="teamid"></param>
        public void ApplyTeamToLobby(uint teamid)
        {
            var apply =
                new ClientGCMsgProtobuf<CMsgApplyTeamToPracticeLobby>((uint) EDOTAGCMsg.k_EMsgGCApplyTeamToPracticeLobby);
            apply.Body.team_id = teamid;
            Send(apply);
        }

        /// <summary>
        ///     Set coach slot in party
        /// </summary>
        /// <param name="coach"></param>
        public void SetPartyCoach(bool coach = false)
        {
            var slot =
                new ClientGCMsgProtobuf<CMsgDOTAPartyMemberSetCoach>((uint) EDOTAGCMsg.k_EMsgGCPartyMemberSetCoach);
            slot.Body.wants_coach = coach;
            Send(slot);
        }

        /// <summary>
        ///     Kick a player from the party
        /// </summary>
        /// <param name="steam_id">Steam ID of player to kick</param>
        public void KickPlayerFromParty(ulong steam_id)
        {
            var kick = new ClientGCMsgProtobuf<CMsgKickFromParty>((uint) EGCBaseMsg.k_EMsgGCKickFromParty);
            kick.Body.steam_id = steam_id;
            Send(kick);
        }

        /// <summary>
        ///     Kick a player from the lobby
        /// </summary>
        /// <param name="steam_id">Account ID of player to kick</param>
        public void KickPlayerFromLobby(uint account_id)
        {
            var kick = new ClientGCMsgProtobuf<CMsgPracticeLobbyKick>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyKick);
            kick.Body.account_id = account_id;
            Send(kick);
        }

        /// <summary>
        ///     Joins a chat channel
        /// </summary>
        /// <param name="name"></param>
        public void JoinChatChannel(string name,
            DOTAChatChannelType_t type = DOTAChatChannelType_t.DOTAChannelType_Custom)
        {
            var joinChannel = new ClientGCMsgProtobuf<CMsgDOTAJoinChatChannel>((uint) EDOTAGCMsg.k_EMsgGCJoinChatChannel);
            joinChannel.Body.channel_name = name;
            joinChannel.Body.channel_type = type;
            Send(joinChannel);
        }

        /// <summary>
        ///     Request a match result
        /// </summary>
        /// <param name="matchId">Match id</param>
        public void RequestMatchResult(ulong matchId)
        {
            var requestMatch =
                new ClientGCMsgProtobuf<CMsgGCMatchDetailsRequest>((uint) EDOTAGCMsg.k_EMsgGCMatchDetailsRequest);
            requestMatch.Body.match_id = matchId;

            Send(requestMatch);
        }

        /// <summary>
        ///     Sends a message in a chat channel.
        /// </summary>
        /// <param name="channelid">Id of channel to join.</param>
        /// <param name="message">Message to send.</param>
        public void SendChannelMessage(ulong channelid, string message)
        {
            var chatMsg = new ClientGCMsgProtobuf<CMsgDOTAChatMessage>((uint) EDOTAGCMsg.k_EMsgGCChatMessage);
            chatMsg.Body.channel_id = channelid;
            chatMsg.Body.text = message;
            Send(chatMsg);
        }

        /// <summary>
        ///     Leaves chat channel
        /// </summary>
        /// <param name="channelid">id of channel to leave</param>
        public void LeaveChatChannel(ulong channelid)
        {
            var leaveChannel =
                new ClientGCMsgProtobuf<CMsgDOTALeaveChatChannel>((uint) EDOTAGCMsg.k_EMsgGCLeaveChatChannel);
            leaveChannel.Body.channel_id = channelid;
            Send(leaveChannel);
        }

        /// <summary>
        ///     Requests a lobby list with an optional password
        /// </summary>
        /// <param name="pass_key">Pass key.</param>
        /// <param name="tournament"> Tournament games? </param>
        public void PracticeLobbyList(string pass_key = null, bool tournament = false)
        {
            var list = new ClientGCMsgProtobuf<CMsgPracticeLobbyList>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyList);
            list.Body.pass_key = pass_key;
            list.Body.tournament_games = tournament;
            Send(list);
        }

        private static IPacketGCMsg GetPacketGCMsg(uint eMsg, byte[] data)
        {
            // strip off the protobuf flag
            uint realEMsg = MsgUtil.GetGCMsg(eMsg);

            if (MsgUtil.IsProtoBuf(eMsg))
            {
                return new PacketClientGCMsgProtobuf(realEMsg, data);
            }
            return new PacketClientGCMsg(realEMsg, data);
        }

        /// <summary>
        ///     Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg(IPacketMsg packetMsg)
        {
            if (packetMsg.MsgType == EMsg.ClientFromGC)
            {
                var msg = new ClientMsgProtobuf<CMsgGCClient>(packetMsg);
                if (msg.Body.appid == (uint)GameID)
                {
                    IPacketGCMsg gcmsg = GetPacketGCMsg(msg.Body.msgtype, msg.Body.payload);
                    var messageMap = new Dictionary<uint, Action<IPacketGCMsg>>
                    {
                        {(uint) EGCBaseClientMsg.k_EMsgGCClientWelcome, HandleWelcome},
                        {(uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyJoinResponse, HandlePracticeLobbyJoinResponse},
                        {(uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyListResponse, HandlePracticeLobbyListResponse},
                        {(uint) ESOMsg.k_ESOMsg_CacheSubscribed, HandleCacheSubscribed},
                        {(uint) ESOMsg.k_ESOMsg_CacheUnsubscribed, HandleCacheUnsubscribed},
                        {(uint) ESOMsg.k_ESOMsg_Destroy, HandleCacheDestroy},
                        {(uint) EGCBaseClientMsg.k_EMsgGCPingRequest, HandlePingRequest},
                        {(uint) EDOTAGCMsg.k_EMsgGCJoinChatChannelResponse, HandleJoinChatChannelResponse},
                        {(uint) EDOTAGCMsg.k_EMsgGCChatMessage, HandleChatMessage},
                        {(uint) EDOTAGCMsg.k_EMsgGCOtherJoinedChannel, HandleOtherJoinedChannel},
                        {(uint) EDOTAGCMsg.k_EMsgGCOtherLeftChannel, HandleOtherLeftChannel},
                        {(uint) ESOMsg.k_ESOMsg_UpdateMultiple, HandleUpdateMultiple},
                        {(uint) EDOTAGCMsg.k_EMsgGCPopup, HandlePopup},
                        {(uint) EDOTAGCMsg.k_EMsgDOTALiveLeagueGameUpdate, HandleLiveLeageGameUpdate},
                        {(uint) EGCBaseMsg.k_EMsgGCInvitationCreated, HandleInvitationCreated},
                        {(uint) EDOTAGCMsg.k_EMsgGCMatchDetailsResponse, HandleMatchDetailsResponse},
                        {(uint) EGCBaseClientMsg.k_EMsgGCClientConnectionStatus, HandleConnectionStatus},
                        {(uint) EDOTAGCMsg.k_EMsgGCProTeamListResponse, HandleProTeamList},
                        {(uint) EDOTAGCMsg.k_EMsgGCFantasyLeagueInfo, HandleFantasyLeagueInfo},
                        {(uint) EDOTAGCMsg.k_EMsgGCPlayerInfo, HandlePlayerInfo}
                    };
                    Action<IPacketGCMsg> func;
                    if (!messageMap.TryGetValue(gcmsg.MsgType, out func))
                    {
                        Client.PostCallback(new UnhandledDotaGCCallback(gcmsg));
                        return;
                    }

                    func(gcmsg);
                }
            }
            else
            {
                if (packetMsg.IsProto && packetMsg.MsgType == EMsg.ClientUDSInviteToGame)
                {
                    var msg = new ClientMsgProtobuf<CMsgClientUDSInviteToGame>(packetMsg);
                    Invitation = msg.Body;
                    Client.PostCallback(new SteamPartyInvite(Invitation));
                }else if (packetMsg.MsgType == EMsg.ClientAuthListAck)
                {
                    var msg = new ClientMsgProtobuf<CMsgClientAuthListAck>(packetMsg);
                    Client.PostCallback(new AuthListAck(msg.Body));
                }
            }
        }

        private void HandleInvitationCreated(IPacketGCMsg obj)
        {
            var msg = new ClientGCMsgProtobuf<CMsgInvitationCreated>(obj);
            Client.PostCallback(new InvitationCreated(msg.Body));
        }

        private void HandleCacheSubscribed(IPacketGCMsg obj)
        {
            var sub = new ClientGCMsgProtobuf<CMsgSOCacheSubscribed>(obj);
            foreach (CMsgSOCacheSubscribed.SubscribedType cache in sub.Body.objects)
            {
                HandleSubscribedType(cache);
            }
        }

        private void HandleSubscribedType(CMsgSOCacheSubscribed.SubscribedType cache)
        {
            if (cache.type_id == 2004)
            {
                HandleLobbySnapshot(cache.object_data[0]);
            }
            else if (cache.type_id == 2003)
            {
                HandlePartySnapshot(cache.object_data[0]);
            }
            else if (cache.type_id == 2006)
            {
                HandlePartyInviteSnapshot(cache.object_data[0]);
            }
        }

        public void HandleCacheDestroy(IPacketGCMsg obj)
        {
            var dest = new ClientGCMsgProtobuf<CMsgSOSingleObject>(obj);
            if (PartyInvite != null && dest.Body.type_id == 2006)
            {
                PartyInvite = null;
                Client.PostCallback(new PartyInviteLeave(null));
            }
        }

        private void HandleCacheUnsubscribed(IPacketGCMsg obj)
        {
            var unSub = new ClientGCMsgProtobuf<CMsgSOCacheUnsubscribed>(obj);
            if (Lobby != null && unSub.Body.owner_soid.id == Lobby.lobby_id)
            {
                Lobby = null;
                Client.PostCallback(new PracticeLobbyLeave(unSub.Body));
            }
            else if (Party != null && unSub.Body.owner_soid.id == Party.party_id)
            {
                Party = null;
                Client.PostCallback(new PartyLeave(unSub.Body));
            }
            else if (PartyInvite != null && unSub.Body.owner_soid.id == PartyInvite.group_id)
            {
                PartyInvite = null;
                Client.PostCallback(new PartyInviteLeave(unSub.Body));
            }
            else
                Client.PostCallback(new CacheUnsubscribed(unSub.Body));
        }

        private void HandleLobbySnapshot(byte[] data, bool update = false)
        {
            using (var stream = new MemoryStream(data))
            {
                var lob = Serializer.Deserialize<CSODOTALobby>(stream);
                CSODOTALobby oldLob = Lobby;
                Lobby = lob;
                if (update)
                    Client.PostCallback(new PracticeLobbyUpdate(lob, oldLob));
                else
                    Client.PostCallback(new PracticeLobbySnapshot(lob));
            }
        }

        private void HandlePartySnapshot(byte[] data, bool update = false)
        {
            using (var stream = new MemoryStream(data))
            {
                var party = Serializer.Deserialize<CSODOTAParty>(stream);
                CSODOTAParty oldParty = Party;
                Party = party;
                if (update)
                    Client.PostCallback(new PartyUpdate(party, oldParty));
                else
                    Client.PostCallback(new PartySnapshot(party));
            }
        }

        private void HandlePartyInviteSnapshot(byte[] data, bool update = false)
        {
            using (var stream = new MemoryStream(data))
            {
                var party = Serializer.Deserialize<CSODOTAPartyInvite>(stream);
                CSODOTAPartyInvite oldParty = PartyInvite;
                PartyInvite = party;
                if (update)
                    Client.PostCallback(new PartyInviteUpdate(party, oldParty));
                else
                    Client.PostCallback(new PartyInviteSnapshot(party));
            }
        }

        private void HandleFantasyLeagueInfo(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAFantasyLeagueInfo>(obj);
            Client.PostCallback(new FantasyLeagueInfo(resp.Body));
        }

        private void HandlePlayerInfo(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgGCPlayerInfo>(obj);
            Client.PostCallback(new PlayerInfo(resp.Body.player_infos));
        }

        private void HandlePracticeLobbyListResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgPracticeLobbyListResponse>(obj);
            Client.PostCallback(new PracticeLobbyListResponse(resp.Body));
        }

        private void HandlePracticeLobbyJoinResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgPracticeLobbyJoinResponse>(obj);
            Client.PostCallback(new PracticeLobbyJoinResponse(resp.Body));
        }

        private void HandlePingRequest(IPacketGCMsg obj)
        {
            var req = new ClientGCMsgProtobuf<CMsgGCClientPing>(obj);
            Client.PostCallback(new PingRequest(req.Body));
        }

        private void HandleJoinChatChannelResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAJoinChatChannelResponse>(obj);
            Client.PostCallback(new JoinChatChannelResponse(resp.Body));
        }

        private void HandleChatMessage(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAChatMessage>(obj);
            Client.PostCallback(new ChatMessage(resp.Body));
        }

        private void HandleMatchDetailsResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgGCMatchDetailsResponse>(obj);
            Client.PostCallback(new MatchResultResponse(resp.Body));
        }

        private void HandleConnectionStatus(IPacketGCMsg obj)
        {
            if (!running)
            {
                // Stahhp
                Stop();
                return;
            }

            var resp = new ClientGCMsgProtobuf<CMsgConnectionStatus>(obj);
            Client.PostCallback(new ConnectionStatus(resp.Body));

            if(resp.Body.status != GCConnectionStatus.GCConnectionStatus_HAVE_SESSION) gcConnectTimer.Start();

            ready = resp.Body.status == GCConnectionStatus.GCConnectionStatus_HAVE_SESSION;
        }

        private void HandleProTeamList(IPacketGCMsg msg)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAProTeamListResponse>(msg);
            Client.PostCallback(new ProTeamListResponse(resp.Body));
        }

        private void HandleOtherJoinedChannel(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAOtherJoinedChatChannel>(obj);
            Client.PostCallback(new OtherJoinedChannel(resp.Body));
        }

        private void HandleOtherLeftChannel(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAOtherLeftChatChannel>(obj);
            Client.PostCallback(new OtherLeftChannel(resp.Body));
        }

        private void HandleUpdateMultiple(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgSOMultipleObjects>(obj);
            bool handled = true;
            foreach (CMsgSOMultipleObjects.SingleObject mObj in resp.Body.objects_modified)
            {
                if (mObj.type_id == 2004)
                {
                    HandleLobbySnapshot(mObj.object_data, true);
                }
                else if (mObj.type_id == 2003)
                {
                    HandlePartySnapshot(mObj.object_data, true);
                }
                else if (mObj.type_id == 2006)
                {
                    //HandlePartyInviteSnapshot(mObj.object_data, true);
                }
                else
                {
                    handled = false;
                }
            }
            if (!handled)
            {
                Client.PostCallback(new UnhandledDotaGCCallback(obj));
            }
        }

        private void HandlePopup(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAPopup>(obj);
            Client.PostCallback(new Popup(resp.Body));
        }

        /// <summary>
        ///     GC tells us if there are tournaments running.
        /// </summary>
        /// <param name="obj"></param>
        private void HandleLiveLeageGameUpdate(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTALiveLeagueGameUpdate>(obj);
            Client.PostCallback(new LiveLeagueGameUpdate(resp.Body));
        }

        //Initial message sent when connected to the GC
        private void HandleWelcome(IPacketGCMsg msg)
        {
            gcConnectTimer.Stop();

            ready = true;
            
            // Clear these. They will be updated in the subscriptions if they exist still.
            Lobby = null;
            Party = null;
            PartyInvite = null;

            var wel = new ClientGCMsgProtobuf<CMsgClientWelcome>(msg);
            Client.PostCallback(new GCWelcomeCallback(wel.Body));

            //Handle any cache subscriptions
            foreach (CMsgSOCacheSubscribed cache in wel.Body.outofdate_subscribed_caches)
                foreach (CMsgSOCacheSubscribed.SubscribedType obj in cache.objects)
                    HandleSubscribedType(obj);
        }
    }
}