using System.Collections.Generic;
using UnityEngine;
using SkySoft.Objects;
using SkySoft.Entities;

namespace SkySoft.Inventory
{
    public enum AnimationSet
    {
        Sword,
        Greatsword,
        Polearm
    }

    [CreateAssetMenu(fileName = "New Weapon", menuName = "SkyEngine/Items/Weapon")]
    public class Weapon : Equipment
    {
        public WeaponType Type;
        public WeaponModel Model;
        public AnimationSet Animations;
        public int Damage;
        public int Defense;
        [Tooltip("The \"Speed\" of a Melee Weapon is the amount of time in seconds it takes to swing the weapon.")]
        public float SpeedMultiplier = 1;
        public float ProjectileSpeed = 20;
        public Stat UseStat;
        [SerializeReference]
        [ContextMenuItem("Add Effect (Damage)", "AddDamageEffect")]
        [ContextMenuItem("Add Effect (Healing)", "AddHealingEffect")]
        [ContextMenuItem("Add Effect (Damage over Time)", "AddDoTEffect")]
        [ContextMenuItem("Add Effect (Sleep)", "AddSleepEffect")]
        public List<Effect> Effects;

        private void AddDamageEffect()
        {
            Effects.Add(new Damage());
        }

        private void AddHealingEffect()
        {
            Effects.Add(new Healing());
        }

        private void AddDoTEffect()
        {
            Effects.Add(new DamageOverTime());
        }

        private void AddSleepEffect()
        {
            Effects.Add(new Sleep());
        }

        public virtual void OnAttack(Entity Target, Entity Attacker) 
        {
            int BDefense = 10 + Target.Properties.Stats[(int)Stat.Dexterity].Value;
            int AAttack = Inventory.LocalInventory.GetComponent<EquipmentManager>().HandDamage(EquipmentSlot.RightHand);

            int Damage = (AAttack * 4) - (BDefense * 4);

            Target.Properties.Vitals[0].Value -= Damage;
        }
    }
}