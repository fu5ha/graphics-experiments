using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace SimpleEngine
{
    public class Skybox
    {
        private Model cube;
        public TextureCube skyboxTexture;
        private Effect effect;
        private float size = 50f;

        private CubeMapFace[] faces = {
            CubeMapFace.PositiveX,
            CubeMapFace.NegativeX,
            CubeMapFace.PositiveY,
            CubeMapFace.NegativeY,
            CubeMapFace.PositiveZ,
            CubeMapFace.NegativeZ,
        };

        public Skybox(string[] textures, ContentManager content, GraphicsDevice g, int size)
        {
            cube = content.Load<Model>("Cube");
            effect = content.Load<Effect>("SkyboxEffect");

            skyboxTexture = new TextureCube(g, size, false, SurfaceFormat.Color);
            byte[] data = new byte[size * size * 4];
            Texture2D tempTexture;

            for (int i = 0; i < 6; i++)
            {
                tempTexture = content.Load<Texture2D>(textures[i]);
                tempTexture.GetData<byte>(data);
                skyboxTexture.SetData<byte>(faces[i], data);
            }
        }

        public void Draw(Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in cube.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                    effect.Parameters["Model"].SetValue(Matrix.CreateScale(size));
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    effect.Parameters["SkyboxTexture"].SetValue(skyboxTexture);
                }
                mesh.Draw();
            }
        }
    }
}
