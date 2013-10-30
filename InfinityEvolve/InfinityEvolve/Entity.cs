using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace InfinityEvolve
{
    public class Entity
    {
        protected Vector2 position;
        protected Texture2D sprite;
        protected double timeOfLastMove = 0;
        protected bool isObstacle = false;

        public static char[] Directions = new char[] { 'N', 'E', 'S', 'W' };

        public Entity()
        {
        }

        public Vector2 Position()
        {
            return position;
        }
        public void SetPosition(Vector2 pos)
        {
            position = pos;
        }

        public virtual char CreateMovement(Entity[,] grid, double curTime)
        {
            return '0'; //  typical entity should not move by default
        }

        public void SetPosition(int x, int y) {
            position.X = x;
            position.Y = y;
        }

        public virtual Color RGBColor()
        {
            return Color.White;
        }

        //  todo: FIX, broken
        public List<Entity> Neighbors(Entity[,] grid)
        {
            List<Entity> l = new List<Entity>();

            for (int i = -1; i < 2; i += 2)
            {
                for (int j = -1; j < 2; j += 2)
                {
                    int x = (int) position.X + i;
                    int y = (int) position.Y + j;

                    if (x < 0 || x >= grid.GetLength(0) || y < 0 || y >= grid.GetLength(1))
                    {
                        continue;
                    }
                    l.Add(grid[x, y]);
                }
            }
            return l;
        }

        public Texture2D Sprite()
        {
            return sprite;
        }

        public void SetSprite(Texture2D mySprite)
        {
            sprite = mySprite;
        }
    }
}
