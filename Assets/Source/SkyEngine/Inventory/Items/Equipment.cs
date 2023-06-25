using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Inventory
{
    [CreateAssetMenu(fileName = "New Equipment", menuName = "SkyEngine/Items/Equipment")]
    public class Equipment : Item
    {
        public EquipmentSlot Slot;
    }
}