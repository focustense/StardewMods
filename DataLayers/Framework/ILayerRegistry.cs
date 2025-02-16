using System.Collections.Generic;

namespace Pathoschild.Stardew.DataLayers.Framework;

/// <summary>The data for a data layer registered through the API.</summary>
/// <param name="UniqueId">The unique ID for this layer, including the mod ID prefix.</param>
/// <param name="LocalId">The unique ID for this layer within the mod, used in color schemes.</param>
/// <param name="Layer">The layer implementation provided by the mod.</param>
internal record LayerRegistration(string UniqueId, string LocalId, IDataLayer Layer);

/// <summary>Manages the data layers registered through the API.</summary>
/// <remarks>This interface is intended as the internal "reader" side to the singleton API instance, which also handles registrations (writes). Generally, most internal code only needs this read-only registry and does not need or want the entire <see cref="Api"/>.</remarks>
internal interface ILayerRegistry
{
    /// <summary>Get the layers registered so far.</summary>
    IEnumerable<LayerRegistration> GetAllRegistrations();
}
