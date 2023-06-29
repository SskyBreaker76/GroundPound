/*
    Developed by Sky MacLennan
 */

using SkySoft;
using SkySoft.Generated;
using UnityEngine;

namespace Sky.GroundPound
{
    public class BGMSwapper : MonoBehaviour
    {
        public E_BGMs BGM;

        private void Awake()
        {
            SkyEngine.BGM.StartBGM(BGM.ToString(), () => { }, false);
        }
    }
}