namespace MatrixServer;

public enum AbortCode : byte
{
    NoError,
    ProtocolError,
    ProtocolVersionMismatch,
    RemoteCreateSocketFailed,
    HostShutdown,
    ConnectionRefusedHostIsFull,
}