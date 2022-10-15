using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace harphoh.src.Renderer
{
    class OutlineRenderer : IRenderer
    {
        public Matrixf ModelMat = new Matrixf();
        float timeAlive = 0;

        ICoreClientAPI api;
        float decayTime;
        Vec3d pos;
        MeshRef meshRef;

        public OutlineRenderer(ICoreClientAPI api, Vec3d pos, MeshData mesh, float decayTime)
        {
            this.api = api;
            this.pos = pos;
            this.decayTime = decayTime;
            meshRef = api.Render.UploadMesh(mesh);
        }

        public double RenderOrder => 1;

        public int RenderRange => 24;

        public void Dispose()
        {
            api.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);

            meshRef.Dispose();
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            timeAlive += deltaTime;
            if (timeAlive >= decayTime)
            {
                Dispose(); return;
            }

            if (meshRef == null) return;

            IRenderAPI rpi = api.Render;
            Vec3d camPos = api.World.Player.Entity.CameraPos;

            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram prog = rpi.PreparedStandardShader((int)pos.X, 0, (int)pos.Z, new Vec4f(200, 200, 200, 200));
            prog.Tex2D = api.BlockTextureAtlas.AtlasTextureIds[0];

            prog.ModelMatrix = ModelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Values
            ;

            prog.RgbaTint = new Vec4f(0.9f, 0.8f, 0.2f, 0.5f + (float)(0.5f * Math.Sin(timeAlive * 8)));
            prog.ViewMatrix = rpi.CameraMatrixOriginf;
            prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(meshRef);
            prog.Stop();
        }
    }
}
