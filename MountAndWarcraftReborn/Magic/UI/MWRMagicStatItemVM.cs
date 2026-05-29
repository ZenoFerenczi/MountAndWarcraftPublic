using TaleWorlds.Library;

namespace MountAndWarcraftReborn.Magic.UI
{
    public class MWRMagicStatItemVM : ViewModel
    {
        public MWRMagicStatItemVM(string label, string value)
        {
            Label = label;
            Value = value;
        }

        [DataSourceProperty]
        public string Label { get; }

        [DataSourceProperty]
        public string Value { get; }
    }
}
