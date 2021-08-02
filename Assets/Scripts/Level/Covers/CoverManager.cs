using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Level.Covers.Classes;
using Level.Covers.Classes.Util;
using Level.Covers.Classes.Util.BinarySerialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using Color = System.Drawing.Color;

namespace Level.Covers
{
    [ExecuteInEditMode]
    // менеджер ковров
    public class CoverManager : MonoBehaviour
    {
        public LayerMask shootableMask;
        public string coverSetName = "HumanCoverSet";
        public bool drawCovers;

        [Header("Character Settings")] public float characterStandHeight;
        public float characterCrouchHeight;
        public float characterRadius;

        [Header("Clusterization Settings")] public int clustersCount = 5;
        public bool showClustersVolumes;
        [Header("Accuracy Settings")] public float linePointsEpsilon = 0.01f;


        private CoverBaker baker;
        private CoverClusterMaker clusterMaker;

        private List<Vector3> centers;
        private List<List<Cover>> efficientCoverList;

        // for visualization
        private List<(Vector3, Vector3)> bounds;
        private List<Cover> covers;
        private List<int> clusterIds;


        private void Awake()
        {
            SerializableCoverList loadedCovers = LoadCoversAsBinary();
            if (loadedCovers != null)
                ApplySerializableCoverList(loadedCovers);
        }

        private void Start()
        {
            UpdateBounds();
            UpdateCenters();
        }

        public Vector3 GetNearestCoverPosition(Vector3 position, Vector3 playerDirection, float angleThreshold, int searchLimiter = Int32.MaxValue)
        {
            int clusterId = GetNearestCluster(position);
            List<Cover> cluster = efficientCoverList[clusterId];

            int bestCoverId = 0;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < Math.Min(cluster.Count, searchLimiter); i++)
            {
                Cover cover = cluster[i];
                float angle = Vector3.Angle(cover.direction, playerDirection);
                if (angle <= angleThreshold)
                {
                    Vector3 delta = position - cover.position;
                    float distance = delta.sqrMagnitude;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestCoverId = i;
                    }
                }
            }

