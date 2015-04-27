using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Charge
{
    class DischargeAnimation : AnimatedWorldEntity
    {
        private static readonly double FrameTime = 0.08; // How long to wait before switching to the next animation frame
        private static readonly int NumAnimationFrames = 5; // How many frames are included in the sprite strip

        private static readonly int DischargeAnimationGrowthRate = 500; // The rate that the sphere will grow at
        private Player player;
        public Circle circle;

        public bool followPlayerHeight = false;

        public DischargeAnimation(Rectangle pos, Texture2D tex, Player player)
            : base(pos, tex, FrameTime, NumAnimationFrames, false)
        {
            init(pos, tex);
            circle = new Circle(new Vector2(pos.X + player.position.Width / 2, pos.Y + player.position.Height / 2), pos.Width / 2);
            this.player = player;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime); //Handle Animation

            int growth = Convert.ToInt32(DischargeAnimationGrowthRate * deltaTime);

            int newHeight = position.Height + growth * 2;

            int yPos = this.position.Center.Y - newHeight / 2;

            if (followPlayerHeight)
            {
                yPos = player.position.Center.Y - newHeight / 2;
            }

            position = new Rectangle(position.Left - growth, yPos, position.Width + growth * 2, newHeight);

            circle.Center = new Vector2(position.X + position.Width / 2, position.Y + position.Height / 2);
            circle.Radius = position.Width / 2;

            // Once the animation fills the screen, destroy it
            //if (position.Right - 20 > GameplayVars.WinWidth && position.Left < -20 && position.Top < -20 && position.Bottom - 20 > GameplayVars.WinHeight)
            if (this.circle.Radius > 3 * GameplayVars.WinWidth / 4)
            {
                destroyMe = true;
            }
        }
    }
}
