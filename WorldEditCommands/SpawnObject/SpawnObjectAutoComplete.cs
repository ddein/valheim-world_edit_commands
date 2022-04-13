using System.Collections.Generic;
using ServerDevcommands;
namespace WorldEditCommands;
public class SpawnObjectAutoComplete : SharedObjectAutoComplete {
  public List<string> NamedParameters;
  public SpawnObjectAutoComplete() {
    NamedParameters = WithSharedParameters(new() {
      "hunt",
      "durability",
      "name",
      "crafter",
      "variant",
      "amount",
      "pos",
      "rot",
      "refPlayer",
      "refPos",
      "refRot"
    });
    AutoComplete.Register(SpawnObjectCommand.Name, (int index) => {
      if (index == 0) return ParameterInfo.Ids;
      return NamedParameters;
    }, WithSharedFetchers(new() {
      {
        "hunt",
        (int index) => ParameterInfo.Flag("Hunt")
      },
      {
        "name",
        (int index) => index == 0 ? ParameterInfo.Create("name", "string", "Name for tameable creatures.") : ParameterInfo.None
      },
      {
        "crafter",
        (int index) => index == 0 ? ParameterInfo.Create("name", "string", "Crafter for items.") : ParameterInfo.None
      },
      {
        "variant",
        (int index) => index == 0 ? ParameterInfo.CreateWithMinMax("variant", "integer", "Variant for items.") : ParameterInfo.None
      },
      {
        "amount",
        (int index) => index == 0 ? ParameterInfo.CreateWithMinMax("amount", "integer", "Amount of spawned objects.") : ParameterInfo.None
      },
      {
        "pos",
        (int index) => ParameterInfo.XZY("pos", "Offset from the player / reference position", index)
      },
      {
        "refPos",
        (int index) => ParameterInfo.XZY("refPos", "Overrides the reference position (player's position)", index)
      },
      {
        "refPlayer",
        (int index) => index == 0 ? ParameterInfo.PlayerNames : ParameterInfo.None
      },
      {
        "rot",
        (int index) => ParameterInfo.YXZ("rot", "Rotation from the player / reference rotation", index)
      },
      {
        "refRot",
        (int index) => ParameterInfo.YXZ("refRot", "Overrides the reference rotation (player's rotation)", index)
      }
    }));
  }
}
