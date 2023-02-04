using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Pet.Utils;

internal static class GoUtils
{
    public static T GetFromResources<T>(Func<T, bool> which) where T : UObject
    {
        return Resources.FindObjectsOfTypeAll<T>().First(t => which(t));
    }

    public static GameObject GetGameObjectFromResources(string name)
    {
        return GetFromResources<GameObject>(go => go.name == name);
    }

    public static PlayMakerFSM GetFsmFromResources(string goName, string fsmName)
    {
        return GetFromResources<PlayMakerFSM>(fsm => fsm.name == goName && fsm.FsmName == fsmName);
    }

    public static T AddComponent<T>(this GameObject go, T component) where T : Component
    {
        T c = go.AddComponent<T>();

        FieldInfo[] fields = typeof(T).GetFields(BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod | BindingFlags.CreateInstance | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty | BindingFlags.ExactBinding | BindingFlags.SuppressChangeType | BindingFlags.OptionalParamBinding | BindingFlags.IgnoreReturn);
        foreach (FieldInfo fieldInfo in fields)
        {
            fieldInfo.SetValue(c, fieldInfo.GetValue(component));
        }

        return c;
    }

    public static T GetComponent<T>(this GameObject go, Func<T, bool> which) where T : Component
    {
        return go.GetComponents<T>().First(c => which == null || which(c));
    }

    public static IEnumerable<T> GetComponents<T>(this GameObject go, Func<T, bool> which) where T : Component
    {
        return go.GetComponents<T>().Where(c => which == null || which(c));
    }

    public static void RemoveComponent<T>(this GameObject go, Func<T, bool> which = null) where T : Component
    {
        var c = go.GetComponent<T>(which);
        UObject.DestroyImmediate(c);
    }

    public static void RemoveComponents<T>(this GameObject go, Func<T, bool> which = null) where T : Component
    {
        foreach (var c in go.GetComponents<T>(which))
        {
            UObject.DestroyImmediate(c);
        }
    }
}
