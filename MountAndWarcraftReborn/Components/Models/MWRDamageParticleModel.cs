using MountAndWarcraftReborn.BattleMechanics;
using TaleWorlds.MountAndBlade;

namespace MountAndWarcraftReborn.Components.Models
{
    public class MWRDamageParticleModel : DefaultDamageParticleModel
    {
        public override void GetMeleeAttackBloodParticles(Agent attacker, Agent victim, in Blow blow, in AttackCollisionData collisionData, out HitParticleResultData particleResultData)
        {
            if (MWRNoBleedRules.ShouldNotBleed(victim))
            {
                SuppressParticles(out particleResultData);
                return;
            }

            base.GetMeleeAttackBloodParticles(attacker, victim, blow, collisionData, out particleResultData);
        }

        public override void GetMeleeAttackSweatParticles(Agent attacker, Agent victim, in Blow blow, in AttackCollisionData collisionData, out HitParticleResultData particleResultData)
        {
            if (MWRNoBleedRules.ShouldNotBleed(victim))
            {
                SuppressParticles(out particleResultData);
                return;
            }

            base.GetMeleeAttackSweatParticles(attacker, victim, blow, collisionData, out particleResultData);
        }

        public override int GetMissileAttackParticle(Agent attacker, Agent victim, in Blow blow, in AttackCollisionData collisionData)
        {
            if (MWRNoBleedRules.ShouldNotBleed(victim))
            {
                return -1;
            }

            return base.GetMissileAttackParticle(attacker, victim, blow, collisionData);
        }

        private static void SuppressParticles(out HitParticleResultData particleResultData)
        {
            particleResultData.ContinueHitParticleIndex = -1;
            particleResultData.StartHitParticleIndex = -1;
            particleResultData.EndHitParticleIndex = -1;
        }
    }
}
