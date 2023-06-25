using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using SkySoft;
using Random = UnityEngine.Random;

public class AutoTree : MonoBehaviour
{
    public bool Run;
    public List<Texture2D> Grasses;
    public int TreeIndex;
    public Mesh GrassMesh;
    public Material GrassMaterial;
    [Space]
    public Vector2 MinimumOffset = new Vector2(-0.25f, -0.25f);
    public Vector2 MaximumOffset = new Vector2(0.25f, 0.25f);
    public Vector3 MinimumScale = new Vector3(2, 2, 2);
    public Vector3 MaximumScale = new Vector3(2.2f, 2.2f, 2.2f);
    public float MinimumRotation = -360;
    public float MaximumRotation = 360;
    public float GrassCutoffOffset = -0.15f;

    public int GrassmapResolution = 1024;

    private Dictionary<Terrain, byte[,]> DensityMasks = new Dictionary<Terrain, byte[,]>();
    private Dictionary<Terrain, byte[,,]> OffsetMasks = new Dictionary<Terrain, byte[,,]>();
    private Dictionary<Terrain, byte[,]> RotationMasks = new Dictionary<Terrain, byte[,]>();
    private Dictionary<Terrain, Vector3[,]> ScaleMap = new Dictionary<Terrain, Vector3[,]>();

    private void OnDrawGizmosSelected()
    {
        SkyEngine.Gizmos.Colour = Color.red;
    }

    private void OnValidate()
    {
        if (Run)
        {
            RenderInstances();
            Run = false;
        }
    }

    public float Density(Vector3 LocalPosition, Terrain T)
    {
        Vector2 NormalizedPosition = new Vector2(Mathf.InverseLerp(0, T.terrainData.size.x, LocalPosition.x), Mathf.InverseLerp(0, T.terrainData.size.z, LocalPosition.z));
        Vector2 AlphamapPosition = new Vector2(NormalizedPosition.x * T.terrainData.alphamapWidth, NormalizedPosition.y * T.terrainData.alphamapHeight);

        float[,,] AlphaMap = T.terrainData.GetAlphamaps(Mathf.RoundToInt(AlphamapPosition.x), Mathf.RoundToInt(AlphamapPosition.y), 1, 1);

        int TargetIndex = -1;

        for (int I = 0; I < T.terrainData.terrainLayers.Length; I++)
        {
            if (Grasses.Contains(T.terrainData.terrainLayers[I].diffuseTexture))
            {
                TargetIndex = I;
                break;
            }
        }

        if (TargetIndex != -1)
        {
            return AlphaMap[0, 0, TargetIndex];
        }

        return 0;
    }

    private byte[,] GetDensityMask(Terrain T)
    {
        if (DensityMasks.ContainsKey(T))
        {
            return DensityMasks[T];
        }

        byte[,] Value = new byte[GrassmapResolution, GrassmapResolution];

        for (int X = 0; X < GrassmapResolution; X++)
        {
            for (int Z = 0; Z < GrassmapResolution; Z++)
            {
                Vector3 NormalizedPosition = new Vector3(X / (float)GrassmapResolution, 0, Z / (float)GrassmapResolution);
                Vector3 FinalPosition = new Vector3(NormalizedPosition.x * T.terrainData.size.x, 0, NormalizedPosition.z * T.terrainData.size.z);
                Value[X, Z] = (byte)(Density(FinalPosition, T) * 255);
            }
        }

        DensityMasks.Add(T, Value);
        return Value;
    }

    private byte[,] GetRotationMask(Terrain T)
    {
        if (RotationMasks.ContainsKey(T))
        {
            return RotationMasks[T];
        }

        byte[,] Value = new byte[GrassmapResolution, GrassmapResolution];

        for (int X = 0; X < GrassmapResolution; X++)
        {
            for (int Z = 0; Z < GrassmapResolution; Z++)
            {
                Value[X, Z] = (byte)Random.Range(0, 255);
            }
        }

        RotationMasks.Add(T, Value);
        return Value;
    }

    private byte[,,] GetOffsetMask(Terrain T)
    {
        if (OffsetMasks.ContainsKey(T))
        {
            return OffsetMasks[T];
        }

        byte[,,] Value = new byte[GrassmapResolution, GrassmapResolution, 2];

        for (int X = 0; X < GrassmapResolution; X++)
        {
            for (int Z = 0; Z < GrassmapResolution; Z++)
            {
                Value[X, Z, 0] = (byte)Random.Range(0, 255);
                Value[X, Z, 1] = (byte)Random.Range(0, 255);
            }
        }

        OffsetMasks.Add(T, Value);
        return Value;
    }

    private Vector3[,] GetScaleMap(Terrain T)
    {
        if (ScaleMap.ContainsKey(T))
        {
            return ScaleMap[T];
        }

        Vector3[,] Value = new Vector3[GrassmapResolution, GrassmapResolution];

        for (int X = 0; X < GrassmapResolution; X++)
        {
            for (int Z = 0; Z < GrassmapResolution; Z++)
            {
                Value[X, Z] = Vector3.Lerp(MinimumScale, MaximumScale, Random.value);
            }
        }

        ScaleMap.Add(T, Value);
        return Value;
    }

    public bool Refresh = true;
    List<Matrix4x4> Positions = new List<Matrix4x4>();

    private bool Trig = false;
    List<TreeInstance> UpdatedTrees = new List<TreeInstance>();
    private Terrain TerrainToUpdate;
    private bool DoneMicroUpdate = false;

