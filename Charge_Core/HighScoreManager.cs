using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;

namespace Charge
{
    /// <summary>
    /// Manages all high score tasks:
    ///     Creating High Score File
    ///     Retrieving High Scores
    ///     Writing Back to High Score File
    /// </summary>
    class HighScoreManager
    {
        //File name of the high scores list
        String fileName = "HighScores.txt";

        //Top scores
        List<Int32> highScores;
        
        /// <summary>
        /// Sets up the HighScoreManager
        /// Creates a new High Score file if one doesn't already exist
        /// Otherwise, it loads in the stored high scores.
        /// </summary>
        public HighScoreManager()
        {
            if (!FileSystemManager.FileExists(fileName))
            {
                FileStream highScoreFileStream = FileSystemManager.GetFileStream("HighScores.txt", FileMode.OpenOrCreate);
                using(StreamWriter writer = new StreamWriter(highScoreFileStream)) {
                    for (int i = 0; i < GameplayVars.NumScores - 1; i++)
                        writer.Write("0 ");
                    writer.Write("0");
                }
                highScoreFileStream.Close();
            }

            LoadHighScores();
        }

        /// <summary>
        /// Updates the high score list given the final score
        /// Will essentially do nothing if score is not a high score
        /// </summary>
        public void updateHighScore(int finalScore)
        {
            highScores.Add(finalScore);
            highScores.Sort();
            highScores.Reverse();
            if (highScores.Count() >= GameplayVars.NumScores)
            {
                highScores.RemoveAt(GameplayVars.NumScores);
            }
            
            
            FileStream highScoreFileStream = FileSystemManager.GetFileStream("HighScores.txt", FileMode.OpenOrCreate);
            using (StreamWriter writer = new StreamWriter(highScoreFileStream))
            {
                for (int i = 0; i < highScores.Count() - 1; i++)
                    writer.Write(highScores[i] + " ");
                writer.Write(highScores[GameplayVars.NumScores - 1]);
            }
        }

        /// <summary>
        /// Returns the rank'th high score
        /// Rank 0 = Highest Score
        /// Rank 1 = Second Highest Score
        /// And so on...
        /// If rank is beyond the recorded high score size, returns 0
        /// </summary>
        /// <param name="rank">The place to check in the high score table</param>
        /// <returns>High score at rank</returns>
        public int getHighScore(int rank)
        {
            if (rank >= highScores.Count()) return 0;
            else return highScores[rank];
        }


        /// <summary>
        /// Clears the high score file and clears the high score data
        /// </summary>
        internal void ClearHighScores()
        {
            highScores.Clear();

            // Opens the high score file with append set to false so that the existing data is overwritten.
            FileStream highScoreFileStream = FileSystemManager.GetFileStream("HighScores.txt", FileMode.OpenOrCreate);
            using (StreamWriter writer = new StreamWriter(highScoreFileStream))
            {
                for (int i = 0; i < GameplayVars.NumScores - 1; i++)
                    writer.Write("0 ");
                writer.Write("0");
            }

            LoadHighScores();
        }

        /// <summary>
        /// Loads the high score data from the high score file
        /// </summary>
        public void LoadHighScores()
        {
            //Processing data in the list of scores
            highScores = new List<Int32>();
            FileStream highScoreFileStream = FileSystemManager.GetFileStream("HighScores.txt", FileMode.Open);
            using (StreamReader reader = new StreamReader(highScoreFileStream))
            {
                String line = reader.ReadLine();
                String[] data = line.Split(' ');
                foreach (String str in data)
                {
                    highScores.Add(Convert.ToInt32(str));
                }
            }
        }

    }
}
