namespace Dota2.Engine.Session.State.Enums
{
    /// <summary>
    ///     General states of the DOTA 2 game session.
    /// </summary>
    public enum States
    {
        /// <summary>
        /// Brief state in which a callback will be fired before disconnection
        /// </summary>
        HANDSHAKE_REJECTED,

        /// <summary>
        /// Initial disconnected state and final state on Close()
        /// </summary>
        DISCONNECTED,

        HANDSHAKE_REQUEST,
        HANDSHAKE_CONNECT,

        CONNECTED,

        LOADING,
        PRESPAWN,
        SPAWN,
        PLAY
    }
}