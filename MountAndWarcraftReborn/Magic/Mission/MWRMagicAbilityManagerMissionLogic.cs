using System;
using System.Collections.Generic;
using System.Linq;
using MountAndWarcraftReborn.Behaviors;
using MountAndWarcraftReborn.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using CampaignState = TaleWorlds.CampaignSystem.Campaign;
using MissionContext = TaleWorlds.MountAndBlade.Mission;

namespace MountAndWarcraftReborn.Magic.Mission
{
    public class MWRMagicAbilityManagerMissionLogic : MissionLogic
    {
        private const string DefaultFireballProjectileItemId = "burning_arrows";
        private const int TimeRequestId = 27661;
        private readonly List<Agent> _trackedCasters = new();
        private readonly List<ActiveProjectile> _activeProjectiles = new();
        private readonly List<MWRMagicCastSession> _activeCastSessions = new();
        private int _nextProjectileId = 1000000;
        private MWRMagicAbilityModeState _mainAgentState = MWRMagicAbilityModeState.Off;
        private MWRMagicAbilityTargetingResult _currentTargetPreview = MWRMagicAbilityTargetingResult.Invalid(string.Empty);

        private sealed class ActiveProjectile
        {
            public ActiveProjectile(int missileId, Agent caster, MWRMagicSpellTemplate template, Vec3 startPosition, Vec3 lastKnownPosition)
            {
                MissileId = missileId;
                Caster = caster;
                Template = template;
                StartPosition = startPosition;
                LastKnownPosition = lastKnownPosition;
            }

            public int MissileId { get; }
            public Agent Caster { get; }
            public MWRMagicSpellTemplate Template { get; }
            public Vec3 StartPosition { get; }
            public Vec3 LastKnownPosition { get; set; }
        }

        public MWRMagicAbilityModeState CurrentState => _mainAgentState;
        public MWRMagicAbilityTargetingResult CurrentTargetPreview => _currentTargetPreview;

        public override void OnAgentCreated(Agent agent)
        {
            base.OnAgentCreated(agent);
            TryRegisterCaster(agent);
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
            affectedAgent?.GetComponent<MWRMagicAgentComponent>()?.SyncHeroMana();
            _trackedCasters.Remove(affectedAgent);
            _activeCastSessions.RemoveAll(session => session.Caster == affectedAgent);
            if (affectedAgent == Agent.Main)
            {
                ClearMainAgentState();
            }
        }

        protected override void OnEndMission()
        {
            base.OnEndMission();
            foreach (Agent agent in _trackedCasters)
            {
                agent.GetComponent<MWRMagicAgentComponent>()?.SyncHeroMana();
            }

            _activeCastSessions.Clear();
            _activeProjectiles.Clear();
            ClearMainAgentState();
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            TickActiveProjectiles();
            TickCastSessions();

            for (int index = _trackedCasters.Count - 1; index >= 0; index--)
            {
                Agent agent = _trackedCasters[index];
                if (agent == null || !agent.IsActive())
                {
                    _trackedCasters.RemoveAt(index);
                    continue;
                }

                MWRMagicAgentComponent? component = agent.GetComponent<MWRMagicAgentComponent>();
                if (component == null)
                {
                    _trackedCasters.RemoveAt(index);
                    continue;
                }

                component.Tick(dt);
                if (agent.IsAIControlled)
                {
                    TickAiCaster(agent, component, dt);
                }
            }

            UpdateMainAgentTargetingPreview();
        }

        public void SetQuickMenuOpen(bool isOpen)
        {
            if (_mainAgentState == MWRMagicAbilityModeState.Casting)
            {
                return;
            }

            if (isOpen)
            {
                if (_mainAgentState != MWRMagicAbilityModeState.Targeting)
                {
                    _mainAgentState = MWRMagicAbilityModeState.QuickMenuSelection;
                }
            }
            else if (_mainAgentState == MWRMagicAbilityModeState.QuickMenuSelection)
            {
                _mainAgentState = MWRMagicAbilityModeState.Off;
            }
        }

        public void CancelTargeting()
        {
            if (_mainAgentState == MWRMagicAbilityModeState.Targeting)
            {
                ClearMainAgentState();
            }
        }

