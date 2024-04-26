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
        private Vector2 ballPosition, ballOrigin, ballDestination, ballStart, ballVelocity, ballPrevPosition;
        private const float ballGravity = 1f;
        private const float ballBounce = 0.66f;
        private const float ballFriction = 0.66f;
        private float ballTerminalVelocity = 18;
        private float ballRadius;
        private const float ballFling = 64;
        private const float updateTime = (float)1 / 30;
        private float timeElapsed;
        private Vector2 windowSize;
        private const int travelTotal = 15;
        private int travelCount;
        private enum BallStates { Physics, Drag, Travel }
        private BallStates ballState = BallStates.Physics;
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
            
            // Store the mouse position as a vector, since they're more convenient to work with.
            var mousePosition = new Vector2(mouseState.X, mouseState.Y);            
            
            // Conditions to enable dragging the ball around.
            if (ballState != BallStates.Drag &&
                mouseState.LeftButton == ButtonState.Pressed &&
                (mousePosition != ballPosition ? Vector2.Distance(mousePosition, ballPosition) : 0) <= ballRadius)
            {
                ballState = BallStates.Drag;
                selectSound.Play();
            }

            // Conditions to make the ball travel to the specified point.
            if (ballState != BallStates.Travel && 
                mouseState.RightButton == ButtonState.Pressed)
            {
                ballState = BallStates.Travel;
                ballDestination = mousePosition;
                ballStart = ballPosition;
                travelCount = travelTotal;                
                selectSound.Play();
            }

            // Conditions to return the ball back to its normal physics state.
            if ((ballState == BallStates.Travel && mouseState.RightButton != ButtonState.Pressed) ||
                (ballState == BallStates.Drag && mouseState.LeftButton != ButtonState.Pressed))
            {
                if (ballPosition != ballPrevPosition)
                    ballVelocity = ballFling * (ballPosition - ballPrevPosition);                    
                ballState = BallStates.Physics;
            }

            // Operations that are dependent on time between updates occur in the following blob.
            timeElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (timeElapsed > updateTime)
            {
                timeElapsed -= updateTime;

                // When physics is enabled, update both velocity and position.
                if (ballState == BallStates.Physics)
                {
                    ballVelocity += new Vector2(0, ballGravity);
                    if (ballVelocity != Vector2.Zero)
                    {
                        var trueMagnitude = ballVelocity.Length();
                        if (trueMagnitude > ballTerminalVelocity)
                            ballVelocity = (ballVelocity / trueMagnitude) * ballTerminalVelocity;
                    }
                    ballPosition += ballVelocity;
                }

                // When the ball is getting moved by the user, always remember previous position
                // to calculate the velocity later.
                if (ballState == BallStates.Drag || ballState == BallStates.Travel)
                    ballPrevPosition = ballPosition;

                // When the ball is getting dragged, just set the ball's position to that of the mouse.
                if (ballState == BallStates.Drag)                                    
                    ballPosition = mousePosition;                

                // When traveling, simply perform interpolation between the destination and start points.
                if (ballState == BallStates.Travel)
                {
                    var amount = (float)travelCount / travelTotal;
                    ballPosition.X = MathHelper.SmoothStep(ballDestination.X, ballStart.X, amount);
                    ballPosition.Y = MathHelper.SmoothStep(ballDestination.Y, ballStart.Y, amount);

                    if (travelCount > 0)
                        travelCount--;
                }

                // Prevent the ball from falling out of the screen.
                // Update ball velocity such that it bounces at the screen's borders.
                // Friction is also applied to limit the ball's movement.
                // This block of code must be last to ensure the draw step
                // doesn't accidentally draw the ball glitching out of the screen.
                if (ballPosition.Y + ballRadius > windowSize.Y)
                {
                    ballPosition.Y = windowSize.Y - ballRadius;
                    ballVelocity.Y *= -ballBounce;
                    ballVelocity.X *= ballFriction;
                }
                if (ballPosition.Y - ballRadius < 0)
                {
                    ballPosition.Y = ballRadius;
                    ballVelocity.Y *= -ballBounce;
                    ballVelocity.X *= ballFriction;
                }
                if (ballPosition.X + ballRadius > windowSize.X)
                {
                    ballPosition.X = windowSize.X - ballRadius;
                    ballVelocity.X *= -ballBounce;
                    ballVelocity.Y *= ballFriction;
                }
                if (ballPosition.X - ballRadius < 0)
                {
                    ballPosition.X = ballRadius;
                    ballVelocity.X *= -ballBounce;
                    ballVelocity.Y *= ballFriction;
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
