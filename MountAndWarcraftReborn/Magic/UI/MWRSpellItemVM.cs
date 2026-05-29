using System;
using TaleWorlds.Library;

namespace MountAndWarcraftReborn.Magic.UI
{
    public class MWRSpellItemVM : ViewModel
    {
        private readonly Action<MWRSpellItemVM> _toggleAction;
        private bool _isSelected;
        private string _castText = string.Empty;

        public MWRSpellItemVM(
            string spellId,
            string name,
            string description,
            string manaCostText,
            string cooldownText,
            string castText,
            bool isSelected,
            Action<MWRSpellItemVM> toggleAction)
        {
            SpellId = spellId;
            Name = name;
            Description = description;
            ManaCostText = manaCostText;
            CooldownText = cooldownText;
            CastText = castText;
            _toggleAction = toggleAction;
            IsSelected = isSelected;
        }

        public string SpellId { get; }

        [DataSourceProperty]
        public string Name { get; }

        [DataSourceProperty]
        public string Description { get; }

        [DataSourceProperty]
        public string ManaCostText { get; }

        [DataSourceProperty]
        public string CooldownText { get; }

        [DataSourceProperty]
        public string CastText
        {
            get => _castText;
            set
            {
                if (value != _castText)
                {
                    _castText = value;
                    OnPropertyChangedWithValue(value, nameof(CastText));
                }
            }
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChangedWithValue(value, nameof(IsSelected));
                }
            }
        }

        public void ExecuteToggleSelected()
        {
            _toggleAction?.Invoke(this);
        }
    }
}
