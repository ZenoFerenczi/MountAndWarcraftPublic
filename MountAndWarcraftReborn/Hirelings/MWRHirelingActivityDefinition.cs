using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace MountAndWarcraftReborn.Hirelings
{
    public sealed class MWRHirelingActivityDefinition
    {
        public MWRHirelingActivityDefinition(string menuText, string tooltipText, SkillObject skill)
        {
            MenuText = new TextObject(menuText);
            TooltipText = new TextObject(tooltipText);
            Skill = skill;
        }

        public TextObject MenuText { get; }

        public TextObject TooltipText { get; }

        public SkillObject Skill { get; }
    }
}
