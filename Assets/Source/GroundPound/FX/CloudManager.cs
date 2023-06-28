/*
    Developed by Sky MacLennan
 */

using SkySoft;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sky.GroundPound
{
    [System.Serializable]
    public class SpriteCollection
    {
        public float SecondsPerFrame = 0.5f;
        public Vector2 CloudCentre;
        public Vector2 CloudSize;
        public Sprite[] Sprites;
    }

    [AddComponentMenu("Ground Pound/Cloud System")]
    public class CloudManager : MonoBehaviour
    {
        public Shader BaseShader;
        [Space]
        [SerializeField] protected SpriteCollection[] CloudSprites;
        public Vector2 SpawnArea = Vector2.one;
        [SerializeField] protected float MinimumSpawnWait = 2f;
        [SerializeField] protected float MaximumSpawnWait = 12f;
        protected float CurrentSpawnWait;
        protected float LastSpawn;
        public int MaxTries = 12;

        [Header("Visuals")]
        public Gradient ColourOverTime = new Gradient();
        public float MinimumCloudLife = 6;
        public float MaximumCloudLife = 24;
        public float CloudMinimumSpeed = 1;
        public float CloudMaximumSpeed = 1.5f;

        private void OnDrawGizmosSelected()
        {
            SkyEngine.Gizmos.Colour = Color.magenta;
            SkyEngine.Gizmos.DrawWireCube(transform.position, new Vector3(SpawnArea.x, SpawnArea.y, 0.001f));
        }

        private IEnumerator HandleCloud(SpriteRenderer Cloud, SpriteCollection Sprites, float Depth)
        {
            float CloudLifeTime = Random.Range(MinimumCloudLife, MaximumCloudLife);
            float CloudSpeed = Mathf.Lerp(CloudMaximumSpeed, CloudMinimumSpeed, Depth);
            float CurrentTime = 0;
            float FrameCounter = Sprites.SecondsPerFrame;
            int TargetSprite = 0;

            while (true)
            {
                Cloud.sprite = Sprites.Sprites[TargetSprite];

                if (FrameCounter < 0)
                {
                    TargetSprite++;
                    if (TargetSprite >= Sprites.Sprites.Length)
                    {
                        TargetSprite = 0;
                    }
                    FrameCounter = Sprites.SecondsPerFrame;
                }

                FrameCounter -= Time.fixedDeltaTime;

                float V = CurrentTime / CloudLifeTime;

                Cloud.transform.position += new Vector3(CloudSpeed * Time.fixedDeltaTime, 0, 0);
                Cloud.color = ColourOverTime.Evaluate(V);

                CurrentTime += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
        }

        private void SpawnCloud()
        {
            int Attempts = 0;

            ChooseCloudAndPosition:
            Attempts++;
            if (Attempts >= MaxTries)
            {
                return;
            }

            float V = Random.value;

            Vector3 Position = transform.position + new Vector3(Random.Range(-SpawnArea.x, SpawnArea.x) / 2, Random.Range(-SpawnArea.y, SpawnArea.y) / 2, V);
            
            SpriteCollection ChosenCloud = CloudSprites[Random.Range(0, CloudSprites.Length)];
            foreach (Collider2D C in Physics2D.OverlapBoxAll(Position, ChosenCloud.CloudSize, 0))
            {
                if (C.tag == "Cloud")
                {
                    goto ChooseCloudAndPosition;
                }
            }

            GameObject Cloud = new GameObject($"CloudObject");
            Cloud.transform.position = Position;
            Cloud.transform.parent = transform;

            Material Mat = CoreUtils.CreateEngineMaterial(BaseShader);
            Mat.SetFloat("_Depth", V);

            SpriteRenderer R = Cloud.AddComponent<SpriteRenderer>();
            R.rendererPriority = -1;
            R.color = Color.clear;
            R.material = Mat;
            R.sprite = ChosenCloud.Sprites[0];

            BoxCollider2D Collider = Cloud.AddComponent<BoxCollider2D>();
            Collider.offset = ChosenCloud.CloudCentre;
            Collider.size = ChosenCloud.CloudSize;

            Cloud.tag = "Cloud";
            Cloud.layer = LayerMask.NameToLayer("Cloud");

            StartCoroutine(HandleCloud(R, ChosenCloud, V));
        }

        private void Update()
        {
            if (Time.time - LastSpawn > CurrentSpawnWait)
            {
                CurrentSpawnWait = Random.Range(MinimumSpawnWait, MaximumSpawnWait);
                SpawnCloud();
                LastSpawn = Time.time;
            }
        }
    }
}