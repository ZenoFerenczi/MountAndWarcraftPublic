namespace MountAndWarcraftReborn.Magic.Campaign
{
    public static class MWRCharacterCreationMagicSelection
    {
        public static bool HasPendingSelection { get; private set; }

        public static MWRMagicClassId PendingClassId { get; private set; } = MWRMagicClassId.None;

        public static void SetPendingClass(MWRMagicClassId classId)
        {
            PendingClassId = classId;
            HasPendingSelection = true;
        }

        public static void Clear()
        {
            PendingClassId = MWRMagicClassId.None;
            HasPendingSelection = false;
        }
    }
}
