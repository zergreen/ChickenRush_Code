using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Numerics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Puzzle_Bobble
{
    public class MainScene : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private List<GameObject> _gameObjects;
        private int _numObject;

        private Texture2D _background;
        private Song _backgroundMusic;
        private SoundEffect _clickSound;

        SpriteFont _font;

        public MainScene()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Window.Title = "ChickenRush";
            _graphics.PreferredBackBufferWidth = Singleton.SCREENWIDTH;
            _graphics.PreferredBackBufferHeight = Singleton.SCREENHEIGHT;
            _graphics.ApplyChanges();

            // Load BG sound
            _backgroundMusic = Content.Load<Song>("backgroundmusic");
            MediaPlayer.Volume = 0.1f;

            // Load click sound
            _clickSound = Content.Load<SoundEffect>("s-chicken");
            Singleton.Instance.clickSoundInstance = _clickSound.CreateInstance();

            _gameObjects = new List<GameObject>();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _background = this.Content.Load<Texture2D>("farm-bg");
            _font = Content.Load<SpriteFont>("GameFont");

            Reset();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Singleton.Instance.CurrentKey = Keyboard.GetState();
            _numObject = _gameObjects.Count;

            //Update
            switch (Singleton.Instance.CurrentGameState)
            {
                case Singleton.GameState.StartNewLife:
                    Singleton.Instance.CurrentGameState = Singleton.GameState.GamePlaying;
                    MediaPlayer.Play(_backgroundMusic);
                    break;
                
                case Singleton.GameState.GamePlaying:
                    for (int i = 0; i < _numObject; i++)
                    {
                        if (_gameObjects[i].IsActive) _gameObjects[i].Update(gameTime, _gameObjects);
                    }
                    for (int i = 0; i < _numObject; i++)
                    {
                        if (!_gameObjects[i].IsActive)
                        {
                            _gameObjects.RemoveAt(i);
                            i--;
                            _numObject--;
                        }
                    }
                    if (Singleton.Instance.BallLeft == 0) Singleton.Instance.CurrentGameState = Singleton.GameState.GameWin;

                    break;
                
                case Singleton.GameState.GameOver:
                    if (!Singleton.Instance.CurrentKey.Equals(Singleton.Instance.PreviousKey) && Singleton.Instance.CurrentKey.GetPressedKeys().Length > 0)
                    {
                        //any keys pressed to start
                        Reset();
                        Singleton.Instance.CurrentGameState = Singleton.GameState.StartNewLife;
                    }
                    break;
                
                case Singleton.GameState.GameWin:
                    if (!Singleton.Instance.CurrentKey.Equals(Singleton.Instance.PreviousKey) && Singleton.Instance.CurrentKey.GetPressedKeys().Length > 0)
                    {
                        //any keys pressed to start
                        MediaPlayer.Stop();
                        Reset();
                        Singleton.Instance.CurrentGameState = Singleton.GameState.StartNewLife;
                    }
                    break;
            }

            Singleton.Instance.PreviousKey = Singleton.Instance.CurrentKey;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            //Draw Background
            _spriteBatch.Draw(_background, new Vector2(0, 0), Color.White);

            _numObject = _gameObjects.Count;

            for (int i = 0; i < _numObject; i++)
            {
                _gameObjects[i].Draw(_spriteBatch);
            }

            //Draw GUI
            //draw Scorce
            _spriteBatch.DrawString(_font, "Score :\n" + Singleton.Instance.Score.ToString(), new Vector2(10, 10), Color.Black);
            //draw Ball Left
            _spriteBatch.DrawString(_font, "Eggs\nLeft : " + Singleton.Instance.BallLeft.ToString(), new Vector2(620, 10), Color.Black);
            //draw Next Ball
            _spriteBatch.DrawString(_font, "Next", new Vector2(22, 575), Color.Black);

            //Draw Game Over When GameState is Game Over
            if (Singleton.Instance.CurrentGameState == Singleton.GameState.GameOver)
            {
                Vector2 fontSize = _font.MeasureString("Game Over");
                _spriteBatch.DrawString(_font,
                    "Game Over",
                    new Vector2((Singleton.SCREENWIDTH - fontSize.X) / 2,
                    (Singleton.SCREENHEIGHT - fontSize.Y) / 2),
                    Color.Red);
            }

            //Draw Game Over When GameState is Game Win
            if (Singleton.Instance.CurrentGameState == Singleton.GameState.GameWin)
            {
                Vector2 fontSize = _font.MeasureString("Game Win");
                _spriteBatch.DrawString(_font,
                    "Game Win",
                    new Vector2((Singleton.SCREENWIDTH - fontSize.X) / 2,
                    (Singleton.SCREENHEIGHT - fontSize.Y) / 2),
                    Color.Green);
            }

            _spriteBatch.End();
            _graphics.BeginDraw();

            base.Draw(gameTime);
        }

        protected void Reset()
        {
            //Reset Value to Initialize Value
            Singleton.Instance.Score = 0;
            Singleton.Instance.FireCount = 0;
            Singleton.Instance.Celling = 40;
            Singleton.Instance.CurrentGameState = Singleton.GameState.StartNewLife;

            //Load GameSprite
            Texture2D gameSprite = this.Content.Load<Texture2D>("Sprite");
            int randIndex = new Random().Next(Singleton.Instance.ballColor.Count);

            _gameObjects.Clear();

            //Create Ball Template for cloning
            Ball[] BallShooter = new Ball[Singleton.Instance.ballColor.Count];
            for (int i = 0; i < BallShooter.Length; i++)
            {
                BallShooter[i] = new Ball(gameSprite)
                {
                    Name = "BallShooter",
                    Viewport = Singleton.Instance.ballColor[i],
                    ColorIndex = i,
                    Position = new Vector2(20, 600),
                    IsConnected = false
                };
            }

            //Random 1 Ball to Player for Next Ball
            Ball NextBall = BallShooter[randIndex].Clone() as Ball;

            //Create and add Player to Object
            _gameObjects.Add(new Player(gameSprite)
            {
                Name = "Player",
                Viewport = new Rectangle(30, 112, 70, 70),
                Position = new Vector2(90, 600),
                Left = Keys.Left,
                Right = Keys.Right,
                Fire = Keys.Space,
                BallShooter = BallShooter,
                NextBall = NextBall
            });

            //Add Next Ball
            _gameObjects.Add(NextBall);

            //Reset BallGrid with 8 * 5 Dimention
            ResetBallGrid();

            foreach (GameObject s in _gameObjects)
            {
                s.Reset();
            }
        }

        protected void ResetBallGrid()
        {
            //Load GameSprite
            Texture2D gameSprite = this.Content.Load<Texture2D>("Sprite");
            Singleton.Instance.BallLeft = 0;

            //Create Ball Template for cloning
            Ball[] newBall = new Ball[Singleton.Instance.ballColor.Count];
            for (int i = 0; i < newBall.Length-1; i++)
            {
                newBall[i] = new Ball(gameSprite)
                {
                    Name = "Ball",
                    Viewport = Singleton.Instance.ballColor[i],
                    ColorIndex = i,
                    Velocity = new Vector2(0, 0),
                    IsConnected = false
                };
            }

            //Add Ball to gameObjects with random Index from Template
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int randIndex = new Random().Next(Singleton.Instance.ballColor.Count-1);
                    Ball cloneBall = newBall[randIndex].Clone() as Ball;

                    cloneBall.Position = new Vector2(100 + (Singleton.BALLWIDTH /8 * j + (Singleton.BALLWIDTH/8 - cloneBall.Rectangle.Width) / 2), Singleton.Instance.Celling + (i * 62));
                    _gameObjects.Add(cloneBall);

                    Singleton.Instance.BallLeft++;
                }
            }
        }
    }
}
