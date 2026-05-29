using System.Linq;
using MountAndWarcraftReborn.Behaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace MountAndWarcraftReborn.BattleMechanics
{
    public class MWRHirelingBattleMissionLogic : MissionLogic
    {
        private bool _fixedHirelingSpawn;

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (!_fixedHirelingSpawn)
            {
                TryFixHirelingSpawn();
            }
        }

        private void TryFixHirelingSpawn()
        {
            MWRHirelingCampaignBehavior hirelingBehavior = Campaign.Current?.GetCampaignBehavior<MWRHirelingCampaignBehavior>();
            if (hirelingBehavior?.IsEnlisted() != true)
            {
                _fixedHirelingSpawn = true;
                return;
            }

            if (Mission.Mode == MissionMode.Deployment || Mission.IsTeleportingAgents || Mission.IsSiegeBattle)
            {
                return;
            }

            PartyBase enlistedParty = hirelingBehavior.EnlistingLord?.PartyBelongedTo?.Party;
            Agent mainAgent = Mission.MainAgent;
            if (enlistedParty == null || mainAgent == null || !mainAgent.IsActive())
            {
                return;
            }

            Agent enlistedPartyAgent = Mission.Agents.FirstOrDefault(agent =>
                agent != mainAgent &&
                agent.IsActive() &&
                agent.IsHuman &&
                agent.Origin?.BattleCombatant == enlistedParty);

            if (enlistedPartyAgent == null)
            {
                return;
            }

            const float maxReasonableSpawnDistance = 12f;
            float maxReasonableSpawnDistanceSquared = maxReasonableSpawnDistance * maxReasonableSpawnDistance;
            float distanceToPartySquared = (enlistedPartyAgent.Position.AsVec2 - mainAgent.Position.AsVec2).LengthSquared;

            if (distanceToPartySquared > maxReasonableSpawnDistanceSquared)
            {
                Vec3 targetPosition = Mission.GetRandomPositionAroundPoint(enlistedPartyAgent.Position, 0.5f, 2f, true);
                mainAgent.TeleportToPosition(targetPosition);
            }

            _fixedHirelingSpawn = true;
        }
    }
}
