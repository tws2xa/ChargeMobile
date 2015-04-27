using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charge
{
    static class LevelGenerationVars
    {
        public static int Tier1Height = 189; //189
        public static int Tier2Height = 513; //513
        public static int Tier3Height = 837; //837

        public static int PlatformHeight = 72;
        public static int SegmentWidth = PlatformHeight;
        public static int BatteryWidth = 90;
        public static int BatteryHeight = 90;
        public static int WallWidth = 108;
        public static int WallHeight = 144;
        public static int EnemyWidth = 80;
        public static int EnemyHeight = 72;

        public static int MaxGroundPieces = 30;
        public static int MinGroundPieces = 3;

        public static int MaxNumSegments = 10;
        public static int MinNumSegments = 2;
        
        public static int[] MaxBetweenSpaces = {600, 600, 200};
        public static int[] MinBetweenSpaces = {150, 150, 90};
        
        public static int MaxNumEnemiesTotal = 3;
        public static int MaxBatteriesPerPlatform = 4;
        public static int MaxEnemiesPerPlatform = 2;
        public static int MaxWallsPerPlatform = 1;
        
        public static float SpeedToMinSpaceMultipler = (float)MinBetweenSpaces[0] / (float)GameplayVars.PlayerStartSpeed;
        public static float SpeedToMaxSpaceMultipler = (float)MaxBetweenSpaces[0] / (float)GameplayVars.PlayerStartSpeed;
        public static float PlatformSpawnFreq = 0.05f;

        /*
         * When spawning content above a platform, it does a die roll from 0-SectionContentRollNum
         * If the result is between 0 and BatterySpawnRollRange, it spawns a battery
         * If it's between BatterySpawnRollRange and (BatterySpawnRollRange + WallSpawnFrequency) it spawns a wall
         * If it's between (BatterySpawnRollRange + WallSpawnFrequency) and (BatterySpawnRollRange + WallSpawnFrequency + EnemySpawnFrequency) it spawns an enemy
         * And if it's in none of these ranges, it spawns nothing.
         */
        public static int SectionContentRollNum = 1000;
        public static int BatterySpawnRollRange = 125;
        public static int WallSpawnFrequency = 12;
        public static int EnemySpawnFrequency = 8;
        public static int MaxBatteryVariation = BatterySpawnRollRange / 3;

    }
}
