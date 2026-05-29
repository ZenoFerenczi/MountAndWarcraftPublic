using MountAndWarcraftReborn.BattleMechanics.Music;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MountAndWarcraftReborn.Behaviors
{
    public class MWRBattleResultMusicCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener(this, OnMissionEnded);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnMissionEnded(IMission missionObject)
        {
            Mission mission = missionObject as Mission;
            MissionResult missionResult = mission?.MissionResult;
            if (mission == null || missionResult == null || !missionResult.BattleResolved)
            {
                return;
            }

            bool playerVictory = missionResult.PlayerVictory;
            bool playerDefeated = missionResult.PlayerDefeated;
            if (!playerVictory && !playerDefeated)
            {
                return;
            }

            CultureObject culture = Hero.MainHero?.Culture;
            if (culture == null)
            {
                return;
            }

            MWRBattleResultMusicHandler.TryPlayForCulture(culture, playerVictory);
        }
    }
}
