using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using SimpleEngine;

namespace Assignment2
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Assignment2 : Game
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

        float reflectivity = 1.0f;
        float fresPower = 5.0f;
        float fresScale = 0.96f;
        float fresBias = 0.04f;
        float textureMixFactor = 0.0f;
        float etaRatio = 0.93f;
        Vector3 dispersionEtaRatio = new Vector3(0.96f, 0.95f, 0.93f);

        Effect effect;
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
        string[] techniques2 =
        {
            "Reflection",
            "Refraction",
            "Dispersion",
            "Fresnel + Dispersion"
        };

        Vector3 lightPos = new Vector3(5, 10, 8);
        Vector3 lightColor = new Vector3(1, 1, 1);
        float lightStrength = 200;

        Model torus;
        Model sphere;
        Model bunny;
        Model cube;
        Model teapot;
        Model helicopter;
        Model currModel;

        Texture2D helicopterTex;

        Skybox skybox;
        Skybox office;
        Skybox daytime;
        Skybox debug;
        Skybox forest;

        string[] officeTextures =
        {
            "skyboximages\\nvlobby_new_posx",
            "skyboximages\\nvlobby_new_negx",
            "skyboximages\\nvlobby_new_posy",
            "skyboximages\\nvlobby_new_negy",
            "skyboximages\\nvlobby_new_posz",
            "skyboximages\\nvlobby_new_negz",
        };

        string[] daytimeTextures =
        {
            "skyboximages\\grandcanyon_posx",
            "skyboximages\\grandcanyon_negx",
            "skyboximages\\grandcanyon_posy",
            "skyboximages\\grandcanyon_negy",
            "skyboximages\\grandcanyon_posz",
            "skyboximages\\grandcanyon_negz",
        };

        string[] debugTextures =
        {
            "skyboximages\\debug_posx",
            "skyboximages\\debug_negx",
            "skyboximages\\debug_posy",
            "skyboximages\\debug_negy",
            "skyboximages\\debug_posz",
            "skyboximages\\debug_negz",
        };

        string[] forestTextures =
        {
            "skyboximages\\posx",
            "skyboximages\\negx",
            "skyboximages\\posy",
            "skyboximages\\negy",
            "skyboximages\\posz",
            "skyboximages\\negz",
        };

        Matrix view;
        Matrix projection;

        Effect basicEffect;
        Effect refrafEffect;

        bool showHelp = false;
        bool showDebug = false;

        public Assignment2()
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

            helicopterTex = Content.Load<Texture2D>("HelicopterTexture");

            basicEffect = Content.Load<Effect>("Lighting");
            refrafEffect = Content.Load<Effect>("ReflectRefract");
            effect = basicEffect;
            torus = Content.Load<Model>("Torus");
            cube = Content.Load<Model>("RealCube");
            sphere = Content.Load<Model>("Sphere");
            teapot = Content.Load<Model>("Teapot");
            bunny = Content.Load<Model>("Bunny");
            helicopter = Content.Load<Model>("Helicopter");
            currModel = torus;

            office = new Skybox(officeTextures, Content, GraphicsDevice, 512);
            daytime = new Skybox(daytimeTextures, Content, GraphicsDevice, 512);
            debug = new Skybox(debugTextures, Content, GraphicsDevice, 256);
            forest = new Skybox(forestTextures, Content, GraphicsDevice, 2048);

            skybox = forest;

            projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.ToRadians(90),
                    GraphicsDevice.Viewport.AspectRatio,
                    0.1f, 100);

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
                Vector3.Transform(
                    new Vector3(0, 1, 0),
                    Matrix.CreateRotationX(angle.Y) * Matrix.CreateRotationY(angle.X)
                )
            );


            prevMouse = currMouse;

            // CHANGE MODEL
            if (Keyboard.GetState().IsKeyDown(Keys.D1))
            {
                currModel = cube;
                textureMixFactor = 0.0f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D2))
            {
                currModel = sphere;
                textureMixFactor = 0.0f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D3))
            {
                currModel = torus;
                textureMixFactor = 0.0f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D4))
            {
                currModel = teapot;
                textureMixFactor = 0.0f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D5))
            {
                currModel = bunny;
                textureMixFactor = 0.0f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D6))
            {
                currModel = helicopter;
                textureMixFactor = 0.2f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D7))
            {
                skybox = debug;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D8))
            {
                skybox = office;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D9))
            {
                skybox = daytime;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D0))
            {
                skybox = forest;
            }

            // CHANGE SHADER
            if (Keyboard.GetState().IsKeyDown(Keys.F1))
            {
                effect = basicEffect;
                technique = 0;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F2))
            {
                effect = basicEffect;
                technique = 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F3))
            {
                effect = basicEffect;
                technique = 2;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F4))
            {
                effect = basicEffect;
                technique = 3;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F5))
            {
                effect = basicEffect;
                technique = 4;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F6))
            {
                effect = basicEffect;
                technique = 5;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F7))
            {
                effect = refrafEffect;
                technique = 0;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F8))
            {
                effect = refrafEffect;
                technique = 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F9))
            {
                effect = refrafEffect;
                technique = 2;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F10))
            {
                effect = refrafEffect;
                technique = 3;
            }

            // DISPERSION CONTROLS
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                float decrease = 1;
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                {
                    decrease = -1;
                }
                dispersionEtaRatio.X += 0.001f * decrease;
                dispersionEtaRatio.X = MathHelper.Clamp(dispersionEtaRatio.X, 0, 1);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.G))
            {
                float decrease = 1;
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                {
                    decrease = -1;
                }
                dispersionEtaRatio.Y += 0.001f * decrease;
                dispersionEtaRatio.Y = MathHelper.Clamp(dispersionEtaRatio.Y, 0, 1);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.B))
            {
                float decrease = 1;
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                {
                    decrease = -1;
                }
                dispersionEtaRatio.Z += 0.001f * decrease;
                dispersionEtaRatio.Z = MathHelper.Clamp(dispersionEtaRatio.Z, 0, 1);
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
                reflectivity -= 0.01f;

            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                reflectivity += 0.01f;
            }

            if (reflectivity < 0) reflectivity = 0;
            if (reflectivity > 1.0) reflectivity = 1.0f;

            // FRESNEL CONTROLS
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                float decrease = 1;
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                {
                    decrease = -1;
                }
                fresPower += 0.01f * decrease;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                float decrease = 1;
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                {
                    decrease = -1;
                }
                fresScale += 0.001f * decrease;
                fresScale = MathHelper.Clamp(fresScale, 0, 1);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                float decrease = 1;
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                {
                    decrease = -1;
                }
                fresBias += 0.001f * decrease;
                fresBias = MathHelper.Clamp(fresBias, 0, 1);
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
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (effect == refrafEffect)
            {
                skybox.Draw(view, projection);
            }

            effect.CurrentTechnique = effect.Techniques[technique];
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in currModel.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;
                        Matrix model = mesh.ParentBone.Transform;
                        if (currModel == torus)
                        {
                            model *= Matrix.CreateScale(0.25f);
                        } else if (currModel == helicopter)
                        {
                            model *= Matrix.CreateScale(2.0f);
                        } else if (currModel == bunny)
                        {
                            model *= Matrix.CreateScale(0.5f);
                        }
                        effect.Parameters["Model"].SetValue(model);
                        effect.Parameters["Projection"].SetValue(projection);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["CameraPosition"].SetValue(cameraPos);

                        if (effect == basicEffect)
                        {
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
                        } else
                        {
                            effect.Parameters["SkyboxTexture"].SetValue(skybox.skyboxTexture);
                            effect.Parameters["HelicopterTexture"].SetValue(helicopterTex);
                            effect.Parameters["EtaRatio"].SetValue(etaRatio);
                            effect.Parameters["DispersionEtaRatio"].SetValue(dispersionEtaRatio);
                            effect.Parameters["TextureMixFactor"].SetValue(textureMixFactor);
                            effect.Parameters["Reflectivity"].SetValue(reflectivity);
                            effect.Parameters["FresBias"].SetValue(fresBias);
                            effect.Parameters["FresScale"].SetValue(fresScale);
                            effect.Parameters["FresPower"].SetValue(fresPower);
                        }

                        pass.Apply();

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
                    "1-6 keys: Change Object; F1-F10 keys: Change Lighting Model",
                    new Vector2(10, 400),
                    Color.White);
                spriteBatch.DrawString(
                    font,
                    "R,G,B: Change Dispersion Eta Ratio (Shift to Decrease)",
                    new Vector2(10, 420),
                    Color.White);
                spriteBatch.DrawString(
                    font,
                    "+/-: Change Reflectivity; Q,W,E: Change Fresnel Power, Scale, Bias (Shift to Decrease)",
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
                    "Camera Angle: " + VectorToString(angle) + "; Dist: " + dist + "; Offset: " + VectorToString(offset),
                    new Vector2(10, 10),
                    Color.White);

                if (effect == basicEffect)
                {
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
                } else
                {
                    spriteBatch.DrawString(
                        font,
                        "Reflectivity: " + reflectivity + "; Fresnel Power: " + fresPower + ", Scale: " + fresScale + ", Bias: " + fresBias,
                        new Vector2(10, 30),
                        Color.White);
                    spriteBatch.DrawString(
                        font,
                        "EtaRatio: " + etaRatio + "; Dispersion EtaRatios: " + VectorToString(dispersionEtaRatio),
                        new Vector2(10, 50),
                        Color.White);
                    spriteBatch.DrawString(
                        font,
                        "Shader Type: " + techniques2[technique],
                        new Vector2(10, 70),
                        Color.White);
                }
                
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
        string VectorToString(Vector2 vec)
        {
            return "(" + vec.X.ToString("0.00") + ", " + vec.Y.ToString("0.00") + ")";
        }
        string VectorToString(Vector3 vec)
        {
            return "(" + vec.X.ToString("0.00") + ", " + vec.Y.ToString("0.00") + ", " + vec.Z.ToString("0.00") + ")";
        }
    }

}
