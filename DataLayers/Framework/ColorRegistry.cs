using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>Tracks loaded color schemes and colors.</summary>
    internal class ColorRegistry
    {
        /*********
        ** Fields
        *********/
        /// <summary>The monitor with which to log error messages.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The color schemes available to apply.</summary>
        private readonly Dictionary<string, ColorScheme> Schemes = new(StringComparer.OrdinalIgnoreCase);


        /*********
        ** Accessors
        *********/
        /// <summary>The collection of all available scheme IDs.</summary>
        public IEnumerable<string> SchemeIds => this.Schemes.Keys;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">The monitor with which to log error messages.</param>
        public ColorRegistry(IMonitor monitor)
        {
            this.Monitor = monitor;
        }

        /// <summary>Load the default color schemes from mod assets.</summary>
        /// <param name="dataHelper">The SMAPI API to read local mod assets.</param>
        public void LoadDefaultSchemes(IDataHelper dataHelper)
        {
            var rawData = dataHelper.ReadJsonFile<Dictionary<string, Dictionary<string, string?>>>(ColorScheme.AssetName);
            this.LoadSchemes(rawData);
        }

        /// <summary>Load color schemes from an alternate source, generally another mod via the API.</summary>
        /// <param name="schemeData">Raw dictionary data from the color scheme JSON. Each entry is a pair whose key is the scheme ID and whose value is a map of color names to color values for that scheme.</param>
        /// <param name="assetName">Name of the asset used to load the data, if not the default asset. Only used for logging errors and does not affect behavior.</param>
        public void LoadSchemes(Dictionary<string, Dictionary<string, string?>>? schemeData, string? assetName = null)
        {
            schemeData = schemeData is not null
                ? new(schemeData, StringComparer.OrdinalIgnoreCase)
                : new(StringComparer.OrdinalIgnoreCase);

            foreach ((string schemeId, Dictionary<string, string?> rawColors) in schemeData)
            {
                Dictionary<string, Color> colors = new(StringComparer.OrdinalIgnoreCase);

                foreach ((string name, string? rawColor) in rawColors)
                {
                    Color? color = Utility.StringToColor(rawColor);

                    if (color is null)
                    {
                        this.Monitor.Log($"Can't load color '{name}' from{(!ColorScheme.IsDefaultColorScheme(schemeId) ? $" color scheme '{schemeId}'" : "")} '{assetName ?? ColorScheme.AssetName}'. The value '{rawColor}' isn't a valid color format.", LogLevel.Warn);
                        continue;
                    }

                    colors[name] = color.Value;
                }
                if (this.Schemes.TryGetValue(schemeId, out var registeredColors))
                    registeredColors.Merge(colors);
                else
                    this.Schemes[schemeId] = new ColorScheme(schemeId, colors, this.Monitor);
            }
        }

        /// <summary>Try to get a color scheme by its ID.</summary>
        /// <param name="schemeId">The scheme ID.</param>
        /// <param name="scheme">The matching color scheme, or <c>null</c> if not found.</param>
        /// <returns>Returns whether a scheme was found.</returns>
        public bool TryGetScheme(string schemeId, [MaybeNullWhen(false)] out ColorScheme scheme)
        {
            return this.Schemes.TryGetValue(schemeId, out scheme);
        }
    }
}
