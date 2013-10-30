using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace InfinityEvolve
{
    public class FoodFade
    {
        public const double FOOD_FADE_TIME = 1.5;

        Vector2 position;
        double timeCounter = 0;
        Texture2D sprite;

        public FoodFade(Vector2 position, Texture2D sprite)
        {
            this.position = position;
            this.sprite = sprite;
        }

        public Vector2 Position() { return position; }
        public Texture2D Sprite() { return sprite; }

        public double TimeCounter() { return timeCounter; }

        public void AddToTimeCounter(double x)
        {
            timeCounter += x;
        }

        public void Update(GameTime gameTime)
        {
            timeCounter += gameTime.ElapsedGameTime.TotalMilliseconds / 1000f;
        }

        public Color Color() {
            double alpha = ((FOOD_FADE_TIME - timeCounter)/FOOD_FADE_TIME)*255d;
            if (timeCounter >= FOOD_FADE_TIME) { alpha = 0; }

            Color color = new Color(255,255,255,(int)alpha);
            return color;
        }

    }
}
