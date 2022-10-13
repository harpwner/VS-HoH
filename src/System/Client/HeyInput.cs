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
        const GlKeys heyKeyBlock = GlKeys.Plus;
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

            capi.Input.RegisterHotKey("heyoverhere", "Hey! Over Here! Mark Position", heyKey);
            capi.Input.SetHotKeyHandler("heyoverhere", HeyOverHere);

            capi.Input.RegisterHotKey("heyoverhereblock", "Hey! Over Here! Mark Block", heyKeyBlock);
            capi.Input.SetHotKeyHandler("heyoverhereblock", HeyOverHereBlock);
        }

        bool HeyOverHere(KeyCombination k)
        {
            Vec3d startPos = Player.Entity.SidedPos.XYZ;
            startPos += Player.Entity.LocalEyePos;

            system.SendPacket(startPos, -Player.Entity.SidedPos.Pitch, Player.Entity.SidedPos.Yaw);

            return true;
        }

        bool HeyOverHereBlock(KeyCombination k)
        {
            Vec3d startPos = Player.Entity.SidedPos.XYZ;
            startPos += Player.Entity.LocalEyePos;

            system.SendPacket(startPos, -Player.Entity.SidedPos.Pitch, Player.Entity.SidedPos.Yaw, true);

            return true;
        }
    }
}
