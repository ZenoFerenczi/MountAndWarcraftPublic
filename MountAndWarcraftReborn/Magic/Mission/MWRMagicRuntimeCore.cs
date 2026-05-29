using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace MountAndWarcraftReborn.Magic.Mission
{
    public enum MWRMagicAbilityModeState
    {
        Off = 0,
        QuickMenuSelection = 1,
        Targeting = 2,
        Casting = 3
    }

    public sealed class MWRMagicAbilityExecutionContext
    {
        public MWRMagicAbilityExecutionContext(
            MWRMagicAbilityManagerMissionLogic missionLogic,
            Agent caster,
            MWRMagicAgentComponent component,
            MWRMagicAbility ability)
        {
            MissionLogic = missionLogic;
            Caster = caster;
            Component = component;
            Ability = ability;
        }

        public MWRMagicAbilityManagerMissionLogic MissionLogic { get; }

        public Agent Caster { get; }

        public MWRMagicAgentComponent Component { get; }

        public MWRMagicAbility Ability { get; }
    }

    public sealed class MWRMagicAbilityTargetingResult
    {
        private MWRMagicAbilityTargetingResult()
        {
        }

        public bool IsValid { get; private set; }

        public Agent? PrimaryAgent { get; private set; }

        public Vec3 TargetPosition { get; private set; }

        public float Distance { get; private set; }

        public string SummaryText { get; private set; } = string.Empty;

        public IReadOnlyList<Agent> AffectedAgents { get; private set; } = Array.Empty<Agent>();

        public static MWRMagicAbilityTargetingResult Invalid(string summaryText)
        {
            return new MWRMagicAbilityTargetingResult
            {
                IsValid = false,
                SummaryText = summaryText ?? string.Empty,
                AffectedAgents = Array.Empty<Agent>()
            };
        }

        public static MWRMagicAbilityTargetingResult Create(
            Vec3 targetPosition,
            float distance,
            string summaryText,
            Agent? primaryAgent = null,
            IEnumerable<Agent>? affectedAgents = null)
        {
            return new MWRMagicAbilityTargetingResult
            {
                IsValid = true,
                PrimaryAgent = primaryAgent,
                TargetPosition = targetPosition,
                Distance = distance,
                SummaryText = summaryText ?? string.Empty,
                AffectedAgents = affectedAgents != null
                    ? affectedAgents.ToList()
                    : (primaryAgent != null ? new List<Agent> { primaryAgent } : Array.Empty<Agent>())
            };
        }
    }

    public sealed class MWRMagicCastSession
    {
        public MWRMagicCastSession(
            Agent caster,
            MWRMagicAgentComponent component,
            MWRMagicAbility ability,
            MWRMagicAbilityTargetingResult target,
            float activationTime,
            bool isMainAgentCast)
        {
            Caster = caster;
            Component = component;
            Ability = ability;
            Target = target;
            ActivationTime = activationTime;
            IsMainAgentCast = isMainAgentCast;
        }

        public Agent Caster { get; }

        public MWRMagicAgentComponent Component { get; }

        public MWRMagicAbility Ability { get; }

        public MWRMagicAbilityTargetingResult Target { get; }

        public float ActivationTime { get; }

        public bool IsMainAgentCast { get; }
    }

    public abstract class MWRMagicAbilityScript
    {
        public abstract bool TryResolveTarget(
            MWRMagicAbilityExecutionContext context,
            out MWRMagicAbilityTargetingResult result,
            out string failureReason);

        public abstract bool Activate(
            MWRMagicAbilityExecutionContext context,
            MWRMagicAbilityTargetingResult target);

        public virtual float ScoreAiUsage(
            MWRMagicAbilityExecutionContext context,
            MWRMagicAbilityTargetingResult target)
        {
            return target.IsValid ? 1f : 0f;
        }
    }

    public abstract class MWRMagicAbility
    {
        protected MWRMagicAbility(MWRMagicSpellTemplate template, MWRMagicAbilityScript script)
        {
            Template = template;
            Script = script;
        }

        public string StringId => Template.StringId;

        public MWRMagicSpellTemplate Template { get; }

        public MWRMagicAbilityScript Script { get; }

        public float CooldownRemaining { get; private set; }

        public bool IsOnCooldown => CooldownRemaining > 0f;

        public bool IsReady => !IsOnCooldown;

        public void Tick(float dt)
        {
            if (CooldownRemaining > 0f)
            {
                CooldownRemaining = Math.Max(0f, CooldownRemaining - dt);
            }
        }

        public void StartCooldown()
        {
            CooldownRemaining = Math.Max(0f, Template.CooldownSeconds);
        }

        public virtual bool IsDisabled(
            Agent caster,
            MWRMagicAgentComponent component,
            MWRMagicAbilityManagerMissionLogic missionLogic,
            out string failureReason)
        {
            if (caster == null || !caster.IsActive() || caster.Health <= 0f)
            {
                failureReason = "Caster is unable to act.";
                return true;
            }

            if (IsOnCooldown)
            {
                failureReason = $"{Template.Name} is cooling down.";
                return true;
            }

            if (component.CurrentMana + 0.001f < Template.ManaCost)
            {
                failureReason = "Not enough mana.";
                return true;
            }

            failureReason = string.Empty;
            return false;
        }

        public bool TryResolveTarget(
            MWRMagicAbilityExecutionContext context,
            out MWRMagicAbilityTargetingResult result,
            out string failureReason)
        {
            return Script.TryResolveTarget(context, out result, out failureReason);
        }

        public bool Activate(
            MWRMagicAbilityExecutionContext context,
            MWRMagicAbilityTargetingResult target)
        {
            return Script.Activate(context, target);
        }
    }

    public sealed class MWRMagicSpell : MWRMagicAbility
    {
        public MWRMagicSpell(MWRMagicSpellTemplate template, MWRMagicAbilityScript script)
            : base(template, script)
        {
        }
    }

    public sealed class MWRSingleTargetDamageAbilityScript : MWRMagicAbilityScript
    {
        public override bool TryResolveTarget(
            MWRMagicAbilityExecutionContext context,
            out MWRMagicAbilityTargetingResult result,
            out string failureReason)
        {
            return MWRMagicAbilityScriptShared.TryResolveSingleAgent(context, true, false, out result, out failureReason);
        }

        public override bool Activate(
            MWRMagicAbilityExecutionContext context,
            MWRMagicAbilityTargetingResult target)
        {
            if (target.PrimaryAgent == null)
            {
                return false;
            }

            MWRMagicMissionHelper.ApplyDamage(target.PrimaryAgent, context.Ability.Template.Power, context.Caster);
            return true;
        }
    }

    public sealed class MWRSingleTargetHealAbilityScript : MWRMagicAbilityScript
    {
        public override bool TryResolveTarget(
            MWRMagicAbilityExecutionContext context,
            out MWRMagicAbilityTargetingResult result,
            out string failureReason)
        {
            if (context.Ability.Template.TargetType == MWRMagicSpellTargetType.Self ||
                context.Ability.Template.CrosshairType == MWRMagicCrosshairType.Self)
            {
                result = MWRMagicAbilityTargetingResult.Create(
                    context.Caster.Position,
                    0f,
                    "Self",
                    context.Caster,
                    new[] { context.Caster });
                failureReason = string.Empty;
                return true;
            }

            return MWRMagicAbilityScriptShared.TryResolveSingleAgent(context, false, true, out result, out failureReason);
        }

        public override bool Activate(
            MWRMagicAbilityExecutionContext context,
            MWRMagicAbilityTargetingResult target)
        {
            Agent healTarget = target.PrimaryAgent ?? context.Caster;
            MWRMagicMissionHelper.ApplyHeal(healTarget, context.Ability.Template.Power);
            return true;
        }

        public override float ScoreAiUsage(
            MWRMagicAbilityExecutionContext context,
            MWRMagicAbilityTargetingResult target)
        {
            if (!target.IsValid || target.PrimaryAgent == null)
            {
                return 0f;
            }

            float missingHealth = Math.Max(0f, target.PrimaryAgent.HealthLimit - target.PrimaryAgent.Health);
            return missingHealth;
        }
    }

    public sealed class MWRAreaDamageAbilityScript : MWRMagicAbilityScript
    {
        public override bool TryResolveTarget(
            MWRMagicAbilityExecutionContext context,
            out MWRMagicAbilityTargetingResult result,
            out string failureReason)
        {
            MWRMagicSpellTemplate template = context.Ability.Template;
            IEnumerable<Agent> targets;
            Vec3 center;

            if (template.CrosshairType == MWRMagicCrosshairType.Self || template.TargetType == MWRMagicSpellTargetType.NearbyEnemies)
            {
                center = context.Caster.Position;
                targets = MWRMagicMissionHelper.FindNearbyAgents(context.Caster, center, template.Radius, true);
            }
            else
            {
                Agent? anchor = MWRMagicMissionHelper.FindBestFacingAgent(context.Caster, template.Range, true, false, template.TargetCaptureRadius);
                center = anchor?.Position ?? MWRMagicMissionHelper.GetPointInFrontOfAgent(context.Caster, template.Range);
                targets = MWRMagicMissionHelper.FindNearbyAgents(context.Caster, center, template.Radius, true);
            }

            List<Agent> affectedAgents = targets.ToList();
            if (affectedAgents.Count == 0)
            {
                result = MWRMagicAbilityTargetingResult.Invalid("No enemies in range.");
                failureReason = "No enemies in range.";
                return false;
            }

            result = MWRMagicAbilityTargetingResult.Create(
                center,
                center.Distance(context.Caster.Position),
                $"{affectedAgents.Count} enemies in area",
                affectedAgents[0],
                affectedAgents);
            failureReason = string.Empty;
            return true;
        }

        public override bool Activate(
            MWRMagicAbilityExecutionContext context,
            MWRMagicAbilityTargetingResult target)
        {
            bool applied = false;
            foreach (Agent affectedAgent in target.AffectedAgents)
            {
                MWRMagicMissionHelper.ApplyDamage(affectedAgent, context.Ability.Template.Power, context.Caster);
                applied = true;
            }

            return applied;
        }

        public override float ScoreAiUsage(
            MWRMagicAbilityExecutionContext context,
            MWRMagicAbilityTargetingResult target)
        {
            return !target.IsValid ? 0f : target.AffectedAgents.Count * context.Ability.Template.Power;
        }
    }

    public sealed class MWRAreaHealAbilityScript : MWRMagicAbilityScript
    {
        public override bool TryResolveTarget(
            MWRMagicAbilityExecutionContext context,
            out MWRMagicAbilityTargetingResult result,
            out string failureReason)
        {
            MWRMagicSpellTemplate template = context.Ability.Template;
            Vec3 center = context.Caster.Position;
            List<Agent> affectedAgents = MWRMagicMissionHelper
                .FindNearbyAgents(context.Caster, center, template.Radius, false)
                .Where(agent => agent.Health < agent.HealthLimit)
                .ToList();

            if (affectedAgents.Count == 0)
            {
                result = MWRMagicAbilityTargetingResult.Create(center, 0f, "Self", context.Caster, new[] { context.Caster });
                failureReason = string.Empty;
                return true;
            }

            result = MWRMagicAbilityTargetingResult.Create(center, 0f, $"{affectedAgents.Count} allies in area", affectedAgents[0], affectedAgents);
            failureReason = string.Empty;
            return true;
        }

        public override bool Activate(
            MWRMagicAbilityExecutionContext context,
            MWRMagicAbilityTargetingResult target)
        {
            bool applied = false;
            foreach (Agent affectedAgent in target.AffectedAgents)
            {
                MWRMagicMissionHelper.ApplyHeal(affectedAgent, context.Ability.Template.Power);
                applied = true;
            }

            if (!applied)
            {
                MWRMagicMissionHelper.ApplyHeal(context.Caster, context.Ability.Template.Power);
                return true;
            }

            return true;
        }
    }

    public sealed class MWRDrainLifeAbilityScript : MWRMagicAbilityScript
    {
        public override bool TryResolveTarget(
            MWRMagicAbilityExecutionContext context,
            out MWRMagicAbilityTargetingResult result,
            out string failureReason)
        {
            return MWRMagicAbilityScriptShared.TryResolveSingleAgent(context, true, false, out result, out failureReason);
        }

        public override bool Activate(
            MWRMagicAbilityExecutionContext context,
            MWRMagicAbilityTargetingResult target)
        {
            if (target.PrimaryAgent == null)
            {
                return false;
            }

            MWRMagicMissionHelper.ApplyDamage(target.PrimaryAgent, context.Ability.Template.Power, context.Caster);
            MWRMagicMissionHelper.ApplyHeal(context.Caster, Math.Max(1, context.Ability.Template.Power / 2));
            return true;
        }
    }

    public sealed class MWRProjectileDamageAbilityScript : MWRMagicAbilityScript
    {
        public override bool TryResolveTarget(
            MWRMagicAbilityExecutionContext context,
            out MWRMagicAbilityTargetingResult result,
            out string failureReason)
        {
            Agent? target = MWRMagicMissionHelper.FindBestFacingAgent(
                context.Caster,
                context.Ability.Template.Range,
                true,
                false,
                context.Ability.Template.TargetCaptureRadius);

            Vec3 targetPosition = target?.GetChestGlobalPosition() ??
                                  MWRMagicMissionHelper.GetPointInFrontOfAgent(context.Caster, context.Ability.Template.Range);

            float distance = targetPosition.Distance(context.Caster.GetEyeGlobalPosition());
            if (distance < Math.Max(0.1f, context.Ability.Template.MinRange))
            {
                result = MWRMagicAbilityTargetingResult.Invalid("Target is too close.");
                failureReason = "Target is too close.";
                return false;
            }

            result = MWRMagicAbilityTargetingResult.Create(
                targetPosition,
                distance,
                target != null ? $"{target.Character.Name}: {distance:0.#}m" : $"Impact point: {distance:0.#}m",
                target,
                target != null ? new[] { target } : Array.Empty<Agent>());
            failureReason = string.Empty;
            return true;
        }

        public override bool Activate(
            MWRMagicAbilityExecutionContext context,
            MWRMagicAbilityTargetingResult target)
        {
            return context.MissionLogic.LaunchProjectileAbility(context.Caster, context.Ability.Template, target);
        }

        public override float ScoreAiUsage(
            MWRMagicAbilityExecutionContext context,
            MWRMagicAbilityTargetingResult target)
        {
            return !target.IsValid ? 0f : context.Ability.Template.Power + Math.Max(0f, context.Ability.Template.Radius * 5f);
        }
    }

    public sealed class MWRStatBuffAbilityScript : MWRMagicAbilityScript
    {
        public override bool TryResolveTarget(
            MWRMagicAbilityExecutionContext context,
            out MWRMagicAbilityTargetingResult result,
            out string failureReason)
        {
            if (context.Ability.Template.TargetType == MWRMagicSpellTargetType.Self)
            {
                result = MWRMagicAbilityTargetingResult.Create(context.Caster.Position, 0f, "Self", context.Caster, new[] { context.Caster });
                failureReason = string.Empty;
                return true;
            }

            return MWRMagicAbilityScriptShared.TryResolveSingleAgent(context, false, false, out result, out failureReason);
        }

        public override bool Activate(
            MWRMagicAbilityExecutionContext context,
            MWRMagicAbilityTargetingResult target)
        {
            Agent buffTarget = target.PrimaryAgent ?? context.Caster;
            return context.MissionLogic.TryApplyTemporaryBuff(buffTarget, context.Ability.Template);
        }
    }

    public static class MWRMagicAbilityFactory
    {
        public static MWRMagicAbility? CreateAbility(MWRMagicSpellTemplate template)
        {
            MWRMagicAbilityScript? script = CreateScript(template);
            return script == null ? null : new MWRMagicSpell(template, script);
        }

        private static MWRMagicAbilityScript? CreateScript(MWRMagicSpellTemplate template)
        {
            switch (template.ScriptType)
            {
                case MWRMagicAbilityScriptType.SingleTargetDamage:
                    return new MWRSingleTargetDamageAbilityScript();
                case MWRMagicAbilityScriptType.SingleTargetHeal:
                    return new MWRSingleTargetHealAbilityScript();
                case MWRMagicAbilityScriptType.AreaDamage:
                    return new MWRAreaDamageAbilityScript();
                case MWRMagicAbilityScriptType.AreaHeal:
                    return new MWRAreaHealAbilityScript();
                case MWRMagicAbilityScriptType.DrainLife:
                    return new MWRDrainLifeAbilityScript();
                case MWRMagicAbilityScriptType.ProjectileDamage:
                    return new MWRProjectileDamageAbilityScript();
                case MWRMagicAbilityScriptType.StatBuff:
                    return new MWRStatBuffAbilityScript();
                default:
                    return CreateLegacyFallbackScript(template);
            }
        }

        private static MWRMagicAbilityScript? CreateLegacyFallbackScript(MWRMagicSpellTemplate template)
        {
            switch (template.EffectType)
            {
                case MWRMagicSpellEffectType.Heal:
                    return new MWRSingleTargetHealAbilityScript();
                case MWRMagicSpellEffectType.AreaDamage:
                    return new MWRAreaDamageAbilityScript();
                case MWRMagicSpellEffectType.AreaHeal:
                    return new MWRAreaHealAbilityScript();
                case MWRMagicSpellEffectType.DamageAndSelfHeal:
                    return new MWRDrainLifeAbilityScript();
                case MWRMagicSpellEffectType.ProjectileDamage:
                    return new MWRProjectileDamageAbilityScript();
                default:
                    return new MWRSingleTargetDamageAbilityScript();
            }
        }
    }

    internal static class MWRMagicAbilityScriptShared
    {
        public static bool TryResolveSingleAgent(
            MWRMagicAbilityExecutionContext context,
            bool enemies,
            bool woundedOnly,
            out MWRMagicAbilityTargetingResult result,
            out string failureReason)
        {
            Agent? target = MWRMagicMissionHelper.FindBestFacingAgent(
                context.Caster,
                context.Ability.Template.Range,
                enemies,
                woundedOnly,
                context.Ability.Template.TargetCaptureRadius);

            if (target == null)
            {
                string targetDescription = woundedOnly ? "wounded ally" : enemies ? "enemy" : "ally";
                result = MWRMagicAbilityTargetingResult.Invalid($"No {targetDescription} in range.");
                failureReason = $"No {targetDescription} in range.";
                return false;
            }

            float distance = target.Position.Distance(context.Caster.Position);
            if (distance < context.Ability.Template.MinRange)
            {
                result = MWRMagicAbilityTargetingResult.Invalid("Target is too close.");
                failureReason = "Target is too close.";
                return false;
            }

            result = MWRMagicAbilityTargetingResult.Create(
                target.GetChestGlobalPosition(),
                distance,
                $"{target.Character.Name}: {distance:0.#}m",
                target,
                new[] { target });
            failureReason = string.Empty;
            return true;
        }
    }
}
