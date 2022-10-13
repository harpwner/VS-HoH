using harphoh.src.Entities;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace harphoh.src.System.Client
{
    class HeyInput
    {
        const GlKeys heyKey = GlKeys.Minus;
        const float range = 64;

        ICoreAPI api;
        IClientPlayer Player => (api as ICoreClientAPI).World.Player;
        HeySystem system;

        public HeyInput(ICoreAPI api, HeySystem system)
        {
            this.api = api;
            this.system = system;

            if(this.api == null || api.Side != EnumAppSide.Client)
            {
                return;
            }

            ICoreClientAPI capi = api as ICoreClientAPI;

            capi.Input.RegisterHotKey("heyoverhere", "Hey! Over Here!", heyKey);
            capi.Input.SetHotKeyHandler("heyoverhere", HeyOverHere);
        }

        bool EntityIgnore(Entity entity)
        {
            return false;
        }

        bool HeyOverHere(KeyCombination k)
        {
            BlockSelection look = new BlockSelection();
            EntitySelection ent = new EntitySelection();

            api.World.RayTraceForSelection(Player.Entity.CameraPos, Player.CameraPitch, Player.Entity.BodyYaw, range, ref look, ref ent, null, new EntityFilter(EntityIgnore));
            api.Logger.Chat(Player.Entity.Attributes.Values.ToString());

            if (look == null) { return false; }

            Vec3d selectTrue = look?.HitPosition + look?.Position.ToVec3d();

            api.Logger.Chat("Position: (" + selectTrue.X + ", " + selectTrue.Y + ", " + selectTrue.Z + ")");

            system.SendPacket(selectTrue);

            return true;
        }
    }
}
