#nullable enable
using HarmonyLib;
using MountAndWarcraftReborn.Portals;
using System.Linq;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.ObjectSystem;

namespace MountAndWarcraftReborn.Patches.Settlement
{
    [HarmonyPatch]
    public static class MWRPortalSettlementPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TaleWorlds.CampaignSystem.Settlements.Settlement), "Deserialize")]
        public static void DeserializePostfix(MBObjectManager objectManager, XmlNode node, TaleWorlds.CampaignSystem.Settlements.Settlement __instance)
        {
            if (__instance.SettlementComponent is not MWRPortalSiteComponent portal || node.Attributes?["owner"] == null)
            {
                return;
            }

            Clan? clan;
            if (Campaign.Current?.CampaignGameLoadingType == Campaign.GameLoadingType.NewCampaign)
            {
                clan = MBObjectManager.Instance.ReadObjectReferenceFromXml<Clan>("owner", node);
            }
            else
            {
                string ownerValue = node.Attributes["owner"]!.Value;
                string clanId = ownerValue.Contains(".") ? ownerValue.Split('.')[1] : ownerValue;
                clan = Clan.All.FirstOrDefault(x => x.StringId == clanId);
            }

            if (clan != null)
            {
                portal.OwnerClan = clan;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TaleWorlds.CampaignSystem.Settlements.Settlement), "OwnerClan", MethodType.Getter)]
        public static bool OwnerClanPrefix(ref Clan? __result, TaleWorlds.CampaignSystem.Settlements.Settlement __instance)
        {
            if (__instance.SettlementComponent is not MWRPortalSiteComponent portal)
            {
                return true;
            }

            __result = portal.OwnerClan ?? portal.SpawnClan;
            return __result == null;
        }
    }
}
