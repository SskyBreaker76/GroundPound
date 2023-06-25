using SkySoft.Events.Graph;
using SkySoft.Inventory;
using SkySoft.IO;
using SkySoft.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SkySoft.Interaction
{
    [Serializable]
    public class LootBoxSave
    {
        public bool Opened = false;
        /// <summary>
        /// This makes sure if a chest will be deleted if the game hasn't saved before quitting to Desktop / to the Menu
        /// </summary>
        public bool Temp = true;
    }

    [AddComponentMenu("SkyEngine/Interaction/Loot Box")]
    public class LootBox : Interactive
    {
        public string ID;
        [Combo(True: "Dialogue", False: "Notify", Label = "Notification Type")]
        public bool RequireConfirmation = true;
        [Combo(True: "Instanced", False: "Default", Label = "Mode")]
        public bool IsInstanced;
        public AudioSource ObtainedSound;
        [SerializeField] private Item QuickAdd;
        public List<ItemStack> Items = new List<ItemStack>();
        public int Currency;
        private bool HasBeenOpened;

        public override void OnValidate()
        {
            base.OnValidate();

            if (QuickAdd)
            {
                Items.Add(new ItemStack(QuickAdd));
                QuickAdd = null;
            }

            if (string.IsNullOrEmpty(ID))
            {
                ID = Guid.NewGuid().ToString();
            }
        }

        private void Awake()
        {
            if (!IsInstanced)
            {
                if (FileManager.FileExists<LootBoxSave>("Loot", ID, ".loot"))
                {
                    FileManager.ReadFile<LootBoxSave>("Loot", ID, File =>
                    {
                        HasBeenOpened = File.Opened;
                    }, ".loot");
                }
            }
        }

        protected override void Interaction(Entities.Entity Entity = null)
        {
            if (!HasBeenOpened)
            {
                if (Items.Count > 0)
                {
                    foreach (ItemStack Item in Items)
                    {
                        ToastHandler.Instance.ShowToast($"Found {Item.Count}x {Item.Item.Name}");
                        Inventory.Inventory.LocalInventory.AddItem(Item);
                    }
                }

                if (Currency > 0)
                {
                    ToastHandler.Instance.ShowToast($"Found {Currency} Crown{(Currency != 1 ? "s" : "")}");
                    Inventory.Inventory.LocalInventory.AddMoney(Currency, true);
                }

                if (!IsInstanced)
                {
                    FileManager.WriteFile("Loot", ID, new LootBoxSave { Opened = true, Temp = true }, Success => { }, ".loot");
                    HasBeenOpened = true;
                }
            }
            else
            {
                ToastHandler.Instance.ShowToast("There's nothing inside...");
            }

            if (IsInstanced)
            {
                Destroy(gameObject);
            }
        }
    }
}