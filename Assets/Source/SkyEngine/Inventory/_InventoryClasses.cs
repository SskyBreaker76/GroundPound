using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Inventory
{
    public enum EquipmentSlot
    {
        Feet,
        Legs,
        Waist,
        Body,
        Wrists,
        Hands,
        Shoulders,
        Head,
        LeftWrist,
        LeftHand,
        LeftFinger,
        RightWrist,
        RightHand,
        RightFinger,
        Neck,
        RightEar,
        LeftEar,
        Back
    }

    public enum ArmourWeight
    {
        Clothing,
        LightArmour,
        MediumArmour,
        HeavyArmour
    }

    public enum WeaponType
    {
        Dagger,
        ShortBlade,
        LongBlade,
        Rapier,
        GreatBlade,
        HandAxe,
        BattleAxe,
        Spear,
        Bow,
        Crossbow,
        SmallFirearm,
        LargeFirearm,
        ThrowingWeapon,
        SmallShield,
        LargeShield
    }

    public enum DamageType
    {
        Physical,
        Fire,
        Ice,
        Water,
        Wind,
        Earth,
        Electrical,
        Psychic
    }

    public enum Stat
    {
        Strength,
        Dexterity,
        Constitution,
        Charisma,
        Knowledge,
        Wisdom
    }

    [System.Serializable]
    public class Effect { }

    [System.Serializable]
    public class Damage : Effect
    {
        public int VitalIndex;
        public int Amount;
        public DamageType Type;
    }

    [System.Serializable]
    public class Healing : Effect
    {
        public int VitalIndex;
        public int Amount;
    }

    [System.Serializable]
    public class DamageOverTime : Effect
    {
        public int VitalIndex;
        public int Duration;
        public int Amount;
    }

    [System.Serializable]
    public class Sleep : Effect
    {
        public float MinimumDuration;
        public float MaximumDuration;
    }

    [System.Serializable]
    public class ItemStack
    {
        public string ItemID;
        [Range(0, SkyEngine.GeneralStackSize)] public int Count;

        public ItemStack()
        {
            ItemID = "";
            Count = 0;
        }

        public ItemStack(Item Base, int StackSize = 1)
        {
            ItemID = Base.ID;
            Count = StackSize;
        }

        public Item Item
        {
            get
            {
                if (SkyEngine.Items.ContainsKey(ItemID))
                {
                    return SkyEngine.Items[ItemID];
                }

                Debug.LogError($"No such item using ID \"{ItemID}\"");
                return ScriptableObject.CreateInstance<Item>();
            }
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public enum CurrencyTier
    {
        Silver, Gold, Copper
    }

    [System.Serializable]
    public struct Currencies
    {
        public Currency Copper;
        public Currency Gold;
        public Currency Silver;
    }

    [System.Serializable]
    public struct Currency
    {
        public CurrencyTier Tier;
        public int Value;

        public Currency(CurrencyTier Tier, int Value)
        {
            this.Tier = Tier;
            this.Value = Value;
        }
    }

    public static class CurrencyManager
    {
        public static Dictionary<CurrencyTier, int> CurrencyValues => new Dictionary<CurrencyTier, int>
        {
            { CurrencyTier.Copper, 1 }
        };

        public static Currency GetValue(int Value)
        {
            return new Currency(CurrencyTier.Copper, Value);
        }

        public static Currency[] GetValues(int Value)
        {
            List<Currency> Currencies = new List<Currency>
            {
                new Currency(CurrencyTier.Copper, Value)
            };

            return Currencies.ToArray();
        }
    }
}