using Dota2.Engine.Session.State.Enums;

namespace Dota2.Engine.Session.State.Interfaces
{
    /// <summary>
    ///     Parses binary messages and returns possible state machine events.
    /// </summary>
    internal interface MessageHandler
    {
        Events? Handle(byte[] message);
        Events? Handle(DotaGameConnection.Message message);
    }
}