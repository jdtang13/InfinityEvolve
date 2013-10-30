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
    class SpriteSheet
    {
        private Texture2D sprite;

        private int width; //  of the single frame, not of the spritesheet entirely
        private int height;

        private int lastUpdateTime = 0;
        private int millisecondsBeforeChangingFrames;
        private bool animating = true;

        private int rows; //  y direction
        private int cols; //  x direction

        private Vector2 position;

        private int currentCol = 0;
        private int currentRow = 0;

        private int scaleWidth = 0;
        private int scaleHeight = 0;

        public SpriteSheet(Texture2D mySprite, int myWidth, int myHeight)
        {
            sprite = mySprite;

            width = myWidth;
            height = myHeight;

            if (sprite.Width / width != sprite.Width / (double)width
                || sprite.Height / height != sprite.Height / (double)height)
            {
                throw new ArgumentException();
            }
            else
            {
                cols = sprite.Width / width;
                rows = sprite.Height / height;
            }

            millisecondsBeforeChangingFrames = 100;
            //  no position by default
        }

        public SpriteSheet(Texture2D mySprite, int myWidth, int myHeight, int myMillisecondsBeforeChangingFrames)
        {
            sprite = mySprite;

            width = myWidth;
            height = myHeight;

            if (sprite.Width / width != sprite.Width / (double)width
                || sprite.Height / height != sprite.Height / (double)height)
            {
                throw new ArgumentException();
            }
            else
            {
                cols = sprite.Width / width;
                rows = sprite.Height / height;
            }

            millisecondsBeforeChangingFrames = myMillisecondsBeforeChangingFrames;
        }

        public void SetScale(int width, int height)
        {
            scaleWidth = width;
            scaleHeight = height;
        }

        public SpriteSheet(Vector2 position, Texture2D mySprite, int myMillisecondsBeforeChangingFrames, int myWidth, int myHeight)
        {
            millisecondsBeforeChangingFrames = myMillisecondsBeforeChangingFrames;
            sprite = mySprite;

            width = myWidth;
            height = myHeight;

            if (sprite.Width / width != sprite.Width / (double)width
                || sprite.Height / height != sprite.Height / (double)height)
            {
                throw new ArgumentException();
            }
            else
            {
                cols = sprite.Width / width;
                rows = sprite.Height / height;
            }
        }
        public void SetAnimating(bool b)
        {
            animating = b;
        }

        public Vector2 Position()
        {
            return position;
        }
        public void SetPosition(Vector2 v)
        {
            position = v;
        }

        public void SetRow(int r)
        {
            currentRow = r;
        }

        public void Update(GameTime gameTime)
        {
            int curTime = (int)gameTime.TotalGameTime.TotalMilliseconds;

            if (animating)
            {
                if (curTime - lastUpdateTime >= millisecondsBeforeChangingFrames)
                {
                    currentCol++;
                    currentCol = currentCol % cols;

                    lastUpdateTime = curTime;
                }
            }
        }

        public int Width()
        {
            return width;
        }

        public int Height()
        {
            return height;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, position,
                new Rectangle(width * currentCol, height * currentRow, width, height), Color.White);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 pos)
        {
            float scale = scaleWidth * scaleHeight / (width * height);

            spriteBatch.Draw(sprite, pos,
                new Rectangle(width * currentCol, height * currentRow, width, height), Color.White,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
