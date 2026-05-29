using MountAndWarcraftReborn.BattleMechanics.NaturalWeapons;
using SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MountAndWarcraftReborn.Components.Models
{
    public class MWRAgentApplyDamageModel : SandboxAgentApplyDamageModel
    {
        public override float ApplyGeneralDamageModifiers(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseDamage)
        {
            float result = base.ApplyGeneralDamageModifiers(attackInformation, collisionData, baseDamage);

            Agent attackerAgent = attackInformation.IsAttackerAgentMount
                ? attackInformation.AttackerAgent?.RiderAgent
                : attackInformation.AttackerAgent;
            BasicCharacterObject attackerCharacter = attackerAgent?.Character;

            if (!MWRNaturalWeaponRules.TryGetUnarmedProfile(attackerCharacter, attackInformation, collisionData, out MWRNaturalWeaponProfile profile))
            {
                return result;
            }

            return MWRNaturalWeaponRules.ApplyProfileDamage(profile, result);
        }
    }
}
