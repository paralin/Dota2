using System.Threading;
using Dota2.Engine.Data;
using Dota2.Engine.Game;
using Dota2.Engine.Session.Actuators;
using Dota2.Engine.Session.Handlers.Handshake;
using Dota2.Engine.Session.Handlers.Signon;
using Dota2.Engine.Session.State.Enums;
using Stateless;

namespace Dota2.Engine.Session
{
    /// <summary>
    ///     An instance of a DOTA 2 game session.
    /// </summary>
    public class DotaGameSession
    {
        /// <summary>
        ///     Connect details
        /// </summary>
        private readonly DOTAConnectDetails _details;

        /// <summary>
        ///     Client thread
        /// </summary>
        private Thread _clientThread;

        /// <summary>
        ///     Init a new game connect session.
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
        ///     Is the game session active.
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        ///     Launch the game session.
        /// </summary>
        public void Start()
        {
            if (Running) return;
            Running = true;
            _clientThread = new Thread(ClientThread);
            _clientThread.Start();
        }

        /// <summary>
        ///     Stop the client session (and reset it).
        /// </summary>
        public void Stop()
        {
            if (!Running) return;
            Running = false;
            _clientThread = null;
            _gameState.Reset();
            _connection = null;
            _stateMachine?.Fire(Events.DISCONNECTED);
            _stateMachine = null;
        }

        /// <summary>
        ///     Main client thread.
        /// </summary>
        private void ClientThread()
        {
            _connection = DotaGameConnection.CreateWith(_details);
            _gameState.Reset();
            _handshake = new DotaHandshake(_details, _gameState, _connection);
            _signon = new DotaSignon(_gameState, _connection, _details);
            _commandGenerator = new UserCmdGenerator(_gameState, _connection);

            _stateMachine = new StateMachine<States, Events>(States.DISCONNECTED);
            _stateMachine.Configure(States.DISCONNECTED)
                .Permit(Events.REQUEST_CONNECT, States.HANDSHAKE_REQUEST);
            //_stateMachine.Configure(States.HANDSHAKE_REQUEST)
            //    .OnEntry(_handshake.RequestHandshake)
            //    .Permit(Events.HANDSHAKE_CHALLENGE, States.HANDSHAKE_CONNECT)
        }

        #region Controllers

        /// <summary>
        ///     Game state
        /// </summary>
        private readonly DotaGameState _gameState;

        /// <summary>
        ///     Connection to the server.
        /// </summary>
        private DotaGameConnection _connection;

        /// <summary>
        ///     Handshake handler.
        /// </summary>
        private DotaHandshake _handshake;

        /// <summary>
        ///     Signon handler.
        /// </summary>
        private DotaSignon _signon;

        /// <summary>
        /// Generates and sends commands.
        /// </summary>
        private UserCmdGenerator _commandGenerator;

        #endregion

        #region State Machine

        /// <summary>
        /// Internal system state machine.
        /// </summary>
        private StateMachine<States, Events> _stateMachine;

        #endregion
    }
}