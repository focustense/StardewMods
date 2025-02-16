using Pathoschild.Stardew.TestDataLayersMod.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Pathoschild.Stardew.TestDataLayersMod;

public class ModEntry : Mod
{
    private IDataLayersApi DataLayers = null!; // Set in Entry

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);

        helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
    }

    [EventPriority(EventPriority.Normal - 1)]
    private void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.DataLayers = this.Helper.ModRegistry.GetApi<IDataLayersApi>("Pathoschild.DataLayers")!;
        this.DataLayers.RegisterLayer(this.ModManifest, "checkerboard", new CheckerboardLayer());
    }
}
