using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace harphoh.src.System.Client
{
    class HeyInput
    {
        const GlKeys heyKeyArrow = GlKeys.Minus;
        const GlKeys heyKeyBlock = GlKeys.Plus;
        const long cooldown = 10000;

        IClientPlayer Player => (api as ICoreClientAPI).World.Player;
        ICoreAPI api;
        bool canMark = true;
        long markTime;
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

            capi.Input.RegisterHotKey("heyoverherearrow", "Hey! Over Here! Mark Position", heyKeyArrow);
            capi.Input.SetHotKeyHandler("heyoverherearrow", HeyOverHereArrow);

            capi.Input.RegisterHotKey("heyoverhereblock", "Hey! Over Here! Mark Block", heyKeyBlock);
            capi.Input.SetHotKeyHandler("heyoverhereblock", HeyOverHereBlock);
        }

        Vec3d GetEyePos()
        {
            Vec3d eyePos = Player.Entity.SidedPos.XYZ;
            eyePos += Player.Entity.LocalEyePos;

            return eyePos;
        }

        void CheckTime()
        {
            this.canMark = api.World.ElapsedMilliseconds >= (markTime + cooldown);
        }

        bool HeyOverHereArrow(KeyCombination k)
        {
            CheckTime();

            if (!canMark) { return false; }

            system.SendPacket(GetEyePos(), -Player.Entity.SidedPos.Pitch, Player.Entity.SidedPos.Yaw);

            canMark = false;
            markTime = api.World.ElapsedMilliseconds;

            return true;
        }

        bool HeyOverHereBlock(KeyCombination k)
        {
            CheckTime();

            if (!canMark) { return false; }

            system.SendPacket(GetEyePos(), -Player.Entity.SidedPos.Pitch, Player.Entity.SidedPos.Yaw, true);

            canMark = false;
            markTime = api.World.ElapsedMilliseconds;

            return true;
        }
    }
}
