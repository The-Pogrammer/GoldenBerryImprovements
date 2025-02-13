using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Celeste.Mod.Entities;
using FMOD;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.GoldenBerryImprovements
{
    [CustomEntity("GoldenBerryImprovements/Controller")]
    [Tracked]
    public class Controller : Entity
    {
        private Player player;
        public Controller(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            AddTag(Tags.PauseUpdate);
            AddTag(Tags.Global);
        }

        public Controller()
            : base()
        {
            AddTag(Tags.PauseUpdate);
            AddTag(Tags.Global);
        }


        public override void Update()
        {
            base.Update();
            player = Scene.Tracker.GetEntity<Player>();

            if (player == null)
            {
                return;
            }

            Strawberry goldenStrawb = null;
            foreach (Follower follower in player.Leader.Followers)
            {
                if (follower.Entity is Strawberry && (follower.Entity as Strawberry).Golden && !(follower.Entity as Strawberry).Winged)
                {
                    goldenStrawb = follower.Entity as Strawberry;
                }
            }

            if (goldenStrawb != null && GoldenBerryImprovementsModule.Settings.DisableRetry)
            {
                SceneAs<Level>().CanRetry = false;
            }
            if (goldenStrawb != null && SceneAs<Level>().InCutscene && GoldenBerryImprovementsModule.Settings.SkipCutscenes)
            {
                SceneAs<Level>().SkipCutscene();
            }
        }
    }
}
