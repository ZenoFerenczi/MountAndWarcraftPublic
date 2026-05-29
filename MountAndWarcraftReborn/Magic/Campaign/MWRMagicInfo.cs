using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace MountAndWarcraftReborn.Magic.Campaign
{
    public class MWRMagicInfo
    {
        [SaveableField(1)]
        public int ClassId;

        [SaveableField(2)]
        public float CurrentMana;

        [SaveableField(3)]
        public List<string> KnownSpellIds = new();

        [SaveableField(4)]
        public List<string> SelectedSpellIds = new();

        [SaveableField(5)]
        public bool StarterKitApplied;

        [SaveableField(6)]
        public float LastManaSyncHours;

        public MWRMagicClassId MagicClassId
        {
            get => (MWRMagicClassId)ClassId;
            set => ClassId = (int)value;
        }
    }
}
