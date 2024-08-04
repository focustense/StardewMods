using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DataLayers.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForDataLayers : IGenericModConfigMenuIntegrationFor<ModConfig>
{
    /*********
    ** Fields
    *********/
    /// <summary>The default mod settings.</summary>
    private readonly ModConfig DefaultConfig = new();

    /// <summary>The color registry holding available schemes and colors.</summary>
    private readonly ColorRegistry ColorRegistry;

    /// <summary>Layers registered by other mods.</summary>
    private readonly ILayerRegistry LayerRegistry;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="layerRegistry">Layers registered by other mods.</param>
    /// <param name="colorSchemes">The color schemes available to apply.</param>
    public GenericModConfigMenuIntegrationForDataLayers(ILayerRegistry layerRegistry, ColorRegistry colorRegistry)
    {
        this.LayerRegistry = layerRegistry;
        this.ColorRegistry = colorRegistry;
    }

    /// <inheritdoc />
    public void Register(GenericModConfigMenuIntegration<ModConfig> menu, IMonitor monitor)
    {
        menu
            .Register()
            .AddSectionTitle(I18n.Config_Section_MainOptions)
            .AddCheckbox(
                name: I18n.Config_ShowGrid_Name,
                tooltip: I18n.Config_ShowGrid_Desc,
                get: config => config.ShowGrid,
                set: (config, value) => config.ShowGrid = value
            )
            .AddCheckbox(
                name: I18n.Config_CombineBorders_Name,
                tooltip: I18n.Config_CombineBorders_Desc,
                get: config => config.CombineOverlappingBorders,
                set: (config, value) => config.CombineOverlappingBorders = value
            )
            .AddDropdown(
                name: I18n.Config_ColorScheme_Name,
                tooltip: I18n.Config_ColorSchene_Desc,
                get: config => config.ColorScheme,
                set: (config, value) => config.ColorScheme = value,
                allowedValues: this.ColorRegistry.SchemeIds.ToArray(),
                formatAllowedValue: key => I18n.GetByKey($"config.color-schemes.{key}").Default(key)
            )

            .AddSectionTitle(I18n.Config_Section_MainControls)
            .AddKeyBinding(
                name: I18n.Config_ToggleLayerKey_Name,
                tooltip: I18n.Config_ToggleLayerKey_Desc,
                get: config => config.Controls.ToggleLayer,
                set: (config, value) => config.Controls.ToggleLayer = value
            )
            .AddKeyBinding(
                name: I18n.Config_PrevLayerKey_Name,
                tooltip: I18n.Config_PrevLayerKey_Desc,
                get: config => config.Controls.PrevLayer,
                set: (config, value) => config.Controls.PrevLayer = value
            )
            .AddKeyBinding(
                name: I18n.Config_NextLayerKey_Name,
                tooltip: I18n.Config_NextLayerKey_Desc,
                get: config => config.Controls.NextLayer,
                set: (config, value) => config.Controls.NextLayer = value
            );

        List<LayerConfigSection> configSections = [
            GetBuiltInSection(config => config.Layers.Accessible, "accessible"),
            GetBuiltInSection(config => config.Layers.Buildable, "buildable"),
            GetBuiltInSection(config => config.Layers.CoverageForBeeHouses, "bee-houses"),
            GetBuiltInSection(config => config.Layers.CoverageForJunimoHuts, "junimo-huts"),
            GetBuiltInSection(config => config.Layers.CoverageForScarecrows, "scarecrows"),
            GetBuiltInSection(config => config.Layers.CoverageForSprinklers, "sprinklers"),
            GetBuiltInSection(config => config.Layers.CropHarvest, "crop-harvest"),
            GetBuiltInSection(config => config.Layers.CropWater, "crop-water"),
            GetBuiltInSection(config => config.Layers.CropPaddyWater, "crop-paddy-water"),
            GetBuiltInSection(config => config.Layers.CropFertilizer, "crop-fertilizer"),
            GetBuiltInSection(config => config.Layers.Machines, "machines"),
            GetBuiltInSection(config => config.Layers.TileGrid, "grid"),
            GetBuiltInSection(config => config.Layers.Tillable, "tillable"),
        ];

        foreach (var registration in this.LayerRegistry.GetAllRegistrations())
        {
            configSections.Add(new(
                config => config.GetModLayerConfig(registration.UniqueId),
                () => I18n.GetByKey(
                    "config.section.layer",
                    new { LayerName = registration.Layer.Name })));
        }
        // Language can change while the game is running, but usually doesn't. This gives us
        // alphabetical order most of the time.
        configSections.Sort((a, b) => a.GetTitle().CompareTo(b.GetTitle()));
        foreach (var section in configSections)
        {
            this.AddLayerConfigSection(menu, section);
        }

    }

    /// <summary>
    /// Derives <see cref="LayerConfigSection"/> data for an internal (built-in) layer type.
    /// </summary>
    /// <param name="getLayer">Function to get the layer field from a config model.</param>
    /// <param name="translationKey">The translation key for this layer.</param>
    /// <returns></returns>
    private static LayerConfigSection GetBuiltInSection(
        Func<ModConfig, LayerConfig> getLayer,
        string translationKey)
    {
        return new(getLayer, () => GetLayerSectionTitle(translationKey));
    }

    /// <summary>Information about a single layer's configuration settings.</summary>
    /// <param name="GetLayer">Function to get the layer field from a config model.</param>
    /// <param name="GetTitle">Function to get the (localized) section title.</param>
    private record LayerConfigSection(
        Func<ModConfig, LayerConfig> GetLayer,
        Func<string> GetTitle);


    /*********
    ** Private methods
    *********/
    /// <summary>Add the config section for a layer.</summary>
    /// <param name="menu">The integration API through which to register the config menu.</param>
    /// <param name="section">Contains the information about this layer/config section.</param>
    private void AddLayerConfigSection(GenericModConfigMenuIntegration<ModConfig> menu, LayerConfigSection section)
    {
        LayerConfig defaultConfig = section.GetLayer(this.DefaultConfig);

        menu
            .AddSectionTitle(section.GetTitle)
            .AddCheckbox(
                name: I18n.Config_LayerEnabled_Name,
                tooltip: I18n.Config_LayerEnabled_Desc,
                get: config => section.GetLayer(config).Enabled,
                set: (config, value) => section.GetLayer(config).Enabled = value
            )
            .AddCheckbox(
                name: I18n.Config_LayerUpdateOnViewChange_Name,
                tooltip: I18n.Config_LayerUpdateOnViewChange_Desc,
                get: config => section.GetLayer(config).UpdateWhenViewChange,
                set: (config, value) => section.GetLayer(config).UpdateWhenViewChange = value
            )
            .AddNumberField(
                name: I18n.Config_LayerUpdatesPerSecond_Name,
                tooltip: () => I18n.Config_LayerUpdatesPerSecond_Desc(defaultValue: defaultConfig.UpdatesPerSecond),
                get: config => (float)section.GetLayer(config).UpdatesPerSecond,
                set: (config, value) => section.GetLayer(config).UpdatesPerSecond = (decimal)value,
                min: 0.1f,
                max: 60f
            )
            .AddKeyBinding(
                name: I18n.Config_LayerShortcut_Name,
                tooltip: I18n.Config_LayerShortcut_Desc,
                get: config => section.GetLayer(config).ShortcutKey,
                set: (config, value) => section.GetLayer(config).ShortcutKey = value
            );
    }

    /// <summary>Get the translated section title for a layer.</summary>
    /// <param name="translationKey">The layer ID.</param>
    private static string GetLayerSectionTitle(string translationKey)
    {
        string layerName = I18n.GetByKey($"{translationKey}.name");
        return I18n.Config_Section_Layer(layerName);
    }
}
