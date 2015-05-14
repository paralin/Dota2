using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Dota2.Engine.Data;
using Dota2.Engine.Game;

namespace Dota2.Engine.Session
{
    /// <summary>
    /// An instance of a DOTA 2 game session.
    /// </summary>
    internal class DotaGameSession
    {
        /// <summary>
        /// Connect details
        /// </summary>
        private DOTAConnectDetails _details;

        /// <summary>
        /// Client thread
        /// </summary>
        private Thread _clientThread;

        #region Controllers

        /// <summary>
        /// Game state
        /// </summary>
        private DotaGameState _gameState;

        /// <summary>
        /// Connection to the server.
        /// </summary>
        private DotaGameConnection _connection;

        #endregion

        /// <summary>
        /// Is the game session active.
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// Init a new game connect session.
        /// </summary>
        /// <param name="deets"></param>
        internal DotaGameSession(DOTAConnectDetails deets)
        {
            _details = deets;
            Running = false;
            _gameState = new DotaGameState(deets);
            _connection = null;
        }

        /// <summary>
        /// Launch the game session.
        /// </summary>
        public void Start()
        {
            if (Running) return;
            Running = true;
            _clientThread = new Thread(ClientThread);
            _clientThread.Start();
        }

        /// <summary>
        /// Stop the client session (and reset it).
        /// </summary>
        public void Stop()
        {
            if (!Running) return;
            Running = false;
            _clientThread = null;
            _gameState.Reset();
            _connection = null;
        }

        /// <summary>
        /// Main client thread.
        /// </summary>
        private void ClientThread()
        {
            _connection = DotaGameConnection.CreateWith(_details);
        }
    }
}
