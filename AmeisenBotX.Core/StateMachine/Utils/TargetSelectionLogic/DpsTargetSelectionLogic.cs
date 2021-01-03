﻿using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic
{
    public class DpsTargetSelectionLogic : ITargetSelectionLogic
    {
        public DpsTargetSelectionLogic(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
        }

        public IEnumerable<int> BlacklistedTargets { get; set; }

        public IEnumerable<int> PriorityTargets { get; set; }

        private WowInterface WowInterface { get; }

        public void Reset()
        {
        }

        public bool SelectTarget(out IEnumerable<WowUnit> possibleTargets)
        {
            if (WowInterface.ObjectManager.TargetGuid != 0 && WowInterface.Target != null
                && (WowInterface.Target.IsDead
                    || (BlacklistedTargets != null && BlacklistedTargets.Contains(WowInterface.Target.DisplayId))
                    || WowInterface.Target.IsNotAttackable
                    || !BotUtils.IsValidUnit(WowInterface.Target)
                    || WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, WowInterface.Target) == WowUnitReaction.Friendly))
            {
                WowInterface.HookManager.WowClearTarget();
                possibleTargets = null;
                return false;
            }

            Vector3 position = WowInterface.ObjectManager.PartyleaderGuid != 0 ? WowInterface.ObjectManager.MeanGroupPosition : WowInterface.ObjectManager.Player.Position;

            if (PriorityTargets != null && PriorityTargets.Any())
            {
                IEnumerable<WowUnit> nearPriorityEnemies = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(position, 64.0)
                    .Where(e => BotUtils.IsValidUnit(e) && (BlacklistedTargets == null || !BlacklistedTargets.Contains(e.DisplayId)) && !e.IsDead && e.Health > 0 && e.IsInCombat && PriorityTargets.Any(x => e.DisplayId == x) && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 80.0)
                    .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position));

                if (nearPriorityEnemies.Any())
                {
                    possibleTargets = nearPriorityEnemies;
                    return true;
                }
            }

            IEnumerable<WowUnit> nearEnemies = WowInterface.ObjectManager
                .GetEnemiesInCombatWithGroup<WowUnit>(position, 64.0)
                .Where(e => (BlacklistedTargets == null || !BlacklistedTargets.Contains(e.DisplayId))
                         && !(WowInterface.ObjectManager.MapId == MapId.HallsOfReflection && e.Name == "The Lich King")
                         && !(WowInterface.ObjectManager.MapId == MapId.DrakTharonKeep && WowInterface.ObjectManager.WowObjects.OfType<WowDynobject>().Any(e => e.SpellId == 47346))) // Novos fix
                .OrderByDescending(e => e.Type) // make sure players are at the top (pvp)
                .ThenByDescending(e => e.IsFleeing) // catch fleeing enemies
                .ThenByDescending(e => e.MaxHealth)
                .ThenBy(e => e.Health);

            // TODO: need to handle duels, our target will
            // be friendly there but is attackable
            if (nearEnemies.Any())
            {
                possibleTargets = nearEnemies;
                return true;
            }

            nearEnemies = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(position, 100.0)
                .Where(e => e.IsInCombat && (BlacklistedTargets == null || !BlacklistedTargets.Contains(e.DisplayId)));

            if (nearEnemies.Any())
            {
                possibleTargets = nearEnemies;
                return true;
            }

            possibleTargets = null;
            return false;
        }
    }
}