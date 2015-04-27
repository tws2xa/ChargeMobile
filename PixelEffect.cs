using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Charge
{
    class Pixel
    {
        public Color col; //Pixel color
        public int xOffset; //Offset from main effect's x position
        public int yOffset; //Offset from main effect's y position
        public float xInc; //Used to track x velocity
        public float yInc; //Used to track y velocity
        public float opacity; //Opacity
        public Vector2 speed;

        public Pixel(Color col, int xOffset, int yOffset, Vector2 speed)
        {
            this.col = col;
            this.xOffset = xOffset;
            this.yOffset = yOffset;
            this.xInc = 0;
            this.yInc = 0;
            this.speed = speed;
            opacity = 1;
        }
    }

    class PixelEffect : WorldEntity
    {

        public int pixelSize; //Width and height of pixel
        public float spawnFrequency; //Chance of spawning a pixel (can be > 1, will spawn more than 1 per frame)
        public float spawnFadeTime; //How quickly the spawn frequency decreases
        public float pixelFadeTime; //How quickly pixel opacity fades

        public float pixelXVel; //X Velocity of each individual pixel
        public float pixelYVel; //Y Velocity of each individual pixel
        public float xVel; //X Velocity of the effect
        public float yVel; //Y Velocity of the effect
        float xInc; //Used to track float speeds with int positions
        float yInc; //Used to track float speeds with int positions

        private bool randomizePixelDirection = false; //Send pixels in a random direction, using (xVel + yVel)/2 as the speed.
        private float randomDirSpeed = 0; //Speed of random pixels

        public bool followCamera; //Follow the camera (like normal world objects)
        Random rand; //Used for random number generation

        List<Pixel> pixels; //List of spawned pixels.
        List<Color> colors; //Possible pixel colors
        
        /// <summary>
        /// Create a pixel effect
        /// Default Values:
        ///     pixel size: 5
        ///     follow camera: true
        ///     spawn frequency: 3
        ///     spawn fade time: 0.5
        ///     pixelXVel: 0
        ///     pixelYVel: 0
        ///     xVel: 0
        ///     yVel: 0
        /// </summary>
        /// <param name="position">Position of the pixel effect</param>
        /// <param name="pixelTex">Texture to use for each individual pixel (will be tinted)</param>
        /// <param name="colors">List of possible pixel colors</param>
        public PixelEffect(Rectangle position, Texture2D pixelTex, List<Color> colors)
        {
            this.position = position;
            this.tex = pixelTex;
            this.colors = colors;

            pixelSize = 5;
            followCamera = true;
            SetSpawnFreqAndFade(3.0f, 0.5f);
            pixelFadeTime = 0.5f;
            xVel = 0;
            yVel = 0;
            xInc = 0;
            yInc = 0;
            pixelXVel = 0;
            pixelYVel = 0;

            pixels = new List<Pixel>();
            rand = new Random();
        }

        public void EnableRandomPixelDirection(float speed)
        {
            randomizePixelDirection = true;
            randomDirSpeed = speed;
        }

        public void DisableRandomPixelDirection(float XVel, float YVel)
        {
            pixelXVel = XVel;
            pixelYVel = yVel;
        }

        /// <summary>
        /// Set the spawn frequency and the number of seconds it should
        /// take to fade out.
        /// </summary>
        /// <param name="frequency">New spawn frequency</param>
        /// <param name="fadeTime">Fade time in seconds</param>
        public void SetSpawnFreqAndFade(float frequency, float fadeTime) {
            spawnFrequency = frequency;
            spawnFadeTime = 1.0f/(spawnFrequency*2);
        }

        /// <summary>
        /// Update each pixel and the main effect
        /// </summary>
        /// <param name="deltaTime">Time passed since last frame</param>
        public override void Update(float deltaTime)
        {
            //Spawn any pixels
            //If spawn frequency is more than one, some will
            //have to be spawned with certainty.
            //Basially: if freq = 2.5,
            //spawn 2 and then have 50% chance of spawning a 3rd.
            double numIters = Math.Floor(spawnFrequency);
            double remainder = spawnFrequency % 1;
            for (int i = 0; i < numIters; i++)
            {
                SpawnPixel();
            }
            if (rand.NextDouble() < remainder)
            {
                SpawnPixel();
            }

            //Update each pixel's opacity
            for (int i = 0; i<pixels.Count; i++)
            {
                Pixel curPix = pixels[i];
                curPix.opacity -= (1 / pixelFadeTime) * deltaTime;
                if (curPix.opacity <= 0)
                {
                    pixels.RemoveAt(i); //Remove if invisible
                    i--;
                }
                curPix.xInc += curPix.speed.X * deltaTime;
                if (Math.Abs(curPix.xInc) > 1)
                {
                    curPix.xOffset += Convert.ToInt32(Math.Floor(curPix.xInc));
                    curPix.xInc %= 1;
                }
                curPix.yInc += curPix.speed.Y * deltaTime;
                if (Math.Abs(curPix.yInc) > 1)
                {
                    curPix.yOffset += Convert.ToInt32(Math.Floor(curPix.yInc));
                    curPix.yInc %= 1;
                }
            }

            //Update the spawn frequency
            if(spawnFadeTime > 0) spawnFrequency -= (1 / spawnFadeTime) * deltaTime;
            if (spawnFrequency <= 0)
            {
                spawnFrequency = 0;
                if (pixels.Count < 0) this.destroyMe = true;
            }

            //Manage movement
            xInc += xVel * deltaTime;
            if (Math.Abs(xInc) > 1)
            {
                position.X += Convert.ToInt32(Math.Floor(xInc));
                xInc = xInc % 1;
            }
            yInc += yVel * deltaTime;
            if (Math.Abs(yInc) > 1)
            {
                position.Y += Convert.ToInt32(Math.Floor(yInc));
                yInc = yInc % 1;
            }

            //Manage moving with camera and bounds check
            if (followCamera)
            {
                base.Update(deltaTime);
            }
            else
            {
                PerformScreenBoundsCheck();
            }
        }

        /// <summary>
        /// Draws each pixel
        /// </summary>
        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach(Pixel pixel in pixels) {
                int x = this.position.X + pixel.xOffset;
                int y = this.position.Y + pixel.yOffset;
                Rectangle pos = new Rectangle(x, y, pixelSize, pixelSize);
                spriteBatch.Draw(tex, pos, pixel.col * pixel.opacity);
            }
        }

        /// <summary>
        /// Create a new pixel
        /// </summary>
        private void SpawnPixel()
        {

            if (position.Width < pixelSize || position.Height < pixelSize) return;

            int colIndex = rand.Next(0, colors.Count);

            int xOffset = rand.Next(0, position.Width - pixelSize);
            int yOffset = rand.Next(0, position.Height - pixelSize);


            float pixXVel = pixelXVel;
            float pixYVel = pixelYVel;

            if (randomizePixelDirection)
            {
                pixXVel = Convert.ToSingle(-randomDirSpeed + rand.NextDouble() * randomDirSpeed * 2);
                pixYVel = Convert.ToSingle(Math.Sqrt(randomDirSpeed * randomDirSpeed - pixXVel * pixXVel));
                if (rand.NextDouble() < 0.5) pixYVel *= -1;
            }

            Pixel newPixel = new Pixel(colors[colIndex], xOffset, yOffset, new Vector2(pixXVel, pixYVel));

            pixels.Add(newPixel);
        }

        
    }
}
