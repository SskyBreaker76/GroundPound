using SkySoft.Events.Graph;
using SkySoft.IO;
using SkySoft.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SkySoft.Interaction
{
    [AddComponentMenu("SkyEngine/Interaction/Interaction Manager")]
    public class InteractionManager : MonoBehaviour
    {
        public static bool AllowInteraction = true;

        public static InteractionManager Instance;
        public Image PromptIcon;
        private Interactive Selected;

        public void SetSelected(Interactive Value) => Selected = Value;
        public void ClearSelected() => Selected = null;
        public bool GetIsSelected(Interactive Value) => Selected == Value;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (Time.timeScale > 0 && AllowInteraction)
            {
                if (Selected)
                {
                    CommandMenu.BlockCancel = true;

                    PromptIcon.GetComponent<Animator>().SetBool("CanInteract", true);
                    PromptIcon.transform.position = Camera.main.WorldToScreenPoint(Selected.IconPosition);

                    if (ConfigManager.GetOption("ShowButtons", 0, "Controls") == 0)
                    {
                        switch (Selected.Icon)
                        {
                            case InteractionType.Talk:
                                PromptIcon.sprite = SkyEngine.Properties.TalkIcon;
                                break;
                            case InteractionType.Use:
                                PromptIcon.sprite = SkyEngine.Properties.UseIcon;
                                break;
                            case InteractionType.Inspect:
                                PromptIcon.sprite = SkyEngine.Properties.InspectIcon;
                                break;
                            case InteractionType.Shop:
                                PromptIcon.sprite = SkyEngine.Properties.ShopIcon;
                                break;
                        }
                    }
                    else
                    {
                        PromptIcon.sprite = SkyEngine.Properties.Buttons[SkyEngine.Properties.GetBinding("interact")];
                    }

                    if (SkyEngine.Input.Gameplay.Interact.WasPressedThisFrame() && !EventGraphManager.IsProcessingEvent)
                    {
                        Selected.Interact(GetComponentInChildren<Entities.Entity>());
                    }
                }
                else
                {
                    CommandMenu.BlockCancel = false;

                    PromptIcon.GetComponent<Animator>().SetBool("CanInteract", false);
                }
            }
            else
            {
                CommandMenu.BlockCancel = false;
            }
        }
    }
}