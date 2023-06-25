using SkySoft.Entities;
using SkySoft.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static DialoguePanel;

namespace SkySoft.Events.Graph
{
    [AddComponentMenu("SkyEngine/Events/Event Manager")]
    public class EventGraphManager : MonoBehaviour
    {
        public static bool IsProcessingEvent = false;
        public DialoguePanel Dialogue;
        public static EventGraphManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public static Entity CurrentEvent { get; private set; }

        public void ExecuteEvent(EventTree Event, Interactive Caller, Action Finished = null)
        {
            Event.Data.EventName = Caller.InstanceID;
            Event.Data.ReadFile(() =>
            {
                CurrentEvent = Caller;
                InteractionManager.Instance.ClearSelected();
                Event.Run(() =>
                {
                    FindObjectOfType<DialoguePanel>().CloseMenu();
                    InteractionManager.Instance.SetSelected(Caller);
                    if (Finished != null)
                        Finished();
                });
            });
        }

        public void ShowText(string Speaker, string Text, PanelLocation Position, int StartDelay, DialogueChoice[] Choices, Action OnDialogueClosed)
        {
            Text = Text.Replace("\\S", Speaker).Replace("\\N", SkyEngine.PlayerEntity.Properties.Name);

            Dialogue.ShowText(Speaker, Text, Position, StartDelay, Choices, OnDialogueClosed);
        }

        public EventTree IncompleteEventError;

        public Entity GetEntity(string InstanceID)
        {
            foreach (Entity E in FindObjectsOfType<Entity>())
            {
                if (E.InstanceID == InstanceID)
                    return E;
            }

            return null;
        }

        public async void PlaySound (AudioClip Sound, Action OnFinished = null) 
        {
            AudioSource Src = new GameObject($"EventGraphSound ({Sound.name})").AddComponent<AudioSource>();
            Src.clip = Sound;
            Src.Play();

            while (Src.isPlaying)
                await Task.Delay(10);

            Destroy(Src.gameObject);
            if (OnFinished != null)
                OnFinished();
        }
    }
}