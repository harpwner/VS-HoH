﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace harphoh.src.Renderer
{
    class PointerRenderer : IRenderer
    {
        public Matrixf ModelMat = new Matrixf();
        const float dieTime = 10;
        float timeAlive = 0;

        ICoreClientAPI api;
        float offset = 0;
        int mult;
        Vec3d pos;
        MeshRef meshRef;

        public PointerRenderer(ICoreClientAPI api, Vec3d pos, MeshData mesh, float offset = 0, int mult = 1)
        {
            this.api = api;
            this.pos = pos;
            this.offset = offset;
            this.mult = mult;
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
            if(timeAlive >= dieTime)
            {
                Dispose(); return;
            }

            if (meshRef == null) return;

            IRenderAPI rpi = api.Render;
            Vec3d camPos = api.World.Player.Entity.CameraPos;

            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram prog = rpi.PreparedStandardShader((int)pos.X, (int)pos.Y, (int)pos.Z);
            prog.Tex2D = api.BlockTextureAtlas.AtlasTextureIds[0];

            prog.ModelMatrix = ModelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0, 1 + (offset / 4) + (0.2f * Math.Sin(timeAlive * 4)), 0)
                .Translate(0.5, 0.5, 0.5)
                .RotateY(offset + (timeAlive * 2) * mult)
                .Translate(-0.5, -0.5, -0.5)
                .Values
            ;

            prog.RgbaTint = new Vec4f(0.98f, 0.98f, 0, 0.8f);
            prog.ViewMatrix = rpi.CameraMatrixOriginf;
            prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(meshRef);
            prog.Stop();
        }
    }
}
