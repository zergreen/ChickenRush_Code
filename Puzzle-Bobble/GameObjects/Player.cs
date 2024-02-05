using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Puzzle_Bobble
{
    class Player : GameObject
    {
        //Template of Ball for cloning
        public Ball[] BallShooter;
        //Next Ball
        public Ball NextBall;
        public Keys Left, Right, Fire;

        public Player(Texture2D texture) : base(texture)
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Position, Viewport, Color.White);
            base.Draw(spriteBatch);
        }

        public override void Reset()
        {
            Position = new Vector2(90, 600);
            base.Reset();
        }

        public override void Update(GameTime gameTime, List<GameObject> gameObjects)
        {
            if (Singleton.Instance.CurrentKey.IsKeyDown(Left))
            {
                Velocity.X = -500;
            }
            if (Singleton.Instance.CurrentKey.IsKeyDown(Right))
            {
                Velocity.X = 500;
            }
            if (Singleton.Instance.CurrentKey.IsKeyDown(Fire) && Singleton.Instance.CurrentKey != Singleton.Instance.PreviousKey)
            {
                // Play Click sound
                Singleton.Instance.clickSoundInstance.Play();

                //Count Fire and increase Ball
                Singleton.Instance.FireCount++;
                if (Singleton.Instance.FireCount > 10) Singleton.Instance.FireCount = 1;

                Singleton.Instance.BallLeft++;

                //clone BallShooter to newBall
                int randIndex = new Random().Next(Singleton.Instance.ballColor.Count);
                var newBall = BallShooter[randIndex].Clone() as Ball;
                newBall.Reset();
                gameObjects.Add(newBall);

                //currentBall
                NextBall.Position = new Vector2(Rectangle.Width / 2 + Position.X - newBall.Rectangle.Width / 2, Position.Y);
                NextBall.Velocity = new Vector2(0, -400f);
                //NextBall.CurrentBallState = Ball.BallState.Shooting;
                NextBall.Reset();

                //add New Ball to player
                NextBall = newBall;

                // Celling dropdown when 10 Fire Pass (Move every Ball 62 Pixels)
                if (Singleton.Instance.FireCount == 10)
                {
                    Singleton.Instance.Celling += 62;
                    if (Singleton.Instance.Celling >= (600 - 70))
                    {
                        Singleton.Instance.CurrentGameState = Singleton.GameState.GameOver;
                    }
                    foreach (GameObject s in gameObjects)
                    {
                        if (s.Name.Equals("Ball"))
                        {
                            float newY = s.Position.Y;
                            newY += 62;
                            if (newY >= (600 - 70))
                            {
                                Singleton.Instance.CurrentGameState = Singleton.GameState.GameOver;
                            }
                            s.Position = new Vector2(s.Position.X, newY);
                        }
                    }
                }
            }

            float newX = Position.X + Velocity.X * gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;
            newX = MathHelper.Clamp(newX, 100, (Singleton.SCREENWIDTH - 100) - Rectangle.Width);

            Position = new Vector2(newX, Position.Y);

            Velocity = Vector2.Zero;

            base.Update(gameTime, gameObjects);
        }
    }
}
