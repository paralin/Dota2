using Dota2.GameClient.Engine.Session.State.Enums;

namespace Dota2.GameClient.Engine.Session.Handlers
{
    /// <summary>
    /// A handler that can process messages
    /// </summary>
    internal interface IHandler
    {
        Events? Handle(byte[] message);
        Events? Handle(DotaGameConnection.Message message);
    }
}
