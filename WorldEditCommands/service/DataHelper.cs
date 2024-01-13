using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;
namespace Service;

public class DataHelper
{
  public static void Init(GameObject obj, Vector3 pos, Quaternion rot, Vector3 scale, ZPackage? data)
  {
    if (data == null && scale == Vector3.one) return;
    if (!obj.TryGetComponent<ZNetView>(out var view)) return;
    var prefab = Utils.GetPrefabName(obj).GetStableHashCode();
    ZNetView.m_initZDO = ZDOMan.instance.CreateNewZDO(pos, prefab);
    if (data != null)
      Load(data, ZNetView.m_initZDO);
    ZNetView.m_initZDO.m_rotation = rot.eulerAngles;
    ZNetView.m_initZDO.Type = view.m_type;
    ZNetView.m_initZDO.Distant = view.m_distant;
    ZNetView.m_initZDO.Persistent = view.m_persistent;
    ZNetView.m_initZDO.m_prefab = prefab;
    if (!view.m_syncInitialScale && WorldEditCommands.WorldEditCommands.IsTweaks)
    {
      ZNetView.m_initZDO.Set(Hash.HasFields, true);
      ZNetView.m_initZDO.Set("HasFieldsZNetView", true);
      ZNetView.m_initZDO.Set("ZNetView.m_syncInitialScale", true);
      view.m_syncInitialScale = true;
      Console.instance.AddString("Note: Scaling set to true.");
    }
    if (view.m_syncInitialScale)
      ZNetView.m_initZDO.Set(ZDOVars.s_scaleHash, scale);
    ZNetView.m_initZDO.DataRevision = 0;
    // This is needed to trigger the ZDO sync.
    ZNetView.m_initZDO.IncreaseDataRevision();
  }
  public static void CleanUp()
  {
    ZNetView.m_initZDO = null;
  }
  public static ZPackage? Deserialize(string data) => data == "" ? null : new(data);

  public static void Serialize(ZDO zdo, ZPackage pkg, string filter)
  {
    var id = zdo.m_uid;
    var vecs = ZDOExtraData.s_vec3.ContainsKey(id) ? ZDOExtraData.s_vec3[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var ints = ZDOExtraData.s_ints.ContainsKey(id) ? ZDOExtraData.s_ints[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var floats = ZDOExtraData.s_floats.ContainsKey(id) ? ZDOExtraData.s_floats[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var quats = ZDOExtraData.s_quats.ContainsKey(id) ? ZDOExtraData.s_quats[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var strings = ZDOExtraData.s_strings.ContainsKey(id) ? ZDOExtraData.s_strings[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var longs = ZDOExtraData.s_longs.ContainsKey(id) ? ZDOExtraData.s_longs[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    var byteArrays = ZDOExtraData.s_byteArrays.ContainsKey(id) ? ZDOExtraData.s_byteArrays[id].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : [];
    if (filter == "")
    {
      vecs.Remove(Hash.Scale);
      vecs.Remove(Hash.SpawnPoint);
      ints.Remove(Hash.Seed);
      ints.Remove(Hash.Location);
      if (strings.ContainsKey(Hash.OverrideItems))
      {
        ints.Remove(Hash.AddedDefaultItems);
        strings.Remove(Hash.Items);
      }
    }
    else if (filter != "all")
    {
      var filters = Parse.Split(filter).Select(s => s.GetStableHashCode()).ToHashSet();
      vecs = FilterZdo(vecs, filters);
      quats = FilterZdo(quats, filters);
      floats = FilterZdo(floats, filters);
      ints = FilterZdo(ints, filters);
      longs = FilterZdo(longs, filters);
      byteArrays = FilterZdo(byteArrays, filters);
    }
    var num = 0;
    if (floats.Count() > 0)
      num |= 1;
    if (vecs.Count() > 0)
      num |= 2;
    if (quats.Count() > 0)
      num |= 4;
    if (ints.Count() > 0)
      num |= 8;
    if (strings.Count() > 0)
      num |= 16;
    if (longs.Count() > 0)
      num |= 64;
    if (byteArrays.Count() > 0)
      num |= 128;
    var conn = ZDOExtraData.s_connectionsHashData.TryGetValue(id, out var c) ? c : null;
    if (conn != null && filter == "all" && conn.m_type != ZDOExtraData.ConnectionType.None && conn.m_hash != 0)
      num |= 256;

    pkg.Write(num);
    if (floats.Count() > 0)
    {
      pkg.Write((byte)floats.Count());
      foreach (var kvp in floats)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (vecs.Count() > 0)
    {
      pkg.Write((byte)vecs.Count());
      foreach (var kvp in vecs)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (quats.Count() > 0)
    {
      pkg.Write((byte)quats.Count());
      foreach (var kvp in quats)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (ints.Count() > 0)
    {
      pkg.Write((byte)ints.Count());
      foreach (var kvp in ints)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (longs.Count() > 0)
    {
      pkg.Write((byte)longs.Count());
      foreach (var kvp in longs)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (strings.Count() > 0)
    {
      pkg.Write((byte)strings.Count());
      foreach (var kvp in strings)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (byteArrays.Count() > 0)
    {
      pkg.Write((byte)byteArrays.Count());
      foreach (var kvp in byteArrays)
      {
        pkg.Write(kvp.Key);
        pkg.Write(kvp.Value);
      }
    }
    if (conn != null && (num & 256) != 0)
    {
      pkg.Write((byte)conn.m_type);
      pkg.Write(conn.m_hash);
    }
  }
  private static void Load(ZPackage pkg, ZDO zdo)
  {
    var id = zdo.m_uid;
    var num = pkg.ReadInt();
    if ((num & 1) != 0)
    {
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        ZDOExtraData.Set(id, pkg.ReadInt(), pkg.ReadSingle());
    }
    if ((num & 2) != 0)
    {
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        ZDOExtraData.Set(id, pkg.ReadInt(), pkg.ReadVector3());
    }
    if ((num & 4) != 0)
    {
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        ZDOExtraData.Set(id, pkg.ReadInt(), pkg.ReadQuaternion());
    }
    if ((num & 8) != 0)
    {
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        ZDOExtraData.Set(id, pkg.ReadInt(), pkg.ReadInt());
    }
    if ((num & 16) != 0)
    {
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        ZDOExtraData.Set(id, pkg.ReadInt(), pkg.ReadString());
    }
    if ((num & 64) != 0)
    {
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        ZDOExtraData.Set(id, pkg.ReadInt(), pkg.ReadLong());
    }
    if ((num & 128) != 0)
    {
      var count = pkg.ReadByte();
      for (var i = 0; i < count; ++i)
        ZDOExtraData.Set(id, pkg.ReadInt(), pkg.ReadByteArray());
    }
    if ((num & 256) != 0)
    {
      var type = (ZDOExtraData.ConnectionType)pkg.ReadByte();
      var hash = pkg.ReadInt();
      ZDOExtraData.SetConnectionData(id, type, hash);
    }
  }
  private static Dictionary<int, T> FilterZdo<T>(Dictionary<int, T> dict, HashSet<int> filters)
  {
    return dict.Where(kvp => filters.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, pair => pair.Value);
  }
}