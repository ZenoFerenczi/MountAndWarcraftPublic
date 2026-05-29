using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace MountAndWarcraftReborn.Magic.Mission
{
    public class MWRMagicHUDVM : ViewModel
    {
        private bool _isVisible;
        private string _manaText = string.Empty;
        private string _selectedSpellName = string.Empty;
        private string _selectedSpellMeta = string.Empty;
        private string _statusText = string.Empty;
        private MBBindingList<MWRMagicHudSpellVM> _spells = new();

        [DataSourceProperty]
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (value != _isVisible)
                {
                    _isVisible = value;
                    OnPropertyChangedWithValue(value, nameof(IsVisible));
                }
            }
        }

        [DataSourceProperty]
        public string ManaText
        {
            get => _manaText;
            set
            {
                if (value != _manaText)
                {
                    _manaText = value;
                    OnPropertyChangedWithValue(value, nameof(ManaText));
                }
            }
        }

        [DataSourceProperty]
        public string SelectedSpellName
        {
            get => _selectedSpellName;
            set
            {
                if (value != _selectedSpellName)
                {
                    _selectedSpellName = value;
                    OnPropertyChangedWithValue(value, nameof(SelectedSpellName));
                }
            }
        }

        [DataSourceProperty]
        public string SelectedSpellMeta
        {
            get => _selectedSpellMeta;
            set
            {
                if (value != _selectedSpellMeta)
                {
                    _selectedSpellMeta = value;
                    OnPropertyChangedWithValue(value, nameof(SelectedSpellMeta));
                }
            }
        }

        [DataSourceProperty]
        public string StatusText
        {
            get => _statusText;
            set
            {
                if (value != _statusText)
                {
                    _statusText = value;
                    OnPropertyChangedWithValue(value, nameof(StatusText));
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<MWRMagicHudSpellVM> Spells
        {
            get => _spells;
            set
            {
                if (value != _spells)
                {
                    _spells = value;
                    OnPropertyChangedWithValue(value, nameof(Spells));
                }
            }
        }

        public void Refresh(Agent? mainAgent, MWRMagicAgentComponent? component, MWRMagicAbilityManagerMissionLogic? logic)
        {
            IsVisible = component != null;
            if (component == null)
            {
                SelectedSpellMeta = string.Empty;
                StatusText = string.Empty;
                return;
            }

            ManaText = $"Mana {component.CurrentMana:0}/{component.MaxMana:0}";
            SelectedSpellName = component.SelectedAbility?.Template.Name ?? "No Spell";
            SelectedSpellMeta = logic?.GetSelectedAbilityMeta(mainAgent) ?? string.Empty;
            StatusText = logic?.GetStatusText(mainAgent) ?? string.Empty;
            RefreshSpellStates(mainAgent, component, logic);
        }

        public void InitializeSpells(MWRMagicAgentComponent? component)
        {
            MBBindingList<MWRMagicHudSpellVM> spellSlots = new();
            if (component != null)
            {
                for (int index = 0; index < component.Abilities.Count; index++)
                {
                    MWRMagicAbility ability = component.Abilities[index];
                    spellSlots.Add(new MWRMagicHudSpellVM((index + 1).ToString())
                    {
                        Name = ability.Template.DisplayShortName,
                        CooldownText = ability.IsReady ? "Ready" : $"{ability.CooldownRemaining:0.#}s",
                        MetaText = $"Mana {ability.Template.ManaCost}",
                        IsSelected = index == component.SelectedSpellIndex
                    });
                }
            }

            Spells = spellSlots;
        }

        public void RefreshSpellStates(Agent? mainAgent, MWRMagicAgentComponent? component, MWRMagicAbilityManagerMissionLogic? logic)
        {
            if (component == null)
            {
                return;
            }

            if (Spells.Count != component.Abilities.Count)
            {
                InitializeSpells(component);
                return;
            }

            for (int index = 0; index < component.Abilities.Count; index++)
            {
                MWRMagicAbility ability = component.Abilities[index];
                MWRMagicHudSpellVM spellVm = Spells[index];
                spellVm.Name = ability.Template.DisplayShortName;
                spellVm.CooldownText = ability.IsReady ? "Ready" : $"{ability.CooldownRemaining:0.#}s";
                spellVm.MetaText = $"Mana {ability.Template.ManaCost}";
                spellVm.IsSelected = index == component.SelectedSpellIndex;
                spellVm.IsDisabled = mainAgent != null && logic != null && ability.IsDisabled(mainAgent, component, logic, out _);
            }
        }
    }
}
