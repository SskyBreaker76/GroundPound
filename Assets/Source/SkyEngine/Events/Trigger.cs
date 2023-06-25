using UnityEngine;
using UnityEngine.Events;

namespace SkySoft.Events 
{
    [AddComponentMenu("SkyEngine/Events/Trigger")]
    public class Trigger : MonoBehaviour
    {
        public UnityEvent<Entities.Entity> OnTriggered;
        public bool AnyEntity = false;

        /// <summary>
        /// Execute the OnTriggered event
        /// </summary>
        /// <param name="Entity">The Entity who caused the Trigger</param>
        protected virtual void PerformTrigger(Entities.Entity Entity)
        {
            Debug.Log($"PerformTrigger({Entity.Properties.Name})");

            if (Entity == SkyEngine.PlayerEntity || AnyEntity)
                OnTriggered.Invoke(Entity);
        }
    }
}