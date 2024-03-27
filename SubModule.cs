// Decompiled with JetBrains decompiler
// Type: ArrangeMarriageForFamily.SubModule
// Assembly: ArrangeMarriageForFamily, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: E1CDC03C-57DC-4A6A-8425-85F26EE94ECE
// Assembly location: C:\Users\andre\Downloads\ArrangeMarriageForFamily.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace ArrangeMarriageForFamily
{
    public class SubModule : MBSubModuleBase
    {
        private static readonly List<Action> ActionsToExecuteNextTick = new List<Action>();

        protected override void OnBeforeInitialModuleScreenSetAsRoot() => base.OnBeforeInitialModuleScreenSetAsRoot();

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (!(game.GameType is Campaign))
                return;
            ((CampaignGameStarter)gameStarterObject).AddBehavior((CampaignBehaviorBase)new ArrangeMarriageForFamilyBehavior());
        }

        public static void ExecuteActionOnNextTick(Action action)
        {
            if (action == null)
                return;
            SubModule.ActionsToExecuteNextTick.Add(action);
        }

        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            foreach (Action action in SubModule.ActionsToExecuteNextTick)
                action();
            SubModule.ActionsToExecuteNextTick.Clear();
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            new Harmony("ArrangeMarriageForFamily").PatchAll();
        }
    }
}
