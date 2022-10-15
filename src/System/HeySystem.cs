using harphoh.src.System.Client;
using harphoh.src.Renderer;
using ProtoBuf;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Common.Entities;

namespace harphoh.src.System
{
    class HeySystem : ModSystem
    {
        HeyInput input;
        const float decayTime = 10;

        ICoreAPI api;
        ICoreServerAPI sapi;
        ICoreClientAPI capi;
        IServerNetworkChannel serverChannel;
        IClientNetworkChannel clientChannel;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            input = new HeyInput(api, this);

            this.api = api;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            this.capi = api;

            clientChannel = capi.Network.RegisterChannel("heyoverhere")
                .RegisterMessageType(typeof(HeyOverHereMessage))
                .RegisterMessageType(typeof(HeyOverHereSender))
                .SetMessageHandler<HeyOverHereMessage>(SpawnPointer);
        }

        /// <summary>Acquires a mesh to pass through to a renderer. Uses the mantle as a dummy base block.</summary>
        /// <param name="type">the string of the mesh to use, will usually be arrow or outline</param>
        /// <returns>the mesh</returns>
        MeshData GetMesh(string type)
        {
            Block block = api.World.BlockAccessor.GetBlock(new AssetLocation("harphoh:texblock"));
            if (block.BlockId == 0) return null;

            MeshData mesh;
            ITesselatorAPI mesher = ((ICoreClientAPI)api).Tesselator;

            Shape shape = Shape.TryGet(api, "harphoh:shapes/" + type + ".json");

            mesher.TesselateShape(block, shape, out mesh);

            return mesh;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            this.sapi = api;

            serverChannel = sapi.Network.RegisterChannel("heyoverhere")
                .RegisterMessageType(typeof(HeyOverHereMessage))
                .RegisterMessageType(typeof(HeyOverHereSender))
                .SetMessageHandler<HeyOverHereSender>(SendToPlayers);
        }

        /// <summary>Method to be called externally, creates and sends a packet to the server for pointer placement</summary>
        /// <param name="playerPos">The player position.</param>
        /// <param name="pitch">Player camera pitch</param>
        /// <param name="yaw">Player camera yaw</param>
        public void SendPacket(Vec3d playerPos, float pitch, float yaw, bool isBlockMarker = false)
        {
            if(api.Side != EnumAppSide.Client) { return; }

            api.Logger.Debug("Sending message to server");

            HeyOverHereSender message = new HeyOverHereSender() { playerPosition = playerPos, pitch = pitch, yaw = yaw, isBlockMarker = isBlockMarker };

            clientChannel.SendPacket(message);
        }

        /// <summary>Delegate bool returning false, ignores entities all the time</summary>
        /// <param name="e">The entity that will be ignored anyway</param>
        /// <returns>Always false</returns>
        bool MarkEntityFilter(Entity e)
        {
            return false;
        }

        /// <summary>Checks a given block to see if a pointer can be placed on it. Ignores transparent blocks and replaceable blocks.</summary>
        /// <param name="p">Position of the checked block</param>
        /// <param name="block">Reference to the checked block</param>
        /// <returns>Whether the block can have a pointer on it or not.</returns>
        bool MarkBlockFilter(BlockPos p, Block block)
        {
            if(block.Id == 0) { return false; }
            if(block.Replaceable >= 5000) { return false; }

            for(int i = 0; i < block.SideOpaque.Length; i++)
            {
                if (block.SideOpaque[i]) { return true; }
            }

            return false;
        }

        /// <summary>Broadcasts a packet from the server to each client, this is used to trigger the spawning of pointers.</summary>
        /// <param name="player">The Player responsible for sending the initial packet</param>
        /// <param name="packet">The client packet containing player info</param>
        void SendToPlayers(IPlayer player, HeyOverHereSender packet)
        {
            if (api.Side != EnumAppSide.Server) { return; }

            api.Logger.Debug("Server recieved meessage! Forming HoH Packet");

            BlockSelection bSelection = new BlockSelection();
            EntitySelection eSelection = new EntitySelection();

            sapi.World.RayTraceForSelection(packet.playerPosition, -packet.pitch, packet.yaw, 64, ref bSelection, ref eSelection, MarkBlockFilter, MarkEntityFilter);

            if(bSelection == null) { return; }

            Vec3d markPosition = bSelection.Position.ToVec3d();
            if (!packet.isBlockMarker) { markPosition += (bSelection.HitPosition - new Vec3d(0.5, 0.75, 0.5)); }

            HeyOverHereMessage clientPacket = new HeyOverHereMessage() { position = markPosition, isBlockMarker = packet.isBlockMarker };

            serverChannel.BroadcastPacket(clientPacket);
        }


        /// <summary>Spawns a Pointer in the world for the current client.</summary>
        /// <param name="packet">Packet from the server for spawn information.</param>
        void SpawnPointer(HeyOverHereMessage packet)
        {
            if(api.Side != EnumAppSide.Client) { return; }

            api.Logger.Debug("Packet received from server, Spawning HoH Arrow!");

            if (packet.isBlockMarker)
            {
                capi.Event.RegisterRenderer(new OutlineRenderer(api as ICoreClientAPI, packet.position, GetMesh("outline"), decayTime), EnumRenderStage.AfterBlit);
            }
            else
            {
                capi.Event.RegisterRenderer(new PointerRenderer(api as ICoreClientAPI, packet.position, GetMesh("arrow"), decayTime, (float)(Math.PI / 2), -1), EnumRenderStage.AfterBlit);
                capi.Event.RegisterRenderer(new PointerRenderer(api as ICoreClientAPI, packet.position, GetMesh("arrow"), decayTime), EnumRenderStage.AfterBlit);
            }
        }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class HeyOverHereMessage
    {
        /// <summary>
        /// Final position of the pointer
        /// </summary>
        public Vec3d position;
        public bool isBlockMarker;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class HeyOverHereSender
    {
        public Vec3d playerPosition;
        public bool isBlockMarker;
        /// <summary>
        /// Source client camera pitch
        /// </summary>
        public float pitch;
        /// <summary>
        /// Source client camera yaw
        /// </summary>
        public float yaw;
    }
}
