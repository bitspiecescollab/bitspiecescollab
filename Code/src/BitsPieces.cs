// todo: hook Celeste.Mod.CollabUtils2.LobbyHelper::GetLobbyForLevelSet to return the correct lobby

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using MonoMod;
using MonoMod.Utils;
using MonoMod.RuntimeDetour;

using Celeste.Mod.UI;
// using FMOD.Studio;
// using Microsoft.Xna.Framework;
using Monocle;
using Celeste;

using Celeste.Mod.CollabUtils2;

namespace Celeste.Mod.BitsPieces {
  public class BitsPiecesModule : EverestModule {
    public static BitsPiecesModule Instance;
    public BitsPiecesModule() { Instance = this; }

    public static string COLLAB_ID = "bitspiecescollab";
    private static IDetour hook_GetLobbyForLevelSet = null;


    // Load runs before Celeste itself has initialized properly.
    public override void Load() {
      Logger.Log(LogLevel.Info, "BitsPieces", $"Hooking CollabUtils2... fingers crossed this goes well!");

      // CollabUtils2.LobbyHelper.GetLobbyForLevelSet("a");
      // On.CollabUtils2.LobbyHelper.GetLobbyForLevelSet += GetLobbyForLevelSet;

      if (typeof(CollabUtils2.LobbyHelper).GetMethod("GetLobbyForLevelSet", BindingFlags.Public | BindingFlags.Static) == null) {
        Logger.Log(LogLevel.Error, "BitsPieces", $"Method public static CollabUtils2.LobbyHelper::GetLobbyForLevelSet was not found to hook. Not hooking! This will cause wierd shit when returning to lobby!");
      } else {
        hook_GetLobbyForLevelSet = new Hook(
          typeof(CollabUtils2.LobbyHelper).GetMethod("GetLobbyForLevelSet", BindingFlags.Public | BindingFlags.Static),
          typeof(BitsPiecesModule).GetMethod("GetLobbyForLevelSet", BindingFlags.NonPublic | BindingFlags.Static)
        );
      }
    }

    // Optional, initialize anything after Celeste has initialized itself properly.
    public override void Initialize() {}

    // Optional, do anything requiring either the Celeste or mod content here.
    public override void LoadContent(bool firstLoad) {
    }

    // Unload the entirety of your mod's content. Free up any native resources.
    public override void Unload() {
      Logger.Log(LogLevel.Info, "BitsPieces", $"Dropping hooks...");

      if (hook_GetLobbyForLevelSet != null) {
        hook_GetLobbyForLevelSet.Dispose();
      }
    }

    private delegate string orig_GetLobbyForLevelSet(string self);
    private static string GetLobbyForLevelSet(
      orig_GetLobbyForLevelSet func,
      string levelSet
    ) {
      if (levelSet.StartsWith($"{COLLAB_ID}/")) {
        string lobby;
        // Don't set the lobby map of lobbies
        if (levelSet.StartsWith($"{COLLAB_ID}/0-Lobbies")) {
          lobby = null;
        } else {
          // Redirect all other maps back to our main lobby: gyms, maps, whatever
          // If this map doesn't exist I'll eat my sock
          lobby = $"{COLLAB_ID}/0-Lobbies/1-Main";
        }

        Logger.Log(LogLevel.Info, "BitsPieces", $"GetLobbyForLevelSet('{levelSet}') => '{lobby}'");
        return lobby;
      }

      return func(levelSet);
    }
  }
}
