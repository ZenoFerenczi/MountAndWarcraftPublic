using System.Collections.Generic;
using System.Linq;
using MountAndWarcraftReborn.Behaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace MountAndWarcraftReborn.Magic.Mission
{
    public class MWRMagicAgentComponent : AgentComponent
    {
        private readonly Hero? _hero;
        private readonly bool _usesPersistentHeroMana;
        private readonly float _manaRegenPerSecond;
        private readonly MWRMagicManagerBehavior? _magicBehavior;

        public MWRMagicAgentComponent(
            Agent agent,
            Hero? hero,
            bool usesPersistentHeroMana,
            float currentMana,
            float maxMana,
            float manaRegenPerSecond,
            IEnumerable<MWRMagicAbility> abilities,
            MWRMagicManagerBehavior? magicBehavior)
            : base(agent)
        {
            _hero = hero;
            _usesPersistentHeroMana = usesPersistentHeroMana;
            _manaRegenPerSecond = manaRegenPerSecond;
            _magicBehavior = magicBehavior;
            CurrentMana = currentMana;
            MaxMana = maxMana;
            Abilities = abilities.ToList();
        }

        public float CurrentMana { get; private set; }

        public float MaxMana { get; }

        public int SelectedSpellIndex { get; private set; }

        public float SecondsUntilNextAiDecision { get; set; }

        public List<MWRMagicAbility> Abilities { get; }

        public MWRMagicAbility? SelectedAbility =>
            SelectedSpellIndex >= 0 && SelectedSpellIndex < Abilities.Count ? Abilities[SelectedSpellIndex] : null;

        public List<MWRMagicRuntimeSpell> Spells => Abilities.Select(ability => new MWRMagicRuntimeSpell(ability)).ToList();

        public MWRMagicRuntimeSpell? SelectedSpell => SelectedAbility != null ? new MWRMagicRuntimeSpell(SelectedAbility) : null;

        public void Tick(float dt)
        {
            foreach (MWRMagicAbility ability in Abilities)
            {
                ability.Tick(dt);
            }

            if (_manaRegenPerSecond > 0f && CurrentMana < MaxMana)
            {
                AddMana(_manaRegenPerSecond * dt);
            }
        }

        public bool SpendMana(float manaCost)
        {
            if (CurrentMana + 0.001f < manaCost)
            {
                return false;
            }

            CurrentMana -= manaCost;
            SyncHeroMana();
            return true;
        }

        public void AddMana(float amount)
        {
            CurrentMana = MBMath.ClampFloat(CurrentMana + amount, 0f, MaxMana);
            SyncHeroMana();
        }

        public void SelectSpell(int index)
        {
            if (index >= 0 && index < Abilities.Count)
            {
                SelectedSpellIndex = index;
            }
        }

        public void SyncHeroMana()
        {
            if (_usesPersistentHeroMana && _hero != null && _magicBehavior != null)
            {
                float liveHeroMana = _magicBehavior.GetMana(_hero);
                _magicBehavior.AddMana(_hero, CurrentMana - liveHeroMana);
            }
        }
    }
}
