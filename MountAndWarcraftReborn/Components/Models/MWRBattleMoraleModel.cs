using MountAndWarcraftReborn.BattleMechanics;
using SandBox.GameComponents;
using TaleWorlds.MountAndBlade;

namespace MountAndWarcraftReborn.Components.Models
{
    public class MWRBattleMoraleModel : SandboxBattleMoraleModel
    {
        public override bool CanPanicDueToMorale(Agent agent)
        {
            if (MWRUndeadMoraleHelper.IsUndeadAgent(agent))
            {
                return false;
            }

            return base.CanPanicDueToMorale(agent);
        }
    }
}
