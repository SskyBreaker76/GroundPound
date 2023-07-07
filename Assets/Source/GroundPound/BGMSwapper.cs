/*
    Developed by Sky MacLennan
 */

using SkySoft;
using SkySoft.Generated;
using SkySoft.IO;
using UnityEngine;

namespace Sky.GroundPound
{
    public class BGMSwapper : MonoBehaviour
    {
        public E_BGMs BGM;
        public int Tempo;

        public void Awake()
        {
            SkyEngine.BGM.StartBGM((BGM != E_BGMs.Title ? BGM : (ConfigManager.GetOption("TitleMusic", 0, "Personalisation") == 1 ? E_BGMs.TitleAlt : E_BGMs.Title)).ToString(), () => {}, false);
        }

        private void Update()
        {
            Game.BGM_Tempo = Tempo;
        }
    }
}