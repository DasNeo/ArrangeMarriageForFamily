using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace ArrangeMarriageForFamily
{
    internal class ArrangeMarriageForFamilyBehavior : CampaignBehaviorBase
    {
        private Hero FamilyMember;
        private Hero SelectedSpouse;
        private bool MarryIntoPlayerClan;
        private bool SameClan = false;

        public override void RegisterEvents() => CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(AddMenuItems));

        private void AddMenuItems(CampaignGameStarter campaignGameStarter)
        {
            foreach (Hero hero in Hero.MainHero.Clan.Heroes)
            {
                if (hero.PartyBelongedTo != null && !hero.PartyBelongedTo.IsCaravan && hero.PartyBelongedTo.ActualClan != hero.Clan)
                    hero.PartyBelongedTo.RemoveParty();
            }
            campaignGameStarter.AddGameMenuOption("town_backstreet", "marry_family", new TextObject("{=arrangemarriage_talk_arrange}Arrange a marriage for clan member").ToString(), (a) =>
            {
                a.IsEnabled = true;
                return true;
            }, (a) => ShowFamilyList(), false, 1, false);
        }

        private void ShowFamilyList()
        {
            List<InquiryElement> inquiryElementList = new List<InquiryElement>();
            foreach (Hero aliveHero in Campaign.Current.AliveHeroes)
            {
                if (aliveHero.Clan == Hero.MainHero.Clan 
                    && (double)aliveHero.Age >= 18.0 && aliveHero.Spouse == null 
                    && (aliveHero.Occupation == Occupation.Lord || aliveHero.Occupation == Occupation.Wanderer))
                    inquiryElementList.Add(new InquiryElement(aliveHero.CharacterObject.HeroObject, aliveHero.Name.ToString() + " - " + aliveHero.Age.ToString("0"), new ImageIdentifier(CharacterCode.CreateFrom(aliveHero.CharacterObject))));
            }
            if (inquiryElementList.Count < 1)
            {
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=arrangemarriage_marriage_not_possible_title}Arrange Marriage Not Possible").ToString(), new TextObject("{=arrangemarriage_marriage_not_possible_no_single_members}You have no single clan members").ToString(), true, false, new TextObject("{=arrangemarriage_ok}OK").ToString(), "", null, null, "", 0.0f, null), true);
                GameMenu.SwitchToMenu("town_backstreet");
            }
            else
                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    titleText: new TextObject("{=arrangemarriage_suitable_members_title}Members Suitable For Marriage").ToString(),
                    descriptionText: "", 
                    inquiryElements: inquiryElementList, 
                    isExitShown: true, 
                    minSelectableOptionCount: 1,
                    maxSelectableOptionCount: 1,
                    affirmativeText: new TextObject("{=arrangemarriage_continue}Continue").ToString(),
                    negativeText: null, 
                    affirmativeAction: args =>
                    {
                        List<InquiryElement> source = args;
                        if (source != null && !source.Any())
                            return;
                        InformationManager.HideInquiry();
                        SubModule.ExecuteActionOnNextTick(() => Part2(args.Select(element => element.Identifier as Hero)));
                    }, 
                    negativeAction: null,
                    soundEventPath: ""), false);
        }

        private void Part2(IEnumerable<Hero> family)
        {
            FamilyMember = family.First();
            ShowMatchesList();
        }

        private void ShowMatchesList()
        {
            List<InquiryElement> inquiryElementList = new List<InquiryElement>();
            if ((double)FamilyMember.Age >= 18.0)
            {
                inquiryElementList.Add(new InquiryElement(new TextObject("{=arrangemarriage_noble}Noble").ToString(), new TextObject("{=arrangemarriage_noble_text}Marry a minor noble").ToString(), null));
                inquiryElementList.Add(new InquiryElement(new TextObject("{=arrangemarriage_peasant}Peasant").ToString(), new TextObject("{=arrangemarriage_peasant_text}Marry a peasant").ToString(), null));
            }
            foreach (Hero aliveHero in Campaign.Current.AliveHeroes)
            {
                if (Campaign.Current.Models.MarriageModel.IsSuitableForMarriage(aliveHero) 
                    && FamilyMember.IsFemale != aliveHero.IsFemale)
                    inquiryElementList.Add(new InquiryElement(aliveHero.CharacterObject.HeroObject, aliveHero.EncyclopediaLinkWithName.ToString() + " " + (aliveHero.Clan != null ? aliveHero.Clan.EncyclopediaLinkWithName.ToString() : "") + " - " + aliveHero.Age.ToString("0"), new ImageIdentifier(CharacterCode.CreateFrom(aliveHero.CharacterObject)), true, HeroStats(aliveHero)));
            }
            if (inquiryElementList.Count < 1)
            {
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=arrangemarriage_no_matches}No Matches found").ToString(), "", true, false, new TextObject("{=arrangemarriage_ok}OK").ToString(), "", null, null, "", 0.0f, null), true);
                GameMenu.SwitchToMenu("town_backstreet");
            }
            else
                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    titleText: new TextObject("{=arrangemarriage_suitable_spouses_for_hero}Suitable Spouses for ").ToString() + FamilyMember.CharacterObject.GetName().ToString(),
                    descriptionText: "",
                    inquiryElements: inquiryElementList,
                    isExitShown: true,
                    minSelectableOptionCount: 1,
                    maxSelectableOptionCount: 1,
                    affirmativeText: new TextObject("{=arrangemarriage_continue}Continue").ToString(),
                    negativeText: null,
                    affirmativeAction: args =>
                {
                    List<InquiryElement> source = args;
                    if (source != null && !source.Any())
                        return;
                    InformationManager.HideInquiry();
                    SubModule.ExecuteActionOnNextTick(() => Part3(args.Select(element => element.Identifier).First()));
                }, 
                    negativeAction: null,
                    soundEventPath: ""), false);
        }

        private string HeroStats(Hero hero)
        {
            SkillObject[] skillObjectArray = new SkillObject[18]
            {
        DefaultSkills.Athletics,
        DefaultSkills.Bow,
        DefaultSkills.Charm,
        DefaultSkills.Crafting,
        DefaultSkills.Crossbow,
        DefaultSkills.Engineering,
        DefaultSkills.Leadership,
        DefaultSkills.Medicine,
        DefaultSkills.OneHanded,
        DefaultSkills.Polearm,
        DefaultSkills.Riding,
        DefaultSkills.Roguery,
        DefaultSkills.Scouting,
        DefaultSkills.Steward,
        DefaultSkills.Tactics,
        DefaultSkills.Throwing,
        DefaultSkills.Trade,
        DefaultSkills.TwoHanded
            };
            string str = "";
            for (int index = 0; index < skillObjectArray.Length; ++index)
            {
                if (hero.GetSkillValue(skillObjectArray[index]) > 0)
                    str = str + skillObjectArray[index].Name.ToString() + " : " + hero.GetSkillValue(skillObjectArray[index]).ToString() + "\n";
            }
            return str;
        }

        private void Part3(object selected)
        {
            if (selected.GetType() == typeof(string))
            {
                if ((string)selected == "Noble")
                {
                    MarryNoble();
                }
                else
                {
                    if (!((string)selected == "Peasant"))
                        return;
                    MarryPeasant();
                }
            }
            else
            {
                SelectedSpouse = (Hero)selected;
                if (SelectedSpouse.Clan == Hero.MainHero.Clan)
                {
                    MarryIntoPlayerClan = true;
                    SameClan = true;
                    Part4();
                }
                else
                {
                    SameClan = false;
                    if (SelectedSpouse.Clan == null)
                    {
                        MarryIntoPlayerClan = true;
                        Marriage();
                    }
                    else
                        MarriageType();
                }
            }
        }

        private void MarryPeasant()
        {
            IReadOnlyList<CharacterObject> lordTemplates = Settlement.CurrentSettlement.Culture.LordTemplates;
            CharacterObject characterObject = lordTemplates[new Random().Next(0, lordTemplates.Count - 1)];
            characterObject.IsFemale = !FamilyMember.IsFemale;
            Hero specialHero = HeroCreator.CreateSpecialHero(characterObject, Settlement.CurrentSettlement, null, FamilyMember.Clan, new Random().Next(Math.Max(18, (int)FamilyMember.Age - 3), (int)FamilyMember.Age + 3));
            specialHero.ChangeState((Hero.CharacterStates)1);
            specialHero.HeroDeveloper.DevelopCharacterStats();
            if (specialHero.IsFemale)
                specialHero.CharacterObject.Equipment.FillFrom(Settlement.CurrentSettlement.Culture.Townswoman.Equipment, true);
            else
                specialHero.CharacterObject.Equipment.FillFrom(Settlement.CurrentSettlement.Culture.Townsman.Equipment, true);
            specialHero.Clan = FamilyMember.Clan;//Hero.MainHero.Clan;
            specialHero.SetNewOccupation(Occupation.Lord);
            FamilyMember.SetNewOccupation(Occupation.Lord);
            MarriageAction.Apply(FamilyMember, specialHero, true);
            if(FamilyMember.PartyBelongedTo != null)
                AddHeroToPartyAction.Apply(specialHero, FamilyMember.PartyBelongedTo /*MobileParty.MainParty*/, true);
        }

        private void MarryNoble()
        {
            IReadOnlyList<CharacterObject> lordTemplates = Settlement.CurrentSettlement.Culture.LordTemplates;
            CharacterObject characterObject = lordTemplates[new Random().Next(0, lordTemplates.Count - 1)];
            characterObject.IsFemale = !FamilyMember.IsFemale;
            Hero specialHero = HeroCreator.CreateSpecialHero(characterObject, Settlement.CurrentSettlement, null, FamilyMember.Clan, new Random().Next(Math.Max(18, (int)FamilyMember.Age - 3), (int)FamilyMember.Age + 3));
            specialHero.ChangeState((Hero.CharacterStates)1);
            specialHero.HeroDeveloper.DevelopCharacterStats();
            specialHero.Clan = FamilyMember.Clan;//Hero.MainHero.Clan;
            specialHero.SetNewOccupation(Occupation.Lord);
            FamilyMember.SetNewOccupation(Occupation.Lord);
            MarriageAction.Apply(FamilyMember, specialHero, true);
            if(FamilyMember.PartyBelongedTo != null)
                AddHeroToPartyAction.Apply(specialHero, FamilyMember.PartyBelongedTo, true);
        }

        public static int GetEquipmentTotalArmourValue(Equipment equipment) => 0 + (int)equipment.GetHeadArmorSum() + (int)equipment.GetHumanBodyArmorSum() + (int)equipment.GetHorseArmorSum() + (int)equipment.GetLegArmorSum() + (int)equipment.GetArmArmorSum();

        private void MarriageType() => InformationManager.ShowInquiry(
                new InquiryData(
                    new TextObject("{=arrangemarriage_marriage_type}Marriage Type").ToString(), 
                    new TextObject("{=arrangemarriage_marriage_fee}You can pay a fee to have {SPOUSE} marry into your clan or alternatively you would be offered a gift if you had {HERO} marry into their clan. If the selected spouse is the leader of their clan, your family member can only marry into their clan").ToString()
                        .Replace("{SPOUSE}", SelectedSpouse.CharacterObject.GetName().ToString())
                        .Replace("{HERO}", FamilyMember.CharacterObject.GetName().ToString()),
            (SelectedSpouse != SelectedSpouse.Clan.Leader ? 1 : 0) != 0, true, new TextObject("{=arrangemarriage_marriage_our_clan}Our Clan").ToString(), new TextObject("{=arrangemarriage_marriage_their_clan}Their Clan").ToString(), () =>
        {
            MarryIntoPlayerClan = true;
            Part4();
        }, () =>
        {
            MarryIntoPlayerClan = false;
            Part4();
        }, "", 0.0f, null), true);

        private void Part4()
        {
            string str;
            if (SameClan)
                str = "";
            else if (MarryIntoPlayerClan)
                str = ".  " + new TextObject("They will become part of your clan.  You will pay a marriage gift fee of {GOLD_AMOUNT} {GOLD_ICON}gold to the {OTHER_CLAN} clan").ToString()
                                .Replace("{GOLD_AMOUNT}", getMarriagePrice(SelectedSpouse).ToString())
                                .Replace("{GOLD_ICON}", "<img src=\"Icons\\Coin@2x\">")
                                .Replace("{OTHER_CLAN}", SelectedSpouse.Clan.Name.ToString());
            else
                str = ".  " + new TextObject("They will become part of the {OTHER_CLAN} clan. You will recieve a marriage gift of {GOLD_AMOUNT} {GOLD_ICON}gold from the {OTHER_CLAN} clan").ToString()
                                .Replace("{OTHER_CLAN}", SelectedSpouse.Clan.Name.ToString())
                                .Replace("{GOLD_AMOUNT}", getMarriagePrice(FamilyMember).ToString())
                                .Replace("{GOLD_ICON}", "<img src=\"Icons\\Coin@2x\">");
            
            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=arrangemarriage_confirm_marriage_title}Confirm Marriage").ToString(), 
                new TextObject("{HERO} will marry {SPOUSE}").ToString()
                        .Replace("{OTHER_CLAN}", SelectedSpouse.CharacterObject.GetName().ToString())
                        .Replace("{HERO}", FamilyMember.CharacterObject.GetName().ToString())
                        + str, 
                true, true, new TextObject("{=arrangemarriage_start_celebration}Start the Celebrations").ToString(), new TextObject("{=arrangemarriage_cancel_marriage}Cancel the Marriage").ToString(), () => Marriage(), () => GameMenu.SwitchToMenu("town_backstreet"), "", 0.0f, null), true);
        }

        private void Marriage()
        {
            if (SameClan)
            {
                //MarriageAction.Apply(Hero.MainHero, this.FamilyMember, true);
                MarriageAction.Apply(FamilyMember, SelectedSpouse, true);
            }
            else
            {
                Hero leader = SelectedSpouse.Clan.Leader;
                if (FamilyMember != Hero.MainHero && PartyBase.MainParty.MemberRoster.FindIndexOfTroop(FamilyMember.CharacterObject) != -1)
                    PartyBase.MainParty.MemberRoster.AddToCountsAtIndex(PartyBase.MainParty.MemberRoster.FindIndexOfTroop(FamilyMember.CharacterObject), -1, 0, 0, true);

                // Set both characters to be lord as only lords can marry
                FamilyMember.Clan = Hero.MainHero.Clan;
                FamilyMember.SetNewOccupation(Occupation.Lord);
                SelectedSpouse.SetNewOccupation(Occupation.Lord);

                if (FamilyMember.IsPlayerCompanion)
                { // Remove companion as only lords can marry
                    FamilyMember.CompanionOf = null;
                }
                MarriageAction.Apply(FamilyMember, SelectedSpouse, true);
                if (MarryIntoPlayerClan)
                {
                    FamilyMember.Clan = Hero.MainHero.Clan;
                    SelectedSpouse.Clan = Hero.MainHero.Clan;

                    Hero.MainHero.Gold -= getMarriagePrice(SelectedSpouse);
                    leader.Gold += getMarriagePrice(SelectedSpouse);
                    if (SelectedSpouse.PartyBelongedTo != null)
                    {
                        MobileParty partyBelongedTo = SelectedSpouse.PartyBelongedTo;
                        partyBelongedTo.ActualClan = Hero.MainHero.Clan;
                        MobileParty newMobileParty = Hero.MainHero.Clan.CreateNewMobileParty(SelectedSpouse);
                        foreach (TroopRosterElement troopRosterElement in partyBelongedTo.MemberRoster.GetTroopRoster())
                            newMobileParty.MemberRoster.AddToCounts(troopRosterElement.Character, (troopRosterElement).Number, false, 0, 0, true, -1);
                        partyBelongedTo.RemoveParty();
                    }
                }
                else
                {
                    FamilyMember.Clan = leader.Clan;
                    SelectedSpouse.Clan = leader.Clan;
                    Hero.MainHero.Gold += getMarriagePrice(FamilyMember);
                    leader.Gold -= getMarriagePrice(FamilyMember);
                }
                ChangeRelationAction.ApplyPlayerRelation(leader, 10, true, true);
                GameMenu.SwitchToMenu("town_backstreet");
            }
        }

        private int getMarriagePrice(Hero hero) => (int)hero.Clan.Renown + hero.Level * 100;

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}
