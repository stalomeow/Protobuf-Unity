namespace Google.Protobuf
{
    public interface ICmdMessage : IMessage
    {
        ushort CmdId { get; }

        string CmdName { get; }
    }
}
