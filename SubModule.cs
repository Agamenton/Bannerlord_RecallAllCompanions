using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem;
using TaleWorlds.InputSystem;
using TaleWorlds.CampaignSystem.Actions;
using SandBox.View.Map;


namespace RecallAllCompanions
{

    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            try
            {
                if (gameStarterObject is CampaignGameStarter)
                {
                    var campaignGameStarter = (CampaignGameStarter)gameStarterObject;
                    InformationManager.DisplayMessage(new InformationMessage("Hotkey to recall companions is Ctrl+R", new Color(0.12f, 0.56f, 1.00f)));
                }
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage("Error initializing Recall All Companions Mod: " + ex.Message, new Color(1f, 0.08f, 0.58f)));
            }
        }

        // TODO: register hotkey instead of checking every tick
        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);

            try
            {
                Campaign campaign = Campaign.Current;
                if (campaign != null)
                {
                    var mapInstance = MapScreen.Instance;
                    if (mapInstance != null)
                    {
                        IInputContext input = MapScreen.Instance.Input;
                        if (input != null && input.IsControlDown() && input.IsKeyPressed(InputKey.R))
                        {
                            InformationManager.DisplayMessage(new InformationMessage("Recalling...", new Color(0.02f, 0.66f, 1.00f)));
                            RecallAllCompanions();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage("Error in Recall All Companions Mod: " + ex.Message, new Color(1f, 0.08f, 0.58f)));
            }            
        }

        /* Check if a hero is recallable:
         * - belongs to player's clan
         * - is active
         * - is not a prisoner
         * - is not in a party (ie caravan or leading another party)
         * - is not a governor
         */
        private bool IsRecallable(Hero hero)
        {
            return hero != null && hero.Clan == Clan.PlayerClan && hero.IsActive && !hero.IsPrisoner && hero.PartyBelongedTo == null && hero.GovernorOf == null;
        }

        private void RecallHero(Hero hero)
        {
            if (IsRecallable(hero))
            {
                if (hero.StayingInSettlement != null)
                {
                    hero.StayingInSettlement = null;
                }
                hero.ChangeState(Hero.CharacterStates.Released); // does nothing?

                AddHeroToPartyAction.Apply(hero, Hero.MainHero.PartyBelongedTo, true); // moves hero to player's party instantly

                hero.UpdateLastKnownClosestSettlement(null);  // does nothing?
                hero.ChangeState(Hero.CharacterStates.Active);  // does nothing?

                InformationManager.DisplayMessage(
                    new InformationMessage(
                        $"Recalled: {hero.Name} | Level: {hero.Level}"
                    )
                );
            }
            else
            {
                InformationManager.DisplayMessage(
                    new InformationMessage(
                        $"Cannot recall: {hero.Name}"
                    )
                );
            }
        }

        private void RecallAllCompanions()
        {
            Hero mainHero = Hero.MainHero;
            if (mainHero != null)
            {
                if(mainHero.IsPrisoner)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Cannot recall companions while you are a prisoner.", new Color(1f, 0.08f, 0.58f)));
                    return;
                }
            }

            Clan playerClan = Clan.PlayerClan;

            if (playerClan == null)
            {return;}

            IReadOnlyList<Hero> recallableCompanions = playerClan.Companions;
            foreach (Hero hero in recallableCompanions)
            {
                RecallHero(hero);                
            }

            IReadOnlyList<Hero> recallableFamilyMembers = playerClan.AliveLords;
            foreach (Hero hero in recallableFamilyMembers)
            {
                RecallHero(hero);
            }
        }
    }
}