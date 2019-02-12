using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Lab03
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Lab03 : Game
    {
        GraphicsDeviceManager graphics;

        Vector2 angle = new Vector2(0, 0);

        MouseState prevMouse;

        Vector3 camerapos = new Vector3(0, 0, 10);
        Vector4 diffuseColor = new Vector4(0.65f, 0.95f, 0.45f, 1);
        Vector4 ambientColor = new Vector4(0.4f, 0.5f, 0.75f, 1);
        float ambientIntensity = 0.2f;
        float specularIntensity = 0.3f;
        float diffuseIntensity = 0.8f;

        Vector3 lightDirection = new Vector3(1, 1, 1);

        Model bunny;

        Matrix view;
        Matrix projection;

        Effect effect;

        public Lab03()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.GraphicsProfile = GraphicsProfile.HiDef;
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

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            effect = Content.Load<Effect>("Lighting");
            bunny = Content.Load<Model>("bunny");

            projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.ToRadians(90),
                    GraphicsDevice.Viewport.AspectRatio,
                    0.1f, 100);

            effect.Parameters["Projection"].SetValue(projection);


            prevMouse = Mouse.GetState();


            // TODO: use this.Content to load your game content here
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


            MouseState currMouse = Mouse.GetState();
            
            if (currMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Pressed)
            {
                angle.X -= (currMouse.X - prevMouse.X) * 0.01f;
                angle.Y -= (currMouse.Y - prevMouse.Y) * 0.01f;
            }

            camerapos = Vector3.Transform(
                    new Vector3(0, 0, 20),
                    Matrix.CreateRotationX(angle.Y) * Matrix.CreateRotationY(angle.X)
                );

            view = Matrix.CreateLookAt(
                camerapos,
                new Vector3(0,2,0),
                new Vector3(0, 1, 0)
            );


            prevMouse = currMouse;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.FromNonPremultiplied(ambientColor));
            GraphicsDevice.BlendState = BlendState.AlphaBlend;


            effect.CurrentTechnique = effect.Techniques[0];
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (ModelMesh mesh in bunny.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;

                        effect.Parameters["Model"].SetValue(mesh.ParentBone.Transform);
                        Matrix worldInverseTranspose =
                            Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);
                        effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
                        effect.Parameters["AmbientColor"].SetValue(ambientColor);
                        effect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);
                        effect.Parameters["DiffuseIntensity"].SetValue(diffuseIntensity);
                        effect.Parameters["SpecularIntensity"].SetValue(specularIntensity);
                        effect.Parameters["LightDirection"].SetValue(lightDirection);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["camerapos"].SetValue(camerapos);

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
