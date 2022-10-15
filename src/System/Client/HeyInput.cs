using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace harphoh.src.System.Client
{
    class HeyInput
    {
        const GlKeys heyKeyArrow = GlKeys.Minus;
        const GlKeys heyKeyBlock = GlKeys.Plus;
        const long cooldown = 100;

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

        /// <summary>Gets the eye position of the player in real world space</summary>
        /// <returns>player eye position in world space</returns>
        Vec3d GetEyePos()
        {
            Vec3d eyePos = Player.Entity.SidedPos.XYZ;
            eyePos += Player.Entity.LocalEyePos;

            return eyePos;
        }

        /// <summary>Called to check to see if a pointer can be placed. If the pointer is not on cooldown, the canMark variable is set to true</summary>
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
