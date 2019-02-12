using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SimpleEngine;

namespace Lab06
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Lab06 : Game
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
        Vector3 cameraPos = new Vector3(0, 0, 0);

        Effect effect;

        Model bunny;

        int technique = 0;

        public Lab06()
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

            effect = Content.Load<Effect>("ReflectRefract");
            bunny = Content.Load<Model>("Bunny");

            skybox = new Skybox(skyboxTextures, Content, GraphicsDevice);
            projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.ToRadians(90),
                    GraphicsDevice.Viewport.AspectRatio,
                    0.1f, 1000);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["SkyboxTexture"].SetValue(skybox.skyboxTexture);
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

            if (Keyboard.GetState().IsKeyDown(Keys.D1)) technique = 0;
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) technique = 1;
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) technique = 2;

            Matrix rot = Matrix.CreateRotationX(angle.X) * Matrix.CreateRotationY(angle.Y);

            cameraPos = Vector3.Transform(new Vector3(0,0,-10), rot) + new Vector3(0,3,0);
            view = Matrix.CreateLookAt(
                cameraPos,
                new Vector3(0, 3, 0),
                new Vector3(0, 1, 0)
            );

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            skybox.Draw(view, projection);

            effect.CurrentTechnique = effect.Techniques[technique];
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (ModelMesh mesh in bunny.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;
                        Matrix model = Matrix.CreateScale(0.9f) * mesh.ParentBone.Transform;
                        effect.Parameters["Model"].SetValue(model);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["CameraPosition"].SetValue(cameraPos);
                        effect.Parameters["iorRatio"].SetValue(1.0003f / 1.05f);

                        GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            part.VertexOffset,
                            part.StartIndex,
                            part.PrimitiveCount);

                    }
                }
            }

            base.Draw(gameTime);
        }
    }
}
