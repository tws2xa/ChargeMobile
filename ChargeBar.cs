using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Charge
{
	class ChargeBar
	{
		public Texture2D tex;

		public Color backColor;
		public Color foreColor;

		public Rectangle position; //Object's position in the world

		public ChargeBar(Rectangle position, Texture2D tex, Color backColor, Color foreColor)
		{
			this.position = position;

			this.tex = tex;

            this.backColor = backColor;
            this.foreColor = foreColor;
		}

		public void Draw(SpriteBatch spriteBatch, float chargeLevel)
		{
			spriteBatch.Draw(tex, position, backColor);

            int chargeRectWidth = Convert.ToInt32((chargeLevel % GameplayVars.ChargeBarCapacity) / GameplayVars.ChargeBarCapacity * position.Width);

            Rectangle chargeRect = new Rectangle(position.Left, position.Top, chargeRectWidth, position.Height);
			spriteBatch.Draw(tex, chargeRect, foreColor);
		}

		public void SetForegroundColor(Color c)
		{
			foreColor = c;
		}

		public void SetBackgroundColor(Color c)
		{
			backColor = c;
		}
	}
}
