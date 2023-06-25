using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft.Inventory
{
    [CreateAssetMenu(fileName = "New Book", menuName = "SkyEngine/Items/Book")]
    public class Book : Item
    {
        public string Author;
        [TextArea] public string Text;
        public Spell LearnedSpell;
    }
}