    private void Update()
    {
        if (CanRender && !Trig)
        {
            Parallel.Invoke(RenderInstances);
        }

        if (Trig)        
        {
            Debug.Log($"Update Terrains");
            TerrainToUpdate.terrainData.SetTreeInstances(UpdatedTrees.ToArray(), true);

            TerrainToUpdate.Flush();
            DoneMicroUpdate = true;
            Trig = false;
        }
    }

    private bool Calc = false;

    private async void RunRefresh()
    {
        Debug.Log("BeginRefresh");
        Calc = true;

        try
        {
            Vector3 CameraPosition = Camera.main.transform.position;

            List<Terrain> NearTerrains = new List<Terrain>();

            foreach (Collider C in Physics.OverlapSphere(SkySoft.SkyEngine.PlayerEntity.transform.position, 30, LayerMask.NameToLayer("Terrain")))
            {
                Terrain T;

                if (T = C.GetComponent<Terrain>())
                    NearTerrains.Add(T);
            }

            foreach (Terrain T in NearTerrains)
            {
                Positions = new List<Matrix4x4>();
                Vector3 PlayerPositionOnTerrain = T.transform.InverseTransformPoint(SkySoft.SkyEngine.PlayerEntity.transform.position);
                Vector3 PlayerPositionOnTerrainN = new Vector3(PlayerPositionOnTerrain.x / T.terrainData.size.x, 0, PlayerPositionOnTerrain.z / T.terrainData.size.z);

                Vector2 PlayerPositionOnGrassmap = new Vector2((int)PlayerPositionOnTerrainN.x * GrassmapResolution, (int)PlayerPositionOnTerrainN.z * GrassmapResolution);

                Debug.Log("Get Masks (Density)");
                byte[,] Densities = GetDensityMask(T);
                Debug.Log("Get Masks (Rotation)");
                byte[,] Rotations = GetRotationMask(T);
                Debug.Log("Get Masks (Offset)");
                byte[,,] Offsets = GetOffsetMask(T);
                Debug.Log("Get Masks (Scale)");
                Vector3[,] Scales = GetScaleMap(T);

                UpdatedTrees = new List<TreeInstance>();

                Debug.Log("Clear old trees");

                for (int I = 0; I < T.terrainData.treeInstanceCount; I++)
                {
                    TreeInstance Tree = T.terrainData.GetTreeInstance(I);

                    if (Tree.prototypeIndex != TreeIndex)
                    {
                        UpdatedTrees.Add(Tree);
                    }
                }

                Debug.Log("Start For Loops");

                for (int X = (int)PlayerPositionOnGrassmap.x - 20; X < (int)PlayerPositionOnGrassmap.x + 20; X++)
                {
                    for (int Z = (int)PlayerPositionOnGrassmap.y - 20; Z < (int)PlayerPositionOnGrassmap.y + 20; Z++)
                    {
                        if (X >= 0 && X < GrassmapResolution && Z >= 0 && Z < GrassmapResolution)
                        {
                            Vector3 P = new Vector3(X + Mathf.Lerp(MinimumOffset.x, MaximumOffset.x, (float)Offsets[X, Z, 0] / 255), 0, Z + Mathf.Lerp(MinimumOffset.y, MaximumOffset.y, (float)Offsets[X, Z, 1] / 255));
                            Vector3 WorldPos = T.GetPosition() + P;
                            Vector3 NormalisedPosition = new Vector3(P.x / GrassmapResolution, 0, P.z / GrassmapResolution);

                            #region Position
                            Vector3 Offset = new Vector3(Mathf.Lerp(MinimumOffset.x, MaximumOffset.x, (float)Offsets[X, Z, 0] / 255), 0, Mathf.Lerp(MinimumOffset.y, MaximumOffset.y, (float)Offsets[X, Z, 1] / 255));
                            Vector3 NormalizedPosition = new Vector3(X / (float)GrassmapResolution, 0, Z / (float)GrassmapResolution);
                            Vector3 FinalPosition = Offset + T.GetPosition() + new Vector3(NormalizedPosition.x * T.terrainData.size.x, 0, NormalizedPosition.z * T.terrainData.size.z);
                            float H = T.SampleHeight(FinalPosition) * T.terrainData.size.y;
                            Vector3 Position = new Vector3(FinalPosition.x, H, FinalPosition.z);
                            #endregion

                            if (Vector3.Distance(CameraPosition, WorldPos) < 30 && Densities[X, Z] > 122)
                            {
                                #region Rotation
                                float Rotation = Mathf.Lerp(MinimumRotation, MaximumRotation, (float)Rotations[X, Z] / 255);
                                #endregion
                                #region  Scale
                                Vector3 Scale = Scales[X, Z];
                                #endregion

                                TreeInstance NewTree = new TreeInstance
                                {
                                    color = Color.white,
                                    heightScale = Scales[X, Z].y,
                                    widthScale = Scales[X, Z].x,
                                    lightmapColor = Color.white,
                                    prototypeIndex = TreeIndex,
                                    position = NormalisedPosition,
                                    rotation = (((float)Rotations[X, Z] / 255) * 360f) * Mathf.Deg2Rad
                                };

                                UpdatedTrees.Add(NewTree);
                            }
                        }
                    }
                }

                Debug.Log("Finished Loops");

                TerrainToUpdate = T;

                DoneMicroUpdate = false;
                Trig = true;

                while (!DoneMicroUpdate)
                    await Task.Yield();
            }
        } catch (Exception E)
        {
            Debug.Log(E);
        }
        Debug.Log("Funk");
        Calc = false;
    }

    private bool CanRender = true;

    private async void RenderInstances()
    {
        CanRender = false;
        RunRefresh();

        while (Calc)
            await Task.Delay(10);

        CanRender = true;
    }
}
