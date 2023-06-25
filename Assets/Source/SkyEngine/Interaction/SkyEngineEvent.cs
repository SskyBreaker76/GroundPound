using SkySoft.Entities;
using SkySoft.Events.Graph;
using UnityEngine;
using UnityEngine.Events;

namespace SkySoft.Interaction
{
    [AddComponentMenu("SkyEngine/Interaction/Event")]
    public class SkyEngineEvent : Interactive
    {
        public UnityEvent OnFinishedEvent;
        public EventTree Event;

#if UNITY_EDITOR
        public int EventIndex;
#endif

        protected override void Interaction(Entity Entity = null)
        {
            EventGraphManager.Instance.ExecuteEvent(Event, this, OnFinishedEvent.Invoke);
        }
    }
}