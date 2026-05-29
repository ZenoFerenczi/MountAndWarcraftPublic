using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Core;

namespace MountAndWarcraftReborn.Components.Models
{
    [HarmonyPatch(typeof(CharacterCreationCampaignBehavior), "InitializeCharacterCreationCultures")]
    public class AddOnlyOurCulturesPatch
    {
        // add only our cultures to the selection screen, skip the original method
        static bool Prefix(CharacterCreationManager characterCreationManager)
        {
            foreach (CultureObject cultureObject in Game.Current.ObjectManager.GetObjectTypeList<CultureObject>())
            {
                if (cultureObject.StringId == "kultiras")  // Replace with your culture's StringId
                {
                    characterCreationManager.CharacterCreationContent.AddCharacterCreationCulture(cultureObject, 1, 10);
                }
            }
            return true;
        }
    }
}
