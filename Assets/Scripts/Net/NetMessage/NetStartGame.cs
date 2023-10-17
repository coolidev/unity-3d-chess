using Unity.Networking.Transport;

public class NetStartGame : NetMessage
{
    public int AssignedTeam { set; get; }

    public NetStartGame()
    {
        Code = OpCode.START_GAME;
    }
    public NetStartGame(DataStreamReader reader)
    {
        Code = OpCode.WELCOME;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
    }
    public override void Deserialize(DataStreamReader reader)
    {
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_START_GAME?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_START_GAME?.Invoke(this, cnn);
    }
}
