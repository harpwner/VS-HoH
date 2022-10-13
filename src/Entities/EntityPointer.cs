using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace harphoh.src.Entities
{
    class EntityPointer : Entity
    {
        const float decayTime = 10;
        float timeAlive = 0;
        ICoreClientAPI capi;

        public override bool IsInteractable => false;
        public override bool ShouldDespawn => ShouldTerminate();

        bool ShouldTerminate()
        {
            return timeAlive >= decayTime;
        }

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);

            if(api.Side == EnumAppSide.Client)
            {
                capi = api as ICoreClientAPI;
            }
            
            api.Logger.Chat("Pointer Spawned!");
        }

        public override void OnGameTick(float dt)
        {
            base.OnGameTick(dt);

            if(Api.Side != EnumAppSide.Client) { return; }

            timeAlive += dt;
            this.Properties.Client.Size = 1 + (0.25f) * (float)Math.Sin(timeAlive);
            this.Properties.Client.Shape.rotateY += (30 * dt);
        }
    }
}
