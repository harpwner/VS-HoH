using harphoh.src.Entities;
using harphoh.src.System.Client;
using harphoh.src.Renderer;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace harphoh.src.System
{
    class HeySystem : ModSystem
    {
        HeyInput input;

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

            api.RegisterEntity("EntityPointer", typeof(EntityPointer));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            this.capi = api;

            clientChannel = capi.Network.RegisterChannel("heyoverhere")
                .RegisterMessageType(typeof(HeyOverHereMessage))
                .RegisterMessageType(typeof(HeyOverHereSender))
                .SetMessageHandler<HeyOverHereMessage>(SpawnArrow);
        }

        MeshData GetMesh(string type)
        {
            Block block = api.World.BlockAccessor.GetBlock(new AssetLocation("game:mantle"));
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

        public void SendPacket(Vec3d playerPos, float pitch, float yaw, bool isBlockMarker = false)
        {
            if(api.Side != EnumAppSide.Client) { return; }

            api.Logger.Chat("Sending message");
            HeyOverHereSender message = new HeyOverHereSender() { playerPosition = playerPos, pitch = pitch, yaw = yaw, isBlockMarker = isBlockMarker };
            clientChannel.SendPacket(message);
        }

        void SendToPlayers(IPlayer player, HeyOverHereSender packet)
        {
            if (api.Side != EnumAppSide.Server) { return; }

            api.Logger.Chat("Server Receieved Message! Forming HoH Packet");

            BlockSelection bSelection = new BlockSelection();
            EntitySelection eSelection = new EntitySelection();

            sapi.World.RayTraceForSelection(packet.playerPosition, -packet.pitch, packet.yaw, 64, ref bSelection, ref eSelection);

            if(bSelection == null) { return; }

            HeyOverHereMessage clientPacket = new HeyOverHereMessage() { position = bSelection.Position.ToVec3d(), isBlockMarker = packet.isBlockMarker };

            serverChannel.BroadcastPacket(clientPacket);
        }

        void SpawnArrow(HeyOverHereMessage packet)
        {
            if(api.Side != EnumAppSide.Client) { return; }
            api.Logger.Chat("Packet Receieved From Server, Spawning HoH Arrow!");

            if (packet.isBlockMarker)
            {
                capi.Event.RegisterRenderer(new OutlineRenderer(api as ICoreClientAPI, packet.position, GetMesh("outline")), EnumRenderStage.AfterBlit);
            }
            else
            {
                capi.Event.RegisterRenderer(new PointerRenderer(api as ICoreClientAPI, packet.position, GetMesh("arrow"), (float)(Math.PI / 2), -1), EnumRenderStage.AfterBlit);
                capi.Event.RegisterRenderer(new PointerRenderer(api as ICoreClientAPI, packet.position, GetMesh("arrow")), EnumRenderStage.AfterBlit);
            }
        }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class HeyOverHereMessage
    {
        public Vec3d position;
        public bool isBlockMarker;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class HeyOverHereSender
    {
        public Vec3d playerPosition;
        public bool isBlockMarker;
        public float pitch;
        public float yaw;
    }
}
