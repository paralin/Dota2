/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System.Collections.Generic;
using Dota2.GC.Dota.Internal;
using Dota2.GC.Internal;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.Internal;

namespace Dota2.GC
{
    public partial class DotaGCHandler
    {
        /// <summary>
        ///     An unhandled cache unsubscribe
        /// </summary>
        public sealed class CacheUnsubscribed : CallbackMsg
        {
            /// <summary>
            /// Result.
            /// </summary>
            public CMsgSOCacheUnsubscribed result;

            internal CacheUnsubscribed(CMsgSOCacheUnsubscribed msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     Chat message received from a channel
        /// </summary>
        public sealed class ChatMessage : CallbackMsg
        {
            /// <summary>
            /// The chat message payload.
            /// </summary>
            public CMsgDOTAChatMessage result;

            internal ChatMessage(CMsgDOTAChatMessage msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     Connection status
        /// </summary>
        public sealed class ConnectionStatus : CallbackMsg
        {
            /// <summary>
            /// The connection status payload.
            /// </summary>
            public CMsgConnectionStatus result;

            internal ConnectionStatus(CMsgConnectionStatus msg)
            {
                result = msg;
            }
        }

        /// <summary>
        /// Called when the GC welcomes the client.
        /// </summary>
        public sealed class GCWelcomeCallback : CallbackMsg
        {
            /// <summary>
            /// The current DOTA 2 version.
            /// </summary>
            public uint Version;

            internal GCWelcomeCallback(CMsgClientWelcome msg)
            {
                Version = msg.version;
            }
        }

        /// <summary>
        ///     Handle invitation created
        /// </summary>
        public sealed class InvitationCreated : CallbackMsg
        {
            /// <summary>
            /// Invitation payload.
            /// </summary>
            public CMsgInvitationCreated invitation;

            internal InvitationCreated(CMsgInvitationCreated msg)
            {
                invitation = msg;
            }
        }

        /// <summary>
        ///     Reponse when trying to join a chat chanel
        /// </summary>
        public sealed class JoinChatChannelResponse : CallbackMsg
        {
            /// <summary>
            /// The join chat channel response payload.
            /// </summary>
            public CMsgDOTAJoinChatChannelResponse result;

            internal JoinChatChannelResponse(CMsgDOTAJoinChatChannelResponse msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     Reponse when trying to list chat channels
        /// </summary>
        public sealed class ChatChannelListResponse : CallbackMsg
        {
            /// <summary>
            /// The channel list payload.
            /// </summary>
            public CMsgDOTARequestChatChannelListResponse result;

            internal ChatChannelListResponse(CMsgDOTARequestChatChannelListResponse msg)
            {
                result = msg;
            }
        }


        /// <summary>
        /// League live game update.
        /// </summary>
        public sealed class LiveLeagueGameUpdate : CallbackMsg
        {
            /// <summary>
            /// Live game update payload.
            /// </summary>
            public CMsgDOTALiveLeagueGameUpdate result;

            internal LiveLeagueGameUpdate(CMsgDOTALiveLeagueGameUpdate msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     Match result response.
        /// </summary>
        public sealed class MatchResultResponse : CallbackMsg
        {
            /// <summary>
            /// Match details response payload.
            /// </summary>
            public CMsgGCMatchDetailsResponse result;

            internal MatchResultResponse(CMsgGCMatchDetailsResponse msg)
            {
                result = msg;
            }
        }

        /// <summary>
        /// Auth list ack.
        /// </summary>
        public sealed class AuthListAck : CallbackMsg
        {
            /// <summary>
            /// Authorization list ack payload.
            /// </summary>
            public CMsgClientAuthListAck authAck;

            internal AuthListAck(CMsgClientAuthListAck ack)
            {
                authAck = ack;
            }
        }

        /// <summary>
        /// Begin session response.
        /// </summary>
        public sealed class BeginSessionResponse : CallbackMsg
        {
            /// <summary>
            /// Begin session response payload.
            /// </summary>
            public MsgClientOGSBeginSessionResponse response;

            internal BeginSessionResponse(MsgClientOGSBeginSessionResponse resp)
            {
                this.response = resp;
            }
        }

        /// <summary>
        ///     Pro team list response
        /// </summary>
        public sealed class ProTeamListResponse : CallbackMsg
        {
            /// <summary>
            /// Pro team list payload.
            /// </summary>
            public CMsgDOTAProTeamListResponse result;

            internal ProTeamListResponse(CMsgDOTAProTeamListResponse msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     A user joined our chat channel
        /// </summary>
        public sealed class OtherJoinedChannel : CallbackMsg
        {
            /// <summary>
            /// Someone else joined chat channel payload.
            /// </summary>
            public CMsgDOTAOtherJoinedChatChannel result;

            internal OtherJoinedChannel(CMsgDOTAOtherJoinedChatChannel msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     A user left out chat chanel
        /// </summary>
        public sealed class OtherLeftChannel : CallbackMsg
        {
            /// <summary>
            /// Someone else left chat channel payload.
            /// </summary>
            public CMsgDOTAOtherLeftChatChannel result;

            internal OtherLeftChannel(CMsgDOTAOtherLeftChatChannel msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     When the party invite is cleared this is sent out.
        /// </summary>
        public sealed class PartyInviteLeave : CallbackMsg
        {
            /// <summary>
            /// Cache unsubscribed payload.
            /// </summary>
            public CMsgSOCacheUnsubscribed result;

            internal PartyInviteLeave(CMsgSOCacheUnsubscribed msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     When the lobby invite is cleared this is sent ou
        /// </summary>
        public sealed class LobbyInviteLeave : CallbackMsg
        {
            /// <summary>
            /// Cache unsubscribed payload.
            /// </summary>
            public CMsgSOCacheUnsubscribed result;

            internal LobbyInviteLeave(CMsgSOCacheUnsubscribed msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     Party invite was updated/snapshotted
        /// </summary>
        public sealed class PartyInviteSnapshot : CallbackMsg
        {
            /// <summary>
            /// The current invite.
            /// </summary>
            public CSODOTAPartyInvite invite;

            /// <summary>
            /// The old invite, possibly null.
            /// </summary>
            public CSODOTAPartyInvite oldInvite;

            internal PartyInviteSnapshot(CSODOTAPartyInvite msg, CSODOTAPartyInvite oldLob)
            {
                invite = msg;
                oldInvite = oldLob;
            }
        }

        /// <summary>
        ///     Lobby invite was updated/snapshotted
        /// </summary>
        public sealed class LobbyInviteSnapshot : CallbackMsg
        {
            /// <summary>
            /// The current lobby invite.
            /// </summary>
            public CSODOTALobbyInvite invite;

            /// <summary>
            /// The old lobby invite, possibly null.
            /// </summary>
            public CSODOTALobbyInvite oldInvite;

            internal LobbyInviteSnapshot(CSODOTALobbyInvite msg, CSODOTALobbyInvite oldLob)
            {
                invite = msg;
                oldInvite = oldLob;
            }
        }

        /// <summary>
        ///     When leaving a party this is sent out
        /// </summary>
        public sealed class PartyLeave : CallbackMsg
        {
            /// <summary>
            /// Cache unsubscribed payload.
            /// </summary>
            public CMsgSOCacheUnsubscribed result;

            internal PartyLeave(CMsgSOCacheUnsubscribed msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     Party was updated/snapshotted.
        /// </summary>
        public sealed class PartySnapshot : CallbackMsg
        {
            /// <summary>
            /// The current party.
            /// </summary>
            public CSODOTAParty party;

            /// <summary>
            /// The old party, possibly null.
            /// </summary>
            public CSODOTAParty oldParty;

            internal PartySnapshot(CSODOTAParty msg, CSODOTAParty oldLob)
            {
                party = msg;
                oldParty = oldLob;
            }
        }

        /// <summary>
        ///     Ping request from GC
        /// </summary>
        public sealed class PingRequest : CallbackMsg
        {
            /// <summary>
            /// Ping response payload.
            /// </summary>
            public CMsgGCClientPing request;

            internal PingRequest(CMsgGCClientPing msg)
            {
                request = msg;
            }
        }

        /// <summary>
        ///     We receive a popup. (e.g. Kicked from lobby)
        /// </summary>
        public sealed class Popup : CallbackMsg
        {
            /// <summary>
            /// Popup message payload.
            /// </summary>
            public CMsgDOTAPopup result;

            internal Popup(CMsgDOTAPopup msg)
            {
                result = msg;
            }
        }

        /// <summary>
        /// When we receive a practice lobby join response.
        /// </summary>
        public sealed class PracticeLobbyJoinResponse : CallbackMsg
        {
            /// <summary>
            /// Practice join response payload.
            /// </summary>
            public CMsgPracticeLobbyJoinResponse result;

            internal PracticeLobbyJoinResponse(CMsgPracticeLobbyJoinResponse msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     When leaving a practice lobby this is sent out
        /// </summary>
        public sealed class PracticeLobbyLeave : CallbackMsg
        {
            /// <summary>
            /// Cache unsubscribed payload.
            /// </summary>
            public CMsgSOCacheUnsubscribed result;

            internal PracticeLobbyLeave(CMsgSOCacheUnsubscribed msg)
            {
                result = msg;
            }
        }

        /// <summary>
        /// Response to a request for practice lobby list.
        /// </summary>
        public sealed class PracticeLobbyListResponse : CallbackMsg
        {
            /// <summary>
            /// Lobby list response payload.
            /// </summary>
            public CMsgPracticeLobbyListResponse result;

            internal PracticeLobbyListResponse(CMsgPracticeLobbyListResponse msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     Lobby was updated
        /// </summary>
        public sealed class PracticeLobbySnapshot : CallbackMsg
        {
            /// <summary>
            /// The current lobby.
            /// </summary>
            public CSODOTALobby lobby;

            /// <summary>
            /// The old lobby, possibly null.
            /// </summary>
            public CSODOTALobby oldLobby;

            internal PracticeLobbySnapshot(CSODOTALobby msg, CSODOTALobby oldLob)
            {
                lobby = msg;
                oldLobby = oldLob;
            }
        }

        /// <summary>
        ///     League view passes list was populated.
        /// </summary>
        public sealed class LeagueViewPassesSnapshot : CallbackMsg
        {
            /// <summary>
            /// All passes received.
            /// </summary>
            public IEnumerable<CSOEconItemLeagueViewPass> passes; 

            internal LeagueViewPassesSnapshot(IEnumerable<CSOEconItemLeagueViewPass> passes)
            {
                this.passes = passes;
            }
        }

        /// <summary>
        ///     Game account client was updated / received.
        /// </summary>
        public sealed class GameAccountClientSnapshot : CallbackMsg
        {
            public CSOEconGameAccountClient client;

            internal GameAccountClientSnapshot(CSOEconGameAccountClient cli)
            {
                client = cli;
            }
        }

        /// <summary>
        ///     When receiving a steam component of the party invite
        /// </summary>
        public sealed class SteamPartyInvite : CallbackMsg
        {
            /// <summary>
            /// The invitation payload.
            /// </summary>
            public CMsgClientUDSInviteToGame result;

            internal SteamPartyInvite(CMsgClientUDSInviteToGame msg)
            {
                result = msg;
            }
        }

        /// <summary>
        /// Called when a DOTA callback is not handled.
        /// </summary>
        public sealed class UnhandledDotaGCCallback : CallbackMsg
        {
            /// <summary>
            /// The unhandled message.
            /// </summary>
            public IPacketGCMsg Message;

            internal UnhandledDotaGCCallback(IPacketGCMsg msg)
            {
                Message = msg;
            }
        }

        /// <summary>
        /// The GC has supplied us with some player information
        /// </summary>
        public sealed class PlayerInfo : CallbackMsg
        {
            /// <summary>
            /// Player info payload.
            /// </summary>
            public CMsgGCPlayerInfo.PlayerInfo[] player_infos;

            internal PlayerInfo(List<CMsgGCPlayerInfo.PlayerInfo> msg)
            {
                player_infos = msg.ToArray();
            }
        }

        /// <summary>
        /// The GC has supplied us with some player information
        /// </summary>
        public sealed class FantasyLeagueInfo : CallbackMsg
        {
            /// <summary>
            /// The fantasy league info payload.
            /// </summary>
            public CMsgDOTAFantasyLeagueInfo info;

            internal FantasyLeagueInfo(CMsgDOTAFantasyLeagueInfo msg)
            {
                info = msg;
            }
        }

        /// <summary>
        ///     Profile response to a RequestProfile call
        /// </summary>
        public sealed class ProfileResponse : CallbackMsg
        {
            /// <summary>
            /// The profile response payload.
            /// </summary>
            public CMsgDOTAProfileResponse result;

            internal ProfileResponse(CMsgDOTAProfileResponse msg)
            {
                result = msg;
            }
        }


        /// <summary>
        ///     Response to setting someones guild role
        /// </summary>
        public sealed class GuildSetRoleResponse : CallbackMsg
        {
            /// <summary>
            /// The account set role payload.
            /// </summary>
            public CMsgDOTAGuildSetAccountRoleResponse result;

            internal GuildSetRoleResponse(CMsgDOTAGuildSetAccountRoleResponse msg)
            {
                result = msg;
            }
        }


        /// <summary>
        ///     Response to inviting someone to a guild
        /// </summary>
        public sealed class GuildInviteResponse : CallbackMsg
        {
            /// <summary>
            /// The invite account response payload.
            /// </summary>
            public CMsgDOTAGuildInviteAccountResponse result;

            internal GuildInviteResponse(CMsgDOTAGuildInviteAccountResponse msg)
            {
                result = msg;
            }
        }


        /// <summary>
        ///     Response to cancelling a guild invite
        /// </summary>
        public sealed class GuildCancelInviteResponse : CallbackMsg
        {
            /// <summary>
            /// The result payload.
            /// </summary>
            public CMsgDOTAGuildCancelInviteResponse result;

            internal GuildCancelInviteResponse(CMsgDOTAGuildCancelInviteResponse msg)
            {
                result = msg;
            }
        }

        /// <summary>
        ///     Response to a RequestGuildData call
        ///     Called once per guild after a RequestGuildData call was made
        /// </summary>
        public sealed class GuildDataResponse : CallbackMsg
        {
            /// <summary>
            /// The result payload.
            /// </summary>
            public CMsgDOTAGuildSDO result;

            internal GuildDataResponse(CMsgDOTAGuildSDO msg)
            {
                result = msg;
            }
        }
        
        /// <summary>
        ///     Called when we've just been kicked from a party.
        /// </summary>
        public sealed class KickedFromParty : CallbackMsg
        {
            /// <summary>
            /// The in-game popup notification.
            /// </summary>
            public CMsgDOTAPopup popup;

            internal KickedFromParty(CMsgDOTAPopup up)
            {
                popup = up;
            }
        }

        /// <summary>
        ///     Called when we've just been kicked from a lobby.
        /// </summary>
        public sealed class KickedFromLobby : CallbackMsg
        {
            /// <summary>
            /// The in-game popup notification.
            /// </summary>
            public CMsgDOTAPopup popup;

            internal KickedFromLobby(CMsgDOTAPopup up)
            {
                popup = up;
            }
        }

        /// <summary>
        ///     Called when we've just been kicked from a team.
        /// </summary>
        public sealed class KickedFromTeam : CallbackMsg
        {
            /// <summary>
            /// The in-game popup notification.
            /// </summary>
            public CMsgDOTAPopup popup;

            internal KickedFromTeam(CMsgDOTAPopup up)
            {
                popup = up;
            }
        }
    }
}