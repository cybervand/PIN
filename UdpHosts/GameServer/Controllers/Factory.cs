using System;
using System.Collections.Concurrent;
using System.Linq;
using GameServer.Extensions;

namespace GameServer.Controllers;

public static class Factory
{
    private static ConcurrentDictionary<(int Ns, int ViewOrdinal), Base> _controllers;

    public static void Init()
    {
        _controllers = new ConcurrentDictionary<(int Ns, int ViewOrdinal), Base>();
    }

    public static T Get<T>()
        where T : Base, new()
    {
        var attr = typeof(T).GetAttribute<TypecodeAttribute>();

        if (attr == null)
        {
            throw new ArgumentNullException(nameof(T), "Type [" + typeof(T).FullName + "] does not have a Typecode Attribute.");
        }

        return _controllers.AddOrUpdate((attr.Namespace, attr.ViewOrdinal), new T(), (_, nc) => nc) as T;
    }

    public static Base Get(int ns, int viewOrdinal)
    {
        if (_controllers.TryGetValue((ns, viewOrdinal), out var controller))
        {
            return controller;
        }

        var t = ForTypecode(ns, viewOrdinal);

        return t != null ? _controllers.AddOrUpdate((ns, viewOrdinal), Activator.CreateInstance(t) as Base, (_, nc) => nc) : null;
    }

    private static Type ForTypecode(int ns, int viewOrdinal)
    {
        var ts = ReflectionUtils.FindTypesByAttribute<TypecodeAttribute>();

        return ts.FirstOrDefault(t =>
        {
            var attr = t.GetAttribute<TypecodeAttribute>();

            return attr != null && attr.Namespace == ns && attr.ViewOrdinal == viewOrdinal;
        });
    }
}
