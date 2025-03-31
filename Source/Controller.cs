using System.Collections.Generic;
using System.Runtime.InteropServices;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.GoldenBerryImprovements
{
    [CustomEntity("GoldenBerryImprovements/Controller")]
    [Tracked]
    public class Controller : Entity
    {
        private Player player;
        private List<string> checkpoints;
        private int _currentCheckpoint;
        private bool _isFirstSet = true;
        private float animationTimer;
        private static List<UIelement> uiElements = [];
        private int currentCheckpoint
        {
            get => _currentCheckpoint;
            set
            {
                if (_isFirstSet)
                {
                    _isFirstSet = false;
                }
                else
                {
                    LoadCheckpoint(value);
                }
                _currentCheckpoint = value;
            }
        }

        private void LoadCheckpoint(int index)
        {
            if (index != 0) {
                SceneAs<Level>().Session.StartCheckpoint = checkpoints[index - 1];
            } 
            else
            {
                SceneAs<Level>().Session.StartCheckpoint = null;
            }
            Engine.Scene = new LevelExit(LevelExit.Mode.Restart, SceneAs<Level>().Session);
        }

        public void giveUIlist(List<UIelement> elements)
        {
            uiElements = elements;
        }

        public Controller(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            SetupController();
        }

        public Controller()
            : base()
        {
            SetupController();
        }

        public void SetupController()
        {
            AddTag(Tags.Global);
        }

        void FindCurrentCheckpoint(Scene scene)
        {
            if (checkpoints == null) { currentCheckpoint = 0; return; }
            string checkpoint = (scene as Level).Session.Level;
            if (checkpoints.Contains(checkpoint)) {
                currentCheckpoint = checkpoints.IndexOf(checkpoint) + 1;
            } 
            else
            {
                currentCheckpoint = 0;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            checkpoints = [.. SaveData.Instance.GetCheckpoints(SceneAs<Level>().Session.Area)];
            FindCurrentCheckpoint(scene);
        }

        public override void Render()
        {
            base.Render();
            TextElement textElement = SceneAs<Level>().Tracker.GetEntity<TextElement>();

            if (textElement != null)
            {
                textElement.setText((currentCheckpoint + 1).ToString() + '/' + (checkpoints.Count + 1).ToString());
            }
        }
        public override void Update()
        {
            base.Update();
            player = SceneAs<Level>().Tracker.GetEntity<Player>();

            animationTimer += Engine.DeltaTime;
            while (animationTimer > 0.1f) {
                animationTimer -= 0.1f;
                foreach (var element in uiElements)
                {
                    element.nextFrame();
                }
            }



            if (player == null)
            {
                return;
            }

            if (GoldenBerryImprovementsModule.Settings.SegmentingMode)
            {
                if (!(currentCheckpoint + 1 > checkpoints.Count))
                {
                    if (GoldenBerryImprovementsModule.Settings.NextCheckpoint.Pressed)
                    {
                        GoldenBerryImprovementsModule.Settings.NextCheckpoint.ConsumePress();
                        currentCheckpoint += 1;
                    }
                }
                if (!(currentCheckpoint - 1 < 0))
                {
                    if (GoldenBerryImprovementsModule.Settings.PreviousCheckpoint.Pressed)
                    {
                        GoldenBerryImprovementsModule.Settings.NextCheckpoint.ConsumePress();
                        currentCheckpoint -= 1;
                    }
                }
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
