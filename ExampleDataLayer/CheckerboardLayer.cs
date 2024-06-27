using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers;
using StardewValley;

namespace Pathoschild.Stardew.ExampleDataLayer;

internal class CheckerboardLayer : IDataLayer
{
    public string Name => I18n.Example_Layer_Title();

    public void Configure(ILegendBuilder legendBuilder)
    {
        legendBuilder
            .Add("example.layer.even", I18n.Example_Layer_Even(), "evencolor", Color.Green)
            .Add("example.layer.odd", I18n.Example_Layer_Odd(), "oddcolor", Color.Red);
    }

    public void Update(
        ILayerBuilder builder,
        GameLocation location,
        Rectangle visibleArea,
        Vector2[] visibleTiles,
        Vector2 cursorTile)
    {
        builder.AddTileGroup(
            "",
            group => group.AddTiles(
                visibleTiles,
                coords => coords.X % 2 == 0 ^ coords.Y % 2 == 0
                    ? "example.layer.even"
                    : "example.layer.odd"));
    }
}
