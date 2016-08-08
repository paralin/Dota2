using System;
using System.Collections.Generic;
using SteamKit2.GC.Dota.Internal;
using SteamKit2.GC.Internal;
using CMsgDOTATournament = Dota2.GC.Dota.Internal.CMsgDOTATournament;
using CSODOTAGameAccountClient = Dota2.GC.Dota.Internal.CSODOTAGameAccountClient;
using CSODOTAGameHeroFavorites = Dota2.GC.Dota.Internal.CSODOTAGameHeroFavorites;
using CSODOTALobby = Dota2.GC.Dota.Internal.CSODOTALobby;
using CSODOTALobbyInvite = Dota2.GC.Dota.Internal.CSODOTALobbyInvite;
using CSODOTAMapLocationState = Dota2.GC.Dota.Internal.CSODOTAMapLocationState;
using CSODOTAParty = Dota2.GC.Dota.Internal.CSODOTAParty;
using CSODOTAPartyInvite = Dota2.GC.Dota.Internal.CSODOTAPartyInvite;
using CSODOTAPlayerChallenge = Dota2.GC.Dota.Internal.CSODOTAPlayerChallenge;
using CSOEconGameAccountClient = Dota2.GC.Internal.CSOEconGameAccountClient;
using CSOEconItem = Dota2.GC.Internal.CSOEconItem;
using CSOEconItemDropRateBonus = Dota2.GC.Internal.CSOEconItemDropRateBonus;
using CSOEconItemEventTicket = Dota2.GC.Internal.CSOEconItemEventTicket;
using CSOEconItemLeagueViewPass = Dota2.GC.Internal.CSOEconItemLeagueViewPass;
using CSOEconItemTournamentPassport = Dota2.GC.Internal.CSOEconItemTournamentPassport;
using CSOItemRecipe = Dota2.GC.Internal.CSOItemRecipe;

namespace Dota2.Base.Data
{
    /// <summary>
    /// Helper to associate cache types with classes.
    /// </summary>
    static class Dota2SOTypes
    {
        /// <summary>
        /// Cache type associations.
        /// </summary>
        public static Dictionary<CSOTypes, Type> SOTypes = new Dictionary<CSOTypes, Type>()
        {
            {CSOTypes.ECON_ITEM, typeof(CSOEconItem)},
            {CSOTypes.ITEM_RECIPE, typeof(CSOItemRecipe)},
            {CSOTypes.ECON_GAME_ACCOUNT_CLIENT, typeof(CSOEconGameAccountClient)},
            {CSOTypes.DROP_RATE_BONUS, typeof(CSOEconItemDropRateBonus)},
            {CSOTypes.LEAGUE_VIEW_PASS, typeof(CSOEconItemLeagueViewPass)},
            {CSOTypes.EVENT_TICKET, typeof(CSOEconItemEventTicket)},
            {CSOTypes.ITEM_TOURNAMENT_PASSPORT, typeof(CSOEconItemTournamentPassport)},
            {CSOTypes.GAME_ACCOUNT_CLIENT, typeof(CSODOTAGameAccountClient)},
            {CSOTypes.PARTY, typeof(CSODOTAParty)},
            {CSOTypes.LOBBY, typeof(CSODOTALobby)},
            {CSOTypes.PARTYINVITE, typeof(CSODOTAPartyInvite)},
            {CSOTypes.GAME_HERO_FAVORITES, typeof(CSODOTAGameHeroFavorites)},
            {CSOTypes.MAP_LOCATION_STATE, typeof(CSODOTAMapLocationState)},
            {CSOTypes.TOURNAMENT, typeof(CMsgDOTATournament)},
            {CSOTypes.PLAYER_CHALLENGE, typeof(CSODOTAPlayerChallenge)},
            {CSOTypes.LOBBYINVITE, typeof(CSODOTALobbyInvite)},
        };
    }
}
