using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace Charge
{
    class SpecialAbilityIconSet
    {
        public static int dischargeIconWidth = 130; //176
        public static int overchargeIconWidth = 130; //176
        public static int shootIconWidth = 130; //176
        public static int iconHeight = 87; //120
        
        private SpecialAbilityIcon dischargeIcon;
        private SpecialAbilityIcon shootIcon;
        private SpecialAbilityIcon overchargeIcon;

        public SpecialAbilityIconSet(int x, int y, int hSpace, Texture2D DischargeIconTex, Texture2D ShootIconTex, Texture2D OverchargeIconTex, Texture2D WhiteTex)
        {
            dischargeIcon = new SpecialAbilityIcon(new Rectangle(x, y, dischargeIconWidth, iconHeight), DischargeIconTex, WhiteTex);
            x += (dischargeIconWidth + hSpace);
            shootIcon = new SpecialAbilityIcon(new Rectangle(x, y, shootIconWidth, iconHeight), ShootIconTex, WhiteTex);
            x += (shootIconWidth + hSpace);
            overchargeIcon = new SpecialAbilityIcon(new Rectangle(x, y, dischargeIconWidth, iconHeight), OverchargeIconTex, WhiteTex);
        }

        public void Update(float deltaTime, float playerSpeed, float objectSpeed = 0.0f)
        {
            dischargeIcon.Update(deltaTime, playerSpeed);
            shootIcon.Update(deltaTime, playerSpeed);
            overchargeIcon.Update(deltaTime, playerSpeed);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawDischargeIcon(spriteBatch);
            DrawShootIcon(spriteBatch);
            DrawOverChargeIcon(spriteBatch);
        }

        public void DrawDischargeIcon(SpriteBatch spriteBatch)
        {
            dischargeIcon.Draw(spriteBatch);
        }

        public void DrawShootIcon(SpriteBatch spriteBatch)
        {
            shootIcon.Draw(spriteBatch);
        }

        public void DrawOverChargeIcon(SpriteBatch spriteBatch)
        {
            overchargeIcon.Draw(spriteBatch);
        }

        public void SetCooldown(float total, float global)
        {
            dischargeIcon.SetCooldown(total, global);
            shootIcon.SetCooldown(total, global);
            overchargeIcon.SetCooldown(total, global);
        }
    }
}
