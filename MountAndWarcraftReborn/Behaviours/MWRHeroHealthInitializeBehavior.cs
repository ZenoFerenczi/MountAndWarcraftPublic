using TaleWorlds.CampaignSystem;

namespace MWRMode.Behaviors
{
    public class MWRHeroHealthInitializeBehavior : CampaignBehaviorBase
    {
        private bool _pendingInitialPlayerHeal;

        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedPartialFollowUpEvent.AddNonSerializedListener(this, OnNewGameCreated);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_pendingInitialPlayerHeal", ref _pendingInitialPlayerHeal);
        }

        private void OnNewGameCreated(CampaignGameStarter starter, int index)
        {
            if (index == 0)
            {
                _pendingInitialPlayerHeal = true;
            }
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            if (!_pendingInitialPlayerHeal)
                return;

            if (Hero.MainHero == null)
                return;

            Hero.MainHero.HitPoints = Hero.MainHero.MaxHitPoints;
            _pendingInitialPlayerHeal = false;
        }
    }
}