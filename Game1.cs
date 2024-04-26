using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;


namespace SimpleGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D ballTexture;
        private SoundEffectInstance selectSound;
        private Vector2 ballPosition, ballOrigin, ballDestination, ballStart;
        private float ballRadius;
        private const float updateTime = (float)1 / 30;
        private float timeElapsed;
        private Vector2 windowSize;
        private const int travelTotal = 15;
        private int travelCount;
        private enum BallStates { Normal, Selected, Travel }
        private BallStates ballState = BallStates.Normal;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create the sprite batch for submitting textures to the graphics device.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the assets with the content manager.
            ballTexture = Content.Load<Texture2D>("textures/ball_0");
            selectSound = Content.Load<SoundEffect>("sounds/select_0").CreateInstance();

            // Check to make sure our assumptions aren't wrong.
            Debug.Assert(ballTexture.Bounds.Width == ballTexture.Bounds.Height);

            // Initialize the properties that will be used in the simple game.
            windowSize = new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            ballRadius = (float)ballTexture.Bounds.Width / 2;            
            ballOrigin = new Vector2(ballRadius, ballRadius);
            ballPosition = new Vector2((float)windowSize.X / 2, (float)windowSize.Y / 2);
        }

        protected override void Update(GameTime gameTime)
        {
            // Get the input states.
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            // Close the application if the escape key is pressed.
            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();
            
            // Determine mouse position and distance to ball.
            var mousePosition = new Vector2(mouseState.X, mouseState.Y);            

            if (ballState != BallStates.Selected &&
                mouseState.LeftButton == ButtonState.Pressed &&
                (mousePosition != ballPosition ? Vector2.Distance(mousePosition, ballPosition) : 0) <= ballRadius)
            {
                ballState = BallStates.Selected;
                selectSound.Play();
            }

            if (ballState == BallStates.Selected)
            {
                ballPosition = mousePosition;
            }

            if (ballState != BallStates.Travel && 
                mouseState.RightButton == ButtonState.Pressed)
            {
                ballState = BallStates.Travel;
                ballDestination = mousePosition;
                ballStart = ballPosition;
                travelCount = travelTotal;                
                selectSound.Play();
            }

            if ((ballState == BallStates.Travel && mouseState.RightButton != ButtonState.Pressed && travelCount == 0) ||
                (ballState == BallStates.Selected && mouseState.LeftButton != ButtonState.Pressed))
            {
                ballState = BallStates.Normal;
            }

            timeElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (timeElapsed > updateTime)
            {
                timeElapsed -= updateTime;

                if (ballState == BallStates.Travel)
                {
                    var amount = (float)travelCount / travelTotal;
                    ballPosition.X = MathHelper.SmoothStep(ballDestination.X, ballStart.X, amount);
                    ballPosition.Y = MathHelper.SmoothStep(ballDestination.Y, ballStart.Y, amount);

                    if (travelCount > 0)
                        travelCount--;
                }            
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            _spriteBatch.Draw(
                texture: ballTexture,
                position: ballPosition,
                sourceRectangle: null,
                color: Color.White,
                rotation: 0,
                origin: ballOrigin,
                scale: 1,
                effects: SpriteEffects.None,
                layerDepth: 0);
            _spriteBatch.End();            
            base.Draw(gameTime);
        }
    }
}
