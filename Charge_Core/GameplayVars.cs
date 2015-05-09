using System;

using Microsoft.Xna.Framework;

namespace Charge
{
    static class GameplayVars
    {
        public static int ChargeBarY = 5;
        public static int ChargeBarHeight = 25; //15 on PC. 25 on Android.
        public static int WinWidth = 1920; //Approx 2x pc width
        public static int WinHeight = 1080; //Approx 1.8x pc width
        public static int BarrierWidth = 100;
        public static int StartPlayerWidth = 84;
        public static int StartPlayerHeight = 128;
        public static int FrontBarrierStartX = WinWidth + 700;
        public static int FrontBarrierStartY = WinHeight + 50;
        public static float GlowThreshold = (FrontBarrierStartX - BackBarrierStartX) / 4.0f; // Div 4 works well for Android. 7 for PC.
        public static int PlayerXBuffer = 30;
        public static int PlayerYBuffer = 27;
        public static int PlayerStartX = WinWidth/3;
        public static int playerNumJmps = 2;
        public static int wallXBuffer = 10;
        public static int wallYBuffer = 54;
        public static int enemyXBuffer = 10;
        public static int enemyYBuffer = 9;
        public static int BackBarrierStartX = -300;
        public static float maxPlayerVSpeed = 90;
        public static float Gravity = 72; // The y-axis starts at 0 at the top of the screen, so gravity should increase Y
        public static float JumpInitialVelocity = -28.8f; // The y-axis starts at 0 at the top of the screen, so jump should decrease Y
        public static float PlayerStartSpeed = 300;
        public static float BarrierStartSpeed = 350;
        public static float EnemyMoveSpeed = 2;
        public static float BulletMoveSpeed = 20;
        public static float ChargeDecreaseRate = 2;
        public static float BatteryChargeReplenish = 5;
        public static float BarrierSpeedUpRate = 6.0f;
        public static float ChargeToSpeedCoefficient = 10.0f;
        public static float TimeToScoreCoefficient = 4.5f;

        //Level coefficients, determine speed changes on levels
        public static float Level1Speed = 1.0f;
        public static float Level2Speed = 0.65f;
        public static float Level3Speed = 0.45f;
        public static float Level4Speed = 0.30f;
        public static float[] LevelSpeeds = { Level1Speed, Level2Speed, Level3Speed, Level4Speed };

        public static float[] DischargeCooldownTime = { 20, 17, 14, 11, 10, 10 }; //By level. In Seconds.
        public static float[] OverchargeCooldownTime = { 20, 17, 14, 11, 10, 10 }; //By level. In Seconds
        public static float[] ShootCooldownTime = { 5, 4.5f, 4, 3.5f, 3, 2.5f }; //By level. In Seconds
       

		public static int ChargeBarCapacity = 75;

        public static float DischargeMaxCost = 50;
        public static float DischargeCost = .3f;
        public static float ShootCost = 10;
        public static float OverchargeMax = 50;
        public static float OverchargePermanentAdd = BatteryChargeReplenish*2;

        public static float OverchargeIncAmt = OverchargeMax * 3.0f; //Should take about 1/3 seconds to reach max speed
        public static float OverchargePermanentAddAmt = OverchargePermanentAdd * 3.0f;
        public static float OverchargeDecAmt = OverchargeMax / 5.0f; //Should take about 5 seconds to reach normality again

        public static float titleScrollSpeed = 100;
        public static int MinPlatformBrightness = 255; //255 = fully bright

        public static int NumScores = 10;
        
        public static readonly String UserSettingsFile = "UserSettings.txt";
        
        public static readonly float VolumeChangeAmount = 0.05f;
        
        public static readonly Color[] ChargeBarLevelColors = { new Color(50, 50, 50), new Color(0, 234, 6), Color.Yellow, Color.Red, Color.Blue, Color.Pink }; // The bar colors for each charge level
        public static readonly Color[] PlatformLevelColors = { Color.White, new Color(0, 234, 6), Color.Yellow, Color.Tomato, Color.Blue, Color.DarkViolet }; // The platform colors for each charge level
        
        public static readonly String DefaultClearHighScoresText = "Clear High Scores";

        public static float DefaultTutorialMessageTimeout = 3.0f; // How long each tutorial message should appear on screen before disappearing (in seconds)
    }
}
