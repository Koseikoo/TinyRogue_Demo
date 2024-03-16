using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TerrainTools;
using UnityEngine;
using Views;
using System.Linq;

namespace DefaultNamespace
{
    public class SegmentPrefabPlacingHandler : EditorWindow
    {
        GameObject prefabToSpawn;
        private GameObject currentSegmentView;
        private UnitType selectedUnitType;
        private static string folderPath = "Assets/Prefabs";
        private bool _placeSegmentUnits;
        private bool _clicked;
        

        private GameObject lastSelection;
        
        [MenuItem("Window/Segment Prefab Placing Handler")]
        static void Init()
        {
            SegmentPrefabPlacingHandler window = GetWindow<SegmentPrefabPlacingHandler>();
            window.Show();
            
            LoadPrefabsFromFolder();
        }

        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            Event guiEvent = Event.current;
            
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && _placeSegmentUnits)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                Vector3 spawnPosition = MathHelper.GetIntersectionPoint(ray.origin, ray.direction);
                SpawnUnit(spawnPosition);
                
                guiEvent.Use(); // Consume the event to prevent other systems from handling it
            }
        }
        
        void OnGUI()
        {
            GUILayout.Label("Use Orthographic Top-Down Camera!", EditorStyles.boldLabel);
            currentSegmentView = EditorGUILayout.ObjectField("Current Segment", currentSegmentView, typeof(GameObject), true) as GameObject;
            selectedUnitType = (UnitType)EditorGUILayout.EnumPopup(selectedUnitType);
            _placeSegmentUnits = EditorGUILayout.Toggle("Place", _placeSegmentUnits);
        }

        private void SpawnUnit(Vector3 position)
        {
            GameObject newPrefabInstance = PrefabUtility.GetPrefabParent(currentSegmentView) as GameObject;
            newPrefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(newPrefabInstance);
                
            SegmentView view = newPrefabInstance.GetComponent<SegmentView>();
            view.Size = 14;

            var segmentDefinition = new SegmentUnitDefinition()
            {
                Type = selectedUnitType,
            };

            view.SegmentUnitDefinitions = view.SegmentUnitDefinitions.Concat(new []{ segmentDefinition }).ToArray();
            GameObject point = new GameObject();
            point.transform.position = position;
            point.name = $"Point_{view.SegmentUnitDefinitions.Length-1}";
            segmentDefinition.Point = point.transform;
            segmentDefinition.Point.SetParent(view.transform);

            var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(currentSegmentView);
            PrefabUtility.SaveAsPrefabAsset(newPrefabInstance, assetPath);
                
            DestroyImmediate(newPrefabInstance);
        }
        
        static void LoadPrefabsFromFolder()
        {
            GameObject[] prefabs = LoadAllPrefabsInFolder(folderPath);
            if (prefabs.Length > 0)
            {
                foreach (GameObject prefab in prefabs)
                {
                    Debug.Log("Loaded prefab: " + prefab.name);
                }
            }
            else
            {
                Debug.Log("No prefabs found in folder: " + folderPath);
            }
        }

        static GameObject[] LoadAllPrefabsInFolder(string folderPath)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { folderPath });
            GameObject[] prefabs = new GameObject[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            return prefabs;
        }
    }
}