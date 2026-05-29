using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;

namespace MountAndWarcraftReborn.CampaignMechanics.PostBattleLoot
{
    public static class PendingLootedTroopManager
    {
        private static readonly TroopRoster MemberAdditions = TroopRoster.CreateDummyTroopRoster();
        private static readonly TroopRoster MemberRemovals = TroopRoster.CreateDummyTroopRoster();
        private static readonly TroopRoster PrisonerAdditions = TroopRoster.CreateDummyTroopRoster();
        private static readonly TroopRoster PrisonerRemovals = TroopRoster.CreateDummyTroopRoster();
        private static bool _clearAllMembers;
        private static bool _clearAllPrisoners;
        private static bool _membersApplied;
        private static bool _prisonersApplied;

        public static bool HasPendingModifications =>
            MemberAdditions.TotalManCount > 0 ||
            MemberRemovals.TotalManCount > 0 ||
            PrisonerAdditions.TotalManCount > 0 ||
            PrisonerRemovals.TotalManCount > 0 ||
            _clearAllMembers ||
            _clearAllPrisoners;

        public static void ClearPendingMembers() => _clearAllMembers = true;

        public static void ClearPendingPrisoners() => _clearAllPrisoners = true;

        public static void ApplyMemberModifications(TroopRoster roster)
        {
            if (_membersApplied)
            {
                return;
            }

            if (_clearAllMembers)
            {
                roster.Clear();
            }

            ApplyRemovals(roster, MemberRemovals);
            ApplyAdditions(roster, MemberAdditions);
            _membersApplied = true;
            TryClearAfterBothApplied();
        }

        public static void ApplyPrisonerModifications(TroopRoster roster)
        {
            if (_prisonersApplied)
            {
                return;
            }

            if (_clearAllPrisoners)
            {
                roster.Clear();
            }

            ApplyRemovals(roster, PrisonerRemovals);
            ApplyAdditions(roster, PrisonerAdditions);
            _prisonersApplied = true;
            TryClearAfterBothApplied();
        }

        public static void ConsumeMemberModifications()
        {
            MemberAdditions.Clear();
            MemberRemovals.Clear();
            _clearAllMembers = false;
            _membersApplied = false;
        }

        public static void ConsumePrisonerModifications()
        {
            PrisonerAdditions.Clear();
            PrisonerRemovals.Clear();
            _clearAllPrisoners = false;
            _prisonersApplied = false;
        }

        public static void ResetAllPendingState()
        {
            MemberAdditions.Clear();
            MemberRemovals.Clear();
            PrisonerAdditions.Clear();
            PrisonerRemovals.Clear();
            _clearAllMembers = false;
            _clearAllPrisoners = false;
            _membersApplied = false;
            _prisonersApplied = false;
        }

        private static void TryClearAfterBothApplied()
        {
            if (_membersApplied && _prisonersApplied)
            {
                ResetAllPendingState();
            }
        }

        private static void ApplyRemovals(TroopRoster roster, TroopRoster removals)
        {
            if (removals.TotalManCount <= 0)
            {
                return;
            }

            foreach (TroopRosterElement element in removals.GetTroopRoster())
            {
                if (element.Number <= 0)
                {
                    continue;
                }

                int currentCount = roster.GetTroopCount(element.Character);
                int toRemove = Math.Min(currentCount, element.Number);
                if (toRemove > 0)
                {
                    roster.AddToCounts(element.Character, -toRemove);
                }
            }
        }

        private static void ApplyAdditions(TroopRoster roster, TroopRoster additions)
        {
            if (additions.TotalManCount <= 0)
            {
                return;
            }

            foreach (TroopRosterElement element in additions.GetTroopRoster())
            {
                if (element.Number > 0)
                {
                    roster.AddToCounts(element.Character, element.Number);
                }
            }
        }
    }
}
