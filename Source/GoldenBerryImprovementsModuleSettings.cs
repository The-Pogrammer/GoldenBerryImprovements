using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.GoldenBerryImprovements;

public class GoldenBerryImprovementsModuleSettings : EverestModuleSettings {
    [SettingSubText("GOLDENBERRYIMPROVEMENTS_SETTINGS_DISABLE_RETRY")]
    public bool DisableRetry { get; set; } = true;
    [SettingSubText("GOLDENBERRYIMPROVEMENTS_SETTINGS_SPEEDUP_DOORS")]
    public bool SpeedupDoors { get; set; } = true;
    [SettingSubText("GOLDENBERRYIMPROVEMENTS_SETTINGS_SKIP_CUTSCENES")]
    public bool SkipCutscenes { get; set; } = true;
    [SettingSubText("GOLDENBERRYIMPROVEMENTS_SETTINGS_SEGMENTING_MODE")]
    public bool SegmentingMode { get; set; } = false;
    public bool ShowCheckpointSwitcherUI { get; set; } = true;
    [DefaultButtonBinding(Buttons.RightTrigger, Keys.D)]
    public ButtonBinding NextCheckpoint { get; set; }
    [DefaultButtonBinding(Buttons.LeftTrigger, Keys.A)]
    public ButtonBinding PreviousCheckpoint { get; set; }
}