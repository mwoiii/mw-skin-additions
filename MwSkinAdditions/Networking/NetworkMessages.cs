using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static MwSkinAdditions.SkinEvents;

namespace MwSkinAdditions.Networking {
    public static class NetworkMessages {
        public static void Init() {
            NetworkingAPI.RegisterMessageType<SyncUseShrine>();
            NetworkingAPI.RegisterMessageType<SyncGetItem>();
            NetworkingAPI.RegisterMessageType<SyncHoldoutZoneCharged>();
        }
    }

    public class SyncGetItem : INetMessage {

        NetworkInstanceId netInstanceId;
        int itemIndex;

        public SyncGetItem() {
        }

        public SyncGetItem(NetworkInstanceId netInstanceId, int itemIndex) {
            this.netInstanceId = netInstanceId;
            this.itemIndex = itemIndex;
        }

        public void Serialize(NetworkWriter writer) {
            writer.Write(netInstanceId);
            writer.Write(itemIndex);
        }

        public void Deserialize(NetworkReader reader) {
            netInstanceId = reader.ReadNetworkId();
            itemIndex = reader.ReadInt32();
        }

        public void OnReceived() {
            GameObject body = Util.FindNetworkObject(netInstanceId);
            if (body != null) {
                InvokeGetItem(body, itemIndex);
            }
        }
    }

    public class SyncUseShrine : INetMessage {

        NetworkInstanceId netInstanceId;
        bool success;

        public SyncUseShrine() {
        }

        public SyncUseShrine(NetworkInstanceId netInstanceId, bool success) {
            this.netInstanceId = netInstanceId;
            this.success = success;
        }

        public void Serialize(NetworkWriter writer) {
            writer.Write(netInstanceId);
            writer.Write(success);
        }

        public void Deserialize(NetworkReader reader) {
            netInstanceId = reader.ReadNetworkId();
            success = reader.ReadBoolean();
        }

        public void OnReceived() {
            GameObject body = Util.FindNetworkObject(netInstanceId);
            if (body != null) {
                InvokeUseShrine(body, success);
            }
        }
    }

    public class SyncHoldoutZoneCharged : INetMessage {

        public void Serialize(NetworkWriter writer) {
        }

        public void Deserialize(NetworkReader reader) {
        }

        public void OnReceived() {
            InvokeHoldoutZoneCharged();
        }
    }
}
