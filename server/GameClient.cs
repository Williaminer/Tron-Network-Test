using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ServerExec;
using System;

namespace ClientExec
{
    public class GameClient : Game
    {
        //Login & User Info
        public bool Spawned { get; set; }
        public bool Started { get; set; }
        public string FailMessage { get; set; }
        public string Username { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int OldX { get; set; }
        public int OldY { get; set; }
        public string Direction { get; set; }
        public bool Dead { get; set; }

        //Data Input & Commands
        public string Input { get; set; }
        public KeyboardState OldKstate { get; set; }

        //Networking
        private Client client;

        //Graphics
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont myFont;
        private Texture2D whiteRectangle;
        private Color textColour;

        public GameClient()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            client = new Client();
            client.StartClient("127.0.0.1", 7777);

            Direction = "up";
            Input = "";
            FailMessage = "";
            Spawned = false;
            Started = false;
            Dead = false;
            textColour = Color.White;
            X = _graphics.PreferredBackBufferWidth / 2;
            Y = _graphics.PreferredBackBufferHeight / 2;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            myFont = Content.Load<SpriteFont>("MyFont");

            whiteRectangle = new Texture2D(GraphicsDevice, 1, 1);
            whiteRectangle.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
            client.ReadMessages();

            KeyboardState kstate = Keyboard.GetState();

            if(client.Restarting)
            {
                Dead = false;
                Started = false;
                Spawned = false;
                Direction = "up";
                Input = "";
                FailMessage = "";
                Username = "";
                client.Started = false;
                client.Restarting = false;
            }

            if (Spawned == false)
                HandleLogin(kstate);
            else
            {
                client.SendPositionPacket(Username, X, Y);
                if (Started == false)
                {
                    Started = client.Started;
                }
                else
                {
                    if (Dead == false)
                    {
                        HandleWalking(kstate, time);
                    }
                }
            }

            OldKstate = kstate;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DimGray);

            Vector2 middle = new Vector2(_graphics.PreferredBackBufferWidth / 2,
                _graphics.PreferredBackBufferHeight / 2);

            _spriteBatch.Begin();
            DrawBorders(_spriteBatch);

            for (int i = client.NearbyPlayers.Count - 1; i >= 0; i--)
            {
                NearbyPlayer player = client.NearbyPlayers[i];

                if (player.Username != Username)
                {
                    player.LastReceived--;
                    _spriteBatch.Draw(whiteRectangle, new Rectangle((int)(player.X / 10) * 10,
                        (int)(player.Y / 10) * 10, 10, 10), GetColour(player.Username));
                }
                if (player.LastReceived < 1)
                {
                    client.NearbyPlayers.RemoveAt(i);
                }
            }

            if(client.LastWinner != "" && !Started)
            {
                _spriteBatch.DrawString(myFont, "Round Winner", new Vector2(middle.X -
                    (myFont.MeasureString("Round Winner").X / 2), middle.Y + 60), textColour);
                _spriteBatch.DrawString(myFont, client.LastWinner, new Vector2(middle.X -
                    (myFont.MeasureString(client.LastWinner).X / 2), middle.Y + 80), textColour);
            }