        public bool TryCastSelectedSpell(Agent agent, out string failureReason)
        {
            MWRMagicAgentComponent? component = agent?.GetComponent<MWRMagicAgentComponent>();
            if (component == null)
            {
                failureReason = "No magic is available.";
                return false;
            }

            MWRMagicAbility? ability = component.SelectedAbility;
            if (ability == null)
            {
                failureReason = "No spell is selected.";
                return false;
            }

            if (agent == Agent.Main && !agent.IsAIControlled && ability.Template.RequiresTargeting)
            {
                if (_mainAgentState != MWRMagicAbilityModeState.Targeting)
                {
                    EnterTargetingMode();
                    UpdateMainAgentTargetingPreview();
                    failureReason = string.Empty;
                    return true;
                }

                return TryStartAbility(agent, component, ability, _currentTargetPreview, out failureReason);
            }

            return TryStartAbility(agent, component, ability, null, out failureReason);
        }

        public void SelectSpell(Agent agent, int index)
        {
            agent?.GetComponent<MWRMagicAgentComponent>()?.SelectSpell(index);
            if (agent == Agent.Main && _mainAgentState == MWRMagicAbilityModeState.Targeting)
            {
                UpdateMainAgentTargetingPreview();
            }
        }

        public string GetStatusText(Agent? mainAgent)
        {
            MWRMagicAgentComponent? component = mainAgent?.GetComponent<MWRMagicAgentComponent>();
            MWRMagicAbility? ability = component?.SelectedAbility;
            if (mainAgent == null || component == null || ability == null)
            {
                return string.Empty;
            }

            if (_mainAgentState == MWRMagicAbilityModeState.Casting)
            {
                return $"Casting {ability.Template.Name}...";
            }

            if (_mainAgentState == MWRMagicAbilityModeState.Targeting)
            {
                return _currentTargetPreview.IsValid
                    ? $"Targeting {ability.Template.Name}: {_currentTargetPreview.SummaryText}  |  F Cast  |  Q Cancel"
                    : $"Targeting {ability.Template.Name}: look at a valid target  |  F Cast  |  Q Cancel";
            }

            if (ability.IsDisabled(mainAgent, component, this, out string disabledReason))
            {
                return disabledReason;
            }

            return ability.Template.RequiresTargeting ? "Q Select  |  F Target and Cast" : "Q Select  |  F Cast";
        }

        public string GetSelectedAbilityMeta(Agent? mainAgent)
        {
            MWRMagicAbility? ability = mainAgent?.GetComponent<MWRMagicAgentComponent>()?.SelectedAbility;
            if (ability == null)
            {
                return string.Empty;
            }

            string cooldownText = ability.IsOnCooldown ? $"{ability.CooldownRemaining:0.#}s CD" : "Ready";
            string castText = ability.Template.CastType == MWRMagicCastType.WindUp ? $"Cast {ability.Template.CastTime:0.#}s" : "Instant";
            return $"Mana {ability.Template.ManaCost}  |  {cooldownText}  |  {castText}";
        }

        public bool LaunchProjectileAbility(Agent caster, MWRMagicSpellTemplate template, MWRMagicAbilityTargetingResult target)
        {
            Vec3 origin = caster.GetEyeGlobalPosition();
            Vec3 destination = target.IsValid ? target.TargetPosition : MWRMagicMissionHelper.GetPointInFrontOfAgent(caster, template.Range);
            Vec3 direction = destination - origin;
            if (direction.LengthSquared < 0.0001f)
            {
                direction = caster.LookDirection;
            }

            if (direction.LengthSquared < 0.0001f)
            {
                direction = Vec3.Forward;
            }

            direction.Normalize();
            string projectileItemId = string.IsNullOrWhiteSpace(template.ProjectileItemId) ? DefaultFireballProjectileItemId : template.ProjectileItemId;
            ItemObject? projectileItem = MBObjectManager.Instance.GetObject<ItemObject>(projectileItemId);
            if (projectileItem == null || MissionContext.Current == null)
            {
                ApplyProjectileImpact(caster, template, destination);
                return true;
            }

            Vec3 startPosition = origin + (direction * 1.2f);
            float projectileSpeed = template.ProjectileSpeed > 0f ? template.ProjectileSpeed : 28f;
            int missileId = _nextProjectileId++;
            MissionWeapon projectile = new MissionWeapon(projectileItem, null, null);
            MissionContext.Current.AddCustomMissile(caster, projectile, startPosition, direction, Mat3.CreateMat3WithForward(direction), projectileSpeed, projectileSpeed, false, null, missileId);
            _activeProjectiles.Add(new ActiveProjectile(missileId, caster, template, startPosition, startPosition));
            return true;
        }

