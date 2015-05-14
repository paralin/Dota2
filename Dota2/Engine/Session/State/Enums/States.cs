namespace Dota2.Engine.Session.State.Enums
{
    /// <summary>
    ///     General states of the DOTA 2 game session.
    /// </summary>
    internal enum States
    {
        HANDSHAKE_REJECTED,
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