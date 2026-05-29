using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace MountAndWarcraftReborn.Magic.Mission
{
    public class MWRMagicRadialItemWidget : ButtonWidget
    {
        public MWRMagicRadialItemWidget(UIContext context)
            : base(context)
        {
            if (!ContainsState("Selected"))
            {
                AddState("Selected");
            }

            if (!ContainsState("Default"))
            {
                AddState("Default");
            }

            if (!ContainsState("Pressed"))
            {
                AddState("Pressed");
            }

            if (!ContainsState("Hovered"))
            {
                AddState("Hovered");
            }

            if (!ContainsState("Disabled"))
            {
                AddState("Disabled");
            }
        }

        protected override void OnConnectedToRoot()
        {
            base.OnConnectedToRoot();
            boolPropertyChanged += OnBoolPropertyChanged;
        }

        protected override void OnDisconnectedFromRoot()
        {
            base.OnDisconnectedFromRoot();
            boolPropertyChanged -= OnBoolPropertyChanged;
        }

        private void OnBoolPropertyChanged(PropertyOwnerObject widget, string propertyName, bool value)
        {
            if (propertyName != "IsSelected")
            {
                return;
            }

            SetState(value ? "Selected" : "Default");
        }
    }
}
