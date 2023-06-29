/*
    Developed by Sky MacLennan
 */

using UnityEngine;

namespace SkySoft
{
    public abstract class LoadingBar : MonoBehaviour
    {
        protected abstract void OnLoadingProgress(float Progress);

        protected void Awake()
        {
            SkyEngine.OnLoadingProgress += OnLoadingProgress;
        }
    }
}
