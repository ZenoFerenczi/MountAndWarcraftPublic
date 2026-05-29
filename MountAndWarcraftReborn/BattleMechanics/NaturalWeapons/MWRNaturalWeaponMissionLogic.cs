using System.Collections.Generic;
using SandBox.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MountAndWarcraftReborn.BattleMechanics.NaturalWeapons
{
    public class MWRNaturalWeaponMissionLogic : MissionLogic
    {
        private readonly List<Agent> _trackedAgents = new();

        public override void AfterStart()
        {
            base.AfterStart();

            if (!IsRelevantBattleMission())
            {
                return;
            }

            foreach (Agent agent in Mission.Agents)
            {
                TrackAgent(agent);
            }
        }

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            base.OnAgentBuild(agent, banner);

            TrackAgent(agent);
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

            _trackedAgents.Remove(affectedAgent);
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (!IsRelevantBattleMission())
            {
                return;
            }

            for (int i = _trackedAgents.Count - 1; i >= 0; i--)
            {
                Agent agent = _trackedAgents[i];
                if (agent == null || !agent.IsActive() || !MWRNaturalWeaponRules.ShouldPreventBlocking(agent))
                {
                    _trackedAgents.RemoveAt(i);
                    continue;
                }

                if (agent.CurrentGuardMode != Agent.GuardMode.None)
                {
                    agent.ResetGuard();
                }
            }
        }

        private void TrackAgent(Agent agent)
        {
            if (agent == null || !IsRelevantBattleMission() || !MWRNaturalWeaponRules.ShouldPreventBlocking(agent))
            {
                return;
            }

            if (_trackedAgents.Contains(agent))
            {
                return;
            }

            _trackedAgents.Add(agent);
            agent.ResetGuard();
        }

        private bool IsRelevantBattleMission()
        {
            return Mission != null &&
                   Mission.Mode == MissionMode.Battle &&
                   Mission.GetMissionBehavior<BattleAgentLogic>() != null;
        }
    }
}
