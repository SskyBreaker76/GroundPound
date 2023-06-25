using SkySoft.IO;
using SkySoft.Objects;
using UnityEngine;
using System;
using SkySoft.Inventory;

namespace SkySoft.Entities
{
    public enum CoreStat
    {
        Strength,
        Dexterity,
        Constitution,
        Charisma,
        Knowledge,
        Wisdom
    }

    public enum CoreSkill
    {
        Daggers,
        ShortBlades,
        LongBlades,
        Rapiers,
        GreatBlades,
        HandAxes,
        BattleAxes,
        Spears,
        Bows,
        Crossbows,
        SmallFirearms,
        LargeFirearms,
        ThrowingWeapons,
        MagicArmour,
        LightArmour,
        MediumArmour,
        HeavyArmour,
        Shields,
        Igna,
        Aqua,
        Terra,
        Erae,
        Licht,
        Dar
    }

    [Serializable]
    public class Stat
    {
        public string LongName;
        public string ShortName;
        public int Value;
    }

    [Serializable]
    public class Skill
    {
        public string Name;
        public int BaseStat;
        public int Bonus;
        public bool HasProficiency;
    }

    [Serializable]
    public class Vital
    {
        public string LongName;
        public string ShortName;
        public int BaseStat;
        [Space]
        public int BonusPerLevel;
        public float Value;
        public float MaxValue;

        public float Value01 => Value / MaxValue;

        public Action<float> OnValueChanged;
    }

    [Serializable]
    public class EntityProperties
    {
        public string ID;
        public Vector3 Position;
        public Vector3 Rotation;
        public string Name;
        public string NickName;

        public CharacterBlend[] BlendShapes;

        public Stat[] Stats =
        {
            new Stat { LongName = "Strength", ShortName = "Str", Value = 10 },
            new Stat { LongName = "Dexterity", ShortName = "Dex", Value = 10 },
            new Stat { LongName = "Constitution", ShortName = "Con", Value = 10 },
            new Stat { LongName = "Charisma", ShortName = "Cha", Value = 10 },
            new Stat { LongName = "Knowledge", ShortName = "Kno", Value = 10 },
            new Stat { LongName = "Wisdom", ShortName = "Wis", Value = 10 }
        };

        public Skill[] Skills =
        {
            new Skill { Name = "Daggers", BaseStat = (int)CoreStat.Dexterity },
            new Skill { Name = "Short Blades", BaseStat = (int)CoreStat.Strength },
            new Skill { Name = "Long Blades", BaseStat = (int)CoreStat.Strength },
            new Skill { Name = "Rapiers", BaseStat = (int)CoreStat.Dexterity },
            new Skill { Name = "Great Blades", BaseStat = (int)CoreStat.Strength },
            new Skill { Name = "Hand Axes", BaseStat = (int)CoreStat.Dexterity },
            new Skill { Name = "Battle Axes", BaseStat = (int)CoreStat.Strength },
            new Skill { Name = "Spears", BaseStat = (int)CoreStat.Dexterity },
            new Skill { Name = "Bows", BaseStat = (int)CoreStat.Dexterity },
            new Skill { Name = "Crossbows", BaseStat = (int)CoreStat.Dexterity },
            new Skill { Name = "Small Firearms", BaseStat = (int)CoreStat.Dexterity },
            new Skill { Name = "Large Firearms", BaseStat = (int)CoreStat.Dexterity },
            new Skill { Name = "Throwing Weapons", BaseStat = (int)CoreStat.Dexterity },
            new Skill { Name = "Magic Armour", BaseStat = (int)CoreStat.Wisdom },
            new Skill { Name = "Light Armour", BaseStat = (int)CoreStat.Dexterity },
            new Skill { Name = "Medium Armour", BaseStat = (int)CoreStat.Strength },
            new Skill { Name = "Heavy Armour", BaseStat = (int)CoreStat.Strength },
            new Skill { Name = "Shields", BaseStat = (int)CoreStat.Dexterity },
            new Skill { Name = "Igna", BaseStat = (int)CoreStat.Wisdom },
            new Skill { Name = "Aqua", BaseStat = (int)CoreStat.Wisdom },
            new Skill { Name = "Terra", BaseStat = (int)CoreStat.Wisdom },
            new Skill { Name = "Erae", BaseStat = (int)CoreStat.Wisdom },
            new Skill { Name = "Licht", BaseStat = (int)CoreStat.Wisdom },
            new Skill { Name = "Dar", BaseStat = (int)CoreStat.Wisdom }
        };

