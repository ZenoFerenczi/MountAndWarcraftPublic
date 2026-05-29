using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using MissionContext = TaleWorlds.MountAndBlade.Mission;

namespace MountAndWarcraftReborn.Magic.Mission
{
    public static class MWRMagicMissionHelper
    {
        public static void ApplyDamage(Agent target, int damageAmount, Agent? caster)
        {
            if (target == null || damageAmount <= 0 || !target.IsHuman || !target.IsActive() || target.Health <= 0f)
            {
                return;
            }

            Agent damager = caster ?? target;

            Vec3 direction = target.LookDirection;
            if (direction.LengthSquared < 0.0001f)
            {
                direction = Vec3.Forward;
            }
            direction.Normalize();

            Blow blow = new Blow(damager.Index)
            {
                DamageType = DamageTypes.Blunt,
                BoneIndex = target.Monster?.HeadLookDirectionBoneIndex ?? sbyte.MinValue,
                GlobalPosition = target.GetChestGlobalPosition(),
                BaseMagnitude = damageAmount,
                InflictedDamage = damageAmount,
                DamageCalculated = true,
                AttackType = AgentAttackType.Standard,
                BlowFlag = BlowFlags.NoSound,
                VictimBodyPart = BoneBodyPartType.Chest,
                StrikeType = StrikeType.Thrust,
                Direction = -direction,
                SwingDirection = -direction
            };

            blow.WeaponRecord.FillAsMeleeBlow(null, null, -1, -1);
            blow.WeaponRecord.Weight = 5f;
            blow.WeaponRecord.Velocity = -direction * damageAmount;

            float remainingHealth = target.Health - damageAmount;
            if (remainingHealth <= 0f)
            {
                target.Die(blow);
                return;
            }

            sbyte mainHandItemBoneIndex = damager.Monster?.MainHandItemBoneIndex ?? (sbyte)-1;
            AttackCollisionData attackCollisionData = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(
                false,
                false,
                false,
                true,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                CombatCollisionResult.StrikeAgent,
                -1,
                1,
                2,
                blow.BoneIndex,
                blow.VictimBodyPart,
                mainHandItemBoneIndex,
                Agent.UsageDirection.AttackUp,
                -1,
                CombatHitResultFlags.NormalHit,
                0.5f,
                1f,
                0f,
                0f,
                0f,
                0f,
                0f,
                0f,
                Vec3.Up,
                blow.Direction,
                blow.GlobalPosition,
                Vec3.Zero,
                Vec3.Zero,
                target.Velocity,
                Vec3.Up);

            target.RegisterBlow(blow, attackCollisionData);
        }

        public static void ApplyHeal(Agent target, int healAmount)
        {
            if (target == null || healAmount <= 0 || !target.IsHuman || !target.IsActive() || target.Health <= 0f)
            {
                return;
            }

            target.Health = MBMath.ClampFloat(target.Health + healAmount, 0f, target.HealthLimit);
        }

        public static IEnumerable<Agent> FindNearbyAgents(Agent caster, Vec3 origin, float range, bool enemies)
        {
            if (MissionContext.Current == null)
            {
                yield break;
            }

            float rangeSquared = range * range;
            foreach (Agent otherAgent in MissionContext.Current.Agents)
            {
                if (otherAgent == null || otherAgent == caster || !otherAgent.IsHuman || !otherAgent.IsActive())
                {
                    continue;
                }

                if (enemies)
                {
                    if (caster.Team == null || otherAgent.Team == null || !caster.Team.IsEnemyOf(otherAgent.Team))
                    {
                        continue;
                    }
                }
                else
                {
                    if (caster.Team == null || otherAgent.Team == null || caster.Team.IsEnemyOf(otherAgent.Team))
                    {
                        continue;
                    }
                }

                if (otherAgent.Position.DistanceSquared(origin) <= rangeSquared)
                {
                    yield return otherAgent;
                }
            }
        }

        public static Agent? FindBestFacingAgent(
            Agent caster,
            float maxRange,
            bool enemies,
            bool woundedOnly,
            float targetCaptureRadius)
        {
            if (caster == null || MissionContext.Current == null)
            {
                return null;
            }

            Vec3 origin = caster.GetEyeGlobalPosition();
            Vec3 lookDirection = caster.LookDirection;
            if (lookDirection.LengthSquared < 0.0001f)
            {
                lookDirection = Vec3.Forward;
            }

            lookDirection.Normalize();
            float radius = Math.Max(0.5f, targetCaptureRadius);

            IEnumerable<Agent> candidates = FindNearbyAgents(caster, origin, maxRange, enemies);
            if (woundedOnly)
            {
                candidates = candidates.Where(agent => agent.Health < agent.HealthLimit);
            }

            Agent? bestAgent = null;
            float bestScore = float.MinValue;

            foreach (Agent candidate in candidates)
            {
                Vec3 toCandidate = candidate.GetChestGlobalPosition() - origin;
                float distanceSquared = toCandidate.LengthSquared;
                if (distanceSquared < 0.0001f)
                {
                    continue;
                }

                float distance = (float)Math.Sqrt(distanceSquared);
                Vec3 normalized = toCandidate;
                normalized.Normalize();

                float dot = Vec3.DotProduct(lookDirection, normalized);
                float radialDistance = distance * (1f - Math.Max(0f, dot));
                if (dot < 0.1f && radialDistance > radius)
                {
                    continue;
                }

                float score = (dot * 1000f) - distanceSquared;
                if (woundedOnly)
                {
                    score += (candidate.HealthLimit - candidate.Health) * 10f;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestAgent = candidate;
                }
            }

            if (bestAgent != null)
            {
                return bestAgent;
            }

            return candidates
                .OrderBy(agent => agent.Position.DistanceSquared(caster.Position))
                .FirstOrDefault();
        }

        public static Vec3 GetPointInFrontOfAgent(Agent caster, float distance)
        {
            Vec3 origin = caster.GetEyeGlobalPosition();
            Vec3 direction = caster.LookDirection;
            if (direction.LengthSquared < 0.0001f)
            {
                direction = Vec3.Forward;
            }

            direction.Normalize();
            float clampedDistance = Math.Max(1f, distance);
            return origin + (direction * clampedDistance);
        }
    }
}
