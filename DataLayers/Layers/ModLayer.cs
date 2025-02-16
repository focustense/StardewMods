using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Pathoschild.Stardew.DataLayers.Layers;

/// <summary>A data layer which implements a layer registered through the mod API.</summary>
internal class ModLayer : ILayer
{
    /*********
    ** Fields
    *********/
    /// <summary>The SMAPI API to log messages.</summary>
    private readonly IMonitor Monitor;

    /// <summary>The layer implementation registered through the mod API.</summary>
    private readonly LayerRegistration Source;

    /// <summary>The configuration data for this layer.</summary>
    private readonly LayerConfig Config;

    /// <summary>The current color scheme for Data Layers.</summary>
    private readonly ColorScheme Colors;

    private Dictionary<string, LegendEntry>? LegendEntries;



    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public string Name => this.Source.Layer.Name;

    /// <inheritdoc />
    public int UpdateTickRate => (int)(60 / this.Config.UpdatesPerSecond);

    /// <inheritdoc />
    public bool UpdateWhenVisibleTilesChange => this.Config.UpdateWhenViewChange;

    /// <inheritdoc />
    public LegendEntry[] Legend => this.GetLegendEntries();

    /// <inheritdoc />
    public KeybindList ShortcutKey => this.Config.ShortcutKey;

    /// <inheritdoc />
    public bool AlwaysShowGrid => false;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance/</summary>
    /// <param name="source"><inheritdoc cref="Source" path="/summary" /></param>
    /// <param name="config"><inheritdoc cref="Config" path="/summary" /></param>
    /// <param name="colors"><inheritdoc cref="Colors" path="/summary" /></param>
    /// <param name="monitor"><inheritdoc cref="Monitor" path="/summary" /></param>
    public ModLayer(LayerRegistration source, LayerConfig config, ColorScheme colors, IMonitor monitor)
    {
        this.Source = source;
        this.Config = config;
        this.Colors = colors;
        this.Monitor = monitor;

        this.Id = source.GetType().FullName!;
    }

