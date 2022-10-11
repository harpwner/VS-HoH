using harphoh.src.Entities;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace harphoh.src.System.Client
{
    class HeyInput
    {
        ICoreAPI api;
        IClientPlayer Player => (api as ICoreClientAPI).World.Player;
        const GlKeys heyKey = GlKeys.Minus;
        const float range = 64;

        public HeyInput(ICoreAPI api, HeySystem modSys)
        {
            this.api = api;
            if(this.api == null || api.Side != EnumAppSide.Client)
            {
                return;
            }

            ICoreClientAPI capi = api as ICoreClientAPI;

            capi.Input.RegisterHotKey("heyoverhere", "Hey! Over Here!", heyKey);
            capi.Input.SetHotKeyHandler("heyoverhere", HeyOverHere);
        }

        public AssetLocation AssetTest()
        {
            var asset = AssetLocation.Create("harphoh:entities/arrowpointer");
            api.Logger.Audit("Asset Found: {0}", asset != null);
            api.Logger.Audit(asset?.Path);
            return asset;
        }

        bool HeyOverHere(KeyCombination k)
        {
            BlockSelection look = new BlockSelection();
            EntitySelection ent = new EntitySelection();
            api.World.RayTraceForSelection(Player.Entity.CameraPos, Player.CameraPitch, Player.Entity.BodyYaw, range, ref look, ref ent);
            api.Logger.Chat(Player.Entity.Attributes.Values.ToString());

            if (look == null) { return false; }

            Vec3d selectTrue = look?.HitPosition + look?.Position.ToVec3d();

            api.Logger.Chat("Position: (" + selectTrue.X + ", " + selectTrue.Y + ", " + selectTrue.Z + ")");

            AssetLocation asset = AssetTest();

            EntityProperties type = api.World.GetEntityType(asset);

            Entity entity = api.World.ClassRegistry.CreateEntity(type);

            if(entity != null)
            {
                entity.ServerPos = new EntityPos(selectTrue.X, selectTrue.Y, selectTrue.Z);
                entity.Pos.SetFrom(entity.ServerPos);

                api.World.SpawnEntity(entity);
            }

            return true;
        }
    }
}
