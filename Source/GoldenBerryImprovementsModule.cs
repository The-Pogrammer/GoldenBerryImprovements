using System;
using System.Reflection;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.GoldenBerryImprovements;

public class GoldenBerryImprovementsModule : EverestModule
{
    public static GoldenBerryImprovementsModule Instance { get; private set; }

    public override Type SettingsType => typeof(GoldenBerryImprovementsModuleSettings);
    public static GoldenBerryImprovementsModuleSettings Settings => (GoldenBerryImprovementsModuleSettings)Instance._Settings;

    public override Type SessionType => typeof(GoldenBerryImprovementsModuleSession);
    public static GoldenBerryImprovementsModuleSession Session => (GoldenBerryImprovementsModuleSession)Instance._Session;

    public override Type SaveDataType => typeof(GoldenBerryImprovementsModuleSaveData);
    public static GoldenBerryImprovementsModuleSaveData SaveData => (GoldenBerryImprovementsModuleSaveData)Instance._SaveData;

    public GoldenBerryImprovementsModule()
    {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(GoldenBerryImprovementsModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(GoldenBerryImprovementsModule), LogLevel.Info);
#endif
    }

    public static string LoggerTag = nameof(GoldenBerryImprovementsModule);
    private static ILHook lockBlock_UnlockRoutine, key_UseRoutine, absorbRoutineHook;

    private static List<MTexture> arrowSprites = [];
    private static List<UIelement> uiElements = [];
    private static Dictionary<int, MTexture> TextSprites = [];

    public override void Load()
    {
        absorbRoutineHook = new ILHook(
            typeof(ClutterSwitch).GetMethod("AbsorbRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(),
            modAbsorbRoutine
        );

        On.Celeste.Player.Die += newOnDie;
        On.Celeste.Checkpoint.Added += newAdded;

        Everest.Events.LevelLoader.OnLoadingThread += AddController;
        IL.Celeste.ClutterSwitch.OnDashed += modClutterSwitch;

        //door speedup done by Viv!! (vividescence)

        MethodInfo m1 = typeof(LockBlock).GetMethod("UnlockRoutine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetStateMachineTarget();
        lockBlock_UnlockRoutine = new ILHook(m1, (il) => LockBlock_UnlockRoutine(il, m1.DeclaringType.GetField("<>4__this")));
        MethodInfo m2 = typeof(Key).GetMethod("UseRoutine", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetStateMachineTarget();
        key_UseRoutine = new ILHook(m2, (il) => Key_UseRoutine(il, m2.DeclaringType.GetField("<>4__this")));

        
    }
    public override void LoadContent(bool firstLoad)
    {
        base.LoadContent(firstLoad);
        uiElements.Add(new UIelement("leftArrow", new Vector2(0.7f, 0.95f), 0.7f, true));
        uiElements.Add(new UIelement("rightArrow", new Vector2(0.3f, 0.95f), 0.7f));
    }

    public override void Unload()
    {
        IL.Celeste.ClutterSwitch.OnDashed -= modClutterSwitch;
        On.Celeste.Player.Die -= newOnDie;
        On.Celeste.Checkpoint.Added -= newAdded;

        Everest.Events.LevelLoader.OnLoadingThread -= AddController;
        absorbRoutineHook?.Dispose();

        lockBlock_UnlockRoutine?.Dispose();
        key_UseRoutine?.Dispose();
    }

    private void newAdded(On.Celeste.Checkpoint.orig_Added orig, Checkpoint self, Scene scene)
    {
        orig(self, scene);
        Level level = scene as Level;
        if (Settings.SegmentingMode)
        {
            level.Session.StartCheckpoint = level.Session.Level;
        }
    }

    private PlayerDeadBody newOnDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        PlayerDeadBody deadBody = orig(self, direction, evenIfInvincible, registerDeathInStats);
        if (deadBody != null)
        {
            if (Settings.SegmentingMode && !deadBody.HasGolden)
            {
                deadBody.DeathAction += () =>
                {
                    Engine.Scene = new LevelExit(LevelExit.Mode.Restart, (deadBody.Scene as Level).Session);
                };
            }
        }
        return deadBody;
    }

    private static void AddController(Level level)
    {
        Controller controller;
        level.Add(controller = new Controller());

        
        foreach (var element in uiElements)
        {
            level.Add(element);
        }

        level.Add(new TextElement("checkpointCount", new Vector2(0.5f, 0.95f), 0.65f, 2));
        controller.giveUIlist(uiElements);

        
    }

    private void modAbsorbRoutine(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(11)))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(ShouldChangeAbsorbValue);
        }
    }

    private static int ShouldChangeAbsorbValue(int prev, object stateMachine)
    {
        Type stateMachineType = stateMachine?.GetType();
        FieldInfo playerField = stateMachineType?.GetField("player", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        Player player = playerField?.GetValue(stateMachine) as Player;

        if (player != null)
        {
            foreach (Follower f in player.Leader.Followers)
            {
                if (f.Entity is Strawberry s && s.Golden && !s.Winged)
                {
                    return 0;
                }
            }
        }
        return prev;
    }

    private static void modClutterSwitch(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.2f)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.EmitDelegate(GetClutterSwitchSpeed);
        }
    }

    private static float GetClutterSwitchSpeed()
    {
        return HasGoldenBerry(Engine.Scene) ? 0f : 0.2f;
    }

    private static void LockBlock_UnlockRoutine(ILContext ctx, FieldInfo _this)
    {
        ILCursor cursor = new(ctx);
        if (cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(1.2f)))
        {
            cursor.Emit(OpCodes.Ldarg_0); cursor.Emit(OpCodes.Ldfld, _this);
            cursor.EmitDelegate(GoldenCutInHalf);
        }
    }
    private static void Key_UseRoutine(ILContext ctx, FieldInfo _this)
    {
        ILCursor cursor = new(ctx);
        if (cursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Ldc_R4 && i.Operand is float f && (f == 1f || f == 0.3f)))
        {
            cursor.Emit(OpCodes.Ldarg_0); cursor.Emit(OpCodes.Ldfld, _this);
            cursor.EmitDelegate(GoldenCutInHalf);
        }
    }
    private static float GoldenCutInHalf(float prev, Entity e)
    {
        if (Settings.SpeedupDoors) // Only applies to LockBlock & Key
        {
            if (HasGoldenBerry(e?.Scene))
            {
                return 0;
            }
        }
        return prev;
    }

    private static bool HasGoldenBerry(Scene scene)
    {
        if (scene is Level level)
        {
            Player p = level.Tracker.GetEntity<Player>();
            if (p != null)
            {
                foreach (Follower f in p.Leader.Followers)
                {
                    if (f.Entity is Strawberry s && s.Golden && !s.Winged)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}