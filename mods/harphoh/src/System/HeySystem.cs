using harphoh.src.Entities;
using harphoh.src.Renderers;
using harphoh.src.System.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace harphoh.src.System
{
    class HeySystem : ModSystem
    {
        HeyInput input;
        ICoreAPI api;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            input = new HeyInput(api, this);

            this.api = api;

            api.RegisterEntity("EntityPointer", typeof(EntityPointer));
        }

        public void SpawnArrow(Vec3d position)
        {
            AssetLocation asset = AssetLocation.Create("harphoh:entities/arrowpointer");

            EntityProperties type = api.World.GetEntityType(asset);

            Entity entity = api.World.ClassRegistry.CreateEntity(type);

            if (entity != null)
            {
                entity.ServerPos = new EntityPos(position.X, position.Y, position.Z);
                entity.Pos.SetFrom(entity.ServerPos);

                api.World.SpawnEntity(entity);
            }
        }
    }
}
