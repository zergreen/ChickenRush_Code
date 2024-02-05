using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Puzzle_Bobble
{
    class Singleton
    {
        public const int SCREENWIDTH = 700;
        public const int SCREENHEIGHT = 700;

        //Ball grid width
        public const int BALLWIDTH = 500;

        //Collect Score, Ball Left, Count of fire, Position of Celling
        public int Score;
        public int BallLeft;
        public int FireCount;
        public int Celling;

        public SoundEffectInstance clickSoundInstance;

        //Viewport of 6 type of ball
        public List<Rectangle> ballColor = new List<Rectangle>()
        {
            new Rectangle(10, 1, 50, 62),
            new Rectangle(74, 1, 50, 62),
            new Rectangle(138, 1, 50, 62),
            new Rectangle(202, 1, 50, 62),
            new Rectangle(266, 1, 50, 62),
            new Rectangle(330, 1, 50, 62),
        };

        public enum GameState
        {
            StartNewLife,
            GamePlaying,
            GameOver,
            GameWin
        }
        public GameState CurrentGameState;

        public KeyboardState PreviousKey, CurrentKey;
        private static Singleton instance;

        private Singleton() { }

        public static Singleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Singleton();
                }
                return instance;
            }
        }
    }
}
