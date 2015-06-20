using Dota2.Engine.Game.Data;

namespace Dota2.Engine.Control
{
    /// <summary>
    /// Internally a commander that can submit orders.
    /// </summary>
    public interface IDotaGameCommander
    {
        /// <summary>
        /// Submit an order
        /// </summary>
        /// <param name="order"></param>
        void Submit(Order order);

        /// <summary>
        /// Submit a console command.
        /// </summary>
        /// <param name="consoleCommand">Console command</param>
        void Submit(string consoleCommand);
    }
}