        public Vital[] Vitals =
        {
            new Vital { LongName = "Health", ShortName = "HP", BaseStat = (int)CoreStat.Constitution, BonusPerLevel = 4, Value = 100, MaxValue = 100 },
            new Vital { LongName = "Spell Slots", ShortName = "SP", BaseStat = (int)CoreStat.Knowledge, BonusPerLevel = 1, Value = 100, MaxValue = 100 },
            new Vital { LongName = "Abilities", ShortName = "AP", BaseStat = (int)CoreStat.Dexterity, BonusPerLevel = 1, Value = 0, MaxValue = 100 }
        };
    }

    [AddComponentMenu("SkyEngine/Entities/Entity")]
    public class Entity : Objects.SerializedObject
    {
        public bool CanBeDamaged = true;
        public Transform ConversationLookTarget;

        public SpellCastProperties Casting;

        public void CastSpell()
        {
        }

        public EntityProperties Properties;
        public int Level;
        public AnimationCurve HealthPerLevel = new AnimationCurve(new Keyframe(1, 123), new Keyframe(100, 9087));
        public AnimationCurve MagickaPerLevel = new AnimationCurve(new Keyframe(1, 60), new Keyframe(100, 520));
        public AnimationCurve ActionsPerLevel = new AnimationCurve(new Keyframe(1, 2), new Keyframe(100, 31));

        public void Damage(float Amount)
        {
            Properties.Vitals[0].Value -= Amount;
            OnDamaged(Amount);
        }

        protected virtual void OnDamaged(float Amount)
        {

        }

        protected virtual void Awake()
        {
            UpdateVitals();

            if (FileManager.FileExists<EntityProperties>("Entities", InstanceID, ".entity"))
                Serialize();
        }

        protected virtual void UpdateVitals(bool Reset = true)
        {
            Properties.Vitals[0].MaxValue = HealthPerLevel.Evaluate(Level);
            Properties.Vitals[1].MaxValue = MagickaPerLevel.Evaluate(Level);
            Properties.Vitals[2].MaxValue = ActionsPerLevel.Evaluate(Level);

            if (Reset)
            {
                Properties.Vitals[0].Value = Properties.Vitals[0].MaxValue;
                Properties.Vitals[1].Value = Properties.Vitals[1].MaxValue;
                Properties.Vitals[2].Value = Properties.Vitals[2].MaxValue;
            }
        }

        /// <summary>
        /// Write the Properties variable to the Entity's storage file
        /// </summary>
        /// <returns>Entity JSON string</returns>
        public override string Serialize()
        {
            Properties.ID = InstanceID;

            return FileManager.WriteFile("Entities", InstanceID, Properties, Complete => { }, ".entity");
        }

        public virtual void Move(Vector3 Direction, bool IsForceMove = false) { }

        /// <summary>
        /// Load the Properties variable from the Entity's storage file
        /// </summary>
        public virtual void DeSerialize()
        {
            EntityProperties Props = FileManager.ReadFile<EntityProperties>("Entities", InstanceID, Complete => { }, ".entity");

            transform.position = Props.Position;
            transform.eulerAngles = Props.Rotation;
            Properties = Props;
        }

        public int WeaponAnimation(Inventory.WeaponType WeaponType)
        {
            switch (WeaponType)
            {
                default:
                    return 0;
                case Inventory.WeaponType.Dagger:
                    return 1;
                case Inventory.WeaponType.Rapier:
                    return 2;
                case Inventory.WeaponType.GreatBlade:
                    return 3;
                case Inventory.WeaponType.BattleAxe:
                    return 4;
                case Inventory.WeaponType.Spear:
                    return 5;
                case Inventory.WeaponType.Bow:
                    return 6;
                case Inventory.WeaponType.Crossbow:
                    return 7;
                case Inventory.WeaponType.SmallFirearm:
                    return 8;
                case Inventory.WeaponType.LargeFirearm:
                    return 9;
                case Inventory.WeaponType.ThrowingWeapon:
                    return 10;
                case Inventory.WeaponType.SmallShield:
                    return 11;
                case Inventory.WeaponType.LargeShield:
                    return 12;
            }
        }
    }
}