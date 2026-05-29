using System;
using System.Collections.Generic;
using System.Linq;
using MountAndWarcraftReborn.Behaviors;
using MountAndWarcraftReborn.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace MountAndWarcraftReborn.Magic.UI
{
    public class MWRSpellBookVM : ViewModel
    {
        private readonly Action _closeAction;
        private readonly List<Hero> _heroes;
        private int _currentHeroIndex;
        private HeroViewModel _currentCharacter = new HeroViewModel();
        private string _heroName = string.Empty;
        private string _className = string.Empty;
        private string _manaText = string.Empty;
        private string _regenText = string.Empty;
        private MBBindingList<MWRSpellItemVM> _spells = new();
        private MBBindingList<MWRMagicStatItemVM> _stats = new();

        public MWRSpellBookVM(Action closeAction)
        {
            _closeAction = closeAction;
            _heroes = GetCasterHeroes();

            if (_heroes.Count == 0 && Hero.MainHero != null)
            {
                _heroes.Add(Hero.MainHero);
            }

            RefreshCurrentHero();
        }

        [DataSourceProperty]
        public HeroViewModel CurrentCharacter
        {
            get => _currentCharacter;
            set
            {
                if (value != _currentCharacter)
                {
                    _currentCharacter = value;
                    OnPropertyChangedWithValue(value, nameof(CurrentCharacter));
                }
            }
        }

        [DataSourceProperty]
        public string HeroName
        {
            get => _heroName;
            set
            {
                if (value != _heroName)
                {
                    _heroName = value;
                    OnPropertyChangedWithValue(value, nameof(HeroName));
                }
            }
        }

        [DataSourceProperty]
        public string ClassName
        {
            get => _className;
            set
            {
                if (value != _className)
                {
                    _className = value;
                    OnPropertyChangedWithValue(value, nameof(ClassName));
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
        public string RegenText
        {
            get => _regenText;
            set
            {
                if (value != _regenText)
                {
                    _regenText = value;
                    OnPropertyChangedWithValue(value, nameof(RegenText));
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<MWRSpellItemVM> Spells
        {
            get => _spells;
            set
            {
                if (value != _spells)
                {
                    _spells = value;
                    OnPropertyChangedWithValue(value, nameof(Spells));
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<MWRMagicStatItemVM> Stats
        {
            get => _stats;
            set
            {
                if (value != _stats)
                {
                    _stats = value;
                    OnPropertyChangedWithValue(value, nameof(Stats));
                }
            }
        }

        public void ExecuteClose()
        {
            _closeAction?.Invoke();
        }

        public void ExecuteSelectPreviousHero()
        {
            if (_heroes.Count <= 1)
            {
                return;
            }

            _currentHeroIndex = (_currentHeroIndex - 1 + _heroes.Count) % _heroes.Count;
            RefreshCurrentHero();
        }

        public void ExecuteSelectNextHero()
        {
            if (_heroes.Count <= 1)
            {
                return;
            }

            _currentHeroIndex = (_currentHeroIndex + 1) % _heroes.Count;
            RefreshCurrentHero();
        }

        private void RefreshCurrentHero()
        {
            Hero hero = _heroes[_currentHeroIndex < 0 ? 0 : _currentHeroIndex];
            HeroName = hero.Name.ToString();
            ClassName = hero.GetMagicClass().ToString();
            ManaText = $"{hero.GetMana():0}/{hero.GetMaxMana():0}";
            RegenText = $"{hero.GetManaRegenPerHour():0.0} / hour";

            HeroViewModel heroViewModel = new HeroViewModel();
            heroViewModel.FillFrom(hero);
            heroViewModel.SetEquipment(EquipmentIndex.ArmorItemEndSlot, default);
            heroViewModel.SetEquipment(EquipmentIndex.HorseHarness, default);
            heroViewModel.SetEquipment(EquipmentIndex.NumAllWeaponSlots, default);
            CurrentCharacter = heroViewModel;

            List<string> selectedSpellIds = hero.GetSelectedSpellIds().ToList();
            List<MWRMagicSpellTemplate> knownSpells = hero.GetKnownSpellTemplates().ToList();

            Stats = new MBBindingList<MWRMagicStatItemVM>
            {
                new MWRMagicStatItemVM("Class", ClassName),
                new MWRMagicStatItemVM("Mana", ManaText),
                new MWRMagicStatItemVM("Regen", RegenText),
                new MWRMagicStatItemVM("Known Spells", knownSpells.Count.ToString()),
                new MWRMagicStatItemVM("Selected", selectedSpellIds.Count.ToString())
            };

            Spells = new MBBindingList<MWRSpellItemVM>();
            foreach (MWRMagicSpellTemplate template in knownSpells)
            {
                Spells.Add(new MWRSpellItemVM(
                    template.StringId,
                    template.Name,
                    template.Description,
                    $"Mana {template.ManaCost}",
                    $"{template.CooldownSeconds:0.#}s",
                    $"{template.CastType} | {template.CrosshairType}",
                    selectedSpellIds.Contains(template.StringId, StringComparer.Ordinal),
                    OnSpellToggle));
            }
        }

        private void OnSpellToggle(MWRSpellItemVM item)
        {
            Hero hero = _heroes[_currentHeroIndex < 0 ? 0 : _currentHeroIndex];
            MWRMagicManagerBehavior? behavior = TaleWorlds.CampaignSystem.Campaign.Current?.GetCampaignBehavior<MWRMagicManagerBehavior>();
            if (behavior == null)
            {
                return;
            }

            List<string> selectedSpellIds = hero.GetSelectedSpellIds().ToList();
            if (selectedSpellIds.Contains(item.SpellId, StringComparer.Ordinal))
            {
                selectedSpellIds.RemoveAll(id => string.Equals(id, item.SpellId, StringComparison.Ordinal));
            }
            else if (selectedSpellIds.Count < 4)
            {
                selectedSpellIds.Add(item.SpellId);
            }

            behavior.SetSelectedSpellIds(hero, selectedSpellIds);
            RefreshCurrentHero();
        }

        private static List<Hero> GetCasterHeroes()
        {
            if (Hero.MainHero == null)
            {
                return new List<Hero>();
            }

            List<Hero> heroes = new List<Hero>();
            if (Hero.MainHero.GetMagicClass() != MWRMagicClassId.None)
            {
                heroes.Add(Hero.MainHero);
            }

            if (MobileParty.MainParty != null)
            {
                foreach (Hero hero in Clan.PlayerClan.Heroes.Where(hero =>
                             hero != Hero.MainHero &&
                             hero.IsAlive &&
                             hero.PartyBelongedTo == MobileParty.MainParty &&
                             hero.GetMagicClass() != MWRMagicClassId.None))
                {
                    heroes.Add(hero);
                }
            }

            return heroes;
        }
    }
}