            return cluster[bestCoverId].position;
        }

        public int GetNearestCluster(Vector3 position)
        {
            int minDistanceIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < centers.Count; i++)
            {
                Vector3 center = centers[i];
                Vector3 delta = position - center;
                float distance = delta.sqrMagnitude;
                if (distance < minDistance)
                {
                    minDistanceIndex = i;
                    minDistance = distance;
                }
            }

            return minDistanceIndex;
        }

        public List<Cover> GetNearestClusterList(Vector3 position)
        {
            return efficientCoverList[GetNearestCluster(position)];
        }

        public Vector3 GetCenter(int index)
        {
            return centers[index];
        }

        public void BakeCovers()
        {
            if (characterRadius < 0.01f)
                throw new Exception("Too low character radius");

            baker = new CoverBaker(shootableMask, characterStandHeight, characterCrouchHeight,
                characterRadius, linePointsEpsilon);
            List<Cover> bakedCovers = baker.BakeCovers();

            clusterMaker = new CoverClusterMaker(clustersCount, bakedCovers);
            List<int> bakedClusterIds = clusterMaker.Clusterize(out centers);


            SerializableCoverList serializableCoverList = new SerializableCoverList(bakedCovers, bakedClusterIds);

            SaveCoversAsBinary(serializableCoverList);
            ApplySerializableCoverList(serializableCoverList);
            PrintClustersInfo();
            /*if (covers.Count > 1000)
                drawCovers = false;
            efficientCoverList = MakeEfficientCoversList(bakedCovers, bakedClusterIds);
            UpdateBounds();*/
        }

        private void PrintClustersInfo()
        {
            int maxClusterSize = 0;
            int maxClusterSizeIndex = 0;
            float averageClusterSize = 0;

            for (int i = 0; i < efficientCoverList.Count; i++)
            {
                int count = efficientCoverList[i].Count;
                averageClusterSize += count;
                if (count > maxClusterSize)
                {
                    maxClusterSize = count;
                    maxClusterSizeIndex = i;
                }
            }

            averageClusterSize /= efficientCoverList.Count;
            print($"Baked covers. Max cluster size: {maxClusterSize} at {GetCenter(maxClusterSizeIndex)}, average size: {averageClusterSize}");
        }


        private void ApplySerializableCoverList(SerializableCoverList serializableCoverList)
        {
            covers = serializableCoverList.covers;
            clusterIds = serializableCoverList.clusterIds;
            if (covers.Count > 0 && clusterIds.Count > 0)
            {
                efficientCoverList = MakeEfficientCoversList(covers, clusterIds);
            }

            UpdateBounds();
            UpdateCenters();
            if (covers.Count > 1000)
                drawCovers = false;
        }


        private void UpdateBounds()
        {
            bounds = new List<(Vector3, Vector3)>();
            for (int i = 0; i < clustersCount; i++)
            {
                bounds.Add(GetClusterBounds(i));
            }
        }

        private void UpdateCenters()
        {
            centers = new List<Vector3>();
            for (int i = 0; i < efficientCoverList.Count; i++)
            {
                centers.Add(CalculateClusterCenter(i));
            }
        }

        private Vector3 CalculateClusterCenter(int clusterId)
        {
            List<Cover> cluster = efficientCoverList[clusterId];
            Vector3 position = Vector3.zero;
            for (int i = 0; i < cluster.Count; i++)
            {
                position += cluster[i].position;
            }

            int count = cluster.Count;
            if (count == 0)
                count = 1;
            return position / count;
        }


        private (Vector3, Vector3) GetClusterBounds(int clusterId)
        {
            List<Cover> cluster = efficientCoverList[clusterId];
            if (cluster.Count <= 0)
                return (Vector3.zero, Vector3.zero);
            Vector3 minBound = cluster[0].position;
            Vector3 maxBound = cluster[0].position;


            for (int i = 1; i < cluster.Count; i++)
            {
                Vector3 position = cluster[i].position
                                   + Vector3.up * (cluster[i].type == CoverType.Stand
                                       ? characterStandHeight
                                       : characterCrouchHeight);

                if (maxBound.x < position.x)
                    maxBound.x = position.x;
                if (maxBound.y < position.y)
                    maxBound.y = position.y;
                if (maxBound.z < position.z)
                    maxBound.z = position.z;

                if (minBound.x > position.x)
                    minBound.x = position.x;
                if (minBound.y > position.y)
                    minBound.y = position.y;
                if (minBound.z > position.z)
                    minBound.z = position.z;
            }

            return (minBound, maxBound);
        }


        // turns out that binary files take ~40% less space than json
        private void SaveCoversAsBinary(SerializableCoverList serializableCoverList)
        {
            string sceneResourcesFolder = GetCurrentSceneResourcesFolder();

            if (!Directory.Exists(sceneResourcesFolder))
                Directory.CreateDirectory(new DirectoryInfo(sceneResourcesFolder).ToString());

            string path = GetPathForSet(coverSetName + ".covers");

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Create);

            formatter.Serialize(stream, new BinarySerializableCoverList(serializableCoverList));
            stream.Close();
        }

        private SerializableCoverList LoadCoversAsBinary()
        {
            string sceneResourcesFolder = GetCurrentSceneResourcesFolder();

            if (!Directory.Exists(sceneResourcesFolder))
                return null;
            string path = GetPathForSet(coverSetName + ".covers");

            if (!File.Exists(path))
                return null;

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            BinarySerializableCoverList binarySerializableCoverList =
                formatter.Deserialize(stream) as BinarySerializableCoverList;

            if (binarySerializableCoverList == null)
            {
                throw new Exception("Deserialized data is null");
            }

            SerializableCoverList serializableCoverList = binarySerializableCoverList.ToSerializableCoverList();
            return serializableCoverList;
        }

        private void SaveCovers(SerializableCoverList serializableCoverList)
        {
            string sceneResourcesFolder = GetCurrentSceneResourcesFolder();

            if (!Directory.Exists(sceneResourcesFolder))
                Directory.CreateDirectory(new DirectoryInfo(sceneResourcesFolder).ToString());

            string path = GetPathForSet(coverSetName + ".json");

            string content = JsonUtility.ToJson(serializableCoverList, false);
            File.WriteAllText(path, content);
        }

        private SerializableCoverList LoadCovers()
        {
            string sceneResourcesFolder = GetCurrentSceneResourcesFolder();

            if (!Directory.Exists(sceneResourcesFolder))
                return null;
            string path = GetPathForSet(coverSetName + ".json");

            if (!File.Exists(path))
                return null;
            string content = File.ReadAllText(path);
            SerializableCoverList list = JsonUtility.FromJson<SerializableCoverList>(content);
            return list;
        }

        private string GetCurrentSceneResourcesFolder()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            FileInfo sceneFileInfo = new FileInfo(currentScene.path);
            string sceneResourcesFolder =
                sceneFileInfo.Directory + Path.DirectorySeparatorChar.ToString() + currentScene.name +
                Path.DirectorySeparatorChar + "Resources";
            return sceneResourcesFolder;
        }

        private string GetPathForSet(string setName)
        {
            return GetCurrentSceneResourcesFolder() +
                   Path.DirectorySeparatorChar + setName;
        }

        private List<List<Cover>> MakeEfficientCoversList(List<Cover> coversList, List<int> clustersList)
        {
            List<List<Cover>> result = new List<List<Cover>>();
            for (int i = 0; i < clustersCount; i++)
            {
                result.Add(new List<Cover>());
            }

            for (int i = 0; i < coversList.Count; i++)
            {
                Cover cover = coversList[i];
                int clusterId = clustersList[i];

                result[clusterId].Add(cover);
            }

            return result;
        }

        private void OnDrawGizmosSelected()
        {
            // visualize covers
            if (covers != null && drawCovers)
            {
                for (int i = 0; i < covers.Count; i++)
                {
                    Gizmos.color = ColorUtil.RandomColor(clusterIds[i]);
                    Cover cover = covers[i];
                    Vector3 offset = new Vector3(0, characterStandHeight, 0);

                    if (cover.type == CoverType.Crouch)
                    {
                        offset = new Vector3(0, characterCrouchHeight, 0);
                    }

                    Gizmos.DrawSphere(cover.position + offset, 0.125f);
                    Gizmos.DrawLine(cover.position + offset, cover.position);
                    Gizmos.DrawLine(cover.position + offset, cover.position + offset + cover.direction);
                }
            }

            if (bounds != null && showClustersVolumes)
            {
                for (int i = 0; i < bounds.Count; i++)
                {
                    Gizmos.color = ColorUtil.RandomColor(i);
                    (Vector3, Vector3) bound = bounds[i];
                    Vector3 center = Vector3.Lerp(bound.Item1, bound.Item2, 0.5f);
                    Vector3 delta = bound.Item1 - bound.Item2;
                    delta.x = Mathf.Abs(delta.x);
                    delta.y = Mathf.Abs(delta.y);
                    delta.z = Mathf.Abs(delta.z);

                    Gizmos.DrawWireCube(center, delta);
                }
            }
        }
    }
}