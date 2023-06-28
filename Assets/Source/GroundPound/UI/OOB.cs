/*
    Developed by Sky MacLennan
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sky.GroundPound
{
    public class OOB : MonoBehaviour
    {
        public static bool IsOutOfBounds;
        public RectTransform OOB_Tracker;
        public Vector2 Padding = new Vector2(4, 4);

        private void Update()
        {
            if (IsOutOfBounds)
            {
                Vector2 MinPosition = new Vector2((OOB_Tracker.rect.width / 2) + Padding.x, (OOB_Tracker.rect.height / 2) + Padding.y);
                Vector2 MaxPosition = new Vector2(Screen.width - ((OOB_Tracker.rect.width / 2) + Padding.x), Screen.height - ((OOB_Tracker.rect.height / 2) + Padding.y));

                Vector2 Pos = GameManager.Instance.MainCamera.WorldToScreenPoint(Player.LocalPlayer.transform.position);
                Pos.x = Mathf.Clamp(Pos.x, MinPosition.x, MaxPosition.x);
                Pos.y = Mathf.Clamp(Pos.y, MinPosition.y, MaxPosition.y);
            }

            OOB_Tracker.gameObject.SetActive(IsOutOfBounds);
        }
    }
}