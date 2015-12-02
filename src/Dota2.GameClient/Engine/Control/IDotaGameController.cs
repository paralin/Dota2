using Dota2.GameClient.Engine.Game;

namespace Dota2.GameClient.Engine.Control
{
    /// <summary>
    /// Capable of controlling a DOTA 2 game client
    /// </summary>
    public interface IDotaGameController
    {
        /// <summary>
        /// Initialize the controller as the client begins to connect.
        /// </summary>
        /// <param name="id">Steam ID</param>
        /// <param name="state">Emulated DOTA game client state</param>
        /// <param name="commander">Command generator</param>
        void Initialize(ulong id, DotaGameState state, IDotaGameCommander commander);

        /// <summary>
        /// Called every tick. Must return near-instantly.
        /// </summary>
        void Tick();
    }
}
