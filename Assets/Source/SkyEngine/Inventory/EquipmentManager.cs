using SkySoft.Inventory.UI;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Inventory 
{
    [AddComponentMenu("SkyEngine/Inventory/Equipment Manager", 1)]
    public class EquipmentManager : MonoBehaviour
    {
        [TextArea] public string EquipmentDebug;
        private Dictionary<EquipmentSlot, Armour> m_Equipment;
        public Dictionary<EquipmentSlot, Armour> Equipment
        {
            get
            {
                if (m_Equipment == null || m_Equipment.Count == 0)
                {
                    m_Equipment = new Dictionary<EquipmentSlot, Armour>();

                    for (int I = 0; I < System.Enum.GetNames(typeof(EquipmentSlot)).Length; I++)
                    {
                        if ((EquipmentSlot)I != EquipmentSlot.RightHand && (EquipmentSlot)I != EquipmentSlot.LeftHand)
                            m_Equipment.Add((EquipmentSlot)I, null);
                    }
                }

                return m_Equipment;
            }
        }

        private Dictionary<EquipmentSlot, Equipment> m_Hands;
        public Dictionary<EquipmentSlot, Equipment> Hands
        {
            get
            {
                if (m_Hands == null || m_Hands.Count == 0)
                {
                    m_Hands = new Dictionary<EquipmentSlot, Equipment>
                    {
                        { EquipmentSlot.LeftHand, null },
                        { EquipmentSlot.RightHand, null }
                    };
                }

                return m_Hands;
            }
        }

        public bool HasItemInSlot(EquipmentSlot Slot)
        {
            foreach (EquipmentSlot S in Equipment.Keys)
            {
                if (Equipment[S] != null)
                    return true;
            }

            foreach (EquipmentSlot S in Hands.Keys)
            {
                if (Equipment[S] != null)
                    return true;
            }

            return false;
        }

        public int ArmourValue
        {
            get
            {
                int Total = 10 + SkyEngine.PlayerEntity.Properties.Stats[(int)Stat.Dexterity].Value;

                for (int I = 0; I < System.Enum.GetNames(typeof(EquipmentSlot)).Length; I++)
                {
                    EquipmentSlot Slot = (EquipmentSlot)I;

                    if (Equipment.ContainsKey(Slot) && Equipment[Slot] != null)
                        Total += Equipment[Slot].Defense;
                }

                return Total;
            }
        }

        public int HandDamage(EquipmentSlot TargetSlot)
        {
            if (Hands.ContainsKey(TargetSlot))
            {
                if (Hands[TargetSlot].GetType() == typeof(Weapon))
                {
                    Weapon Wpn = (Weapon)Hands[TargetSlot];

                    int Damage = Wpn.Damage;

                    foreach (Effect E in Wpn.Effects)
                    {
                        Damage Dmg = E as Damage;
                        if (Dmg != null)
                            Damage = Dmg.Amount;
                    }

                    return Damage;
                }
                else
                {
                    return 0; // Don't report an error, player is probably holding a shield in this hand
                }
            }

            Debug.LogError($"There is no slot {TargetSlot} under \"Hands\"!");
            return 0;
        }

        public int HandDefense(EquipmentSlot TargetSlot)
        {
            if (Hands.ContainsKey(TargetSlot))
            {
                if (Hands[TargetSlot].GetType() == typeof(Armour))
                {
                    return ((Armour)Hands[TargetSlot]).Defense;
                }
                else if (Hands[TargetSlot].GetType() == typeof(Weapon))
                {
                    return ((Weapon)Hands[TargetSlot]).Defense;
                }
            }

            Debug.LogError($"There is no slot {TargetSlot} under \"Hands\"!");
            return 0;
        }

        public bool TryEquipItem(Equipment Equip, EquipmentSlot TargetSlot, GUISlot StartSlot)
        {
            try
            {
                Debug.Log($"Equip Item {Equip.Name} to {TargetSlot} (Equip is {Equip.Slot})");
                Debug.Log($"StartSlot was {StartSlot.name}");

                if ((SkyEngine.IsHandSlot(Equip.Slot) && SkyEngine.IsHandSlot(TargetSlot)) || Equip.Slot == TargetSlot)
                {
                    if (SkyEngine.IsHandSlot(TargetSlot))
                    {
                        Hands[TargetSlot] = Equip;

                        Weapon Wpn = Equip as Weapon;

                        if (Wpn != null)
                        {
                            SkyEngine.PlayerEntity.GetComponent<Objects.Charactermanager>().WeaponModel = Wpn.Model;
                        }

                        SkyEngine.OnEquipmentUpdated(this);

                        Inventory.LocalInventory.CurrentMenuOffset = 0;
                        Inventory.LocalInventory.ItemsMenu.SelectedIndex = 0;

                        return true;
                    }
                    else
                    {
                        Armour A = Equip as Armour;

                        if (A != null)
                        {

                            if (Equipment[TargetSlot] != null)
                            {
                                if (StartSlot)
                                    StartSlot.Item = new ItemStack { Count = 1, ItemID = Equipment[TargetSlot].ID };
                            }


                            Equipment[TargetSlot] = A;

                            SkyEngine.OnEquipmentUpdated(this);

                            Inventory.LocalInventory.CurrentMenuOffset = 0;
                            Inventory.LocalInventory.ItemsMenu.SelectedIndex = 0;

                            return true;
                        }
                    }
                }
            } catch { }

            return false;
        }
    }
}