using System;
using System.Threading.Tasks;
using UnityEngine;

namespace SkySoft.Interaction
{
    [AddComponentMenu("SkyEngine/Interaction/Save Point (USE THE SAVEGAME EVENT)")]
    public class SavePoint : Interactive
    {
        protected override void Interaction(Entities.Entity Entity = null)
        {
            SkyEngine.SaveGame(() => { });
        }
    }
}