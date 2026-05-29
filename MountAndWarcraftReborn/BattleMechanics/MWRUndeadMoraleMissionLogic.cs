using TaleWorlds.MountAndBlade;

namespace MountAndWarcraftReborn.BattleMechanics
{
    public class MWRUndeadMoraleMissionLogic : MissionLogic
    {
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            Mission.CanAgentRout_AdditionalCondition += CanAgentRout;
        }

        protected override void OnEndMission()
        {
            Mission.CanAgentRout_AdditionalCondition -= CanAgentRout;
            base.OnEndMission();
        }

        private bool CanAgentRout(Agent agent)
        {
            return !MWRUndeadMoraleHelper.IsUndeadAgent(agent);
        }
    }
}
