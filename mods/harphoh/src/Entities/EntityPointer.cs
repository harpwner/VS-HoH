using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace harphoh.src.Entities
{
    class EntityPointer : Entity
    {
        const float decayTime = 10;
        Vec3d suppliedPos;
        float timeAlive = 0;
        public override bool IsInteractable => false;
        public override bool ShouldDespawn => ShouldTerminate();

        bool ShouldTerminate()
        {
            return timeAlive >= decayTime;
        }

        public EntityPointer(Vec3d pos)
        {
            this.suppliedPos = pos;
        }

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);

            api.Logger.Chat("Pointer Spawned!");
        }

        public override void OnGameTick(float dt)
        {
            base.OnGameTick(dt);

            timeAlive += dt;
            this.Properties.Client.Shape.rotateY += (30 * dt);
        }
    }
}
