using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sky.GroundPound
{
    [ExecuteInEditMode, AddComponentMenu("Ground Pound/Character Colour Manager")]
    public class CharacterColourManager : MonoBehaviour
    {
        private SpriteRenderer m_Renderer;
        protected SpriteRenderer Renderer
        {
            get
            {
                if (!m_Renderer)
                    m_Renderer = GetComponent<SpriteRenderer>();

                return m_Renderer;
            }
        }

        private const string ColourKey = "_Colour";
        public Shader BaseShader;

        private Material m_Material;
        protected Material Material
        {
            get
            {
                if (!m_Material)
                    m_Material = UnityEngine.Rendering.CoreUtils.CreateEngineMaterial(BaseShader);
                return m_Material;
            }
        }

        private void Update()
        {
            if (BaseShader)
                UpdateRenderer();
        }

        public void UpdateRenderer()
        {
            Material.SetColor(ColourKey, Renderer.color);
            m_Renderer.sharedMaterial = Material;
        }
    }
}