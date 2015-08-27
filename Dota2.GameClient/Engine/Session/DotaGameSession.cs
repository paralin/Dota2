using System;
using System.Collections.Generic;
using System.Threading;
using Dota2.Engine.Control;
using Dota2.Engine.Data;
using Dota2.Engine.Game;
using Dota2.Engine.Game.Entities;
using Dota2.Engine.Session.Actuators;
using Dota2.Engine.Session.Handlers;
using Dota2.Engine.Session.Handlers.Game;
using Dota2.Engine.Session.Handlers.Handshake;
using Dota2.Engine.Session.Handlers.Signon;
using Dota2.Engine.Session.State.Enums;
using Stateless;
using SteamKit2;

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
        /// Called when closed
        /// </summary>
        internal event EventHandler Closed;

        /// <summary>
        /// Called when the session needs to fire a callback
        /// </summary>
        internal event EventHandler<CallbackEventArgs> Callback;

        /// <summary>
        /// Game controllers
        /// </summary>
        internal IDotaGameController[] Controllers;

        /// <summary>
        ///     Init a new game connect session.
        /// </summary>
        /// <param name="deets"></param>
        /// <param name="toArray"></param>
        /// <param name="poolBuilder">Pool builder</param>
        internal DotaGameSession(DOTAConnectDetails deets, IDotaGameController[] controllers, DotaEntityPool.Builder poolBuilder)
        {
            _details = deets;
            Running = false;
            _gameState = new DotaGameState(deets);
            _gameState.EntityPool = poolBuilder.Build(_gameState);
            _connection = null;
            Controllers = controllers;
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
            _connection?.Dispose();
            _connection = null;
            _commandGenerator?.Reset();
            _commandGenerator = null;
            _game = null;
            _handshake = null;
            _signon = null;
            _stateMachine?.Fire(Events.DISCONNECTED);
            _stateMachine = null;
            Closed?.Invoke(this, null);
        }

        /// <summary>
        ///     Main client thread.
        /// </summary>
        private void ClientThread()
        {
            _gameState.Reset();

            _connection = DotaGameConnection.CreateWith(_details);
            _handshake = new DotaHandshake(_details, _gameState, _connection);
            _signon = new DotaSignon(_gameState, _connection, _details);
            _game = new DotaGame(_gameState, _connection);
            _commandGenerator = new UserCmdGenerator(_gameState, _connection);

            foreach (var cont in Controllers)
                cont.Initialize(_details.SteamId, _gameState, _commandGenerator);

            long handshake_requested = 0;
            long handshake_giveup = new TimeSpan(0, 0, 0, 10).Ticks;

            // Map states in the StateMachine to handlers
            var metastates = new Dictionary<States, Metastates>() {
                { States.HANDSHAKE_REQUEST, Metastates.HANDSHAKE },
                { States.HANDSHAKE_CONNECT, Metastates.HANDSHAKE },
                { States.CONNECTED, Metastates.SIGNON },
                { States.LOADING, Metastates.SIGNON },
                { States.PRESPAWN, Metastates.SIGNON },
                { States.SPAWN, Metastates.SIGNON },
                { States.PLAY, Metastates.GAME },
            };

            var processors = new Dictionary<Metastates, IHandler>() {
                { Metastates.HANDSHAKE, _handshake },
                { Metastates.SIGNON, _signon },
                { Metastates.GAME, _game }
            };


            _stateMachine = new StateMachine<States, Events>(States.DISCONNECTED);
            //temporary shit
            _stateMachine.OnTransitioned(transition =>
            {
                if (transition.Source == transition.Destination) return;
                Callback?.Invoke(this, new CallbackEventArgs(new DotaGameClient.SessionStateTransition(transition.Source, transition.Destination)));
            });

            _stateMachine.OnUnhandledTrigger((states, events) =>
            {
                Console.WriteLine("Unhandled trigger: " + events.ToString("G"));

            });

            var disconnected = new Action(() =>
            {
                if (_connection == null) return;
                Running = true;
                Stop();
            });

            _stateMachine.Configure(States.DISCONNECTED)
                .Ignore(Events.TICK)
                .Ignore(Events.DISCONNECTED)
                .OnEntry(disconnected)
                .Permit(Events.REQUEST_CONNECT, States.HANDSHAKE_REQUEST);
            _stateMachine.Configure(States.HANDSHAKE_REJECTED)
                .Permit(Events.DISCONNECTED, States.DISCONNECTED)
                .OnEntry(() =>
                {
                    Callback?.Invoke(this, new CallbackEventArgs(new DotaGameClient.HandshakeRejected(_handshake.rejected_reason)));
                    _stateMachine.Fire(Events.DISCONNECTED);
                });
            _stateMachine.Configure(States.HANDSHAKE_REQUEST)
                .OnEntry(() => handshake_requested = DateTime.Now.Ticks)
                .OnEntry(_handshake.RequestHandshake)
                .Ignore(Events.TICK)
                .Permit(Events.HANDSHAKE_CHALLENGE, States.HANDSHAKE_CONNECT)
                .Permit(Events.REJECTED, States.HANDSHAKE_REJECTED)
                .Permit(Events.DISCONNECTED, States.DISCONNECTED);
            _stateMachine.Configure(States.HANDSHAKE_CONNECT)
                .OnEntry(_handshake.RespondHandshake)
                .Ignore(Events.TICK)
                .PermitReentry(Events.HANDSHAKE_CHALLENGE) // possibly re-enter?
                .Permit(Events.HANDSHAKE_COMPLETE, States.CONNECTED)
                .Permit(Events.REJECTED, States.HANDSHAKE_REJECTED)
                .Permit(Events.DISCONNECTED, States.DISCONNECTED);
            _stateMachine.Configure(States.CONNECTED)
                .OnEntry(_signon.EnterConnected)
                .Ignore(Events.TICK)
                .Permit(Events.LOADING_START, States.LOADING)
                .Permit(Events.DISCONNECTED, States.DISCONNECTED);
            _stateMachine.Configure(States.LOADING)
                .OnEntry(_signon.EnterNew)
                .Ignore(Events.TICK)
                .Permit(Events.CONNECTED, States.CONNECTED)
                .Permit(Events.PRESPAWN_START, States.PRESPAWN)
                .Permit(Events.DISCONNECTED, States.DISCONNECTED);
            _stateMachine.Configure(States.PRESPAWN)
                .OnEntry(_signon.EnterPrespawn)
                .Ignore(Events.TICK)
                .Permit(Events.SPAWNED, States.SPAWN)
                .Permit(Events.DISCONNECTED, States.DISCONNECTED);
            _stateMachine.Configure(States.SPAWN)
                .OnEntry(_signon.EnterSpawn)
                .Ignore(Events.TICK)
                .Permit(Events.BASELINE, States.PLAY)
                .Permit(Events.DISCONNECTED, States.DISCONNECTED);
            _stateMachine.Configure(States.PLAY)
                .OnEntryFrom(Events.BASELINE, () =>
                {
                    _game.EnterGame();
                    _commandGenerator.Reset();
                })
                .OnEntryFrom(Events.TICK, () =>
                {
                    _gameState.Update();
                    foreach(var cont in Controllers) cont.Tick();
                    _commandGenerator.Tick();
                    _gameState.Created.Clear();
                    _gameState.Deleted.Clear();
                })
                .PermitReentry(Events.TICK)
                .Permit(Events.DISCONNECTED, States.DISCONNECTED);

            _stateMachine.Fire(Events.REQUEST_CONNECT);

            long next_tick = DateTime.Now.Ticks;
            while (Running && _stateMachine.State != States.DISCONNECTED && _stateMachine.State != States.HANDSHAKE_REJECTED)
            {
                try
                {
                    if (next_tick > DateTime.Now.Ticks)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    if (_stateMachine == null) break;

                    if (_stateMachine.State == States.HANDSHAKE_REQUEST &&
                        (DateTime.Now.Ticks - handshake_requested) > handshake_giveup)
                    {
                        _stateMachine.Fire(Events.DISCONNECTED);
                        continue;
                    }

                    if (_connection.state == DotaGameConnection.State.Closed)
                    {
                        _stateMachine.Fire(Events.DISCONNECTED);
                        continue;
                    }

                    if (_connection == null) break;

                    List<byte[]> outBand = _connection.GetOutOfBand();
                    List<DotaGameConnection.Message> inBand = _connection.GetInBand();

                    foreach (byte[] message in outBand)
                    {
                        Nullable<Events> e = processors[metastates[_stateMachine.State]].Handle(message);
                        if (e.HasValue)
                        {
                            _stateMachine.Fire(e.Value);
                        }
                    }

                    foreach (DotaGameConnection.Message message in inBand)
                    {
                        Nullable<Events> e = processors[metastates[_stateMachine.State]].Handle(message);
                        if (e.HasValue)
                        {
                            _stateMachine.Fire(e.Value);
                        }
                    }

                    _stateMachine.Fire(Events.TICK);

                    if (_gameState.TickInterval > 0)
                    {
                        next_tick += (uint)(_gameState.TickInterval * 1000 * 10000 /* ticks per ms */);
                    }
                    else
                    {
                        next_tick += 50 * 1000;
                    }
                    int remain = (int)(next_tick - DateTime.Now.Ticks) / 10000;
                    if (remain > 0)
                    {
                        Thread.Sleep(1);
                    }
                    else if (remain < 0)
                    {
                        next_tick = DateTime.Now.Ticks;
                    }
                }
                catch (Exception ex)
                {
                    Callback?.Invoke(this, new CallbackEventArgs(new DotaGameClient.LogMessage("Ignored error in session loop, " + ex.Message)));
                }
            }

            if (Running) Stop();
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

        /// <summary>
        /// Game handler.
        /// </summary>
        private DotaGame _game;

        #endregion

        #region State Machine

        /// <summary>
        /// Internal system state machine.
        /// </summary>
        private StateMachine<States, Events> _stateMachine;

        /// <summary>
        /// Overall general handler states
        /// </summary>
        private enum Metastates
        {
            HANDSHAKE,
            SIGNON,
            GAME
        }

        #endregion

        internal class StringEventArgs : EventArgs
        {
            public readonly string str;

            internal StringEventArgs(string str)
            {
                this.str = str;
            }
        }

        internal class CallbackEventArgs : EventArgs
        {
            public readonly CallbackMsg msg;

            internal CallbackEventArgs(CallbackMsg msg)
            {
                this.msg = msg;
            }
        }
    }
}