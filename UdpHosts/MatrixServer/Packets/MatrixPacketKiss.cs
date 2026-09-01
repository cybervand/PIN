using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using Shared.Udp;

namespace MatrixServer.Packets;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct MatrixPacketKiss
{
    public readonly uint SocketID;
    private fixed byte _type[4];

    public string Type
    {
        get
        {
            fixed (byte* t = _type)
            {
                return Deserializer.ReadFixedString(t, 4);
            }
        }
        set
        {
            fixed (byte* t = _type)
            {
                Serializer.WriteFixed(t, Encoding.ASCII.GetBytes(value[..4]));
            }
        }
    }

    private readonly uint _recievedSocketID;

    public readonly uint ReceivedSocketID =>
        BinaryPrimitives.ReverseEndianness(_recievedSocketID);

    private readonly ushort _streamingProtocolVersion;

    public readonly ushort StreamingProtocolVersion =>
        BinaryPrimitives.ReverseEndianness(_streamingProtocolVersion);
}