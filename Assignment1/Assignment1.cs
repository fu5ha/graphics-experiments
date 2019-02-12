using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Assignment1
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Assignment1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont font;

        Vector2 angle = new Vector2(0, 0);
        float dist = 5;
        Matrix dragRotLock;
        Vector3 offset = new Vector3(0, 0, 0);

        MouseState prevMouse;
        KeyboardState prevKey;

        Vector3 cameraPos = new Vector3(0, 0, -10);
        Vector4 diffuseColor = new Vector4(0.65f, 0.65f, 0.65f, 1);
        Vector4 ambientColor = new Vector4(0.4f, 0.5f, 0.75f, 1);
        float ambientIntensity = 0.25f;
        float specularIntensity = 0.3f;
        float diffuseIntensity = 0.8f;
        float shininess = 20.0f;

        int technique = 0;
        string[] techniques =
        {
            "Gourad",
            "Phong",
            "Blinn",
            "Schlick",
            "Toon",
            "HalfLife"
        };

        Vector3 lightPos = new Vector3(5, 10, 8);
        Vector3 lightColor = new Vector3(1, 1, 1);
        float lightStrength = 200;

        Model torus;
        Model sphere;
        Model bunny;
        Model cube;
        Model teapot;
        Model currModel;

        Matrix view;
        Matrix projection;

        Effect effect;

        bool showHelp = false;
        bool showDebug = false;

        public Assignment1()
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
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Font");

            effect = Content.Load<Effect>("Lighting");
            torus = Content.Load<Model>("Torus");
            cube = Content.Load<Model>("Cube");
            sphere = Content.Load<Model>("Sphere");
            teapot = Content.Load<Model>("Teapot");
            bunny = Content.Load<Model>("Bunny");
            currModel = torus;

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

            KeyboardState currKey = Keyboard.GetState();
            MouseState currMouse = Mouse.GetState();

            if (currMouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Pressed)
            {
                angle.X -= (currMouse.X - prevMouse.X) * 0.01f;
                angle.Y -= (currMouse.Y - prevMouse.Y) * 0.01f;
            }

            if (currMouse.RightButton == ButtonState.Pressed && prevMouse.RightButton == ButtonState.Pressed)
            {
                dist += (prevMouse.X - currMouse.X) * 0.01f;
            }

            if (currMouse.MiddleButton == ButtonState.Pressed && prevMouse.MiddleButton == ButtonState.Released)
            {
                dragRotLock = Matrix.CreateRotationX(angle.Y) * Matrix.CreateRotationY(angle.X);
            }

            if (currMouse.MiddleButton == ButtonState.Pressed && prevMouse.MiddleButton == ButtonState.Pressed)
            {
                offset += Vector3.Transform(
                    new Vector3(
                        (prevMouse.X - currMouse.X) * 0.01f,
                        (prevMouse.Y - currMouse.Y) * -0.01f,
                        0),
                    dragRotLock
                );
            }

            cameraPos = offset + Vector3.Transform(
                    new Vector3(0, 0, dist),
                    Matrix.CreateRotationX(angle.Y) * Matrix.CreateRotationY(angle.X)
                );

            view = Matrix.CreateLookAt(
                cameraPos,
                offset,
                new Vector3(0, 1, 0)
            );


            prevMouse = currMouse;

            // CHANGE MODEL
            if (Keyboard.GetState().IsKeyDown(Keys.D1))
            {
                currModel = cube;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D2))
            {
                currModel = sphere;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D3))
            {
                currModel = torus;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D4))
            {
                currModel = teapot;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D5))
            {
                currModel = bunny;
            }

            // CHANGE SHADER
            if (Keyboard.GetState().IsKeyDown(Keys.F1))
            {
                technique = 0;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F2))
            {
                technique = 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F3))
            {
                technique = 2;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F4))
            {
                technique = 3;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F5))
            {
                technique = 4;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F6))
            {
                technique = 5;
            }

            // LIGHT CONTROLS
            if (Keyboard.GetState().IsKeyDown(Keys.L))
            {
                float decrease = 1;
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                {
                    decrease = -1;
                }
                lightStrength += 2 * decrease;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                float decrease = 1;
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                {
                    decrease = -1;
                }
                lightColor.X += 0.01f * decrease;
                if (lightColor.X < 0)
                {
                    lightColor.X = 0;
                }
                if (lightColor.X > 1)
                {
                    lightColor.X = 1;
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.G))
            {
                float decrease = 1;
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                {
                    decrease = -1;
                }
                lightColor.Y += 0.01f * decrease;
                if (lightColor.Y < 0)
                {
                    lightColor.Y = 0;
                }
                if (lightColor.Y > 1)
                {
                    lightColor.Y = 1;
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.B))
            {
                float decrease = 1;
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                {
                    decrease = -1;
                }
                lightColor.Z += 0.01f * decrease;
                if (lightColor.Z < 0)
                {
                    lightColor.Z = 0;
                }
                if (lightColor.Z > 1)
                {
                    lightColor.Z = 1;
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                lightPos.X -= 0.5f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                lightPos.X += 0.5f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                lightPos.Y -= 0.5f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                lightPos.Y += 0.5f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                lightPos.Z -= 0.5f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                lightPos.Z += 0.5f;
            }

            // MATERIAL CONTROLS
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                {
                    specularIntensity -= 0.01f;
                }
                else
                {
                    shininess -= 0.5f;
                }
                
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                {
                    specularIntensity += 0.01f;
                }
                else
                {
                    shininess += 0.5f;
                }

            }

            if (shininess < 0)
            {
                shininess = 0;
            }
            if (specularIntensity < 0)
            {
                specularIntensity = 0;
            }

            // Overlay Controls

            if (currKey.IsKeyDown(Keys.OemQuestion) && !prevKey.IsKeyDown(Keys.OemQuestion))
            {
                showHelp = !showHelp;
            }
            if (currKey.IsKeyDown(Keys.H) && !prevKey.IsKeyDown(Keys.H))
            {
                showDebug = !showDebug;
            }

            if (currKey.IsKeyDown(Keys.S))
            {
                lightPos = new Vector3(5, 10, 8);
                lightColor = new Vector3(1, 1, 1);
                lightStrength = 200;
                dist = 5;
                angle = new Vector2(0, 0);
                offset = new Vector3(0, 0, 0);
            }

            prevKey = currKey;

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


            effect.CurrentTechnique = effect.Techniques[technique];
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                foreach (ModelMesh mesh in currModel.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;
                        Matrix model = mesh.ParentBone.Transform;
                        if (currModel == torus)
                        {
                            model = Matrix.CreateScale(0.25f) * mesh.ParentBone.Transform;
                        }
                        effect.Parameters["Model"].SetValue(model);
                        Matrix modelInverseTranspose =
                            Matrix.Transpose(Matrix.Invert(model));
                        effect.Parameters["ModelInverseTranspose"].SetValue(modelInverseTranspose);
                        effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
                        effect.Parameters["AmbientColor"].SetValue(ambientColor);
                        effect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);
                        effect.Parameters["DiffuseIntensity"].SetValue(diffuseIntensity);
                        effect.Parameters["SpecularIntensity"].SetValue(specularIntensity);
                        effect.Parameters["Shininess"].SetValue(shininess);
                        effect.Parameters["LightPosition"].SetValue(lightPos);
                        effect.Parameters["LightStrength"].SetValue(lightStrength);
                        effect.Parameters["LightColor"].SetValue(lightColor);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["CameraPosition"].SetValue(cameraPos);

                        GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            part.VertexOffset,
                            part.StartIndex,
                            part.PrimitiveCount);

                    }
                }
            }
            spriteBatch.Begin();
            if (showHelp)
            {
                spriteBatch.DrawString(
                    font,
                    "Rotate Camera: Left Mouse; Change Camera Distance: Right Mouse; Translate Camera: Middle Mouse",
                    new Vector2(10, 360),
                    Color.White);
                spriteBatch.DrawString(
                    font,
                    "Move Light: Arrow Keys; Change Camera Distance: Right Mouse; Translate Camera: Middle Mouse",
                    new Vector2(10, 380),
                    Color.White);
                spriteBatch.DrawString(
                    font,
                    "1-5 keys: Change Object; F1-F6 keys: Change Lighting Model",
                    new Vector2(10, 400),
                    Color.White);
                spriteBatch.DrawString(
                    font,
                    "L,R,G,B: Change Light Intensity/Color (Shift to Decrease)",
                    new Vector2(10, 420),
                    Color.White);
                spriteBatch.DrawString(
                    font,
                    "+/-: Change Specular Intensity; L-Ctrl and +/-: Change Shininess",
                    new Vector2(10, 440),
                    Color.White);
                spriteBatch.DrawString(
                    font,
                    "?: Toggle Help; H: Toggle Debug Info; Reset Camera/Light: S",
                    new Vector2(10, 460),
                    Color.White);

            }

            if (showDebug)
            {
                spriteBatch.DrawString(
                    font,
                    "Camera Angle: " + angle + "; Dist: " + dist + "; Offset: " + offset,
                    new Vector2(10, 10),
                    Color.White);
                spriteBatch.DrawString(
                    font,
                    "Light Pos: " + lightPos + "; Intensity: " + lightStrength + "; Color: (R: " + lightColor.X + ", G: " + lightColor.Y + ", B: " + lightColor.Z + ")",
                    new Vector2(10, 30),
                    Color.White);
                spriteBatch.DrawString(
                    font,
                    "Matrial Specular Intensity: " + specularIntensity + "; Shininess: " + shininess,
                    new Vector2(10, 50),
                    Color.White);
                spriteBatch.DrawString(
                    font,
                    "Shader Type: " + techniques[technique],
                    new Vector2(10, 70),
                    Color.White);
            }

            spriteBatch.End();  

            base.Draw(gameTime);
        }
    }
}
