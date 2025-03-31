using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.GoldenBerryImprovements
{
    [CustomEntity("GoldenBerryImprovements/TextElement")]
    [Tracked]
    public class TextElement : Entity
    {
        // 0-9: 0-9, 10: '/'
        private Dictionary<int, MTexture> Sprites = [];
        public string Name { get; }
        private Vector2 PositionRatio;
        private readonly float opacity;
        private readonly int padding;
        private List<int> translatedText = [];

        public TextElement(string name, Vector2 positionRatio, float opacity, int padding)
        {
            Name = name;
            this.PositionRatio = positionRatio;
            this.opacity = opacity;
            this.padding = padding;
            Depth = Depths.Top;

            AddTag(Tags.Global);

            for (int i = 0; i < 10; i++)
            {
                Sprites.Add(i, GFX.Game[i.ToString()]);
            }
            Sprites.Add(10, GFX.Game["slash"]);
        }

        public void setText(string text)
        {

            translatedText.Clear();
            foreach (char c in text)
            {
                if (char.IsDigit(c))
                {
                    translatedText.Add(c - '0');
                }
                else if (c == '/')
                {
                    translatedText.Add(10);
                }
            }
        }

        public override void Render()
        {
            base.Render();

            if (!GoldenBerryImprovementsModule.Settings.SegmentingMode || !GoldenBerryImprovementsModule.Settings.ShowCheckpointSwitcherUI)
                return;

            Camera camera = SceneAs<Level>().Camera;
            if (camera == null || translatedText.Count == 0) return;

            int slashIndex = translatedText.IndexOf(10);
            if (slashIndex == -1) return;

            float leftWidth = 0f, rightWidth = 0f;
            for (int i = 0; i < slashIndex; i++)
            {
                if (Sprites.TryGetValue(translatedText[i], out MTexture sprite))
                    leftWidth += sprite.Width + padding;
            }
            for (int i = slashIndex + 1; i < translatedText.Count; i++)
            {
                if (Sprites.TryGetValue(translatedText[i], out MTexture sprite))
                    rightWidth += sprite.Width + padding;
            }

            float slashWidth = 0f;
            if (Sprites.TryGetValue(10, out MTexture slashSprite))
                slashWidth = slashSprite.Width;

            Vector2 basePosition = getScreenPosition(camera, slashSprite, PositionRatio);
            float slashX = basePosition.X;
            float slashY = basePosition.Y;

            float xOffset = -leftWidth;

            for (int i = 0; i < translatedText.Count; i++)
            {
                if (!Sprites.TryGetValue(translatedText[i], out MTexture sprite))
                    continue;

                Vector2 drawPosition = new Vector2(slashX + xOffset, slashY);
                sprite.Draw(drawPosition, Vector2.Zero, Color.White * opacity);

                xOffset += sprite.Width + padding;
            }
        }

        private Vector2 getScreenPosition(Camera camera, MTexture texture, Vector2 positionRatio)
        {
            float screenX = camera.Left + camera.Viewport.Width * positionRatio.X;
            float screenY = camera.Top + camera.Viewport.Height * positionRatio.Y;

            screenX -= MathF.Ceiling(texture.Width / 2f);
            screenY -= MathF.Ceiling(texture.Height / 2f);
            
            return new Vector2(screenX, screenY);
        }

    }
}
