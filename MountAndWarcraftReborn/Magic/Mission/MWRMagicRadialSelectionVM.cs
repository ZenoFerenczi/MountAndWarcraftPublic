using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace MountAndWarcraftReborn.Magic.Mission
{
    public class MWRMagicRadialSelectionVM : ViewModel
    {
        private bool _isVisible;
        private string _currentSpellName = "No Spell Selected";
        private string _currentSpellMeta = "Q Open/Close  |  F Cast";
        private string _currentSpellDescription = "Choose a spell, then press F to cast it.";
        private string _stateText = string.Empty;
        private MBBindingList<MWRMagicRadialItemVM> _items = new();

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
        public MBBindingList<MWRMagicRadialItemVM> Items
        {
            get => _items;
            set
            {
                if (value != _items)
                {
                    _items = value;
                    OnPropertyChangedWithValue(value, nameof(Items));
                }
            }
        }

        [DataSourceProperty]
        public string CurrentSpellName
        {
            get => _currentSpellName;
            set
            {
                if (value != _currentSpellName)
                {
                    _currentSpellName = value;
                    OnPropertyChangedWithValue(value, nameof(CurrentSpellName));
                }
            }
        }

        [DataSourceProperty]
        public string CurrentSpellMeta
        {
            get => _currentSpellMeta;
            set
            {
                if (value != _currentSpellMeta)
                {
                    _currentSpellMeta = value;
                    OnPropertyChangedWithValue(value, nameof(CurrentSpellMeta));
                }
            }
        }

        [DataSourceProperty]
        public string CurrentSpellDescription
        {
            get => _currentSpellDescription;
            set
            {
                if (value != _currentSpellDescription)
                {
                    _currentSpellDescription = value;
                    OnPropertyChangedWithValue(value, nameof(CurrentSpellDescription));
                }
            }
        }

        [DataSourceProperty]
        public string StateText
        {
            get => _stateText;
            set
            {
                if (value != _stateText)
                {
                    _stateText = value;
                    OnPropertyChangedWithValue(value, nameof(StateText));
                }
            }
        }

        public void Refresh(MWRMagicAgentComponent? component, MWRMagicAbilityManagerMissionLogic? logic, Action<int> selectAction)
        {
            if (component == null)
            {
                Items = new MBBindingList<MWRMagicRadialItemVM>();
                SetCurrentSpell(null);
                StateText = string.Empty;
                return;
            }

            if (Items.Count != component.Abilities.Count)
            {
                InitializeItems(component, logic, selectAction);
            }

            for (int index = 0; index < component.Abilities.Count && index < Items.Count; index++)
            {
                MWRMagicAbility ability = component.Abilities[index];
                MWRMagicRadialItemVM itemVm = Items[index];
                itemVm.IsSelected = index == component.SelectedSpellIndex;
                itemVm.MetaText = ability.IsReady ? $"Mana {ability.Template.ManaCost}" : $"{ability.CooldownRemaining:0.#}s";
                itemVm.IsDisabled = logic != null && Agent.Main != null && ability.IsDisabled(Agent.Main, component, logic, out _);
            }

            SetCurrentSpell(component.SelectedAbility?.Template);
            StateText = logic?.GetStatusText(Agent.Main) ?? string.Empty;
        }

        public void InitializeItems(MWRMagicAgentComponent? component, MWRMagicAbilityManagerMissionLogic? logic, Action<int> selectAction)
        {
            MBBindingList<MWRMagicRadialItemVM> items = new();
            if (component != null)
            {
                for (int index = 0; index < component.Abilities.Count; index++)
                {
                    MWRMagicAbility ability = component.Abilities[index];
                    items.Add(new MWRMagicRadialItemVM(
                        index,
                        (index + 1).ToString(),
                        ability.Template.Name,
                        index == component.SelectedSpellIndex,
                        selectAction)
                    {
                        MetaText = ability.IsReady ? $"Mana {ability.Template.ManaCost}" : $"{ability.CooldownRemaining:0.#}s",
                        IsDisabled = logic != null && Agent.Main != null && ability.IsDisabled(Agent.Main, component, logic, out _)
                    });
                }
            }

            Items = items;
            SetCurrentSpell(component?.SelectedAbility?.Template);
            StateText = logic?.GetStatusText(Agent.Main) ?? string.Empty;
        }

        private void SetCurrentSpell(MWRMagicSpellTemplate? spellTemplate)
        {
            if (spellTemplate == null)
            {
                CurrentSpellName = "No Spell Selected";
                CurrentSpellMeta = "Q Open/Close  |  F Cast";
                CurrentSpellDescription = "Choose a spell, then press F to cast it.";
                return;
            }

            CurrentSpellName = spellTemplate.Name;
            CurrentSpellMeta = $"Mana {spellTemplate.ManaCost}  |  Cooldown {spellTemplate.CooldownSeconds:0.#}s  |  {spellTemplate.CastType}";
            CurrentSpellDescription = spellTemplate.Description;
        }
    }
}