        public bool TryApplyTemporaryBuff(Agent target, MWRMagicSpellTemplate template)
        {
            if (target == null || !target.IsActive())
            {
                return false;
            }

            MWRMagicMissionHelper.ApplyHeal(target, Math.Max(1, template.Power / 2));
            return true;
        }

        private bool TryStartAbility(Agent caster, MWRMagicAgentComponent component, MWRMagicAbility ability, MWRMagicAbilityTargetingResult? lockedTarget, out string failureReason)
        {
            if (ability.IsDisabled(caster, component, this, out failureReason))
            {
                return false;
            }

            MWRMagicAbilityExecutionContext context = new MWRMagicAbilityExecutionContext(this, caster, component, ability);
            MWRMagicAbilityTargetingResult target = lockedTarget != null && lockedTarget.IsValid
                ? lockedTarget
                : MWRMagicAbilityTargetingResult.Invalid(string.Empty);

            if (!target.IsValid && !ability.TryResolveTarget(context, out target, out failureReason))
            {
                return false;
            }

            if (!component.SpendMana(ability.Template.ManaCost))
            {
                failureReason = "Not enough mana.";
                return false;
            }

            if (ability.Template.CastType == MWRMagicCastType.WindUp && ability.Template.CastTime > 0.05f)
            {
                float activationTime = MissionContext.Current?.CurrentTime + ability.Template.CastTime ?? ability.Template.CastTime;
                bool isMainAgentCast = caster == Agent.Main;
                _activeCastSessions.Add(new MWRMagicCastSession(caster, component, ability, target, activationTime, isMainAgentCast));
                if (isMainAgentCast)
                {
                    _mainAgentState = MWRMagicAbilityModeState.Casting;
                    SetTimeDilation(false);
                }

                failureReason = string.Empty;
                return true;
            }

            bool activated = ability.Activate(context, target);
            if (!activated)
            {
                component.AddMana(ability.Template.ManaCost);
                failureReason = "The spell could not be applied.";
                return false;
            }

            ability.StartCooldown();
            if (caster == Agent.Main)
            {
                ClearMainAgentState();
            }

            failureReason = string.Empty;
            return true;
        }

        private void TickAiCaster(Agent agent, MWRMagicAgentComponent component, float dt)
        {
            if (agent.State != AgentState.Active || agent.Formation == null || agent.Team == null)
            {
                return;
            }

            if (agent.Formation.FiringOrder.OrderType == OrderType.HoldFire)
            {
                return;
            }

            component.SecondsUntilNextAiDecision -= dt;
            if (component.SecondsUntilNextAiDecision > 0f)
            {
                return;
            }

            component.SecondsUntilNextAiDecision = 2.25f + MBRandom.RandomFloatRanged(0f, 1.25f);
            float bestScore = 0f;
            MWRMagicAbility? bestAbility = null;
            MWRMagicAbilityTargetingResult? bestTarget = null;

            foreach (MWRMagicAbility ability in component.Abilities)
            {
                if (ability.IsDisabled(agent, component, this, out _))
                {
                    continue;
                }

                MWRMagicAbilityExecutionContext context = new MWRMagicAbilityExecutionContext(this, agent, component, ability);
                if (!ability.TryResolveTarget(context, out MWRMagicAbilityTargetingResult target, out _))
                {
                    continue;
                }

                float score = ability.Script.ScoreAiUsage(context, target);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestAbility = ability;
                    bestTarget = target;
                }
            }

            if (bestAbility == null || bestTarget == null)
            {
                return;
            }

            int selectedIndex = component.Abilities.IndexOf(bestAbility);
            if (selectedIndex >= 0)
            {
                component.SelectSpell(selectedIndex);
            }

            TryStartAbility(agent, component, bestAbility, bestTarget, out _);
        }

