using System.Collections.Generic;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DataLayers.Framework;

/// <inheritdoc cref="IDataLayersApi" />
public class Api : IDataLayersApi, ILayerRegistry
{
    /*********
    ** Fields
    *********/
    /// <summary>Manages available color schemes and colors.</summary>
    private readonly ColorRegistry ColorRegistry;

    /// <summary>Encapsulates monitoring and logging.</summary>
    private readonly IMonitor Monitor;

    /// <summary>The data layers registered through the API.</summary>
    private readonly Dictionary<string, LayerRegistration> Registrations = [];


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="colorRegistry">Manages available color schemes and colors.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    internal Api(ColorRegistry colorRegistry, IMonitor monitor)
    {
        this.ColorRegistry = colorRegistry;
        this.Monitor = monitor;
    }

    /// <inheritdoc />
    IEnumerable<LayerRegistration> ILayerRegistry.GetAllRegistrations() // explicit interface implementation to avoid needing to make `ILayerRegistry` public and have Pintail try to pick it up
    {
        return this.Registrations.Values;
    }

    /// <inheritdoc />
    public void RegisterColorSchemes(Dictionary<string, Dictionary<string, string?>> schemeData, string assetName)
    {
        this.ColorRegistry.LoadSchemes(schemeData, assetName);
    }

    /// <inheritdoc />
    public void RegisterLayer(IManifest mod, string id, IDataLayer layer)
    {
        string globalId = $"{mod.UniqueID}:{id}";
        var registration = new LayerRegistration(globalId, id, layer);

        if (!this.Registrations.TryAdd(globalId, registration))
            this.Monitor.Log($"Couldn't register layer with ID '{id}' for mod '{mod.UniqueID}' because the mod already has another layer with the same ID.", LogLevel.Error);
    }
}
