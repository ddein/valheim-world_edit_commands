using ServerDevcommands;
using UnityEngine;

namespace WorldEditCommands {
  public class Item {
    public string Name;
    public int Variant;
    public Item(string value) {
      var values = Parse.Split(value);
      Name = Parse.TryString(values, 0);
      Variant = Parse.TryInt(values, 1, 0);
    }
  }
  public class SharedObjectParameters {
    public Range<Vector3> Scale = new Range<Vector3>(Vector3.one);
    public Range<int> Level = new Range<int>(1);
    public Range<float> Health = new Range<float>(0f);
    public Item Helmet = null;
    public Item LeftHand = null;
    public Item RightHand = null;
    public Item Chest = null;
    public Item Shoulders = null;
    public Item Legs = null;
    public Item Utility = null;
    public float Radius = 0f;

    public virtual bool ParseArgs(string[] args, Terminal terminal) {
      foreach (var arg in args) {
        var split = arg.Split('=');
        var name = split[0].ToLower();
        if (split.Length < 2) continue;
        var value = split[1];
        if (name == "health" || name == "durability")
          Health = Parse.TryFloatRange(value, 0);
        if (name == "stars" || name == "star") {
          Level = Parse.TryIntRange(value, 0);
          Level.Max++;
          Level.Min++;
        }
        if (name == "level" || name == "levels")
          Level = Parse.TryIntRange(value);
        if (name == "radius" || name == "range")
          Radius = Parse.TryFloat(value);
        if (name == "sc" || name == "scale")
          Scale = Parse.TryScaleRange(value);
        if (name == "helmet") Helmet = new Item(value);
        if (name == "left_hand") LeftHand = new Item(value);
        if (name == "right_hand") RightHand = new Item(value);
        if (name == "chest") Chest = new Item(value);
        if (name == "shoulders") Shoulders = new Item(value);
        if (name == "legs") Legs = new Item(value);
        if (name == "utility") Utility = new Item(value);
      }
      return true;
    }
  }
}