        private void TickCastSessions()
        {
            if (_activeCastSessions.Count == 0 || MissionContext.Current == null)
            {
                return;
            }

            float currentTime = MissionContext.Current.CurrentTime;
            for (int index = _activeCastSessions.Count - 1; index >= 0; index--)
            {
                MWRMagicCastSession session = _activeCastSessions[index];
                if (session.Caster == null || !session.Caster.IsActive())
                {
                    if (session.IsMainAgentCast)
                    {
                        ClearMainAgentState();
                    }

                    _activeCastSessions.RemoveAt(index);
                    continue;
                }

                if (currentTime < session.ActivationTime)
                {
                    continue;
                }

                MWRMagicAbilityExecutionContext context = new MWRMagicAbilityExecutionContext(this, session.Caster, session.Component, session.Ability);
                if (session.Ability.Activate(context, session.Target))
                {
                    session.Ability.StartCooldown();
                }

                if (session.IsMainAgentCast)
                {
                    ClearMainAgentState();
                }

                _activeCastSessions.RemoveAt(index);
            }
        }

        private void TickActiveProjectiles()
        {
            if (MissionContext.Current == null || _activeProjectiles.Count == 0)
            {
                return;
            }

            for (int index = _activeProjectiles.Count - 1; index >= 0; index--)
            {
                ActiveProjectile projectile = _activeProjectiles[index];
                if (projectile.Caster == null)
                {
                    _activeProjectiles.RemoveAt(index);
                    continue;
                }

                var missile = MissionContext.Current.MissilesList.FirstOrDefault(x => x.Index == projectile.MissileId);
                if (missile != null)
                {
                    projectile.LastKnownPosition = missile.GetPosition();
                    float travelledDistanceSquared = projectile.LastKnownPosition.DistanceSquared(projectile.StartPosition);
                    float maxDistanceSquared = projectile.Template.Range * projectile.Template.Range;
                    if (travelledDistanceSquared >= maxDistanceSquared)
                    {
                        MissionContext.Current.RemoveMissileAsClient(projectile.MissileId);
                        ApplyProjectileImpact(projectile.Caster, projectile.Template, projectile.LastKnownPosition);
                        _activeProjectiles.RemoveAt(index);
                    }

                    continue;
                }

                ApplyProjectileImpact(projectile.Caster, projectile.Template, projectile.LastKnownPosition);
                _activeProjectiles.RemoveAt(index);
            }
        }

        private void ApplyProjectileImpact(Agent caster, MWRMagicSpellTemplate template, Vec3 impactPosition)
        {
            float impactRadius = template.Radius > 0f ? template.Radius : 2.5f;
            List<Agent> targets = MWRMagicMissionHelper.FindNearbyAgents(caster, impactPosition, impactRadius, true).ToList();
            if (targets.Count == 0)
            {
                Agent? closestTarget = MWRMagicMissionHelper.FindBestFacingAgent(caster, impactRadius + 1.5f, true, false, template.TargetCaptureRadius);
                if (closestTarget != null)
                {
                    MWRMagicMissionHelper.ApplyDamage(closestTarget, template.Power, caster);
                }

                return;
            }

            foreach (Agent target in targets)
            {
                MWRMagicMissionHelper.ApplyDamage(target, template.Power, caster);
            }
        }

        private void UpdateMainAgentTargetingPreview()
        {
            if (_mainAgentState != MWRMagicAbilityModeState.Targeting)
            {
                _currentTargetPreview = MWRMagicAbilityTargetingResult.Invalid(string.Empty);
                return;
            }

            Agent? mainAgent = Agent.Main;
            MWRMagicAgentComponent? component = mainAgent?.GetComponent<MWRMagicAgentComponent>();
            MWRMagicAbility? ability = component?.SelectedAbility;
            if (mainAgent == null || component == null || ability == null)
            {
                ClearMainAgentState();
                return;
            }

            MWRMagicAbilityExecutionContext context = new MWRMagicAbilityExecutionContext(this, mainAgent, component, ability);
            if (!ability.TryResolveTarget(context, out MWRMagicAbilityTargetingResult result, out string failureReason))
            {
                _currentTargetPreview = MWRMagicAbilityTargetingResult.Invalid(failureReason);
                return;
            }

            _currentTargetPreview = result;
        }

