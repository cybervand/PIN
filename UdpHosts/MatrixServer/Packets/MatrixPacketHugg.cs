using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using Shared.Udp;

namespace MatrixServer.Packets;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct MatrixPacketHugg
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

    private ushort _sequenceStart;
    private ushort _gameServerPort;

    public ushort SequenceStart
    {
        readonly get => BinaryPrimitives.ReverseEndianness(_sequenceStart);
        set => _sequenceStart = BinaryPrimitives.ReverseEndianness(value);
    }

    public ushort GameServerPort
    {
        readonly get => BinaryPrimitives.ReverseEndianness(_gameServerPort);
        set => _gameServerPort = BinaryPrimitives.ReverseEndianness(value);
    }

    public MatrixPacketHugg(ushort seqStart, ushort port)
    {
        SocketID = 0;
        SequenceStart = seqStart;
        GameServerPort = port;
        Type = "HUGG";
    }
}