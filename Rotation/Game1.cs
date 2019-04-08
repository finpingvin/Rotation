using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CoreMotion;
using Foundation;
using AudioToolbox;
using Microsoft.Xna.Framework.Input.Touch;
using System.Globalization;

namespace Rotation.iOS
{
    public struct Projection
    {
        public float min;
        public float max;

        public Projection(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Overlap(Projection other)
        {
            return this.max >= other.min && other.max >= this.min;
        }
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        CMMotionManager motionManager;
        SpriteFont debugFont;
        Texture2D textureShip;
        Texture2D textureBlock;
        float cameraScale = 2f;
        int blockSize = 32;
        Vector2 shipVelocity;
        Vector2 shipPosition;
        float currentTime = 0;

        enum GameState
        {
            Running,
            Dead,
            Won
        }

        GameState currentState;

        int[,] map = new int[24, 24] {
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            {1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1},
            {1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1},
            {1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1},
            {2, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1},
            {2, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1},
            {1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
        };

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.IsFullScreen = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            motionManager = new CMMotionManager();
            motionManager.DeviceMotionUpdateInterval = 1 / 60;
            motionManager.StartDeviceMotionUpdates();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            debugFont = Content.Load<SpriteFont>("Debug");
            textureShip = Content.Load<Texture2D>("Ship");
            textureBlock = Content.Load<Texture2D>("Block");

            ResetGame();
            shipVelocity = new Vector2(0, 10);

            //TODO: Use Content to load your game content here 
        }

        protected void Crash()
        {
            SystemSound.Vibrate.PlaySystemSound();
            currentState = GameState.Dead;
        }

        protected void Win()
        {
            currentState = GameState.Won;
        }

        protected void ResetGame()
        {
            shipPosition = new Vector2(13 * blockSize, 24 * blockSize);
            currentTime = 0;
            currentState = GameState.Running;
        }

        protected Vector2 Rotate(Vector2 position, Vector2 origin, float angle)
        {
            Matrix transform = Matrix.CreateTranslation(-origin.X, -origin.Y, 0f) *
                               Matrix.CreateRotationZ(angle) *
                               Matrix.CreateTranslation(origin.X, origin.Y, 0f);

            return Vector2.Transform(position, transform);
        }

        protected Vector2 Normal(Vector2 v)
        {
            return new Vector2(-v.Y, v.X);
        }

        protected Projection CreateProjection(Vector2 axis, Vector2[] vertices)
        {
            float min = Vector2.Dot(axis, vertices[0]);
            float max = min;

            for (int i = 1; i < vertices.Length; i++)
            {
                float p = Vector2.Dot(axis, vertices[i]);
                min = Math.Min(min, p);
                max = Math.Max(max, p);
            }

            return new Projection(min, max);
        }

        protected int ShipHasCollided()
        {
            int currentCellX = (int)(shipPosition.X / blockSize);
            int currentCellY = (int)(shipPosition.Y / blockSize);

            int[,] cellsToTest = {
                {currentCellX - 1, currentCellY + 1},
                {currentCellX - 1, currentCellY},
                {currentCellX + 1, currentCellY - 1},
                {currentCellX, currentCellY - 1},
                {currentCellX - 1, currentCellY - 1},
                {currentCellX, currentCellY},
                {currentCellX, currentCellY + 1},
                {currentCellX + 1, currentCellY},
                {currentCellX + 1, currentCellY + 1}
            };

            float shipHalfWidth = textureShip.Width / 2;
            float shipHalfHeight = textureShip.Height / 2;
            float yaw = (float)motionManager.DeviceMotion.Attitude.Yaw;

            Vector2[] shipVertices =
            {
                Rotate(new Vector2(shipPosition.X, shipPosition.Y - shipHalfHeight), shipPosition, -yaw),
                Rotate(new Vector2(shipPosition.X + shipHalfWidth, shipPosition.Y + shipHalfHeight), shipPosition, -yaw),
                Rotate(new Vector2(shipPosition.X - shipHalfWidth, shipPosition.Y + shipHalfHeight), shipPosition, -yaw)
            };

            Vector2[] shipAxes =
            {
                Normal(shipVertices[1] - shipVertices[0]),
                Normal(shipVertices[2] - shipVertices[1]),
                Normal(shipVertices[0] - shipVertices[2])
            };
            
            for (int i = 0; i < cellsToTest.GetLength(0); i++)
            {
                int x = cellsToTest[i, 0];
                int y = cellsToTest[i, 1];

                if (x >= 24 || y >= 24)
                {
                    continue;
                }

                if (map[y, x] == 1 || map[y, x] == 2)
                {
                    int cellWorldX = x * blockSize;
                    int cellWorldY = y * blockSize;

                    // SAT

                    Vector2[] blockVertices =
                    {
                        new Vector2(cellWorldX, cellWorldY),
                        new Vector2(cellWorldX + blockSize, cellWorldY),
                        new Vector2(cellWorldX + blockSize, cellWorldY + blockSize),
                        new Vector2(cellWorldX, cellWorldY + blockSize)
                    };

                    Vector2[] blockAxes =
                    {
                        Normal(blockVertices[1] - blockVertices[0]),
                        Normal(blockVertices[2] - blockVertices[1])
                    };

                    bool foundGap = false;

                    for (int j = 0; j < blockAxes.Length; j++)
                    {
                        Vector2 axis = blockAxes[j];
                        Projection blockProjection = CreateProjection(axis, blockVertices);
                        Projection shipProjection = CreateProjection(axis, shipVertices);

                        if (!blockProjection.Overlap(shipProjection))
                        {
                            foundGap = true;
                            break;
                        }
                    }

                    if (foundGap)
                    {
                        continue;
                    }

                    for (int j = 0; j < shipAxes.Length; j++)
                    {
                        Vector2 axis = shipAxes[j];
                        Projection shipProjection = CreateProjection(axis, shipVertices);
                        Projection blockProjection = CreateProjection(axis, blockVertices);

                        if (!shipProjection.Overlap(blockProjection))
                        {
                            foundGap = true;
                            break;
                        }
                    }

                    if (!foundGap)
                    {
                        return map[y, x];
                    }

                    // AABB
                    /*
                    if (shipPosition.X <= cellWorldX + blockSize &&
                        shipPosition.X + shipSize > cellWorldX &&
                        shipPosition.Y <= cellWorldY + blockSize &&
                        shipPosition.Y + shipSize >= cellWorldY)
                    {
                        return true;
                    }
                    */
                }
            }

            return -1;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (motionManager.DeviceMotion != null)
            {
                if (currentState == GameState.Dead || currentState == GameState.Won)
                {
                    TouchCollection tc = TouchPanel.GetState();
                    foreach (TouchLocation tl in tc)
                    {
                        if (TouchLocationState.Pressed == tl.State)
                        {
                            ResetGame();
                            break;
                        }
                    }
                }
                else
                {
                    float yaw = (float)motionManager.DeviceMotion.Attitude.Yaw;
                    shipVelocity = new Vector2((float)-Math.Sin(yaw), (float)-Math.Cos(yaw)) * 50;
                    shipPosition += shipVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    int collision = ShipHasCollided();

                    if (collision == 1)
                    {
                        Crash();
                    }
                    else if (collision == 2)
                    {
                        Win();
                    }
                    else
                    {
                        currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Blue);

            /*GraphicsDevice.Viewport = new Viewport
            {
                X = 8 * 32 - (150 / 2),
                Y = 10 * 32 - (150 / 2),
                Width = 32 * 24,
                Height = 32 * 24
            };*/

            if (motionManager.DeviceMotion != null)
            {
                float yaw = (float)motionManager.DeviceMotion.Attitude.Yaw;
                Matrix camera = Matrix.CreateTranslation(-shipPosition.X, -shipPosition.Y, 0) *
                                Matrix.CreateRotationZ(yaw) *
                                Matrix.CreateScale(cameraScale) *
                                Matrix.CreateTranslation(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.5f, 0);

                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera);

                for (int y = 0; y < map.GetLength(0); y++)
                {
                    for (int x = 0; x < map.GetLength(1); x++)
                    {
                        if (map[y, x] == 1)
                        {
                            spriteBatch.Draw(textureBlock, new Vector2(x * blockSize, y * blockSize), null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
                        }
                    }
                }

                spriteBatch.Draw(textureShip, shipPosition, null, Color.White, -yaw, new Vector2(textureShip.Width / 2, textureShip.Height / 2), Vector2.One, SpriteEffects.None, 0);

                spriteBatch.End();


                spriteBatch.Begin();

                string timeMsg = currentTime.ToString("F2", CultureInfo.InvariantCulture);
                Vector2 timeMsgSize = debugFont.MeasureString(timeMsg);
                spriteBatch.DrawString(debugFont, timeMsg, new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height - 50), Color.Pink, yaw, timeMsgSize / 2, 1, SpriteEffects.None, 0);

                if (currentState == GameState.Dead)
                {
                    string deadMsg = "Oops, you died. Tap the screen and try again.";
                    Vector2 deadMsgSize = debugFont.MeasureString(deadMsg);
                    spriteBatch.DrawString(debugFont, deadMsg, new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2), Color.Pink, yaw, deadMsgSize / 2, 1, SpriteEffects.None, 0);
                }
                else if (currentState == GameState.Won)
                {
                    string winMsg = "Congratulations, you won!";
                    Vector2 winMsgSize = debugFont.MeasureString(winMsg);
                    spriteBatch.DrawString(debugFont, winMsg, new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2), Color.LimeGreen, yaw, winMsgSize / 2, 1, SpriteEffects.None, 0);
                }

                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}

