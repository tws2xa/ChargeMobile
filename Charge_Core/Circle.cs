using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Charge
{
    class Circle
    {
        public Vector2 Center { get; set; }
        public float Radius { get; set; }

        public Circle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public bool Intersects(Rectangle rect)
        {
            float xDist = Math.Abs(this.Center.X - rect.X);
            float yDist = Math.Abs(this.Center.Y - rect.Y);

            if (xDist > (rect.Width / 2 + this.Radius))
                return false;
            if (yDist > (rect.Height / 2 + this.Radius))
                return false;

            if (xDist <= (rect.Width / 2))
                return true;
            if (yDist <= (rect.Height / 2))
                return true;

            double cornerDist = Math.Pow((xDist - rect.Width / 2), 2) + Math.Pow((yDist - rect.Height / 2), 2);

            return (cornerDist <= Math.Pow(this.Radius, 2));
        }
    }
}
