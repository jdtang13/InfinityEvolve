using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace InfinityEvolve
{
    public class BirthEffect
    {

        public const int BIRTH_EFFECT_FADE_TIME = 3; // 3 seconds

        private Ant mom;
        private Ant dad;
        private Ant baby;

        private double timeCounter = 0;  //  use this for fadetime of effects

        public BirthEffect(Ant myMom, Ant myDad, Ant myBaby)
        {
            mom = myMom;
            dad = myDad;
            baby = myBaby;
        }

        public Color EffectColor()
        {
            if (baby == null) return Color.Yellow;
            return new Color((int)baby.Attributes()["redColor"], (int)baby.Attributes()["greenColor"], (int)baby.Attributes()["blueColor"]);
        }

        public double TimeCounter()
        {
            return timeCounter;
        }
        public void AddToTimeCounter(double t)
        {
            timeCounter += t;
        }

        public Ant Mom() { return mom; }
        public Ant Dad() { return dad; }
        public Ant Baby() { return baby; }

        public Vector2 MomPosition()
        {
            if (mom != null)
            {
                return mom.Position();
            }
            return new Vector2(-1, -1);
        }
        public Vector2 DadPosition() {
            if (dad != null)
            {
                return dad.Position();
            }
            return new Vector2(-1, -1);
        }
        public Vector2 BabyPosition() {
            if (baby != null)
            {
                return baby.Position();
            }
            return new Vector2(-1,-1);
        }
    }
}
