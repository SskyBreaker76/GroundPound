using UnityEngine;
using UnityEngine.UI;
using SkySoft.UI;

namespace SkySoft.Inventory.UI 
{
    [AddComponentMenu("SkyEngine/UI/Inventory/Slot")]
    public class GUISlot : MonoBehaviour
    {
        #region Statics
        public static ItemStack CurrentGrabbedStack = new ItemStack();
        public static GUISlot CurrentInteractionBase;

        public static bool IsDragging
        {
            get
            {
                return CurrentGrabbedStack.Count > 0;
            }
        }
        #endregion

        public ItemStack Item;
        [Space]
        public Image Icon;
        public Text Label;
        public Text Count;

        [Header("Type Properties")]
        public bool IsEquipmentSlot;
        public EquipmentSlot TargetSlot;
        public Sprite DefaultIcon;

        public void Refresh(bool TargetAll = false)
        {
            if (!IsEquipmentSlot)
            {
                if (!string.IsNullOrEmpty(Item.ItemID))
                {
                    Icon.sprite = Item.Item.Icon;

                    if (Label)
                        Label.text = Item.Item.Name;

                    if (Item.Count > 0)
                        if (Item.Count > 1)
                            Count.text = SkyEngine.ItemCountText.Replace("<COUNT>", Item.Count.ToString());
                        else
                            Count.text = "";
                    else
                        Destroy(gameObject); // If no count left, Destroy the slot
                }
                else
                    Destroy(gameObject);
            }
            else
            {
                try
                {
                    Icon.sprite = string.IsNullOrEmpty(Item.ItemID) ? DefaultIcon : Item.Item.Icon;
                }
                catch { }

                if (Label)
                    Label.text = string.IsNullOrEmpty(Item.ItemID) ? "None" : Item.Item.Name;
                Count.text = "";
            }
        }

        private void OnValidate()
        {
            if (IsEquipmentSlot && DefaultIcon)
            {
                Icon.sprite = DefaultIcon;
                Label.text = "";
            }
        }

        public static void RefreshAll()
        {
            foreach (GUISlot Slot in FindObjectsOfType<GUISlot>())
            {
                Slot.Refresh();
            }
        }

        public virtual void OnDropItem()
        {
            if (IsDragging)
            {
                if (CurrentInteractionBase == this)
                {
                    CurrentInteractionBase = null;
                    CurrentGrabbedStack = new ItemStack();
                }
                else
                {
                    InventoryMoveResult Res = Inventory.LocalInventory.MoveItem(CurrentInteractionBase, this);

                    if (Res.State == InventoryMoveState.OK)
                    {
                        CurrentInteractionBase = null;
                        CurrentGrabbedStack = new ItemStack();
                    }
                    else
                    {
                        Inventory.LocalInventory.Error();
                    }
                }
            }
        }

        public virtual void OnGrabItem()
        {
            if (!IsDragging)
            {
                CurrentGrabbedStack = Item;
                CurrentInteractionBase = this;
            }
        }
    }
}