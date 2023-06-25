using UnityEngine;
using UnityEngine.Events;
using SkySoft.Entities;
using System.Collections.Generic;
using SkySoft.Audio;

namespace SkySoft.Inventory
{
    [CreateAssetMenu(fileName = "New Consumable", menuName = "SkyEngine/Items/Consumable")]
    public class Consumable : Item
    {
        public UnityEvent<Entity> OnItemUsed;
        public AudioClip UseSound;
        [Space]
        [ContextMenuItem("Add Effect (Damage)", "AddDamageEffect")]
        [ContextMenuItem("Add Effect (Healing)", "AddHealingEffect")]
        [ContextMenuItem("Add Effect (Damage over Time)", "AddDoTEffect")]
        [ContextMenuItem("Add Effect (Sleep)", "AddSleepEffect")]
        [SerializeReference] public List<Effect> Effects = new List<Effect>();

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

        public override void OnUse(Entity User)
        {
            foreach (Effect E in Effects)
            {
                if (E.GetType() == typeof(Damage))
                {
                    Damage Dmg = E as Damage;

                    User.Properties.Vitals[Dmg.VitalIndex].Value -= Dmg.Amount;
                }
                if (E.GetType() == typeof(Healing))
                {
                    Healing Heal = E as Healing;

                    User.Properties.Vitals[Heal.VitalIndex].Value += Heal.Amount;
                }
                if (E.GetType() == typeof(DamageOverTime))
                {
                    Debug.Log("Damage over time not added yet!");
                }
                if (E.GetType() == typeof(Sleep))
                {
                    Debug.Log("Sleep not added yet!");
                }
            }

            if (UseSound)
            {
                SkyEngine.PlayerEntity.GetComponent<AudioSource>().PlayOneShot(UseSound);
            }

            OnItemUsed.Invoke(User);
        }
    }
}
