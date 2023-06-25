using SkySoft.Entities;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Inventory
{
    [CreateAssetMenu(fileName = "New Item", menuName = "SkyEngine/Items/Item")]
    public class Item : ScriptableObject
    {
        public string ID;
        [Space]
        public string Name;
        public Sprite Icon;
        [TextArea] public string Description;
        public int Value;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ID))
            {
                ID = SkyEngine.CreateID();
            }    
        }

        public Dictionary<CurrencyTier, int> DisplayValue
        {
            get
            {
                Currency[] Values = CurrencyManager.GetValues(Value);

                Dictionary<CurrencyTier, int> Result = new Dictionary<CurrencyTier, int>();
                foreach (Currency V in Values)
                {
                    Result.Add(V.Tier, V.Value);
                }

                return Result;
            }
        }

        public virtual void OnUse(Entity User) { }
        public virtual void OnEquip(EquipmentManager Equipper) { }

        public override bool Equals(object other)
        {
            return base.Equals(other);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}