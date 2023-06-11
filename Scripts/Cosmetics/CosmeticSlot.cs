using System;
using System.Collections.Generic;
using Fusion;

namespace Fusion.VR.Cosmetics
{
    public struct CosmeticSlot : INetworkStruct
    {
        public NetworkString<_16> SlotName;
        public NetworkString<_32> CosmeticName;

        public static List<CosmeticSlot> CopyFrom(Dictionary<string, string> cosmetics)
        {
            List<CosmeticSlot> slots = new List<CosmeticSlot>();
            foreach (KeyValuePair<string, string> cos in cosmetics)
            {
                slots.Add(new CosmeticSlot()
                {
                    SlotName = cos.Key,
                    CosmeticName = cos.Value
                });
            }

            return slots;
        }
    }
}