using SkySoft.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Inventory
{
    public enum SpellTarget
    {
        None,
        Target,
        Self,
        AOE
    }

    public class SpellCastProperties
    {
        public Spell Spell;
        public Entity Target;
        public Entity Caster;
    }

    [CreateAssetMenu(fileName = "New Spell", menuName = "SkyEngine/Spell")]
    public class Spell : ScriptableObject
    {
        public string Name;
        [TextArea] public string Description;
        public float ManaCost;
        public SpellTarget Target;
        [Combo("Yes", "No")]
        public bool IsProjectile;

        public GameObject CastEffect;

        [SerializeReference]
        [ContextMenuItem("Add Effect (Damage)", "AddDamageEffect")]
        [ContextMenuItem("Add Effect (Healing)", "AddHealingEffect")]
        [ContextMenuItem("Add Effect (Damage over Time)", "AddDoTEffect")]
        [ContextMenuItem("Add Effect (Sleep)", "AddSleepEffect")]
        public List<Effect> Effects;

        public Stat CastingSkill;

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

        public float Damage
        {
            get
            {
                float Value = 0;

                foreach (Effect E in Effects)
                {
                    Damage AsDamage = E as Damage;

                    if (AsDamage != null)
                    {
                        Value += AsDamage.Amount;
                    }

                    Healing AsHealing = E as Healing;

                    if (AsHealing != null)
                    {
                        Value -= AsHealing.Amount;
                    }
                }

                return Value;
            }
        }

        public void Cast(Entity Caster, Entity Target)
        {
            Caster.Casting = new SpellCastProperties 
            { 
                Caster = Caster, 
                Target = Target, 
                Spell = this 
            };
        }

        public static void FinishCast(SpellCastProperties Properties)
        {
            Properties.Caster.Properties.Vitals[1].Value -= Properties.Spell.ManaCost;
            if (!Properties.Spell.IsProjectile)
            {
                Instantiate(Properties.Spell.CastEffect, Properties.Target.transform.position, Properties.Target.transform.rotation);
            }
            else
            {
                Instantiate(Properties.Spell.CastEffect, Properties.Caster.transform.position, Properties.Caster.transform.rotation);
            }
        }
    }
}