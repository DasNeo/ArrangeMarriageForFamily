// Decompiled with JetBrains decompiler
// Type: ArrangeMarriageForFamily.ArrangeMarriageForFamilyBehavior
// Assembly: ArrangeMarriageForFamily, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: E1CDC03C-57DC-4A6A-8425-85F26EE94ECE
// Assembly location: C:\Users\andre\Downloads\ArrangeMarriageForFamily.dll

using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace ArrangeMarriageForFamily
{
    internal class ArrangeMarriageForFamilyBehavior : CampaignBehaviorBase
    {
        private Hero FamilyMember;
        private Hero SelectedSpouse;
        private bool MarryIntoPlayerClan;
        private bool SameClan = false;

        public override void RegisterEvents() => CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.AddMenuItems));

        private void AddMenuItems(CampaignGameStarter campaignGameStarter)
        {
            foreach (Hero hero in Hero.MainHero.Clan.Heroes)
            {
                if (hero.PartyBelongedTo != null && !hero.PartyBelongedTo.IsCaravan && hero.PartyBelongedTo.ActualClan != hero.Clan)
                    hero.PartyBelongedTo.RemoveParty();
            }
            campaignGameStarter.AddGameMenuOption("town_backstreet", "marry_family", "Arrange a marriage for clan member", (a) =>
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
                if (aliveHero.Clan == Hero.MainHero.Clan && (double)aliveHero.Age >= 18.0 && aliveHero.Spouse == null && (aliveHero.Occupation == Occupation.Lord || aliveHero.Occupation == Occupation.Wanderer))
                    inquiryElementList.Add(new InquiryElement((object)aliveHero.CharacterObject.HeroObject, ((object)aliveHero.Name).ToString() + " - " + aliveHero.Age.ToString("0"), new ImageIdentifier(CharacterCode.CreateFrom((BasicCharacterObject)aliveHero.CharacterObject))));
            }
            if (inquiryElementList.Count < 1)
            {
                InformationManager.ShowInquiry(new InquiryData("Arrange Marriage Not Possible", "You have no single clan members", true, false, "OK", "", (Action)null, (Action)null, "", 0.0f, (Action)null), true);
                GameMenu.SwitchToMenu("town_backstreet");
            }
            else
                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    titleText: "Members Suitable For Marriage",
                    descriptionText: "", 
                    inquiryElements: inquiryElementList, 
                    isExitShown: true, 
                    minSelectableOptionCount: 1,
                    maxSelectableOptionCount: 1,
                    affirmativeText: "Continue",
                    negativeText: (string)null, 
                    affirmativeAction: (Action<List<InquiryElement>>)(args =>
                {
                    List<InquiryElement> source = args;
                    if (source != null && !((IEnumerable<InquiryElement>)source).Any<InquiryElement>())
                        return;
                    InformationManager.HideInquiry();
                    SubModule.ExecuteActionOnNextTick((Action)(() => this.Part2(((IEnumerable<InquiryElement>)args).Select<InquiryElement, Hero>((Func<InquiryElement, Hero>)(element => element.Identifier as Hero)))));
                }), 
                    negativeAction: (Action<List<InquiryElement>>)null,
                    soundEventPath: ""), false);
        }

        private void Part2(IEnumerable<Hero> family)
        {
            this.FamilyMember = family.First<Hero>();
            this.ShowMatchesList();
        }

        private void ShowMatchesList()
        {
            List<InquiryElement> inquiryElementList = new List<InquiryElement>();
            if ((double)this.FamilyMember.Age >= 18.0)
            {
                inquiryElementList.Add(new InquiryElement((object)"Noble", "Marry a minor noble", (ImageIdentifier)null));
                inquiryElementList.Add(new InquiryElement((object)"Peasant", "Marry a peasant", (ImageIdentifier)null));
            }
            foreach (Hero aliveHero in Campaign.Current.AliveHeroes)
            {
                if (Campaign.Current.Models.MarriageModel.IsSuitableForMarriage(aliveHero) 
                    && this.FamilyMember.IsFemale != aliveHero.IsFemale)
                    inquiryElementList.Add(new InquiryElement((object)aliveHero.CharacterObject.HeroObject, ((object)aliveHero.EncyclopediaLinkWithName).ToString() + " " + (aliveHero.Clan != null ? ((object)aliveHero.Clan.EncyclopediaLinkWithName).ToString() : "") + " - " + aliveHero.Age.ToString("0"), new ImageIdentifier(CharacterCode.CreateFrom((BasicCharacterObject)aliveHero.CharacterObject)), true, this.HeroStats(aliveHero)));
            }
            if (inquiryElementList.Count < 1)
            {
                InformationManager.ShowInquiry(new InquiryData("No Matches found", "", true, false, "OK", "", (Action)null, (Action)null, "", 0.0f, (Action)null), true);
                GameMenu.SwitchToMenu("town_backstreet");
            }
            else
                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    titleText: "Suitable Spouses for " + ((object)((MBObjectBase)this.FamilyMember.CharacterObject).GetName()).ToString(),
                    descriptionText: "",
                    inquiryElements: inquiryElementList,
                    isExitShown: true,
                    minSelectableOptionCount: 1,
                    maxSelectableOptionCount: 1,
                    affirmativeText: "Continue",
                    negativeText: (string)null,
                    affirmativeAction: (Action<List<InquiryElement>>)(args =>
                {
                    List<InquiryElement> source = args;
                    if (source != null && !((IEnumerable<InquiryElement>)source).Any<InquiryElement>())
                        return;
                    InformationManager.HideInquiry();
                    SubModule.ExecuteActionOnNextTick((Action)(() => this.Part3(((IEnumerable<InquiryElement>)args).Select<InquiryElement, object>((Func<InquiryElement, object>)(element => element.Identifier)).First<object>())));
                }), 
                    negativeAction: (Action<List<InquiryElement>>)null,
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
                    str = str + ((object)((PropertyObject)skillObjectArray[index]).Name).ToString() + " : " + hero.GetSkillValue(skillObjectArray[index]).ToString() + "\n";
            }
            return str;
        }

        private void Part3(object selected)
        {
            if (selected.GetType() == typeof(string))
            {
                if ((string)selected == "Noble")
                {
                    this.MarryNoble();
                }
                else
                {
                    if (!((string)selected == "Peasant"))
                        return;
                    this.MarryPeasant();
                }
            }
            else
            {
                this.SelectedSpouse = (Hero)selected;
                if (this.SelectedSpouse.Clan == Hero.MainHero.Clan)
                {
                    this.MarryIntoPlayerClan = true;
                    this.SameClan = true;
                    this.Part4();
                }
                else
                {
                    this.SameClan = false;
                    if (this.SelectedSpouse.Clan == null)
                    {
                        this.MarryIntoPlayerClan = true;
                        this.Marriage();
                    }
                    else
                        this.MarriageType();
                }
            }
        }

        private void MarryPeasant()
        {
            IReadOnlyList<CharacterObject> lordTemplates = Settlement.CurrentSettlement.Culture.LordTemplates;
            CharacterObject characterObject = lordTemplates[new Random().Next(0, ((IReadOnlyCollection<CharacterObject>)lordTemplates).Count - 1)];
            ((BasicCharacterObject)characterObject).IsFemale = !this.FamilyMember.IsFemale;
            Hero specialHero = HeroCreator.CreateSpecialHero(characterObject, Settlement.CurrentSettlement, (Clan)null, FamilyMember.Clan, new Random().Next(Math.Max(18, (int)this.FamilyMember.Age - 3), (int)this.FamilyMember.Age + 3));
            specialHero.ChangeState((Hero.CharacterStates)1);
            specialHero.HeroDeveloper.DevelopCharacterStats();
            if (specialHero.IsFemale)
                ((BasicCharacterObject)specialHero.CharacterObject).Equipment.FillFrom(((BasicCharacterObject)Settlement.CurrentSettlement.Culture.Townswoman).Equipment, true);
            else
                ((BasicCharacterObject)specialHero.CharacterObject).Equipment.FillFrom(((BasicCharacterObject)Settlement.CurrentSettlement.Culture.Townsman).Equipment, true);
            specialHero.Clan = FamilyMember.Clan;//Hero.MainHero.Clan;
            specialHero.SetNewOccupation(Occupation.Lord);
            this.FamilyMember.SetNewOccupation(Occupation.Lord);
            MarriageAction.Apply(this.FamilyMember, specialHero, true);
            if(FamilyMember.PartyBelongedTo != null)
                AddHeroToPartyAction.Apply(specialHero, FamilyMember.PartyBelongedTo /*MobileParty.MainParty*/, true);
        }

        private void MarryNoble()
        {
            IReadOnlyList<CharacterObject> lordTemplates = Settlement.CurrentSettlement.Culture.LordTemplates;
            CharacterObject characterObject = lordTemplates[new Random().Next(0, ((IReadOnlyCollection<CharacterObject>)lordTemplates).Count - 1)];
            ((BasicCharacterObject)characterObject).IsFemale = !this.FamilyMember.IsFemale;
            Hero specialHero = HeroCreator.CreateSpecialHero(characterObject, Settlement.CurrentSettlement, (Clan)null, FamilyMember.Clan, new Random().Next(Math.Max(18, (int)this.FamilyMember.Age - 3), (int)this.FamilyMember.Age + 3));
            specialHero.ChangeState((Hero.CharacterStates)1);
            specialHero.HeroDeveloper.DevelopCharacterStats();
            specialHero.Clan = FamilyMember.Clan;//Hero.MainHero.Clan;
            specialHero.SetNewOccupation(Occupation.Lord);
            this.FamilyMember.SetNewOccupation(Occupation.Lord);
            MarriageAction.Apply(this.FamilyMember, specialHero, true);
            if(FamilyMember.PartyBelongedTo != null)
                AddHeroToPartyAction.Apply(specialHero, FamilyMember.PartyBelongedTo, true);
        }

        public static int GetEquipmentTotalArmourValue(Equipment equipment) => 0 + (int)equipment.GetHeadArmorSum() + (int)equipment.GetHumanBodyArmorSum() + (int)equipment.GetHorseArmorSum() + (int)equipment.GetLegArmorSum() + (int)equipment.GetArmArmorSum();

        private void MarriageType() => InformationManager.ShowInquiry(new InquiryData("Marriage Type", "You can pay a fee to have " + ((object)((MBObjectBase)this.SelectedSpouse.CharacterObject).GetName()).ToString() + " marry into your clan or alternatively you would be offered a gift if you had " + ((object)((MBObjectBase)this.FamilyMember.CharacterObject).GetName()).ToString() + " marry into their clan.  If the selected spouse is the leader of their clan, your family member can only marry into their clan", (this.SelectedSpouse != this.SelectedSpouse.Clan.Leader ? 1 : 0) != 0, true, "Our Clan", "Their Clan", (Action)(() =>
        {
            this.MarryIntoPlayerClan = true;
            this.Part4();
        }), (Action)(() =>
        {
            this.MarryIntoPlayerClan = false;
            this.Part4();
        }), "", 0.0f, (Action)null), true);

        private void Part4()
        {
            string str;
            if (this.SameClan)
                str = "";
            else if (this.MarryIntoPlayerClan)
                str = ".  They will become part of your clan.  You will pay a marriage gift fee of " + this.getMarriagePrice(this.SelectedSpouse).ToString() + " <img src=\"Icons\\Coin@2x\">gold to the " + ((object)this.SelectedSpouse.Clan.Name).ToString() + " clan";
            else
                str = ".  They will become part of the " + ((object)this.SelectedSpouse.Clan.Name).ToString() + " clan.  You will recieve a marriage gift of " + this.getMarriagePrice(this.FamilyMember).ToString() + " <img src=\"Icons\\Coin@2x\">gold from the " + ((object)this.SelectedSpouse.Clan.Name).ToString() + " clan";
            InformationManager.ShowInquiry(new InquiryData("Confirm Marriage", ((object)((MBObjectBase)this.FamilyMember.CharacterObject).GetName()).ToString() + " will marry " + ((object)((MBObjectBase)this.SelectedSpouse.CharacterObject).GetName()).ToString() + str, true, true, "Start the Celebrations", "Cancle the Marriage", (Action)(() => this.Marriage()), (Action)(() => GameMenu.SwitchToMenu("town_backstreet")), "", 0.0f, (Action)null), true);
        }

        private void Marriage()
        {
            if (this.SameClan)
            {
                //MarriageAction.Apply(Hero.MainHero, this.FamilyMember, true);
                MarriageAction.Apply(this.FamilyMember, this.SelectedSpouse, true);
            }
            else
            {
                Hero leader = this.SelectedSpouse.Clan.Leader;
                if (this.FamilyMember != Hero.MainHero && PartyBase.MainParty.MemberRoster.FindIndexOfTroop(this.FamilyMember.CharacterObject) != -1)
                    PartyBase.MainParty.MemberRoster.AddToCountsAtIndex(PartyBase.MainParty.MemberRoster.FindIndexOfTroop(this.FamilyMember.CharacterObject), -1, 0, 0, true);

                // Set both characters to be lord as only lords can marry
                this.FamilyMember.Clan = Hero.MainHero.Clan;
                this.FamilyMember.SetNewOccupation(Occupation.Lord);
                this.SelectedSpouse.SetNewOccupation(Occupation.Lord);

                if (this.FamilyMember.IsPlayerCompanion)
                { // Remove companion as only lords can marry
                    this.FamilyMember.CompanionOf = null;
                }
                MarriageAction.Apply(this.FamilyMember, this.SelectedSpouse, true);
                if (this.MarryIntoPlayerClan)
                {
                    this.FamilyMember.Clan = Hero.MainHero.Clan;
                    this.SelectedSpouse.Clan = Hero.MainHero.Clan;

                    Hero.MainHero.Gold -= this.getMarriagePrice(this.SelectedSpouse);
                    leader.Gold += this.getMarriagePrice(this.SelectedSpouse);
                    if (this.SelectedSpouse.PartyBelongedTo != null)
                    {
                        MobileParty partyBelongedTo = this.SelectedSpouse.PartyBelongedTo;
                        partyBelongedTo.ActualClan = Hero.MainHero.Clan;
                        MobileParty newMobileParty = Hero.MainHero.Clan.CreateNewMobileParty(this.SelectedSpouse);
                        foreach (TroopRosterElement troopRosterElement in partyBelongedTo.MemberRoster.GetTroopRoster())
                            newMobileParty.MemberRoster.AddToCounts(troopRosterElement.Character, (troopRosterElement).Number, false, 0, 0, true, -1);
                        partyBelongedTo.RemoveParty();
                    }
                }
                else
                {
                    this.FamilyMember.Clan = leader.Clan;
                    this.SelectedSpouse.Clan = leader.Clan;
                    Hero.MainHero.Gold += this.getMarriagePrice(this.FamilyMember);
                    leader.Gold -= this.getMarriagePrice(this.FamilyMember);
                }
                ChangeRelationAction.ApplyPlayerRelation(leader, 10, true, true);
                GameMenu.SwitchToMenu("town_backstreet");
            }
        }

        private int getMarriagePrice(Hero hero) => (int)hero.Clan.Renown + hero.Level * 100;

        private void TakeMenuAction()
        {
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}
