using System;
using System.Reflection;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using On.Celeste;

namespace Celeste.Mod.GoldenBerryImprovements;

public class GoldenBerryImprovementsModule : EverestModule {
    public static GoldenBerryImprovementsModule Instance { get; private set; }

    public override Type SettingsType => typeof(GoldenBerryImprovementsModuleSettings);
    public static GoldenBerryImprovementsModuleSettings Settings => (GoldenBerryImprovementsModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(GoldenBerryImprovementsModuleSession);
    public static GoldenBerryImprovementsModuleSession Session => (GoldenBerryImprovementsModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(GoldenBerryImprovementsModuleSaveData);
    public static GoldenBerryImprovementsModuleSaveData SaveData => (GoldenBerryImprovementsModuleSaveData) Instance._SaveData;

    public GoldenBerryImprovementsModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(GoldenBerryImprovementsModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(GoldenBerryImprovementsModule), LogLevel.Info);
#endif
    }

    public override void Load()
    {
        Everest.Events.LevelLoader.OnLoadingThread += AddController;

        //door speedup done by Viv!! (vividescence)
        
        MethodInfo m1 = typeof(LockBlock).GetMethod("UnlockRoutine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetStateMachineTarget();
        lockBlock_UnlockRoutine = new ILHook(m1, (il) => LockBlock_UnlockRoutine(il, m1.DeclaringType.GetField("<>4__this")));
        MethodInfo m2 = typeof(Key).GetMethod("UseRoutine", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetStateMachineTarget();
        key_UseRoutine = new ILHook(m2, (il) => Key_UseRoutine(il, m2.DeclaringType.GetField("<>4__this")));
    }

    public override void Unload()
    {
        Everest.Events.LevelLoader.OnLoadingThread -= AddController;
        lockBlock_UnlockRoutine?.Dispose();
        key_UseRoutine?.Dispose();
    }

    private static void AddController(Level level)
    {
        level.Add(new Controller());
    }

    private static ILHook lockBlock_UnlockRoutine, key_UseRoutine;
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
        if (Settings.SpeedupDoors)
        {
            Player p = e?.Scene?.Tracker.GetEntity<Player>();
            if (p == null) return prev;
            foreach (Follower f in p.Leader.Followers)
            {
                if (f.Entity is Strawberry s && s.Golden && !s.Winged) return 0;//prev / 0f;
            }
        }
        return prev;
    }
}