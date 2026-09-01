using System;
using Aero.Protocol;

namespace GameServer.Controllers;

/// <summary>
///     Marks a controller handler method with the universal (version agnostic) protocol message it handles.
///     The wire id is resolved against the configured protocol version at dispatch table build time.
/// </summary>
public class MessageIDAttribute : Attribute
{
    public MessageIDAttribute(GssMessage protocolId)
    {
        ProtocolId = protocolId;
    }

    public MessageIDAttribute(GssCharacterCommand protocolId)
    {
        ProtocolId = protocolId;
    }

    public MessageIDAttribute(GssVehicleCommand protocolId)
    {
        ProtocolId = protocolId;
    }

    public MessageIDAttribute(GssTurretCommand protocolId)
    {
        ProtocolId = protocolId;
    }

    /// <summary>
    ///     A member of one of the Aero.Protocol message enums (GssMessage, GssCharacterCommand, ...)
    /// </summary>
    public Enum ProtocolId { get; }
}
