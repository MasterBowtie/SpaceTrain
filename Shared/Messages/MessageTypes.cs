
namespace Shared.Messages
{
    public enum Type : UInt16
    {
        ConnectAck,     // Server to client
        NewEntity,      // Server to client
        UpdateEntity,   // Server to client
        RemoveEntity,   // Server to client
        //SendScore,
        Join,           // Client to server
        Leave,          // Client to server
        Input,          // Client to server
        Disconnect      // Client to server
        //GetScore
    }
}
