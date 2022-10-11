using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace harphoh.src.Renderers
{
    class ArrowRenderer : IRenderer
    {
        public Matrixf ModelMat = new Matrixf();
        internal bool ShouldRender = true;
        const float timeout = 10;
        float elapsedTime = 0;

        ICoreClientAPI api;
        Vec3d pos;
        MeshRef meshRef;

        public ArrowRenderer(ICoreClientAPI api, Vec3d pos, MeshData mesh)
        {
            this.api = api;
            this.pos = pos;
            meshRef = api.Render.UploadMesh(mesh);
        }

        public double RenderOrder => 0;

        public int RenderRange => 24;

        public void Dispose()
        {
            api.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);

            meshRef.Dispose();
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            elapsedTime += deltaTime;
            if(elapsedTime >= timeout) { Dispose(); }

            if (meshRef == null || !ShouldRender) return;

            IRenderAPI rpi = api.Render;
            Vec3d camPos = api.World.Player.Entity.CameraPos;

            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram prog = rpi.PreparedStandardShader((int)pos.X, (int)pos.Y, (int)pos.Z);
            prog.Tex2D = api.BlockTextureAtlas.AtlasTextureIds[0];

            float angle = 0;


            prog.ModelMatrix = ModelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0, angle, 0)
                .Values
            ;

            prog.ViewMatrix = rpi.CameraMatrixOriginf;
            prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(meshRef);
            prog.Stop();
        }
    }
}
