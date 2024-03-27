using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using HarmonyLib;
using TaleWorlds.CampaignSystem.Party;
using SandBox.CampaignBehaviors;

namespace ArrangeMarriageForFamily.Patches
{
    [HarmonyPatch(typeof(CompanionRolesCampaignBehavior), "ClanNameSelectionIsDone")]
    class CreateCompanionToLordClanPatch
    {
        public static void Postfix(string clanName)
        {
            Hero hero = Hero.OneToOneConversationHero;
            if (hero.Spouse != null)
            {
                hero.Spouse.Clan = hero.Clan;
                if (hero.Spouse.CompanionOf == Hero.MainHero.Clan)
                {
                    hero.Spouse.CompanionOf = null;
                    hero.Spouse.SetNewOccupation(Occupation.Lord);
                }

                if(MobileParty.MainParty.MemberRoster.Contains(hero.Spouse.CharacterObject))
                    MobileParty.MainParty.MemberRoster.RemoveTroop(hero.Spouse.CharacterObject);
                if(hero.PartyBelongedTo != null)
                    hero.PartyBelongedTo.MemberRoster.AddToCounts(hero.Spouse.CharacterObject, 1);
            }
        }
    }
}
