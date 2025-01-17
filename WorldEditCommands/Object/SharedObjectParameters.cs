using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
namespace WorldEditCommands;
public class Item
{
  public string Name;
  public int Variant;
  public Item(string value)
  {
    var values = Parse.Split(value);
    Name = Parse.String(values, 0);
    Variant = Parse.Int(values, 1, 0);
  }
}
public class SharedObjectParameters
{
  public Range<Vector3>? Scale;
  public Range<int>? Level;
  public Range<float>? Health;
  public bool isHealthPercentage = false;
  public Range<float>? Damage;
  public Range<int>? Ammo;
  public string? AmmoType;
  public bool? Baby;
  public Item? Helmet;
  public Item? LeftHand;
  public Item? RightHand;
  public Item? Chest;
  public Item? Shoulders;
  public Item? Legs;
  public Item? Utility;
  public Range<float>? Radius;
  public Range<int>? Model;
  public Dictionary<string, object> Fields = [];
  public Dictionary<string, string> DataParameters = [];

  protected virtual void ParseArgs(string[] args)
  {
    foreach (var arg in args)
    {
      var split = arg.Split(['='], 2);
      var name = split[0].ToLower();
      if (name == "baby") Baby = true;
      if (split.Length < 2) continue;
      var value = split[1].Trim();
      if (name == "par")
      {
        var kvp = Parse.Kvp(value);
        if (kvp.Key == "") throw new InvalidOperationException($"Invalid data parameter {value}.");
        DataParameters[$"<{kvp.Key}>"] = kvp.Value;
      }
      if (name == "health" || name == "durability")
      {
        if (value.EndsWith("%"))
        {
          isHealthPercentage = true;
          value = value.Substring(0, value.Length - 1);
        }
        Health = Parse.FloatRange(value, 0);
        if (isHealthPercentage)
        {
          Health.Max /= 100f;
          Health.Min /= 100f;
        }
      }
      if (name == "damage")
        Damage = Parse.FloatRange(value, 0);
      if (name == "ammo")
        Ammo = Parse.IntRange(value, 0);
      if (name == "ammotype")
        AmmoType = value;
      if (name == "stars" || name == "star")
      {
        Level = Parse.IntRange(value, 0);
        Level.Max++;
        Level.Min++;
      }
      if (name == "model")
        Model = Parse.IntRange(value, 0);
      if (name == "level" || name == "levels")
        Level = Parse.IntRange(value);
      if (name == "radius" || name == "range" || name == "circle")
        Radius = Parse.FloatRange(value);
      if (name == "sc" || name == "scale")
        Scale = Parse.ScaleRange(value);
      if (name == "helmet") Helmet = new(value);
      if (name == "left_hand") LeftHand = new(value);
      if (name == "right_hand") RightHand = new(value);
      if (name == "chest") Chest = new(value);
      if (name == "shoulders") Shoulders = new(value);
      if (name == "legs") Legs = new(value);
      if (name == "utility") Utility = new(value);
      if (name == "field" || name == "f")
      {
        var values = value.Split(',');
        if (values.Length < 3) continue;
        var prefab = FieldAutoComplete.PrefabFromCommand(string.Join(" ", args));
        var component = FieldAutoComplete.RealComponent(prefab, values[0]);
        var field = FieldAutoComplete.RealField(component, values[1], out bool zdoField);
        var fieldValue = string.Join(",", values.Skip(2));
        var type = FieldAutoComplete.GetType(component, field, zdoField);
        var key = zdoField ? field : $"{component}.{field}";
        if (type == typeof(int))
          Fields.Add(key, Parse.Int(fieldValue));
        else if (type == typeof(float))
          Fields.Add(key, Parse.Float(fieldValue));
        else if (type == typeof(string))
          Fields.Add(key, fieldValue);
        else if (type == typeof(bool))
          Fields.Add(key, fieldValue == "1" || fieldValue == "true" || fieldValue == "True" ? 1 : 0);
        else if (type == typeof(Vector3))
          Fields.Add(key, Parse.VectorXZY(values, 2));
        else if (type == typeof(Quaternion))
          Fields.Add(key, Parse.AngleYXZ(values, 2));
        else if (type == typeof(GameObject) || type == typeof(ItemDrop) || type == typeof(EffectList))
          Fields.Add(key, fieldValue);
        else if (type == typeof(ObjectHash) || type == typeof(LocationHash) || type == typeof(RoomHash))
          Fields.Add(key, fieldValue.GetStableHashCode());
        else if (type.IsEnum)
          Fields.Add(key, ToEnum(type, fieldValue));
        else if (type == typeof(void))
          throw new InvalidOperationException($"Field {key} of component {component} is not supported.");
        else
          throw new InvalidOperationException($"Unhandled type {type} for field {key}");
      }
    }
  }

  public static int ToEnum(Type type, string str) => ToEnum(type, ToList(str));
  public static int ToEnum(Type type, List<string> list)
  {
    try
    {
      int value = 0;
      foreach (var item in list)
        value += (int)Enum.Parse(type, item, true);
      return value;
    }
    catch
    {
      throw new InvalidOperationException($"Failed to parse enum {type.Name} with values {string.Join(", ", list)}");
    }
  }
  public static List<string> ToList(string str, bool removeEmpty = true) => [.. Split(str, removeEmpty)];

  public static string[] Split(string arg, bool removeEmpty = true, char split = ',') => arg.Split(split).Select(s => s.Trim()).Where(s => !removeEmpty || s != "").ToArray();
}