            if (Spawned)
            {
                _spriteBatch.Draw(whiteRectangle, new Rectangle((int)(X / 10) * 10, (int)(Y
                    / 10) * 10, 10, 10), GetColour(Username));

                foreach (LineTile line in client.LineTiles)
                {
                    _spriteBatch.Draw(whiteRectangle, new Rectangle((int)(line.X / 10) * 10,
                            (int)(line.Y / 10) * 10, 10, 10), GetColour(line.Player));
                }

                if (Started == false)
                {
                    Vector2 messageSize = myFont.MeasureString("Waiting for game to start...");
                    _spriteBatch.DrawString(myFont, "Waiting for game to start...", new Vector2(
                        middle.X - (messageSize.X / 2), middle.Y - messageSize.Y - 2), textColour);

                    messageSize = myFont.MeasureString("Your colour is... " + Username);
                    _spriteBatch.DrawString(myFont, "Your colour is... " + Username, new Vector2(
                        middle.X - (messageSize.X / 2), middle.Y + messageSize.Y), textColour);
                }
                else
                    DrawGrid(_spriteBatch);
            }
            else
            {
                Vector2 messageSize = myFont.MeasureString("please input a colour (red, blue, green, or yellow)");
                _spriteBatch.DrawString(myFont, "Please input a username (red, blue, green, or yellow)", new Vector2(
                    middle.X - (messageSize.X / 2), middle.Y - messageSize.Y - 2), textColour);

                messageSize = myFont.MeasureString(FailMessage);
                _spriteBatch.DrawString(myFont, FailMessage, new Vector2(
                    middle.X - (messageSize.X / 2), middle.Y + messageSize.Y * 2 - 2), textColour);

                _spriteBatch.DrawString(myFont, Input, new Vector2(middle.X -
                    (myFont.MeasureString(Input).X / 2), middle.Y), textColour);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        public void HandleWalking(KeyboardState kstate, float time)
        {
            int speed = 60;
            if((int)(OldX / 10) * 10 != (int)(X / 10) * 10)
            {
                client.SendLinePacket(Username, (int)(OldX / 10) * 10,
                    (int)(OldY / 10) * 10);
                OldX = X;
            }
            if ((int)(OldY / 10) * 10 != (int)(Y / 10) * 10)
            {
                client.SendLinePacket(Username, (int)(OldX / 10) * 10,
                    (int)(OldY / 10) * 10);
                OldY = Y;
            }

            if ((kstate.IsKeyDown(Keys.W) || kstate.IsKeyDown(Keys.Up)) && Direction != "down")
                Direction = "up";
            if ((kstate.IsKeyDown(Keys.S) || kstate.IsKeyDown(Keys.Down)) && Direction != "up")
                Direction = "down";
            if ((kstate.IsKeyDown(Keys.A) || kstate.IsKeyDown(Keys.Left)) && Direction != "right")
                Direction = "left";
            if ((kstate.IsKeyDown(Keys.D) || kstate.IsKeyDown(Keys.Right)) && Direction != "left")
                Direction = "right";

            if (Direction == "up")
            {
                Y = (int)(Y - speed * time);
            }
            else if (Direction == "down")
            {
                Y = (int)(Y + speed * time);
            }
            else if (Direction == "left")
            {
                X = (int)(X - speed * time);
            }
            else if (Direction == "right")
            {
                X = (int)(X + speed * time);
            }

            int size = 10;
            int border = 2;
            if (X > _graphics.PreferredBackBufferWidth - whiteRectangle.Width * size - size * border + 8)
            {
                Dead = true;
            }
            else if (X < whiteRectangle.Width + size * border)
            {
                Dead = true;
            }

            if (Y > _graphics.PreferredBackBufferHeight - whiteRectangle.Height * size - size * border + 5)
            {
                Dead = true;
            }
            else if (Y < whiteRectangle.Height + size * border)
            {
                Dead = true;
            }

            foreach(LineTile line in client.LineTiles)
            {
                if (client.LineTiles.Exists(line => line.X == (int)(X / 10) * 10 &&
                    line.Y == (int)(Y / 10) * 10))
                    Dead = true;
            }

            if (Dead)
                client.SendDeadPacket(Username);
        }

        public void HandleLogin(KeyboardState kstate)
        {
            HandleInput(kstate, false);

            if (kstate.IsKeyDown(Keys.Enter) && !OldKstate.IsKeyDown(Keys.Enter))
            {
                if (Input == "RED" || Input == "BLUE" || Input == "GREEN" || Input == "YELLOW")
                {
                    Vector2 middle = new Vector2(_graphics.PreferredBackBufferWidth / 2,
                        _graphics.PreferredBackBufferHeight / 2);

                    if (Input == "RED")
                    {
                        X = (int)(middle.X / 2);
                        Y = (int)(middle.Y / 2);
                    }
                    else if (Input == "BLUE")
                    {
                        X = (int)(middle.X * 1.5);
                        Y = (int)(middle.Y / 2);
                    }
                    else if (Input == "GREEN")
                    {
                        X = (int)(middle.X / 2);
                        Y = (int)(middle.Y * 1.5);
                    }
                    else if (Input == "YELLOW")
                    {
                        X = (int)(middle.X * 1.5);
                        Y = (int)(middle.Y * 1.5);
                    }
                    OldX = X;
                    OldY = Y;

                    client.SendSpawnPacket(Input, X, Y);

                    System.Threading.Thread.Sleep(60);
                    string message = client.ReadMessages();
                    if (message == "spawn")
                    {
                        Username = Input;
                        Spawned = true;
                    }
                    else if (message == "reject")
                    {
                        FailMessage = "Colour was rejected";
                    }
                    else
                    {
                        FailMessage = "No response from Server";
                    }
                }
                else
                {
                    FailMessage = "Invalid colour";
                }

                Input = "";
            }
        }

        public void HandleInput(KeyboardState kstate, bool spaces)
        {
            foreach (Keys key in kstate.GetPressedKeys())
            {
                if (Char.IsLetter((char)key) && !OldKstate.IsKeyDown(key))
                    Input = Input + key.ToString();
                else if (Char.IsDigit((char)key) && !OldKstate.IsKeyDown(key))
                    Input = Input + key.ToString().Substring(1);

                if (spaces!)
                    if (key == Keys.Space && !OldKstate.IsKeyDown(key))
                        Input = Input + " ";
            }

            if (kstate.IsKeyDown(Keys.Escape) && !OldKstate.IsKeyDown(Keys.Escape))
            {
                Input = "";
            }

            if (kstate.IsKeyDown(Keys.Back) && !OldKstate.IsKeyDown(Keys.Back) &&
                Input.Length > 0)
            {
                Input = Input.Substring(0, Input.Length - 1);
            }
        }

        public Color GetColour(string colour)
        {
            if (colour == "RED")
                return Color.Red;
            else if (colour == "BLUE")
                return Color.Blue;
            else if (colour == "YELLOW")
                return Color.Yellow;
            else
                return Color.Green;
        }

        public void DrawBorders(SpriteBatch spritebatch)
        {
            spritebatch.Draw(whiteRectangle, new Rectangle(0, 0,
                20, _graphics.PreferredBackBufferHeight), Color.DarkGray);
            spritebatch.Draw(whiteRectangle, new Rectangle(_graphics.
                PreferredBackBufferWidth - 20, 0, 20, _graphics.PreferredBackBufferHeight),
                Color.DarkGray);
            spritebatch.Draw(whiteRectangle, new Rectangle(20, 0, _graphics.
               PreferredBackBufferWidth - 40, 20), Color.DarkGray);
            spritebatch.Draw(whiteRectangle, new Rectangle(20, _graphics.
                PreferredBackBufferHeight - 20, _graphics.PreferredBackBufferWidth - 40, 20),
                Color.DarkGray);
        }

        public void DrawGrid(SpriteBatch spritebatch)
        {
            for(int i = 20; i < _graphics.PreferredBackBufferHeight - 20; i+= 10)
                spritebatch.Draw(whiteRectangle, new Rectangle(20, i, _graphics.
               PreferredBackBufferWidth - 40, 2), Color.DarkGray);

            for(int i = 20; i < _graphics.PreferredBackBufferWidth - 20; i += 10)
                spritebatch.Draw(whiteRectangle, new Rectangle(i, 20, 2, _graphics.
                    PreferredBackBufferWidth - 40), Color.DarkGray);
        }
    }
}
