using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Core;

namespace MWRMode.CharacterCreationContent
{
    public static class MWRCharacterCreationFaceGenHelper
    {
        public static int GetPreviewRace(CharacterCreationManager characterCreationManager)
        {
            return CharacterObject.PlayerCharacter.Race;
        }

        public static BodyProperties GetPreviewBodyProperties(CharacterCreationManager characterCreationManager)
        {
            return CharacterObject.PlayerCharacter.GetBodyProperties(CharacterObject.PlayerCharacter.Equipment, -1);
        }

        public static BodyProperties GetAgedPreviewBodyProperties(CharacterCreationManager characterCreationManager, float age)
        {
            BodyProperties bodyProperties = GetPreviewBodyProperties(characterCreationManager);
            return FaceGen.GetBodyPropertiesWithAge(ref bodyProperties, age);
        }
    }
}