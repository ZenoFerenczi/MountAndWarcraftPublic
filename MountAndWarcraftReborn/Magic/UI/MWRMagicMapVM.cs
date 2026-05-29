using TaleWorlds.CampaignSystem;
using MountAndWarcraftReborn.Extensions;
using TaleWorlds.Library;

namespace MountAndWarcraftReborn.Magic.UI
{
    public class MWRMagicMapVM : ViewModel
    {
        private bool _isVisible;
        private string _manaText = string.Empty;
        private string _classText = string.Empty;

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
        public string ClassText
        {
            get => _classText;
            set
            {
                if (value != _classText)
                {
                    _classText = value;
                    OnPropertyChangedWithValue(value, nameof(ClassText));
                }
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            if (Hero.MainHero == null)
            {
                IsVisible = false;
                return;
            }

            MWRMagicClassId classId = Hero.MainHero.GetMagicClass();
            IsVisible = classId != MWRMagicClassId.None;
            ClassText = classId.ToString();
            ManaText = $"{Hero.MainHero.GetMana():0}/{Hero.MainHero.GetMaxMana():0}";
        }

        public void ExecuteOpenSpellBook()
        {
            MWRMagicScreenHelper.OpenSpellBook();
        }
    }
}
