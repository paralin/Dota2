using Dota2.GameClient.Engine.Session.State.Enums;
using SteamKit2;

namespace Dota2.GameClient.Engine
{
    public partial class DotaGameClient
    {
        /// <summary>
        /// Client status callback
        /// </summary>
        public sealed class SessionStateTransition : CallbackMsg
        {
            /// <summary>
            /// The previous state
            /// </summary>
            public readonly States OldStatus;

            /// <summary>
            /// The new state
            /// </summary>
            public readonly States NewStatus;

            internal SessionStateTransition(States state, States newstate)
            {
                this.OldStatus = state;
                this.NewStatus = newstate;
            }
        }

        /// <summary>
        /// Connection handshake to the server rejected with a reason
        /// </summary>
        public sealed class HandshakeRejected : CallbackMsg
        {
            /// <summary>
            /// Reason for the rejection as given by the dota server
            /// </summary>
            public readonly string reason;

            internal HandshakeRejected(string reason)
            {
                this.reason = reason;
            }
        }

        /// <summary>
        /// A debug output message from the client
        /// </summary>
        public sealed class LogMessage : CallbackMsg
        {
            /// <summary>
            /// The log message string
            /// </summary>
            public readonly string message;

            internal LogMessage(string msg)
            {
                this.message = msg;
            }
        }
    }
}
