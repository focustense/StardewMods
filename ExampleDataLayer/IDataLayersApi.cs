using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;
using System;

namespace Pathoschild.Stardew.DataLayers;

/// <summary>
/// API for mods to interact with Data Layers.
/// </summary>
public interface IDataLayersApi
{
    /// <summary>
    /// Registers a new layer to be managed by Data Layers.
    /// </summary>
    /// <remarks>
    /// After registering, the layer will be visited when cycling through all layers, and can be
    /// configured with its own key binding in the Data Layers GMCM.
    /// </remarks>
    /// <param name="mod">Manifest for the registering mod; used for configuration.</param>
    /// <param name="id">A unique (within this mod) ID for the layer. Can be left empty if the mod
    /// only provides a single layer.</param>
    /// <param name="layer">Implementation of the layer to register.</param>
    void Register(IManifest mod, string id, IDataLayer layer);
}

/// <summary>
/// External definition of a data layer, to use with <see cref="IDataLayersApi"/>.
/// </summary>
public interface IDataLayer
{
    /// <summary>
    /// Name of the layer to display in HUD, GMCM, etc.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Performs one-time configuration for this layer.
    /// </summary>
    /// <param name="legendBuilder">Builder for configuring the legend entries. For the layer to
    /// draw correctly, all tile type IDs that could be referenced in an <see cref="Update"/> must
    /// be registered in the legend.</param>
    void Configure(ILegendBuilder legendBuilder);

    /// <summary>
    /// Updates the layer's state and gets the current set of tiles in the layer.
    /// </summary>
    /// <param name="location">The location where the layers will be drawn; i.e. the player's
    /// current location.</param>
    /// <param name="builder">The builder instance to which current tile groups and tiles may be
    /// added.</param>
    /// <param name="location">The current location.</param>
    /// <param name="visibleArea">The tile area currently visible on the screen.</param>
    /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
    /// <param name="cursorTile">The tile position under the cursor.</param>
    void Update(
        ILayerBuilder builder,
        GameLocation location,
        Rectangle visibleArea,
        Vector2[] visibleTiles,
        Vector2 cursorTile);
}

/// <summary>
/// API for setting up the legend entries (names corresponding to colors) for a data layer.
/// </summary>
public interface ILegendBuilder
{
    /// <summary>
    /// Adds an item to the legend.
    /// </summary>
    /// <param name="id">Identifies the tile type within this layer. Corresponds to the
    /// <c>defaultTileTypeId</c> used in <see cref="ILayerBuilder.AddTileGroup)"/> and the
    /// <c>typeId</c> for <see cref="ITileGroupBuilder.AddTile"/>.</param>
    /// <param name="name">Descriptive text to show in the actual legend UI.</param>
    /// <param name="colorId">The ID of the color to display for this type in the layer's color
    /// scheme (i.e. the field name in <c>colors.json</c>, not including the layer ID).</param>
    /// <param name="color">The default overlay color for tiles tagged with <paramref name="id"/>,
    /// if no color is specified in the <see cref="ColorScheme"/>.</param>
    /// <returns>The current builder instance, for configuring additional entries.</returns>
    ILegendBuilder Add(string id, string name, string colorId, Color color);
}

/// <summary>
/// API for sending tile output for a layer during an update.
/// </summary>
public interface ILayerBuilder
{
    /// <summary>
    /// Starts a new tile group that will display on the next update frame.
    /// </summary>
    /// <remarks>
    /// A tile group is a set tiles that are related in some way - for example, tiles in the
    /// coverage area of a craftable device like a sprinkler, or tiles along a path between two
    /// objects coordinating their functions. If no such relationships exist, then the grouping can
    /// be arbitrary, although a common convention is to provide one group per tile type.
    /// </remarks>
    /// <param name="defaultTileTypeId">A default type ID, previously registered in
    /// <see cref="IDataLayer.Configure"/>, that will apply as default for any tiles added to this
    /// group without an explicit color.</param>
    /// <param name="buildGroup">Action to build the group, i.e. add tiles to it and configure
    /// additional options.</param>
    /// <returns>The current builder instance, to optionally add more groups.</returns>
    ILayerBuilder AddTileGroup(string defaultTileTypeId, Action<ITileGroupBuilder> buildGroup);
}

/// <summary>
/// API for adding tiles to an in-progress tile group, as part of the current output of a layer.
/// </summary>
public interface ITileGroupBuilder
{
    /// <summary>
    /// Adds a single tile to the group.
    /// </summary>
    /// <param name="position">X/Y coordinates of the tile.</param>
    /// <param name="typeId">Identifies what kind of tile this is, which controls what color will be
    /// shown for it. Must have been previously registered with the <see cref="ILegendBuilder"/>.
    /// Can be omitted if it is the same as the <c>defaultTileTypeId</c> specified when creating the
    /// group from <see cref="ILayerBuilder.AddTileGroup"/>.</param>
    /// <returns>The current builder instance, for adding more tiles or setting additional
    /// options.</returns>
    ITileGroupBuilder AddTile(Vector2 position, string? typeId = null);

    /// <summary>
    /// Adds multiple tiles to the group.
    /// </summary>
    /// <param name="positions">Sequence containing X/Y coordinates of individual tiles.</param>
    /// <param name="typeIdSelector">A function that accepts a single X/Y coordinate pair and
    /// returns a value indicating what type of tile it is, which controls what color will be shown
    /// for it. Must have been previously registered with the <see cref="ILegendBuilder"/>. If this
    /// argument is omitted, then all tiles added will use the <c>defaultTileTypeId</c> specified
    /// when creating the group from <see cref="ILayerBuilder.AddTileGroup"/>.</param>
    /// <returns>The current builder instance, for adding more tiles or setting additional
    /// options.</returns>
    ITileGroupBuilder AddTiles(IEnumerable<Vector2> positions, Func<Vector2, string>? typeIdSelector);

    /// <summary>
    /// Configures a border drawn around the outer edges of all outer tiles in the group.
    /// </summary>
    /// <remarks>
    /// Borders will be drawn around all "islands" - i.e. all edges that are not shared between more
    /// than one tile.
    /// </remarks>
    /// <param name="color">The color of the border to draw. If this is <c>null</c>, no border will
    /// be drawn.</param>
    /// <returns>The current builder instance, for adding more tiles or setting additional
    /// options.</returns>
    ITileGroupBuilder SetOuterBorderColor(Color? color);
}
