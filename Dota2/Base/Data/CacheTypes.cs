namespace Dota2.Base.Data
{
    /// <summary>
    /// Cache types
    /// </summary>
    internal enum CSOTypes : int
    {
        /// <summary>
        /// An economy item.
        /// </summary>
        ECON_ITEM = 1,

        /// <summary>
        /// An econ item recipe.
        /// </summary>
        ITEM_RECIPE = 5,

        /// <summary>
        /// Game account client for Econ.
        /// </summary>
        ECON_GAME_ACCOUNT_CLIENT = 7,

        /// <summary>
        /// Selected item preset.
        /// </summary>
        SELECTED_ITEM_PRESET = 35,

        /// <summary>
        /// Item preset instance.
        /// </summary>
        ITEM_PRESET_INSTANCE = 36,

        /// <summary>
        /// Active drop rate bonus.
        /// </summary>
        DROP_RATE_BONUS = 38,

        /// <summary>
        /// Pass to view a league.
        /// </summary>
        LEAGUE_VIEW_PASS = 39,

        /// <summary>
        /// Event ticket.
        /// </summary>
        EVENT_TICKET = 40,

        /// <summary>
        /// Item tournament passport.
        /// </summary>
        ITEM_TOURNAMENT_PASSPORT = 42,

        /// <summary>
        /// DOTA 2 game account client.
        /// </summary>
        GAME_ACCOUNT_CLIENT = 2002,

        /// <summary>
        /// A Dota 2 party.
        /// </summary>
        PARTY = 2003,

        /// <summary>
        /// A Dota 2 lobby.
        /// </summary>
        LOBBY = 2004,

        /// <summary>
        /// A party invite.
        /// </summary>
        PARTYINVITE = 2006,

        /// <summary>
        /// Game hero favorites.
        /// </summary>
        GAME_HERO_FAVORITES = 2007,

        /// <summary>
        /// Ping map location state.
        /// </summary>
        MAP_LOCATION_STATE = 2008,

        /// <summary>
        /// Tournament.
        /// </summary>
        TOURNAMENT = 2009,

        /// <summary>
        /// A player challenge.
        /// </summary>
        PLAYER_CHALLENGE = 2010,

        /// <summary>
        /// A lobby invite, introduced in Reborn.
        /// </summary>
        LOBBYINVITE = 2011
    }
}