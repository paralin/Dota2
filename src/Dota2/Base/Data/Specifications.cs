using System;
using System.Collections.Generic;
using Dota2.GC.Dota.Internal;
using SteamKit2.GC.Internal;
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
            {CSOTypes.SELECTED_ITEM_PRESET, typeof(CSOSelectedItemPreset)},
            {CSOTypes.ITEM_PRESET_INSTANCE, typeof(CSOEconItemPresetInstance)},
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
