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
using Vintagestory.API.MathTools;

namespace harphoh.src.System
{
    class HeySystem : ModSystem, IRenderer
    {
        public double RenderOrder => 0.01;
        public int RenderRange => 100;
        HeyInput input;
        ICoreAPI api;

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage) { }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            input = new HeyInput(api, this);

            this.api = api;

            api.RegisterEntity("EntityPointer", typeof(EntityPointer));
        }
    }
}
