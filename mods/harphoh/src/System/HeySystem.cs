using harphoh.src.Entities;
using harphoh.src.System.Client;
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
        ICoreServerAPI serverAPI;
        ICoreClientAPI clientAPI;
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

            this.clientAPI = api;

            clientChannel = clientAPI.Network.RegisterChannel("heyoverhere")
                .RegisterMessageType(typeof(HeyOverHereMessage));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            this.serverAPI = api;

            serverChannel = serverAPI.Network.RegisterChannel("heyoverhere")
                .RegisterMessageType(typeof(HeyOverHereMessage))
                .SetMessageHandler<HeyOverHereMessage>(SpawnArrow);
        }

        public void SendPacket(Vec3d inputPos)
        {
            if(api.Side != EnumAppSide.Client) { return; }

            api.Logger.Chat("Sending message");
            HeyOverHereMessage message = new HeyOverHereMessage() { position = inputPos };
            clientChannel.SendPacket(message);
        }

        void SpawnArrow(IPlayer player, HeyOverHereMessage packet)
        {
            if (api.Side != EnumAppSide.Server) { return; }

            Vec3d position = packet.position;

            AssetLocation asset = AssetLocation.Create("harphoh:arrow");

            EntityProperties type = api.World.GetEntityType(asset);

            Entity entity = api.ClassRegistry.CreateEntity(type);

            api.Logger.Chat("Hi");

            if (entity != null)
            {
                entity.ServerPos = new EntityPos(position.X, position.Y, position.Z);
                entity.Pos.SetFrom(entity.ServerPos);

                api.World.SpawnEntity(entity);
            }
        }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class HeyOverHereMessage
    {
        public Vec3d position;
    }
}
