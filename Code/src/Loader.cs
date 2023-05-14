using FloatyOshiroC = Celeste.Mod.BitsPieces.FloatyOshiro;

namespace Celeste.Mod.BitsPieces {
  public class LoaderModule : EverestModule {
    public BitsPiecesModule BitsPiecesInstance;

    // Load runs before Celeste itself has initialized properly.
    public override void Load() {
      BitsPiecesInstance.Load();
      FloatyOshiroC.Load();
    }

    // Optional, initialize anything after Celeste has initialized itself properly.
    public override void Initialize() {
      BitsPiecesInstance.Initialize();
    }

    // Optional, do anything requiring either the Celeste or mod content here.
    public override void LoadContent(bool firstLoad) {
      BitsPiecesInstance.LoadContent(firstLoad);
    }

    // Unload the entirety of your mod's content. Free up any native resources.
    public override void Unload() {
      BitsPiecesInstance.Unload();
      FloatyOshiroC.Unload();
    }
  }
}
