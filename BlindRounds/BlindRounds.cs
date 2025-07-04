using System;
using BepInEx;
using UnboundLib;
using UnboundLib.Networking;
using UnboundLib.Utils.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HarmonyLib;
using BepInEx.Configuration;
using Photon.Pun;
using BepInEx.Logging;

namespace BlindRounds
{
    [BepInDependency("com.willis.rounds.unbound")]
    [BepInPlugin(ModId, ModName, Version)]
    public class BlindRounds : BaseUnityPlugin
    {
        private const string ModId = "com.fabian.rounds.blindrounds";
        private const string ModName = "Blind Rounds";
        private const string Version = "0.0.0";
        public static ManualLogSource Log;
        public static BlindRounds instance;
        public static ConfigEntry<bool> BlindRoundsEnabled;
        internal static bool isEnabled;

        void Awake()
        {
            BlindRounds.instance = this;
            Log = Logger;

            BlindRoundsEnabled = Config.Bind(
                "BlindRounds",
                "Enable Blind Rounds",
                false,
                "Enable or disable the Blind Rounds game mode."
            );

            new Harmony(ModId).PatchAll();
        }

        void Start()
        {
            isEnabled = BlindRoundsEnabled.Value;

            Unbound.RegisterMenu(ModName, () => { }, this.NewGUI, null, false);
            Unbound.RegisterHandshake(ModId, this.OnHandshakeCompleted);
        }

        private void OnHandshakeCompleted()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                NetworkingManager.RPC_Others(typeof(BlindRounds), nameof(SyncSettings), new object[] { isEnabled });
            }
        }

        [UnboundRPC]
        private static void SyncSettings(bool host_enabled)
        {
            isEnabled = host_enabled;
        }

        private void NewGUI(GameObject menu)
        {
            MenuHandler.CreateText(ModName + " Settings", menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateToggle(BlindRoundsEnabled.Value, "Enable Blind Rouunds", menu, EnabledChanged, 30);

            void EnabledChanged(bool value)
            {
                BlindRoundsEnabled.Value = value;
                isEnabled = value;
                OnHandshakeCompleted();
            }
        }

        [UnboundRPC]
        public static void RPC_RequestSync(int requestingPlayer)
        {
            NetworkingManager.RPC(typeof(BlindRounds), nameof(BlindRounds.RPC_SyncResponse), requestingPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
        }

        [UnboundRPC]
        public static void RPC_SyncResponse(int requestingPlayer, int readyPlayer)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == requestingPlayer)
            {
                BlindRounds.instance.RemovePendingRequest(readyPlayer, nameof(BlindRounds.RPC_RequestSync));
            }
        }

    }
}
