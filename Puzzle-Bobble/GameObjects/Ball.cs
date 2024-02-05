using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Puzzle_Bobble
{
    class Ball : GameObject
    {
        public float DistanceMoved;

        //Boolean for check if ball connected --> Initialize to false
        public Boolean IsConnected;

        public Ball(Texture2D texture) : base(texture)
        {

        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Position, Viewport, Color.White);
            base.Draw(spriteBatch);
        }

        public override void Reset()
        {
            IsConnected = false;
            DistanceMoved = 0;
            base.Reset();
        }

        //find ball that same color of BallShooter then return List of them
        public List<Ball> findSameColor(List<GameObject> gameObjects, Ball current)
        {
            List<Ball> sameColor = new List<Ball>();
            foreach (GameObject s in gameObjects)
            {
                if (s.Name.Equals("Ball"))
                {
                    if (!sameColor.Contains(s) && (current.ColorIndex == s.ColorIndex))
                    {
                        sameColor.Add((Ball)(s));
                    }
                }
                
            }
            sameColor.Insert(0, current);
            return sameColor;
        }

        //check Ball is Neighbor of consider Ball then return Boolean
        public Boolean IsNeighbor(Ball current, Ball s)
        {
            //Ball is Top Connect
            if (s.Position.Y == (current.Position.Y - 62) && s.Position.X == current.Position.X)
            {
                return true;
            }
            //Ball is Down Connect
            else if (s.Position.Y == (current.Position.Y + 62) && s.Position.X == current.Position.X)
            {
                return true;
            }
            //Ball is Left Connect
            else if (s.Position.X == (current.Position.X - 62) && s.Position.Y == current.Position.Y)
            {
                return true;
            }
            //Ball is Right Connect
            else if (s.Position.X == (current.Position.X + 62) && s.Position.Y == current.Position.Y)
            {
                return true;
            }
            else return false;
        }

        //find if ball is connected and same color then return List of them
        public List<Ball> findConneted(List<Ball> sameColor)
        {
            List<Ball> connected = new List<Ball>();
            
            for (int i = 0; i < sameColor.Count; i++)
            {
                Ball s = sameColor[i];
                for (int j = 0; j < sameColor.Count; j++)
                {
                    if (s.IsConnected == true)
                    {
                        if (IsNeighbor(s, sameColor[j]))
                        {
                            sameColor[j].IsConnected = true;
                        }
                    }
                }     
            }

            foreach (var item in sameColor)
            {
                if (item.IsConnected == true)
                {
                    connected.Add(item);
                    item.IsConnected = false;
                }
            }
            return connected;
        }

        public override void Update(GameTime gameTime, List<GameObject> gameObjects)
        {
            //Update by type of Ball
            switch (Name)
            {
                case "BallShooter":
                    DistanceMoved += Math.Abs(Velocity.Y * gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond);
                    Position = Position + Velocity * gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;

                    //if Hit same color and connected 3 or more
                    foreach (GameObject s in gameObjects)
                    {
                        if (Name.Equals("BallShooter"))
                        {
                            if ((IsTouching(s) && s.Name.Equals("Ball")))
                            {
                                Name = "Ball";
                                Position = new Vector2(s.Position.X, s.Position.Y + 62);

                                // if Transparent Ball Change Color to hit Ball
                                if (ColorIndex == 5)
                                {
                                    int index = s.ColorIndex;
                                    ColorIndex = index;
                                    Viewport = Singleton.Instance.ballColor[index];
                                }

                                if (Position.Y >= (600 - 70))
                                {
                                    Singleton.Instance.CurrentGameState = Singleton.GameState.GameOver;
                                }
                                IsConnected = true;

                                List<Ball> connected = findConneted(findSameColor(gameObjects, this));

                                int connectedCount = connected.Count;
                                if (connectedCount >= 3)
                                {
                                    for (int i = 0; i  < connectedCount; i++)
                                    {
                                        connected[i].Name = "BallDropping";
                                    }
                                    Singleton.Instance.BallLeft -= connectedCount;
                                    Singleton.Instance.Score += connectedCount * 100;
                                }
                            }
                            Reset();
                        }
                    }
                    
                    //if Hit Celling Change BallShooter to Ball
                    if (Position.Y <= Singleton.Instance.Celling)
                    {
                        Name = "Ball";
                        Position = new Vector2(100 + (Singleton.BALLWIDTH / 8 * (int)((Position.X-90)/62.5) + (Singleton.BALLWIDTH / 8 - Rectangle.Width) / 2), Singleton.Instance.Celling);

                        // if Transparent Ball Random new Color
                        if (ColorIndex == 5)
                        {
                            int randIndex = new Random().Next(Singleton.Instance.ballColor.Count - 1);
                            ColorIndex = randIndex;
                            Viewport = Singleton.Instance.ballColor[randIndex];
                        }

                        if (Position.Y >= (600 - 70))
                        {
                            Singleton.Instance.CurrentGameState = Singleton.GameState.GameOver;
                        }
                        IsConnected = true;

                        List<Ball> connected = findConneted(findSameColor(gameObjects, this));

                        int connectedCount = connected.Count;
                        if (connectedCount >= 3)
                        {
                            for (int i = 0; i < connectedCount; i++)
                            {
                                connected[i].Name = "BallDropping";
                            }
                            Singleton.Instance.BallLeft -= connectedCount;
                            Singleton.Instance.Score += connectedCount * 100;
                        }
                        Reset();
                    }

                    //if Ball Position is more Screen height Delete This Ball
                    if (DistanceMoved >= Singleton.SCREENHEIGHT) IsActive = false;
                    break;
                
                case "BallDropping":
                    Velocity.Y = 200f;
                    Position = Position + Velocity * gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;
                    if (Position.Y >= Singleton.SCREENHEIGHT) IsActive = false;
                    break;

                case "Ball":
                    //Check if Ball dont have ball on Top then change to BallDropping
                    foreach (GameObject s in gameObjects)
                    {
                        if (s.Name.Equals("Ball") && (s.Position.Y > Singleton.Instance.Celling))
                        {
                            s.Name = "BallDropping";
                            for (int i = 0; i < gameObjects.Count; i++)
                            {
                                //Ball is Top Connect
                                if (gameObjects[i].Name.Equals("Ball") && gameObjects[i].Position.Y == (s.Position.Y - 62) && gameObjects[i].Position.X == s.Position.X)
                                {
                                    s.Name = "Ball";
                                    break;
                                }
                            }
                            if (s.Name.Equals("BallDropping"))
                            {
                                Singleton.Instance.BallLeft--;
                                Singleton.Instance.Score += 100;
                            }
                        }
                    }
                    break;
            }

            base.Update(gameTime, gameObjects);
        }
    }
}
