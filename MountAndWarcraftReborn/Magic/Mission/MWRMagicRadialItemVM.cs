using System;
using TaleWorlds.Library;

namespace MountAndWarcraftReborn.Magic.Mission
{
    public class MWRMagicRadialItemVM : ViewModel
    {
        private readonly Action<int> _selectAction;
        private bool _isSelected;
        private bool _isDisabled;
        private string _metaText = string.Empty;

        public MWRMagicRadialItemVM(int index, string label, string name, bool isSelected, Action<int> selectAction)
        {
            Index = index;
            Label = label;
            Name = name;
            ShortName = BuildShortName(name);
            _selectAction = selectAction;
            IsSelected = isSelected;
        }

        public int Index { get; }

        [DataSourceProperty]
        public string Label { get; }

        [DataSourceProperty]
        public string Name { get; }

        [DataSourceProperty]
        public string ShortName { get; }

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
        public bool ShowShortName => !string.IsNullOrWhiteSpace(ShortName);

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

        public void ExecuteSelect()
        {
            _selectAction?.Invoke(Index);
        }

        private static string BuildShortName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            string[] parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string candidate = parts.Length > 1 ? parts[0] : name;
            return candidate.Length <= 10 ? candidate : candidate.Substring(0, 10);
        }
    }
}
