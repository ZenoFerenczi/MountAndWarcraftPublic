using TaleWorlds.Library;

namespace MountAndWarcraftReborn.Magic.Mission
{
    public class MWRMagicHudSpellVM : ViewModel
    {
        private bool _isSelected;
        private bool _isDisabled;
        private string _name = string.Empty;
        private string _cooldownText = string.Empty;
        private string _metaText = string.Empty;

        public MWRMagicHudSpellVM(string slotLabel)
        {
            SlotLabel = slotLabel;
        }

        [DataSourceProperty]
        public string SlotLabel { get; }

        [DataSourceProperty]
        public string Name
        {
            get => _name;
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChangedWithValue(value, nameof(Name));
                }
            }
        }

        [DataSourceProperty]
        public string CooldownText
        {
            get => _cooldownText;
            set
            {
                if (value != _cooldownText)
                {
                    _cooldownText = value;
                    OnPropertyChangedWithValue(value, nameof(CooldownText));
                }
            }
        }

        [DataSourceProperty]
        public string MetaText
        {
            get => _metaText;
            set
            {
                if (value != _metaText)
                {
                    _metaText = value;
                    OnPropertyChangedWithValue(value, nameof(MetaText));
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

        [DataSourceProperty]
        public bool IsDisabled
        {
            get => _isDisabled;
            set
            {
                if (value != _isDisabled)
                {
                    _isDisabled = value;
                    OnPropertyChangedWithValue(value, nameof(IsDisabled));
                }
            }
        }
    }
}