        private void EnterTargetingMode()
        {
            _mainAgentState = MWRMagicAbilityModeState.Targeting;
            SetTimeDilation(true);
        }

        private void ClearMainAgentState()
        {
            _mainAgentState = MWRMagicAbilityModeState.Off;
            _currentTargetPreview = MWRMagicAbilityTargetingResult.Invalid(string.Empty);
            SetTimeDilation(false);
        }

        private void SetTimeDilation(bool enable)
        {
            if (MissionContext.Current == null)
            {
                return;
            }

            bool requestActive = MissionContext.Current.GetRequestedTimeSpeed(TimeRequestId, out _);
            if (enable && !requestActive)
            {
                MissionContext.Current.AddTimeSpeedRequest(new MissionContext.TimeSpeedRequest(0.35f, TimeRequestId));
            }
            else if (!enable && requestActive)
            {
                MissionContext.Current.RemoveTimeSpeedRequest(TimeRequestId);
            }
        }

        private void TryRegisterCaster(Agent agent)
        {
            if (agent == null || !agent.IsHuman || agent.GetComponent<MWRMagicAgentComponent>() != null)
            {
                return;
            }

            List<MWRMagicAbility> abilities = new();
            Hero? hero = null;
            bool usePersistentHeroMana = false;
            float currentMana = 0f;
            float maxMana = 0f;
            float manaRegenPerSecond = 0f;

            if (agent.Character is CharacterObject characterObject && characterObject.IsHero)
            {
                hero = characterObject.HeroObject;
                if (hero == null || hero.GetMagicClass() == MWRMagicClassId.None)
                {
                    return;
                }

                usePersistentHeroMana = true;
                abilities.AddRange(hero.GetSelectedSpellIds()
                    .Select(MWRMagicDataManager.GetSpellTemplate)
                    .Where(template => template != null)
                    .Select(template => MWRMagicAbilityFactory.CreateAbility(template!))
                    .Where(ability => ability != null)!
                    .Cast<MWRMagicAbility>());
                currentMana = hero.GetMana();
                maxMana = hero.GetMaxMana();
                manaRegenPerSecond = hero.GetManaRegenPerHour() / 3600f;
            }
            else
            {
                MWRMagicAssignment? assignment = MWRMagicDataManager.GetAssignment(agent.Character?.StringId ?? string.Empty);
                if (assignment == null || assignment.ClassId == MWRMagicClassId.None)
                {
                    return;
                }

                MWRMagicClassDefinition? definition = MWRMagicDataManager.GetClassDefinition(assignment.ClassId);
                if (definition == null)
                {
                    return;
                }

                IEnumerable<string> selectedSpellIds = assignment.GetSelectedSpellIds().Count > 0
                    ? assignment.GetSelectedSpellIds()
                    : definition.GetDefaultSelectedSpellIds();

                if (!selectedSpellIds.Any())
                {
                    selectedSpellIds = assignment.GetKnownSpellIds().Count > 0
                        ? assignment.GetKnownSpellIds()
                        : definition.GetStarterSpellIds();
                }

                abilities.AddRange(selectedSpellIds
                    .Select(MWRMagicDataManager.GetSpellTemplate)
                    .Where(template => template != null)
                    .Select(template => MWRMagicAbilityFactory.CreateAbility(template!))
                    .Where(ability => ability != null)!
                    .Cast<MWRMagicAbility>());
                currentMana = definition.MissionTroopMana;
                maxMana = definition.MissionTroopMana;
                manaRegenPerSecond = definition.ManaRegenPerHour / 3600f;
            }

            if (abilities.Count == 0 || maxMana <= 0f)
            {
                return;
            }

            MWRMagicManagerBehavior? magicBehavior = CampaignState.Current?.GetCampaignBehavior<MWRMagicManagerBehavior>();
            MWRMagicAgentComponent component = new MWRMagicAgentComponent(agent, hero, usePersistentHeroMana, currentMana, maxMana, manaRegenPerSecond, abilities, magicBehavior);
            if (agent.IsAIControlled)
            {
                component.SecondsUntilNextAiDecision = 2.25f + MBRandom.RandomFloatRanged(0f, 1.25f);
            }

            agent.AddComponent(component);
            _trackedCasters.Add(agent);
        }
    }
}
