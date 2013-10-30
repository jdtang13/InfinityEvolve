using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace InfinityEvolve
{
    public class FoodEffect
    {
        public const double FOOD_EFFECT_FADE_TIME = 1.5; // in seconds

        private Ant ant;
        private Vector2 foodPosition;

        private double timeCounter;  //  use this for fadetime of effects

        public FoodEffect(Ant myAnt, Vector2 foodPosition)
        {
            this.ant = myAnt;
            this.foodPosition = foodPosition;
        }

        public double TimeCounter()
        {
            return timeCounter;
        }
        public void AddToTimeCounter(double t)
        {
            timeCounter += t;
        }

        public Color EffectColor()
        {
            double alpha = ((FOOD_EFFECT_FADE_TIME - timeCounter) / (FOOD_EFFECT_FADE_TIME))*255;
            if (timeCounter >= FOOD_EFFECT_FADE_TIME) {
                alpha = 0;
            }

            //  green color
            return new Color(0, 128, 0, (int)alpha);
        }

        public Vector2 AntPosition() {
            if (ant != null)
            {
                return ant.Position();
            }
            return new Vector2(-1, -1);
        }
        public Vector2 FoodPosition() { return foodPosition; }
    }
}
