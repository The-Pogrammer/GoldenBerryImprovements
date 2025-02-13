namespace Celeste.Mod.GoldenBerryImprovements;

public class GoldenBerryImprovementsModuleSettings : EverestModuleSettings {
    [SettingSubText("GOLDENBERRYIMPROVEMENTS_SETTINGS_DISABLE_RETRY")]
    public bool DisableRetry { get; set; } = true;
    [SettingSubText("GOLDENBERRYIMPROVEMENTS_SETTINGS_SPEEDUP_DOORS")]
    public bool SpeedupDoors { get; set; } = true;
    [SettingSubText("GOLDENBERRYIMPROVEMENTS_SETTINGS_SKIP_CUTSCENES")]
    public bool SkipCutscenes { get; set; } = true;
}