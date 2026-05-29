using MountAndWarcraftReborn.BattleMechanics.NaturalWeapons;
using System;
using SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MountAndWarcraftReborn.Components.Models
{
    public class MWRAgentStatCalculateModel : SandboxAgentStatCalculateModel
    {
        public override void InitializeAgentStats(Agent agent, Equipment spawnEquipment, AgentDrivenProperties agentDrivenProperties, AgentBuildData agentBuildData)
        {
            base.InitializeAgentStats(agent, spawnEquipment, agentDrivenProperties, agentBuildData);
            ApplyNaturalWeaponDefenseRules(agent, agentDrivenProperties);
        }

        public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            base.UpdateAgentStats(agent, agentDrivenProperties);
            ApplyNaturalWeaponDefenseRules(agent, agentDrivenProperties);
        }

        private static void ApplyNaturalWeaponDefenseRules(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            if (!MWRNaturalWeaponRules.ShouldPreventBlocking(agent))
            {
                return;
            }

            agent.SetAgentFlags(agent.GetAgentFlags() & ~AgentFlag.CanDefend);
            agent.Defensiveness = 0.001f;
            agentDrivenProperties.SwingSpeedMultiplier *= 1.5f;
            agentDrivenProperties.AiMovementDelayFactor *= 0.25f;
            agentDrivenProperties.AIHoldingReadyMaxDuration = Math.Min(agentDrivenProperties.AIHoldingReadyMaxDuration, 0.3f);
            agentDrivenProperties.CombatMaxSpeedMultiplier *= 1.1f;
        }
    }
}
