﻿using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Inventory
{
    public class CharacterEquipment
    {
        public CharacterEquipment(HookManager hookManager)
        {
            HookManager = hookManager;
            Equipment = new Dictionary<EquipmentSlot, IWowItem>();
        }

        public Dictionary<EquipmentSlot, IWowItem> Equipment { get; private set; }

        private HookManager HookManager { get; }

        public void Update()
        {
            string resultJson = HookManager.GetEquipmentItems();

            try
            {
                List<WowBasicItem> rawEquipment = ItemFactory.ParseItemList(resultJson);

                Equipment.Clear();
                foreach (WowBasicItem rawItem in rawEquipment)
                {
                    IWowItem item = ItemFactory.BuildSpecificItem(rawItem);
                    Equipment.Add(item.EquipSlot, item);
                }
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log($"Failed to parse Equipment JSON:\n{resultJson}\n{e.ToString()}", LogLevel.Error);
            }
        }
    }
}
