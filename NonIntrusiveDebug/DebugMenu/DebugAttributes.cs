using System;
using DebugMenu;
using System.Reflection;
using System.Collections.Generic;
using Godot;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class OnScreenDebug : Attribute
{
    private static Dictionary<string, Color> ColorLookupDictionary = new Dictionary<string, Color>();
    public readonly string DebugCategory;
    public readonly string Name;
    public readonly Color Color;

    public static void Init()
    {
        foreach (PropertyInfo info in typeof(Colors).GetProperties())
        {
            ColorLookupDictionary.Add(info.Name.ToLower(), (Color)info.GetValue(null, null));
        }
    }

    public OnScreenDebug(string debugCategory, string name, string color = "White")
    {
        this.DebugCategory = debugCategory;
        this.Name = name;
        this.Color = GetColor(color);
    }

    private Color GetColor(string color)
    {
        string colorLower = color.ToLower();
        if (ColorLookupDictionary.Count == 0)
        {
            Init();
        }
        if (ColorLookupDictionary.ContainsKey(colorLower))
        {
            return ColorLookupDictionary[colorLower];
        }

        return Colors.White;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class DebugAction : Attribute
{
    public readonly object[] Parameters;
    public readonly string Name;

    public DebugAction(string name, params object[] parameters)
    {
        this.Name = name;
        this.Parameters = parameters;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class DebugVisibilityGroups : Attribute
{
    public readonly string[] Groups;

    public DebugVisibilityGroups(params string[] groupNames)
    {
        this.Groups = groupNames;
    }
}