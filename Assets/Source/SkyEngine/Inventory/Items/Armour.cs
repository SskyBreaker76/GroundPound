using UnityEngine;
using SkySoft.Entities;

namespace SkySoft.Inventory 
{
    [CreateAssetMenu(fileName = "New Armour", menuName = "SkyEngine/Items/Armour")]
    public class Armour : Equipment
    {
        public ArmourWeight Weight;
        public int Defense;

        public virtual void OnDamageReceive (Entity Attacker, Entity Self) 
        {
            int ADamage = Attacker.Properties.Stats[(int)Stat.Strength].Value + 10;
            int BDefense = Inventory.LocalInventory.GetComponent<EquipmentManager>().ArmourValue;

            int FinalDamage = (ADamage * 4) - (BDefense * 4);
            Self.Properties.Vitals[0].Value -= FinalDamage;
        }
    }
}