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
[HarmonyPatch(typeof(CardInfoDisplayer), "DrawCard")]
    class CardInfoDisplayer_DrawCard_Patch
    {
        static bool Prefix(
            ref CardInfoStat[] stats,
            ref string cardName,
            ref string description,
            ref Sprite image,
            ref bool charge)
        {
            if (!BlindRounds.isEnabled) return true;
            cardName = "???";
            description = "???";
            stats = new CardInfoStat[] { };
            image = null;
            charge = false;
            return true;
        }
    }

    [HarmonyPatch(typeof(CardBar), "AddCard")]
    class CardBar_AddCard_Patch
    {
        static CardInfo localCard;
        static string localCardName;
        static bool Prefix(CardInfo card)
        {
            if (!BlindRounds.isEnabled) return true;
            localCard = card;
            localCardName = card.cardName;
            card.cardName = "?  ";
            return true;
        }

        static void Postfix()
        {
            localCard.cardName = localCardName;
        }
    }

    [HarmonyPatch(typeof(CardVisuals), "Start")]
    class CardVisuals_Start_Patch
    {
        static void Postfix(CardVisuals __instance)
        {
            if (!BlindRounds.isEnabled) return;
            var artTransform = __instance.transform.Find("Canvas/Front/Background/Art");
            if (artTransform != null)
            {
                foreach (Transform child in artTransform)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            var anims = __instance.GetComponentsInChildren<CardAnimation>();
            foreach (var anim in anims)
            {
                anim.enabled = false;
            }

            if (__instance.images != null)
            {
                foreach (var img in __instance.images)
                {
                    if (img != null)
                    {
                        img.enabled = false;
                    }
                }
            }
        }
    }
}