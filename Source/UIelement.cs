using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.GoldenBerryImprovements
{
    [CustomEntity("GoldenBerryImprovements/UIelement")]
    [Tracked]
    public class UIelement : Entity
    {
        private List<MTexture> Sprites = [];
        public string Name { get; }
        private int currentFrame = 0;
        private Vector2 PositionRatio;
        private readonly float opacity;
        private readonly bool flipped;

        public UIelement(string name, Vector2 PositionRatio, float opacity, bool flipped = false)
        {
            Name = name;
            this.PositionRatio = PositionRatio;
            this.opacity = opacity;
            this.flipped = flipped;
            Depth = Depths.Top;

            for (int i = 1; i < 6; i++)
            {
                Sprites.Add(GFX.Game["checkpointArrow" + i]);
            }
            

            AddTag(Tags.Global);
        }

        public void nextFrame()
        {
            currentFrame++;
            if (currentFrame >= Sprites.Count)
            {
                currentFrame = 0;
            }
        }

        public override void Render()
        {
            base.Render();

            if (!GoldenBerryImprovementsModule.Settings.SegmentingMode || !GoldenBerryImprovementsModule.Settings.ShowCheckpointSwitcherUI)
                return;

            Camera camera = SceneAs<Level>().Camera;
            if (camera != null)
            {
                Vector2 screenPosition = getScreenPosition(camera, Sprites[currentFrame], PositionRatio);
                Sprites[currentFrame].Draw(screenPosition, Vector2.One, Color.White * opacity, 1, 0, flipped ? Microsoft.Xna.Framework.Graphics.SpriteEffects.None : Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally);
            }
        }

        private Vector2 getScreenPosition(Camera camera, MTexture texture, Vector2 positionRatio)
        {
            float screenX = camera.Left + camera.Viewport.Width * positionRatio.X;
            float screenY = camera.Top + camera.Viewport.Height * positionRatio.Y;

            screenX -= texture.Width / 2f;
            screenY -= texture.Height / 2f;

            return new Vector2(screenX, screenY);
        }
    }
}
