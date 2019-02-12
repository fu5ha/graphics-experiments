using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SimpleEngine;

namespace Lab05
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Lab05 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Skybox skybox;
        string[] skyboxTextures =
        {
            "skyboximages\\SunsetPNG1",
            "skyboximages\\SunsetPNG2",
            "skyboximages\\SunsetPNG3",
            "skyboximages\\SunsetPNG4",
            "skyboximages\\SunsetPNG5",
            "skyboximages\\SunsetPNG6",
        };

        Matrix view;
        Matrix projection;

        Vector2 angle = new Vector2(0, 0);

        public Lab05()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void Initialize()
        {

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            skybox = new Skybox(skyboxTextures, Content, GraphicsDevice);
            projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.ToRadians(90),
                    GraphicsDevice.Viewport.AspectRatio,
                    0.1f, 100);
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                angle.Y -= 0.075f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                angle.Y += 0.075f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                angle.X += 0.075f;
                if (angle.X > 3.14159f / 2)
                {
                    angle.X = 3.14159f / 2;
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                angle.X -= 0.075f;
                if (angle.X < -3.14159f / 2)
                {
                    angle.X = -3.14159f / 2;
                }
            }

            view = Matrix.CreateRotationY(angle.Y) * Matrix.CreateRotationX(-angle.X);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            skybox.Draw(view, projection);

            base.Draw(gameTime);
        }
    }
}
