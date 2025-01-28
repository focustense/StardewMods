using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Integrations.TrainStation;
using StardewModdingAPI;

namespace Pathoschild.Stardew.CentralStation.Framework.Integrations;

/// <summary>An integration which adds stops from the Train Station mod.</summary>
internal class TrainStationStopProvider : ICustomStopProvider
{
    /*********
    ** Fields
    *********/
    /// <summary>Encapsulates monitoring and logging.</summary>
    private readonly IMonitor Monitor;

    /// <summary>Whether the Expanded Preconditions Utility mod is installed.</summary>
    private readonly bool HasExpandedPreconditionsUtility;

    /// <summary>The integration with the Train Station mod.</summary>
    private readonly TrainStationIntegration TrainStation;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public TrainStationStopProvider(IModRegistry modRegistry, IMonitor monitor)
    {
        this.Monitor = monitor;

        this.HasExpandedPreconditionsUtility = modRegistry.IsLoaded("Cherry.ExpandedPreconditionsUtility");
        this.TrainStation = new TrainStationIntegration(modRegistry, monitor);
    }

    /// <summary>Whether the integration is needed.</summary>
    public bool IsNeeded()
    {
        return this.TrainStation.IsLoaded;
    }

    /// <inheritdoc />
    public IEnumerable<StopModel> GetAvailableStops(StopNetwork? network)
    {
        var api = this.TrainStation;

        // skip if not applicable
        if (!api.IsLoaded || network is not (null or StopNetwork.Boat or StopNetwork.Train))
            yield break;

        // get enumerator
        IEnumerator<ITrainStationStopModel?>? enumerator = null;
        try
        {
            if (network is null)
            {
                enumerator =
                    (api.GetAvailableStops(true))
                    .Concat(api.GetAvailableStops(false))
                    .GetEnumerator();
            }
            else
            {
                enumerator = api
                    .GetAvailableStops(isBoat: network is StopNetwork.Boat)
                    .GetEnumerator();
            }
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"Could not load {network} stops from the Train Station mod because its API returned an unexpected error.\nTechnical details: {ex}", LogLevel.Warn);
            enumerator?.Dispose();
            yield break;
        }

        // yield each result
        while (true)
        {
            // get next stop
            ITrainStationStopModel? stop;
            try
            {
                if (!enumerator.MoveNext())
                {
                    enumerator.Dispose();
                    yield break;
                }

                stop = enumerator.Current;
                if (stop is null)
                    continue;
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Could not load {network} stops from the Train Station mod because its API returned an unexpected error.\nTechnical details: {ex}", LogLevel.Warn);
                yield break;
            }

            // ignore stops which duplicate a Central Station stop
            switch (stop.Id)
            {
                case "Cherry.TrainStation_BoatTunnel":
                case "Cherry.TrainStation_GingerIsland":
                case "Cherry.TrainStation_Railroad":
                    continue;
            }

            // add stop
            yield return new StopModel(
                id: stop.Id,
                displayName: I18n.Destinations_FromTrainStationMod(stopName: stop.DisplayName),
                toLocation: stop.TargetMapName,
                toTile: new Point(stop.TargetX, stop.TargetY),
                toFacingDirection: stop.FacingDirectionAfterWarp.ToString(),
                cost: stop.Cost,
                networks: [stop.IsBoat ? StopNetwork.Boat : StopNetwork.Train],
                conditions: this.ConvertExpandedPreconditionsToGameStateQuery(stop.Conditions)
            );
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Convert Expanded Preconditions Utility's conditions to its equivalent game state query syntax.</summary>
    /// <param name="conditions">The Expanded Preconditions Utility conditions.</param>
    private string? ConvertExpandedPreconditionsToGameStateQuery(string[]? conditions)
    {
        // skip if nothing to do
        if (conditions?.Length is null or 0)
            return null;

        // skip if Expanded Preconditions Utility not installed
        if (!this.HasExpandedPreconditionsUtility)
        {
            this.Monitor.LogOnce("The Train Station mod adds destinations with Expanded Preconditions Utility conditions, but you don't have Expanded Preconditions Utility installed. The destinations will default to always visible.", LogLevel.Warn);
            return null;
        }

        // convert to its game state query syntax
        const string expandedPreconditionsQuery = "Cherry.ExpandedPreconditionsUtility";
        switch (conditions.Length)
        {
            case 1:
                return $"{expandedPreconditionsQuery} {conditions[0]}";

            default:
                {
                    string[] queries = new string[conditions.Length];
                    for (int i = 0; i < conditions.Length; i++)
                        queries[i] = $"{expandedPreconditionsQuery} {conditions[i]}";

                    return "ANY \"" + string.Join("\" \"", queries) + "\"";
                }
        }
    }
}
