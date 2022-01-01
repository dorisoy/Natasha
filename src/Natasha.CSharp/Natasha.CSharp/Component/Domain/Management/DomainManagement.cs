﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using static System.Runtime.Loader.AssemblyLoadContext;


public class DomainManagement
{

    public static readonly ConcurrentDictionary<string, WeakReference> Cache;

    static DomainManagement()
    {
        Cache = new ConcurrentDictionary<string, WeakReference>();
    }


    public static NatashaDomain Random
    {
        get { return Create("N" + Guid.NewGuid().ToString("N")); }
    }


    public static NatashaDomain Create(string key)
    {
        if (Cache.ContainsKey(key))
        {
            return (NatashaDomain)(Cache[key].Target!);
        }
        else
        {
            Clear();
            var domain = new NatashaDomain(key);
            Add(key, domain);
            return domain;
        }
    }


    public static void Clear()
    {
        foreach (var item in Cache)
        {
            if (!item.Value.IsAlive)
            {
                Cache!.Remove(item.Key);
            }
        }
    }



    public static ContextualReflectionScope Lock(string key)
    {
        if (Cache.ContainsKey(key))
        {
            return ((NatashaDomain)(Cache[key].Target)!).EnterContextualReflection();
        }
        return Default.EnterContextualReflection();
    }
    public static ContextualReflectionScope Lock(NatashaDomain domain)
    {
        return domain.EnterContextualReflection();
    }
    public static ContextualReflectionScope CreateAndLock(string key)
    {
        return Lock(Create(key));
    }
    public static NatashaDomain CurrentDomain
    {
        get
        {
            return CurrentContextualReflectionContext == default ?
                NatashaDomain.DefaultDomain :
                (NatashaDomain)CurrentContextualReflectionContext;
        }
    }



    public static void Add(string key, NatashaDomain domain)
    {
        if (Cache.ContainsKey(key))
        {
            if (!Cache[key].IsAlive)
            {
                Cache[key] = new WeakReference(domain);
            }
        }
        else
        {
            Cache[key] = new WeakReference(domain, trackResurrection: true);
        }
    }


    public static WeakReference Remove(string key)
    {
        if (Cache.ContainsKey(key))
        {
            var result = Cache!.Remove(key);
            if (result != default)
            {
                ((NatashaDomain)(result.Target!)).Dispose();
            }
            return result!;
        }

        throw new System.Exception($"Can't find key : {key}!");
    }


    public static bool IsDeleted(string key)
    {
        if (Cache.ContainsKey(key))
        {
            return !Cache[key].IsAlive;
        }
        return true;
    }


    public static NatashaDomain? Get(string key)
    {
        if (Cache.ContainsKey(key))
        {
            return (NatashaDomain)Cache[key].Target!;
        }
        return null;
    }


    //public static int Count(string key)
    //{
    //    return ((NatashaDomain)(Cache[key].Target!)).Count;
    //}
}