using System.Collections.Generic;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DataLayers.Framework;

/// <summary>
/// Implementation of the Data Layers API.
/// </summary>
/// <param name="colorRegistry">Color registry containing available schemes and colors.</param>
/// <param name="monitor">Monitor instance for logging.</param>
public class Api : IDataLayersApi, ILayerRegistry
{
    private readonly ColorRegistry ColorRegistry;
    private readonly IMonitor Monitor;
    private readonly Dictionary<string, LayerRegistration> Registrations = [];

    internal Api(ColorRegistry colorRegistry, IMonitor monitor)
    {
        this.ColorRegistry = colorRegistry;
        this.Monitor = monitor;
    }

    // Explicit interface implementation so that we don't need to make `ILayerRegistry` public and
    // subsequently have Pintail try to pick it up (and choke).
    IEnumerable<LayerRegistration> ILayerRegistry.GetAllRegistrations()
    {
        return this.Registrations.Values;
    }

    public void RegisterColorSchemes(Dictionary<string, Dictionary<string, string?>> schemeData, string assetName)
    {
        this.ColorRegistry.LoadSchemes(schemeData, assetName);
    }

    public void RegisterLayer(IManifest mod, string id, IDataLayer layer)
    {
        string globalId = $"{mod.UniqueID}:{id}";
        var registration = new LayerRegistration(globalId, layer);
        if (!this.Registrations.TryAdd(globalId, registration))
        {
            this.Monitor.Log(
                $"Couldn't register layer with ID '{id}' for mod '{mod.UniqueID}' because the " +
                "mod already has another layer with the same ID.",
                LogLevel.Error);
        }
    }
}
