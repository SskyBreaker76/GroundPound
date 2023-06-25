using UnityEngine;
using UnityEngine.UI;

namespace SkySoft.UI
{
    [AddComponentMenu("SkyEngine/UI/Tooltip")]
    public class Tooltip : MonoBehaviour
    {
        public static Tooltip Instance;

        public Text Label;
        public Color HeaderColour;
        public Color BodyColour;

        private void Awake()
        {
            Instance = this;
        }

        public static void Set(string Header, string Body = "")
        {
            if (!string.IsNullOrEmpty(Header))
            {
                string Text = $"<color=#{ColorUtility.ToHtmlStringRGBA(Instance.HeaderColour)}>{Header}</color>{(string.IsNullOrEmpty(Body) ? "" : "\n")}<color=#{ColorUtility.ToHtmlStringRGBA(Instance.HeaderColour)}>{Body}</color>";
                Instance.Label.text = Text;
                Instance.gameObject.SetActive(true);
            }
            else
            {
                Clear();
            }
        }

        public static void Clear()
        {
            Instance.gameObject.SetActive(true);
        }
    }
}