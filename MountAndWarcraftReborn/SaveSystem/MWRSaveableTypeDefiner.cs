#nullable enable
using System.Collections.Generic;
using MountAndWarcraftReborn.Magic.Campaign;
using MountAndWarcraftReborn.Portals;
using TaleWorlds.SaveSystem;

namespace MountAndWarcraftReborn.SaveSystem
{
    public class MWRSaveableTypeDefiner : SaveableTypeDefiner
    {
        public MWRSaveableTypeDefiner()
            : base(892310)
        {
        }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(MWRPortalSiteComponent), 1);
            AddClassDefinition(typeof(MWRPortalRaidingPartyComponent), 2);
            AddClassDefinition(typeof(MWRMagicInfo), 3);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(Dictionary<string, double>));
            ConstructContainerDefinition(typeof(Dictionary<string, int>));
            ConstructContainerDefinition(typeof(Dictionary<string, string>));
            ConstructContainerDefinition(typeof(Dictionary<string, MWRMagicInfo>));
            ConstructContainerDefinition(typeof(List<string>));
        }
    }
}
