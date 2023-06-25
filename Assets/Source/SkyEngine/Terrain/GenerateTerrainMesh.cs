using JetBrains.Annotations;
using Steamworks.ServerList;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SkySoft.Terrain
{
    public class GenerateTerrainMesh : MonoBehaviour
    {
        public UnityEngine.Terrain Base;
        public Texture2D TerrainSplat;
        public Color[] GrassySurfaces = { Color.red };
        public Material GrassMaterial;

        public bool CreateHeightmap;
        public bool CreateGrassVisibilityMap;

        public float DrawDistance = 32;

        private void OnDrawGizmosSelected()
        {
            SkyEngine.Gizmos.Colour = Color.green;

            foreach (GameObject G in ObjectsCache)
            {
               SkyEngine.Gizmos.DrawSphere(G.GetComponent<Renderer>().bounds.center, 1);
            }
        }

        private void Update()
        {
            foreach (GameObject Patch in ObjectsCache)
            {
                Patch.SetActive(Vector2.Distance(new Vector2(Patch.GetComponent<Renderer>().bounds.center.x, Patch.GetComponent<Renderer>().bounds.center.z), new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z)) < DrawDistance);
            }
        } 

        private void OnValidate()
        {
            if (CreateHeightmap)
            {
#if UNITY_EDITOR
                string ExportPath = UnityEditor.EditorUtility.SaveFilePanelInProject("Export Terrain Heightmap", $"{Base.name}.png", "png", "");
                Texture2D HeightmapTmp = new Texture2D(Base.terrainData.heightmapResolution, Base.terrainData.heightmapResolution, TextureFormat.ASTC_12x12, 1, true);

                float[,] RawHeights = new float[,] { };
                Texture2D DuplicateHeightMap = new Texture2D(Base.terrainData.heightmapResolution, Base.terrainData.heightmapResolution);
                RawHeights = Base.terrainData.GetHeights(0, 0, Base.terrainData.heightmapResolution, Base.terrainData.heightmapResolution);

                for (int Y = 0; Y < DuplicateHeightMap.height; Y++)
                {
                    for (int X = 0; X < DuplicateHeightMap.width; X++)
                    {
                        Color Colour = new Color(RawHeights[X, Y], RawHeights[X, Y], RawHeights[X, Y]);
                        DuplicateHeightMap.SetPixel(X, Y, Colour);
                    }
                }

                DuplicateHeightMap.Apply();
                File.WriteAllBytes(ExportPath, DuplicateHeightMap.EncodeToPNG());
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
            if (CreateGrassVisibilityMap)
            {
#if UNITY_EDITOR
                string ExportPath = UnityEditor.EditorUtility.SaveFilePanelInProject("Export Terrain Splatmap", $"{Base.name}.png", "png", "");
                Texture2D TerrainSplatmap = new Texture2D(TerrainSplat.width, TerrainSplat.height);

                for (int X = 0; X < TerrainSplatmap.width; X++)
                {
                    for (int Y = 0; Y < TerrainSplatmap.height; Y++)
                    {
                        Color Multiplier = new Color();
                        foreach (Color C in GrassySurfaces)
                        {
                            Multiplier.r += C.r;
                            Multiplier.g += C.g;
                            Multiplier.b += C.b;
                        }
                        Multiplier.r = Mathf.Clamp01(Multiplier.r);
                        Multiplier.g = Mathf.Clamp01(Multiplier.g);
                        Multiplier.b = Mathf.Clamp01(Multiplier.b);

                        TerrainSplatmap.SetPixel(X, Y, Color.Lerp(Color.black, Color.white, (TerrainSplat.GetPixel(X, Y) * Multiplier).grayscale));
                    }
                }

                TerrainSplatmap.Apply();
                File.WriteAllBytes(ExportPath, TerrainSplatmap.EncodeToPNG());
                CreateGrassVisibilityMap = false;
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
        }

        public List<GameObject> ObjectsCache = new List<GameObject>();
        public int GridSize = 100;

        private void Generate()
        {
            foreach (GameObject Obj in ObjectsCache)
            {
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlaying)
                {
                    Destroy(Obj);
                }
                else
                {
                    DestroyImmediate(Obj);
                }
#endif
            }

            for (int X = 0; X < 1000; X += GridSize)
            {
                for (int Y = 0; Y < 1000; Y += GridSize)
                {
                    Material MatInstance = new Material(GrassMaterial);
                    Texture2D GrassSampler = new Texture2D(TerrainSplat.width / GridSize, TerrainSplat.height / GridSize);

                    List<Vector3> Vertices = new List<Vector3>();
                    List<int> Triangles = new List<int>();

                    for (int I = 0; I < GridSize; I++)
                    {
                        for (int J = 0; J < GridSize; J++)
                        {
                            int XI = X + I;
                            int YJ = Y + J;

                            int MapX = (int)(((float)I / GridSize) * TerrainSplat.width);
                            int MapY = (int)(((float)J / GridSize) * TerrainSplat.height);

                            GrassSampler.SetPixel(MapX, MapY, Color.white * Mathf.Lerp(0, 1, TerrainSplat.GetPixel(XI, YJ).r));

                            Vertices.Add(Base.transform.position + new Vector3(I, Base.terrainData.GetHeight(XI - (GridSize / 2), YJ - (GridSize / 2)), J));

                            if (I == 0 || J == 0)
                                continue;

                            Triangles.Add(GridSize * I + J);
                            Triangles.Add(GridSize * I + J - 1);
                            Triangles.Add(GridSize * (I - 1) + J - 1);
                            Triangles.Add(GridSize * (I - 1) + J - 1);
                            Triangles.Add(GridSize * (I - 1) + J);
                            Triangles.Add(GridSize * I + J);
                        }
                    }

                    GrassSampler.Apply();
                    MatInstance.SetTexture("_GrassMap", GrassSampler);

                    Vector2[] UVs = new Vector2[Vertices.Count];
                    for (int I = 0; I < UVs.Length; I++)
                    {
                        UVs[I] = new Vector2(Vertices[I].x, Vertices[I].z);
                    }

                    Mesh M = new Mesh();
                    M.vertices = Vertices.ToArray();
                    M.uv = UVs;
                    M.triangles = Triangles.ToArray();
                    M.RecalculateNormals();

                    GameObject NewObj = new GameObject($"{Base.name}_X={X}_Y={Y}");
                    MeshFilter F = NewObj.AddComponent<MeshFilter>();
                    F.mesh = M;
                    MeshRenderer R = NewObj.AddComponent<MeshRenderer>();
                    R.material = MatInstance;
                    NewObj.transform.parent = transform;
                    NewObj.transform.localScale = Vector3.one;
                    NewObj.transform.localPosition = new Vector3(X, 0, Y);
                    ObjectsCache.Add(NewObj);
                }
            }
        }
    }
}