    /// <inheritdoc />
    public TileGroup[] Update(ref readonly GameLocation location, ref readonly Rectangle visibleArea, ref readonly IReadOnlySet<Vector2> visibleTiles, ref readonly Vector2 cursorTile)
    {
        // skip if not initialized yet
        if (this.LegendEntries is null)
            return [];

        // get from source
        LayerBuilder builder = new(this.Id, this.Name, this.LegendEntries, this.Monitor);
        this.Source.Layer.Update(builder, location, visibleArea, visibleTiles, cursorTile);
        return [.. builder.TileGroups];
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get cached legend entries from the underlying source.</summary>
    private LegendEntry[] GetLegendEntries()
    {
        if (this.LegendEntries is null)
        {
            LegendBuilder builder = new(this.Source.LocalId, this.Colors);
            this.Source.Layer.Configure(builder);
            this.LegendEntries = builder.Entries.ToDictionary(entry => entry.Id);
        }

        return [.. this.LegendEntries.Values];
    }

    /// <inheritdoc cref="ILegendBuilder" />
    private class LegendBuilder : ILegendBuilder
    {
        /*********
        ** Fields
        *********/
        /// <summary>The unique ID for this layer within the mod, matching <see cref="LayerRegistration.LocalId"/>.</summary>
        private readonly string LayerId;

        /// <summary>The current color scheme for Data Layers.</summary>
        private readonly ColorScheme Colors;


        /*********
        ** Accessors
        *********/
        /// <summary>The registered legend entries.</summary>
        public List<LegendEntry> Entries { get; } = [];


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="layerId"><inheritdoc cref="LayerId" path="/summary" /></param>
        /// <param name="colors"><inheritdoc cref="Colors" path="/summary" /></param>
        public LegendBuilder(string layerId, ColorScheme colors)
        {
            this.LayerId = layerId;
            this.Colors = colors;
        }

        /// <inheritdoc />
        public ILegendBuilder Add(string id, string name, Color defaultColor)
        {
            this.Entries.Add(new LegendEntry(id, name, this.Colors.Get(this.LayerId, id, defaultColor)));

            return this;
        }
    }

    /// <inheritdoc cref="ILayerBuilder" />
    private class LayerBuilder : ILayerBuilder
    {
        /*********
        ** Fields
        *********/
        /// <summary>The unique ID for this layer within the mod, matching <see cref="LayerRegistration.LocalId"/>.</summary>
        private readonly string LayerId;

        /// <summary>The translated display name to show in-game.</summary>
        private readonly string LayerName;

        /// <summary>The registered legend entries.</summary>
        private readonly IDictionary<string, LegendEntry> LegendEntries;

        /// <summary>The SMAPI API to log messages.</summary>
        private readonly IMonitor Monitor;


        /*********
        ** Accessors
        *********/
        /// <summary>The registered tile groups.</summary>
        public List<TileGroup> TileGroups { get; } = [];


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="layerId"><inheritdoc cref="LayerId" path="/summary" /></param>
        /// <param name="layerName"><inheritdoc cref="LayerName" path="/summary" /></param>
        /// <param name="legendEntries"><inheritdoc cref="LegendEntries" path="/summary" /></param>
        /// <param name="monitor"><inheritdoc cref="Monitor" path="/summary" /></param>
        public LayerBuilder(string layerId, string layerName, IDictionary<string, LegendEntry> legendEntries, IMonitor monitor)
        {
            this.LayerId = layerId;
            this.LayerName = layerName;
            this.LegendEntries = legendEntries;
            this.Monitor = monitor;
        }

        /// <inheritdoc />
        public ILayerBuilder AddTileGroup(string defaultTileTypeId, Action<ITileGroupBuilder> buildGroup)
        {
            TileGroupBuilder tileGroupBuilder = new(this.LayerId, this.LayerName, this.LegendEntries, defaultTileTypeId, this.Monitor);
            buildGroup(tileGroupBuilder);
            this.TileGroups.Add(new TileGroup(tileGroupBuilder.Tiles, tileGroupBuilder.OuterBorderColor));

            return this;
        }
    }

    /// <inheritdoc cref="ITileGroupBuilder" />
    private class TileGroupBuilder : ITileGroupBuilder
    {
        /*********
        ** Fields
        *********/
        /// <summary>The unique ID for this layer within the mod, matching <see cref="LayerRegistration.LocalId"/>.</summary>
        private readonly string LayerId;

        /// <summary>The translated display name to show in-game.</summary>
        private readonly string LayerName;

        /// <summary>The registered legend entries.</summary>
        private readonly IDictionary<string, LegendEntry> LegendEntries;

        /// <summary>The default tile type ID for tiles which don't have an explicit color.</summary>
        private readonly string DefaultTileTypeId;

        /// <summary>The SMAPI API to log messages.</summary>
        private readonly IMonitor Monitor;


        /*********
        ** Accessors
        *********/
        /// <summary>The border drawn around the outer edges of the tile group 'islands'.</summary>
        internal Color? OuterBorderColor { get; private set; }

        /// <summary>The tiles registered for the tile group.</summary>
        internal List<TileData> Tiles { get; } = [];


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="layerId"><inheritdoc cref="LayerId" path="/summary" /></param>
        /// <param name="layerName"><inheritdoc cref="LayerName" path="/summary" /></param>
        /// <param name="legendEntries"><inheritdoc cref="LegendEntries" path="/summary" /></param>
        /// <param name="defaultTileTypeId"><inheritdoc cref="DefaultTileTypeId" path="/summary" /></param>
        /// <param name="monitor"><inheritdoc cref="Monitor" path="/summary" /></param>
        public TileGroupBuilder(string layerId, string layerName, IDictionary<string, LegendEntry> legendEntries, string defaultTileTypeId, IMonitor monitor)
        {
            this.LayerId = layerId;
            this.LayerName = layerName;
            this.LegendEntries = legendEntries;
            this.DefaultTileTypeId = defaultTileTypeId;
            this.Monitor = monitor;
        }

        /// <inheritdoc />
        public ITileGroupBuilder AddTile(Vector2 position, string? typeId = null)
        {
            typeId ??= this.DefaultTileTypeId;

            if (string.IsNullOrEmpty(typeId))
                return this;

            if (this.LegendEntries.TryGetValue(typeId, out var legendEntry))
                this.Tiles.Add(new(position, legendEntry));
            else
            {
                // Layers can update many times per second, so only log once per bad type ID to avoid
                // relentless spamming. We also don't include the position in the log entry, because
                // that could exponentially increase the number of "unique" warnings generated that are
                // all essentially saying the same thing.
                this.Monitor.LogOnce($"Invalid (unregistered) tile type {typeId} provided in layer {this.LayerName} ({this.LayerId}).", LogLevel.Warn);
            }

            return this;
        }

        /// <inheritdoc />
        public ITileGroupBuilder AddTiles(IEnumerable<Vector2> positions, Func<Vector2, string>? typeIdSelector)
        {
            foreach (var pos in positions)
            {
                this.AddTile(pos, typeIdSelector?.Invoke(pos));
            }
            return this;
        }

        /// <inheritdoc />
        public ITileGroupBuilder SetOuterBorderColor(Color? color)
        {
            this.OuterBorderColor = color;
            return this;
        }
    }
}
