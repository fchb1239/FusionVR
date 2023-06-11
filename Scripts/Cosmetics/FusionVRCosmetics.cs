using System.Collections.Generic;

namespace Fusion.VR.Cosmetics
{
    // This is because stupid JsonUtillity can't read a stupid list unless it's within another class
    // And just found out Fusion can't either. AHHHHHHHHHHHHHHH
    public struct FusionVRCosmetics
    {
        public List<CosmeticSlot> Cosmetics;

        /// <summary>
        /// Creates a FusionVRCosmetics from a List<CosmeticSlot>
        /// </summary>
        /// <param name="_cosmetics">The cosmetics that will be cloned to the FusionVRCosmetics</param>
        public FusionVRCosmetics(List<CosmeticSlot> _cosmetics)
        {
            Cosmetics = _cosmetics;
        }
    }
}