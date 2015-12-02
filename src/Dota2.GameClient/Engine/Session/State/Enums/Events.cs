namespace Dota2.GameClient.Engine.Session.State.Enums
{
    /// <summary>
    ///     General events of the DOTA 2 game session.
    /// </summary>
    internal enum Events
    {
        DISCONNECTED,
        REJECTED,

        REQUEST_CONNECT,
        HANDSHAKE_CHALLENGE,
        HANDSHAKE_COMPLETE,

        CONNECTED,
        LOADING_START,
        PRESPAWN_START,
        SPAWNED,

        BASELINE,
        TICK
    }
}