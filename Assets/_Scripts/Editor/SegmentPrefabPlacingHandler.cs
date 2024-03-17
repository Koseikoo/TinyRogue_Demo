using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TerrainTools;
using UnityEngine;
using Views;
using System.Linq;
using _Testing;
using UnityEngine.SceneManagement;

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
        
        [MenuItem("Tools/Segment Prefab Placing Handler")]
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
            EditorGUILayout.Space(20);
            currentSegmentView = EditorGUILayout.ObjectField("Current Segment", currentSegmentView, typeof(GameObject), true) as GameObject;
            selectedUnitType = (UnitType)EditorGUILayout.EnumPopup(selectedUnitType);
            _placeSegmentUnits = EditorGUILayout.Toggle("Place", _placeSegmentUnits);
            
            if (GUILayout.Button("Reset Segment"))
            {
                ResetSegment();
            }
            
            EditorGUILayout.Space(20);
            
            if (GUILayout.Button("ClearVisualization"))
            {
                UpdateVisualization(null);
            }
            
            if (GUILayout.Button("Visualize") && currentSegmentView != null)
            {
                UpdateVisualization(currentSegmentView.GetComponent<SegmentView>());
            }
        }

        private void ResetSegment()
        {
            SegmentView view = GetTempPrefabInstance();

            for (int i = 0; i < view.SegmentUnitDefinitions.Length; i++)
            {
                DestroyImmediate(view.SegmentUnitDefinitions[i].Point.gameObject);
            }

            view.SegmentUnitDefinitions = Array.Empty<SegmentUnitDefinition>();
            
            UpdateVisualization(view);
            SavePrefab(view.gameObject);
            DestroyImmediate(view.gameObject);
        }

        private void SpawnUnit(Vector3 position)
        {
            SegmentView view = GetTempPrefabInstance();
            
            var segmentDefinition = new SegmentUnitDefinition()
            {
                Type = selectedUnitType,
            };

            view.SegmentUnitDefinitions = view.SegmentUnitDefinitions.Concat(new []{ segmentDefinition }).ToArray();
            GameObject point = new GameObject();
            point.transform.position = position;
            point.name = $"Point_{view.SegmentUnitDefinitions.Length-1}";
            segmentDefinition.Point = point.transform;
            segmentDefinition.Point.SetParent(view.PointParent);
            
            UpdateVisualization(view);
            SavePrefab(view.gameObject);
            DestroyImmediate(view.gameObject);
        }

        private void UpdateVisualization(SegmentView view)
        {
            SegmentDesignerVisualizationView segmentVisualization = GameObject.Find("SegmentVisualization")
                .GetComponent<SegmentDesignerVisualizationView>();
            
            if(segmentVisualization == null)
                Debug.Log("<color=red>Add Object with Name SegmentVisualization with SegmentDesignerVisualizationView Component to Scene!</color>");
            else
            {
                if(view == null)
                    segmentVisualization.ClearVisualization();
                else
                    segmentVisualization.UpdateVisualization(view);
            }
        }

        private SegmentView GetTempPrefabInstance()
        {
            GameObject newPrefabInstance = PrefabUtility.GetPrefabParent(currentSegmentView) as GameObject;
            newPrefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(newPrefabInstance);
                
            SegmentView view = newPrefabInstance.GetComponent<SegmentView>();
            return view;
        }

        private void SavePrefab(GameObject instance)
        {
            var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(currentSegmentView);
            PrefabUtility.SaveAsPrefabAsset(instance, assetPath);
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