using System.Collections.Generic;

namespace Pathoschild.Stardew.DataLayers.Framework;

/// <summary>
/// Data for a single mod-provided layer registered through the API.
/// </summary>
/// <param name="UniqueId">Unique ID for this layer, including both the unique mod ID and the local layer
/// ID specified by that mod.</param>
/// <param name="LocalId">Local layer ID for the mod; used in color schemes.</param>
/// <param name="Layer">The layer configuration provided by the mod.</param>
internal record LayerRegistration(string UniqueId, string LocalId, IDataLayer Layer);

/// <summary>
/// Provides access to registered layers.
/// </summary>
/// <remarks>
/// This interface is intended as the internal "reader" side to the singleton API instance, which
/// also handles registrations (writes). Generally, most internal code only needs this read-only
/// registry and does not need or want the entire <see cref="Api"/>.
/// </remarks>
internal interface ILayerRegistry
{
    /// <summary>
    /// Returns the layers registered so far.
    /// </summary>
    IEnumerable<LayerRegistration> GetAllRegistrations();
}
