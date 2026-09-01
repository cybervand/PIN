#nullable enable
using System;
using System.Collections.Concurrent;
using System.Linq;
using Aero.Gen.Attributes;
using Aero.Protocol;

namespace GameServer.Protocol;

/// <summary>
///     Resolves wire ids (typecode + message id) for Aero message types against a protocol version,
///     using the versioned routing tables from Aero.Protocol.
/// </summary>
public static class WireIds
{
    private static readonly ConcurrentDictionary<(Type Type, GssVersion Version), GssWireId> GssCache = new();
    private static readonly ConcurrentDictionary<(Type Type, MatrixVersion Version), byte> MatrixCache = new();

    /// <summary>
    ///     Wire ids of a GSS message/command/view type in the given protocol version.
    ///     For views the typecode is the view typecode and the message id is 0.
    /// </summary>
    public static bool TryGetGssWireIds(Type messageType, GssVersion version, out byte typecode, out byte messageId)
    {
        var key = (messageType, version);
        if (GssCache.TryGetValue(key, out var cached))
        {
            typecode = cached.Typecode;
            messageId = cached.MessageId;
            return cached.Valid;
        }

        var result = ResolveGss(messageType, version);
        GssCache[key] = result;

        typecode = result.Typecode;
        messageId = result.MessageId;
        return result.Valid;
    }

    /// <summary>
    ///     Wire id of a matrix message type in the given protocol version.
    /// </summary>
    public static bool TryGetMatrixWireId(Type messageType, MatrixVersion version, out byte messageId)
    {
        var key = (messageType, version);
        if (MatrixCache.TryGetValue(key, out var cached))
        {
            messageId = cached;
            return cached != 0;
        }

        var result = ResolveMatrix(messageType, version);
        MatrixCache[key] = result;

        messageId = result;
        return result != 0;
    }

    /// <summary>
    ///     Universal (namespace, view ordinal) route carried by a wire typecode in the given protocol version.
    ///     View routes resolve to their view ordinal, namespace routes to a view ordinal of -1.
    ///     Typecodes that match no known route fall back to the root namespace: the client sends
    ///     root-namespace messages on typecode 0 as well as on the unrouted shard typecode (251).
    /// </summary>
    public static void ResolveGssRoute(GssVersion version, byte typecode, out int nsIndex, out int viewOrdinal)
    {
        if (GssTables.TryFindView(version, typecode, out nsIndex, out viewOrdinal))
        {
            return;
        }

        var ns = GssTables.FindNamespace(version, typecode, GssTables.Kind.Message);
        if (ns != GssTables.Ns.Unknown)
        {
            nsIndex = ns;
            viewOrdinal = -1;
            return;
        }

        nsIndex = GssTables.Ns.Root;
        viewOrdinal = -1;
    }

    private static GssWireId ResolveGss(Type messageType, GssVersion version)
    {
        if (GetAeroMessageId(messageType) is not { Typ: AeroMessageIdAttribute.MsgType.GSS, MessageEnum: not null } attr)
        {
            return default;
        }

        var versionTo = attr.VersionTo < 0 ? GssTables.VersionCount - 1 : attr.VersionTo;
        if ((int)version < attr.VersionFrom || (int)version > versionTo)
        {
            return default;
        }

        if (!GssTables.TryGetProtocolEnumInfo(attr.MessageEnum, out var nsIndex, out var kind))
        {
            throw new InvalidOperationException($"Unknown protocol enum {attr.MessageEnum} for type {messageType.FullName}");
        }

        if (kind == GssTables.Kind.View)
        {
            var viewTypecode = GssTables.GetMessageId(version, nsIndex, kind, attr.MessageOrdinal);
            return viewTypecode != 0 ? new GssWireId(viewTypecode, 0, true) : default;
        }

        var messageId = GssTables.GetMessageId(version, nsIndex, kind, attr.MessageOrdinal);
        if (messageId == 0)
        {
            return default;
        }

        byte typecode;
        if (attr.ViewEnum is { Length: > 0 } viewEnum)
        {
            // Messages ride on the route typecode of the view/controller that carries them.
            if (!GssTables.TryGetProtocolEnumInfo(viewEnum, out var viewNs, out var viewKind)
                || viewKind != GssTables.Kind.View)
            {
                throw new InvalidOperationException($"Invalid view route {viewEnum} for type {messageType.FullName}");
            }

            typecode = GssTables.GetMessageId(version, viewNs, GssTables.Kind.View, attr.ViewOrdinal);
            if (typecode == 0)
            {
                return default;
            }
        }
        else
        {
            typecode = GssTables.GetNamespaceTypecode(version, nsIndex);
            if (typecode == 255)
            {
                return default;
            }
        }

        return new GssWireId(typecode, messageId, true);
    }

    private static byte ResolveMatrix(Type messageType, MatrixVersion version)
    {
        if (GetAeroMessageId(messageType) is not { Typ: AeroMessageIdAttribute.MsgType.Matrix, MessageEnum: not null } attr)
        {
            return 0;
        }

        var versionTo = attr.VersionTo < 0 ? MatrixTables.VersionCount - 1 : attr.VersionTo;
        if ((int)version < attr.VersionFrom || (int)version > versionTo)
        {
            return 0;
        }

        if (attr.MessageEnum != nameof(MatrixMessage))
        {
            return 0;
        }

        return MatrixTables.GetMessageId(version, (MatrixMessage)attr.MessageOrdinal);
    }

    private static AeroMessageIdAttribute? GetAeroMessageId(Type messageType)
    {
        return (AeroMessageIdAttribute?)messageType.GetCustomAttributes(typeof(AeroMessageIdAttribute), false).FirstOrDefault();
    }

    private readonly record struct GssWireId(byte Typecode, byte MessageId, bool Valid);
}
