using Dota2.GameClient.Engine.Game.Data;
using Dota2.GC.Dota.Internal;

namespace Dota2.GameClient.Engine.Game.Entities.Dota
{
    /// <summary>
    /// Game rules mapping.
    /// </summary>
    public class GameRules : MappedEntityClass
    {
        public const string TableName = "CDOTAGamerulesProxy";

        #region State

        /// <summary>
        /// Game state.
        /// </summary>
        public EnumValue<DOTA_GameState> GameState;

        /// <summary>
        /// Game match ID
        /// </summary>
        public TypedValue<ulong> MatchID;

        /// <summary>
        /// DOTA game mode.
        /// </summary>
        public EnumValue<DOTA_GameMode> GameMode;

        /// <summary>
        /// Which team paused?
        /// </summary>
        public EnumValue<DOTA_ServerTeam> PauseTeam;

        /// <summary>
        /// Who won?
        /// </summary>
        public EnumValue<DOTA_ServerTeam> GameWinner;

        #endregion
        #region Time

        /// <summary>
        /// Game time.
        /// </summary>
        public TypedValue<float> GameTime;

        /// <summary>
        /// Game time when creeps spawn.
        /// </summary>
        public TypedValue<float> GameStartTime;

        /// <summary>
        /// The time when the game ended.
        /// </summary>
        public TypedValue<float> GameEndTime;

        /// <summary>
        /// The time when pre-game started.
        /// </summary>
        public TypedValue<float> PreGameStartTime;

        /// <summary>
        /// Networked time of day, ranges from 1-65536.
        /// </summary>
        public TypedValue<ulong> NetTimeOfDay;

        #endregion
        #region Drafting

        /// <summary>
        /// The starting team for the draft.
        /// </summary>
        public EnumValue<DOTA_ServerTeam> DraftStartingTeam;

        /// <summary>
        /// Active team for the draft.
        /// </summary>
        public EnumValue<DOTA_ServerTeam> DraftActiveTeam;

        /// <summary>
        /// Hero pick state.
        /// </summary>
        public EnumValue<DOTA_HeroPickState> HeroPickState;

        /// <summary>
        /// Bonus pick time remaining.
        /// </summary>
        public TypedValue<float> ExtraTimeRemaining;

        /// <summary>
        /// Banned heros
        /// </summary>
        //todo: public RangeValue<uint> BannedHeros; 

        /// <summary>
        /// Selected heros
        /// </summary>
        //todo: public RangeValue<uint> SelectedHeros; 

        #endregion


        public GameRules(uint id, DotaGameState state) : base(id, state)
        {
            const string t = "DT_DOTAGamerules";

            GameState = bindE<DOTA_GameState>(t, "m_nGameState");
            GameTime = bind<float>(t, "m_fGameTime");
            GameStartTime = bind<float>(t, "m_flGameStartTime");
            GameEndTime = bind<float>(t, "m_flGameEndTime");
            PreGameStartTime = bind<float>(t, "m_flPreGameStartTime");
            //MatchID = bind<ulong>(t, "m_unMatchID"); broken
            GameMode = bindE<DOTA_GameMode>(t, "m_nGameState");
            PauseTeam = bindE<DOTA_ServerTeam>(t, "m_iPauseTeam");
            GameWinner = bindE<DOTA_ServerTeam>(t, "m_nGameWinner");
            NetTimeOfDay = bind<ulong>(t, "m_iNetTimeOfDay");
            DraftStartingTeam = bindE<DOTA_ServerTeam>(t, "m_iStartingTeam");
            DraftActiveTeam = bindE<DOTA_ServerTeam>(t, "m_iActiveTeam");
            HeroPickState = bindE<DOTA_HeroPickState>(t, "m_nHeroPickState");
            //ExtraTimeRemaining = bind<float>(t, "m_fExtraTimeRemaining"); broken

            //todo: BannedHeros = range<uint>("m_BannedHeroes", )
            //todo: handle m_nGGTeam and m_flGGEndsAtTime
        }

        /// <summary>
        /// Server team. Winner will be five unless game is over.
        /// </summary>
        public enum DOTA_ServerTeam
        {
            UNKNOWN = 0,
            RADIANT = 2,
            DIRE = 3,
            SPECTATOR = 5
        }

        /// <summary>
        /// Hero pick state
        /// </summary>
        public enum DOTA_HeroPickState
        {
            ALL_PICK = 1,
            SINGLE_DRAFT = 2,
            RANDOM_DRAFT = 4,
            ALL_RANDOM = 27,
            BAN_1 = 6,
            BAN_2 = 7,
            BAN_3 = 8,
            BAN_4 = 9,
            PICK_1 = 16,
            PICK_2 = 17,
            PICK_3 = 18,
            PICK_4 = 19,
            BAN_5 = 10,
            BAN_6 = 11,
            BAN_7 = 12,
            BAN_8 = 13,
            PICK_5 = 20,
            PICK_6 = 21,
            PICK_7 = 22,
            PICK_8 = 23,
            BAN_9 = 14,
            BAN_10 = 15,
            PICK_9 = 24,
            PICK_10 = 25,
            COMPLETE = 26
        }
    }
}