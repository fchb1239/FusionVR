using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Fusion.VR;
using Fusion.VR.Cosmetics;

namespace Fusion.VR.Saving
{
    public class FusionVRPrefs
    {
        private const string CosmeticSlotLocation = "CosmeticSlot_";

        public static Dictionary<string, string> GetCosmetics(List<string> slots)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            for (int i = 0; i < slots.Count; i++)
            {
                result[slots[i]] = PlayerPrefs.GetString($"{CosmeticSlotLocation}{slots[i]}");
                Debug.Log($"{CosmeticSlotLocation}{slots[i]}: {result[slots[i]]}");
            }

            return result;
        }

        public static bool SaveCosmetics(Dictionary<string, string> cosmetics)
        {
            try
            {
                foreach (KeyValuePair<string, string> cosmetic in cosmetics)
                {
                    PlayerPrefs.SetString($"{CosmeticSlotLocation}{cosmetic.Key}", cosmetic.Value);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }
    }
}