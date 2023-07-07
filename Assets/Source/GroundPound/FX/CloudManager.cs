/*
    Developed by Sky MacLennan
 */

using SkySoft;
using System;
using System.Collections;
using System.Runtime.Versioning;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

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
        public float CloudLifetime = 24;
        public float CloudMinimumSpeed = 1;
        public float CloudMaximumSpeed = 1.5f;

        public int StartClouds = 12;

        private void Awake()
        {
            for (int I = 0; I < StartClouds; I++)
            {
                SpawnCloud(true);
            }
        }

        private void OnDrawGizmosSelected()
        {
            SkyEngine.Gizmos.Colour = Color.magenta;
            SkyEngine.Gizmos.DrawWireCube(transform.position, new Vector3(SpawnArea.x, SpawnArea.y, 0.001f));
        }

        private int TargetSprite;

        private IEnumerator HandleCloud(SpriteRenderer Cloud, float Depth)
        {
            float CloudLifeTime = CloudLifetime;
            float CloudSpeed = Mathf.Lerp(CloudMaximumSpeed, CloudMinimumSpeed, Depth);
            float CurrentTime = 0;

            while (true)
            {
                float V = CurrentTime / CloudLifeTime;

                Cloud.transform.position += new Vector3(CloudSpeed * Time.fixedDeltaTime, 0, 0);
                Cloud.color = ColourOverTime.Evaluate(V);

                CurrentTime += Time.fixedDeltaTime;

                if (GameManager.Instance)
                {
                    if (!GameManager.Instance.IsSpriteInBounds(Cloud) && CurrentTime > CloudLifetime)
                    {
                        Destroy(Cloud.gameObject);
                        break;
                    }
                }
                else
                {
                    if (CurrentTime > 128)
                    {
                        Destroy(Cloud.gameObject);
                        break;
                    }
                }

                yield return new WaitForFixedUpdate();
            }
        }

        private void SpawnCloud(bool InitialSpawn = false)
        {
            int Attempts = 0;

            ChooseCloudAndPosition:
            Attempts++;
            if (Attempts >= MaxTries)
            {
                return;
            }

            float V = Random.value;

            Vector3 Position = transform.position;

            if (InitialSpawn)
                Position += new Vector3(Random.Range(-SpawnArea.x, SpawnArea.x), Random.Range(-SpawnArea.y, SpawnArea.y), V * 2) / 2;
            else
                Position += new Vector3(-SpawnArea.x, Random.Range(-SpawnArea.y, SpawnArea.y), V * 2) / 2;

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

            AnimatedElement Animator = Cloud.AddComponent<AnimatedElement>();
            Animator.FrameDuration = 1;
            Animator.Frames = ChosenCloud.Sprites;
            Animator.SetFrame(Random.Range(0, CloudSprites.Length));

            BoxCollider2D Collider = Cloud.AddComponent<BoxCollider2D>();
            Collider.offset = ChosenCloud.CloudCentre;
            Collider.size = ChosenCloud.CloudSize;

            Cloud.tag = "Cloud";
            Cloud.layer = LayerMask.NameToLayer("Cloud");

            if (R == null)
                Debug.Log("NO RENDERER");

            StartCoroutine(HandleCloud(R, V));
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