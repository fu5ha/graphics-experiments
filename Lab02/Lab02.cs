using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab02
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Lab02 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        VertexPositionTexture[] vertices =
        {
            new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(-1, 0, 0), new Vector2(0.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1.0f, 1.0f)),
        };

        Effect effect;

        Matrix view = Matrix.CreateLookAt(
            new Vector3(1, 1, 1),
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0)
        );

        Matrix projection;

        Matrix model = Matrix.Identity;

        public Lab02()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        double angle = 0;
        float dist = 2;
        Vector3 tripos = new Vector3(0,0,0);

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rs;
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            effect = Content.Load<Effect>("SimplestShader");
            effect.Parameters["MyTexture"].SetValue(Content.Load<Texture2D>("logo_mg"));
            effect.Parameters["Model"].SetValue(model);
            effect.Parameters["View"].SetValue(view);
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90),
                GraphicsDevice.Viewport.AspectRatio,
                0.1f, 100
            );
            effect.Parameters["Projection"].SetValue(projection);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);

            if(Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                angle -= 0.02;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                angle += 0.02;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                dist -= 0.02f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                dist += 0.02f;
            }


            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                tripos.Y -= 0.02f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                tripos.Y += 0.02f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                tripos.X -= 0.02f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                tripos.X += 0.02f;
            }

            model = Matrix.CreateTranslation(tripos);
            effect.Parameters["Model"].SetValue(model);

            Vector3 cameraPos = new Vector3(dist * (float)System.Math.Sin(angle), 1, dist * (float)System.Math.Cos(angle));
            view = Matrix.CreateLookAt(
                cameraPos,
                tripos + new Vector3(0, 0.5f, 0),
                new Vector3(0, 1, 0)
            );
            effect.Parameters["camerapos"].SetValue(cameraPos);
            effect.Parameters["View"].SetValue(view);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
            }

            base.Draw(gameTime);
        }
    }
}
