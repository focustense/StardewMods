using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Pathoschild.Stardew.DataLayers.Layers;

/// <summary>
/// Adapter layer that takes a mod-provided <see cref="IDataLayer"/> registered via the API and
/// converts it to an internal <see cref="ILayer"/> used for rendering.
/// </summary>
/// <param name="source">The registration data from the source mod, including the
/// <see cref="IDataLayer"/> definition.</param>
/// <param name="config">Configuration data for this layer.</param>
/// <param name="colors">Global color scheme for Data Layers.</param>
internal class ModLayer(
    LayerRegistration source,
    LayerConfig config,
    ColorScheme colors,
    IMonitor monitor)
    : ILayer
{
    public string Id { get; } = source.GetType().FullName!;

    public string Name => source.Layer.Name;

    public int UpdateTickRate => (int)(60 / config.UpdatesPerSecond);

    public bool UpdateWhenVisibleTilesChange => config.UpdateWhenViewChange;

    public LegendEntry[] Legend => this.GetLegendEntries();

    public KeybindList ShortcutKey => config.ShortcutKey;

    public bool AlwaysShowGrid => false;

    private Dictionary<string, LegendEntry>? LegendEntries;

    public TileGroup[] Update(
        GameLocation location,
        in Rectangle visibleArea,
        in Vector2[] visibleTiles,
        in Vector2 cursorTile)
    {
        if (this.LegendEntries is null)
        {
            // Not initialized yet.
            return [];
        }
        var builder = new LayerBuilder(this.Id, this.Name, this.LegendEntries, monitor);
        source.Layer.Update(builder, location, visibleArea, visibleTiles, cursorTile);
        return [.. builder.TileGroups];
    }

    private LegendEntry[] GetLegendEntries()
    {
        if (this.LegendEntries is null)
        {
            var builder = new LegendBuilder(source.LocalId, colors);
            source.Layer.Configure(builder);
            this.LegendEntries = builder.Entries.ToDictionary(entry => entry.Id);
        }
        return [.. this.LegendEntries.Values];
    }

    private class LegendBuilder(string layerId, ColorScheme colors) : ILegendBuilder
    {
        internal readonly List<LegendEntry> Entries = [];

        public ILegendBuilder Add(string id, string name, Color defaultColor)
        {
            this.Entries.Add(new(id, name, colors.Get(layerId, id, defaultColor)));
            return this;
        }
    }

    private class LayerBuilder(
        string layerId,
        string layerName,
        IDictionary<string, LegendEntry> legendEntries,
        IMonitor monitor)
        : ILayerBuilder
    {
        internal List<TileGroup> TileGroups { get; } = [];

        public ILayerBuilder AddTileGroup(
            string defaultTileTypeId,
            Action<ITileGroupBuilder> buildGroup)
        {
            var tileGroupBuilder =
                new TileGroupBuilder(layerId, layerName, legendEntries, defaultTileTypeId, monitor);
            buildGroup(tileGroupBuilder);
            this.TileGroups.Add(new(tileGroupBuilder.Tiles, tileGroupBuilder.OuterBorderColor));
            return this;
        }
    }

    private class TileGroupBuilder(
        string layerId,
        string layerName,
        IDictionary<string, LegendEntry> legendEntries,
        string defaultTileTypeId,
        IMonitor monitor)
        : ITileGroupBuilder
    {
        internal Color? OuterBorderColor { get; private set; }
        internal List<TileData> Tiles { get; } = [];

        public ITileGroupBuilder AddTile(Vector2 position, string? typeId = null)
        {
            typeId ??= defaultTileTypeId;
            if (string.IsNullOrEmpty(typeId))
            {
                return this;
            }
            if (legendEntries.TryGetValue(typeId, out var legendEntry))
            {
                this.Tiles.Add(new(position, legendEntry));
            }
            else
            {
                // Layers can update many times per second, so only log once per bad type ID to avoid
                // relentless spamming. We also don't include the position in the log entry, because
                // that could exponentially increase the number of "unique" warnings generated that are
                // all essentially saying the same thing.
                monitor.LogOnce(
                    $"Invalid (unregistered) tile type {typeId} provided in layer {layerName} ({layerId}).",
                    LogLevel.Warn);
            }
            return this;
        }

        public ITileGroupBuilder AddTiles(IEnumerable<Vector2> positions, Func<Vector2, string>? typeIdSelector)
        {
            foreach (var pos in positions)
            {
                this.AddTile(pos, typeIdSelector?.Invoke(pos));
            }
            return this;
        }

        public ITileGroupBuilder SetOuterBorderColor(Color? color)
        {
            this.OuterBorderColor = color;
            return this;
        }
    }
}
