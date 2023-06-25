using SkySoft.Inventory.UI;
using SkySoft.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SkySoft.Inventory 
{
    public enum InventoryMoveState
    {
        OK,
        Failed
    }

    public struct InventoryMoveResult
    {
        public InventoryMoveState State;
        public ItemStack ResultingItem;
    }

    [AddComponentMenu("SkyEngine/Inventory/Inventory", 0)]
    public class Inventory : MonoBehaviour
    {
        public static Inventory LocalInventory { get; private set; }
        public const int MaxRenderedSlots = 7;

        public UnityEngine.UI.Text Copper, Gold, Silver;

        public List<ItemStack> Items = new List<ItemStack>();
        private GUISlot[] Slots = new GUISlot[MaxRenderedSlots];
        public GUISlot[] EquipmentSlots;

        public Events.ObjectEventHandler InventoryBtn;
        public GUISlot ItemSlotPrefab;
        public Transform ItemSlotRoot;

        public CommandMenu ItemsMenu, EquipmentMenu;

        public UnityEngine.UI.Text ItemDescription;

        public int CurrentMenuOffset;
        [Space]
        public UnityEvent OnError;
        [Space]
        public UnityEvent OnItemGrab;
        public UnityEvent OnItemDrop;

        public int Size
        {
            get
            {
                int Count = 0;

                foreach (ItemStack Stack in Items)
                {
                    if (Stack.Count > 0)
                    {
                        Count++;
                    }
                }

                return Count;
            }
        }

        public ItemStack[] RegisteredItems
        {
            get
            {
                List<ItemStack> AllItems = new List<ItemStack>();

                foreach (ItemStack I in Items)
                {
                    if (I.Count > 0)
                    {
                        AllItems.Add(I);
                    }
                }

                return AllItems.ToArray();
            }
        }

        public bool AddItem(ItemStack Stack)
        {
            foreach (ItemStack Item in Items)
            {
                if (Item.ItemID == Stack.ItemID)
                {
                    if (Item.Count + Stack.Count < ((Stack.Item.GetType() == typeof(Equipment) || Stack.Item.GetType() == typeof(Armour) || Stack.Item.GetType() == typeof(Weapon)) ? SkyEngine.EquipmentStackSize : SkyEngine.GeneralStackSize))
                    { 
                        Item.Count += Stack.Count;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            Items.Add(Stack);
            return true;
        }

        public bool AddItem(Item Item, int StackSize = 1)
        {
            return AddItem(new ItemStack(Item, StackSize));
        }

        public void RemoveItem(ItemStack Stack)
        {
            foreach (ItemStack Item in Items)
            {
                if (Item.ItemID == Stack.ItemID)
                {
                    Item.Count -= Stack.Count;

                    if (Item.Count < 0)
                        Items.Remove(Item);

                    return;
                }
            }
        }

        public void RemoveItem(Item Item, int Amount)
        {
            RemoveItem(new ItemStack(Item, Amount));
        }

        public void Error()
        {
            OnError.Invoke();
        }

        private void Awake()
        {
            LocalInventory = this;
            CurrentMenuOffset = 0;
            GenerateIcons(CurrentMenuOffset);
        }

        public void OnNavigate(CommandMenu Menu, int ValueBeforeClamp, out bool Shifted)
        {
            int ModifySelectedIndex = -1;

            Shifted = false;

            Debug.Log($"Navigate: ValueBeforeClamp = {ValueBeforeClamp}, MaxRenderedSlots = {MaxRenderedSlots}");

            if (ValueBeforeClamp >= MaxRenderedSlots)
            {
                ModifySelectedIndex = Mathf.Clamp(ValueBeforeClamp, 0, MaxRenderedSlots - 1);
                CurrentMenuOffset++;
            }
            if (ValueBeforeClamp < 0 && CurrentMenuOffset > 0)
            {
                ModifySelectedIndex = Mathf.Clamp(ValueBeforeClamp, 0, MaxRenderedSlots - 1);
                CurrentMenuOffset--;
            }

            if (ModifySelectedIndex != -1)
            {
                Menu.SelectedIndex = Mathf.Clamp(ValueBeforeClamp, 0, MaxRenderedSlots - 1);

                //Shifted = true;
                CurrentMenuOffset = Mathf.Clamp(CurrentMenuOffset, 0, Size - MaxRenderedSlots);

                CommandMenu.WasInventoryRefresh = true;
                GenerateIcons(CurrentMenuOffset);
            }
        }

        public void GrabItem(CommandMenu Menu)
        {
            Debug.Log("GRAB");

            Equipment Equip = Items[Menu.SelectedIndex + CurrentMenuOffset].Item as Equipment;

            if (Equip) // Equipment Behaviour
            {
                ItemsMenu.RetainSelection = false;
                ItemsMenu.enabled = false;
                EquipmentMenu.enabled = true;
                Slots[Menu.SelectedIndex].OnGrabItem();
                EquipmentMenu.Selection.GetComponent<UnityEngine.UI.Image>().enabled = true;
            }
            else
            {
                Consumable Cumsum = Items[Menu.SelectedIndex + CurrentMenuOffset].Item as Consumable;

                if (Cumsum) // Consumable Behaviour
                {
                    Cumsum.OnUse(SkyEngine.PlayerEntity);
                    RemoveItem(new ItemStack(Cumsum, 1));
                    LocalInventory.GenerateIcons();
                    SkyEngine.OnEquipmentUpdated(GetComponent<EquipmentManager>());
                }
            }
        }

        public void DropItem(CommandMenu Menu)
        {
            Debug.Log("DROP");

            ItemsMenu.enabled = true;
            EquipmentMenu.enabled = false;
            EquipmentSlots[Menu.SelectedIndex].OnDropItem();
            EquipmentMenu.Selection.GetComponent<UnityEngine.UI.Image>().enabled = false;

            GenerateIcons(0);
        }

        public void GenerateIcons(int IndexOffset = -1)
        {
            if (IndexOffset == -1)
                IndexOffset = CurrentMenuOffset;

            foreach (Transform T in ItemSlotRoot)
            {
                Destroy(T.gameObject);
            }

            for (int X = 0; X < MaxRenderedSlots; X++)
            {
                int I = X + IndexOffset;

                try
                {
                    if (Size > I)
                    {
                        if (RegisteredItems[I].Count > 0)
                        {
                            GameObject SSlot = Instantiate(ItemSlotPrefab.gameObject, ItemSlotRoot);
                            GUISlot Slot;

                            if (Slot = SSlot.GetComponent<GUISlot>())
                            {
                                Slot.Item = RegisteredItems[I];
                                Slot.Refresh();
                                Slots[X] = Slot;

                                Events.ObjectEventHandler EV;

                                if (EV = Slot.GetComponent<Events.ObjectEventHandler>())
                                {
                                    if (RegisteredItems[I].Item.GetType() == typeof(Item))
                                        EV.Interactable = false;

                                    EV.m_Event.AddListener(() => { GrabItem(ItemsMenu); });
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            EquipmentManager Equips = GetComponent<EquipmentManager>();

            foreach (GUISlot Slot in EquipmentSlots)
            {
                try
                {
                    if (!SkyEngine.IsHandSlot(Slot.TargetSlot))
                    {
                        Slot.Item = new ItemStack(Equips.Equipment[Slot.TargetSlot]);
                    }
                    else
                    {
                        Slot.Item = new ItemStack(Equips.Hands[Slot.TargetSlot]);
                    }
                }
                catch { }

                Events.ObjectEventHandler EV;

                if (EV = Slot.GetComponent<Events.ObjectEventHandler>())
                {
                    EV.m_Event = new UnityEvent();

                    EV.m_Event.AddListener(() =>
                    {
                        DropItem(EquipmentMenu);
                    });
                }

                Slot.Refresh();
            }
        }

        public InventoryMoveResult MoveItem(GUISlot From, GUISlot To)
        {
            if (!To.IsEquipmentSlot)
            {
                if (To.Item.Count == 0 || string.IsNullOrEmpty(To.Item.ItemID) || To.Item.Item == null) // Is the slot empty / invalid?
                {
                    To.Item = From.Item;
                    From.Item.ItemID = "";
                    From.Item.Count = 0;
                    From.Refresh();
                    To.Refresh();
                    return new InventoryMoveResult { State = InventoryMoveState.OK, ResultingItem = To.Item };
                }
                else
                {
                    try
                    {
                        ItemStack Original = To.Item;
                        To.Item = From.Item;
                        From.Item = Original;
                        To.Refresh();
                        From.Refresh();
                        return new InventoryMoveResult { State = InventoryMoveState.OK, ResultingItem = To.Item };
                    }
                    catch
                    {

                    }
                }
            }
            else
            {
                if (To.Item.Count == 0 || string.IsNullOrEmpty(To.Item.ItemID) || To.Item.Item == null) 
                {
                    GetComponent<EquipmentManager>().TryEquipItem(From.Item.Item as Equipment, To.TargetSlot, To);
                    RemoveItem(From.Item);
                }
                else
                {
                    ItemStack OldStack = To.Item;
                    GetComponent<EquipmentManager>().TryEquipItem(From.Item.Item as Equipment, To.TargetSlot, To);
                    RemoveItem(From.Item);
                    AddItem(OldStack);
                }
            }

            return new InventoryMoveResult { State = InventoryMoveState.Failed, ResultingItem = null };
        }

        public Currencies Money;
        public int TestAddAmount;
        public bool RunTestAdd = false;
        public AudioSource CoinSound;
        public AudioClip[] CopperAdd, GoldAdd, SilverAdd;
        public AudioClip PurchaseSound;

        public void AddMoney(int Count, bool Mute = false)
        {
            Money.Copper.Value += Count;

            if (Count != 0 && !Mute)
                CoinSound.PlayOneShot(CopperAdd[Random.Range(0, CopperAdd.Length)]);
        }

        Equipment HandLastFrame = null;

        private void Update()
        {
            InventoryBtn.Interactable = Size > 0;

            if (RunTestAdd)
            {
                AddMoney(TestAddAmount);
                TestAddAmount = 0;
                RunTestAdd = false;
            }

            Copper.text = Money.Copper.Value.ToString();

            Equipment CurEquip;

            if (CurEquip = GetComponent<EquipmentManager>().Hands[EquipmentSlot.RightHand])
            {
                if (HandLastFrame == null || HandLastFrame != CurEquip)
                {
                    SkyEngine.PlayerEntity.GetComponent<Objects.Charactermanager>().WeaponModel = ((Weapon)GetComponent<EquipmentManager>().Hands[EquipmentSlot.RightHand]).Model;
                    HandLastFrame = CurEquip;
                }
            }

            try
            {
                if (ItemsMenu.enabled)
                {
                    ItemDescription.text = Slots[ItemsMenu.SelectedIndex].Item.Item.Description.
                        Replace("P.H", SkyEngine.PlayerEntity.Properties.Vitals[0].Value.ToString()).
                        Replace("P.MH", SkyEngine.PlayerEntity.Properties.Vitals[0].MaxValue.ToString()).
                        Replace("P.F", SkyEngine.PlayerEntity.Properties.Vitals[1].Value.ToString()).
                        Replace("P.MF", SkyEngine.PlayerEntity.Properties.Vitals[1].MaxValue.ToString()).
                        Replace("P.A", SkyEngine.PlayerEntity.Properties.Vitals[2].Value.ToString()).
                        Replace("P.MA", SkyEngine.PlayerEntity.Properties.Vitals[2].MaxValue.ToString());
                }
                else
                {
                    if (GUISlot.IsDragging)
                        ItemDescription.text = $"Equip {GUISlot.CurrentGrabbedStack.Item.Name}";
                    else
                        ItemDescription.text = "";
                }
            }
            catch { ItemDescription.text = ""; }

            EquipmentManager EQ = GetComponent<EquipmentManager>();
            if (EQ)
            {
                EQ.EquipmentDebug = "";

                foreach (Armour Armour in EQ.Equipment.Values)
                {
                    try
                    {
                        EQ.EquipmentDebug += $"{Armour.Slot}: {Armour.Name}\n";
                    }
                    catch { }
                }

                foreach (Equipment Equip in EQ.Hands.Values)
                {
                    try
                    {
                        EQ.EquipmentDebug += $"{Equip.Slot}: {Equip.Name}\n";
                    }
                    catch { }
                }
            }
        }
    }
}