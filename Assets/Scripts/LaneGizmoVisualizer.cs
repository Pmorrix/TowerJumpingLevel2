using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class LaneGizmoVisualizer : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerMove playerMove;

    [Header("Buildings Source")]
    [Tooltip("Si se asigna, se usarán todos sus hijos como edificios.")]
    [SerializeField] private Transform buildingsRoot;

    [Tooltip("Si no hay buildingsRoot, buscará objetos con este tag.")]
    [SerializeField] private string buildingTag = "Building";

    [Tooltip("Incluir objetos inactivos al leer hijos de buildingsRoot.")]
    [SerializeField] private bool includeInactiveChildren = true;

    [Header("Lane Z (Player lanes)")]
    [SerializeField] private Color laneColor = Color.yellow;

    [Header("Building X Positions")]
    [SerializeField] private Color xColor = Color.cyan;
    [SerializeField] private bool showBuildingNames = true;

    [Header("Visual")]
    [SerializeField] private float lineLength = 20f;
    [SerializeField] private float lineThickness = 4f;
    [SerializeField] private bool showLabels = true;
    [SerializeField] private float labelHeight = 0.5f;

    private readonly List<Transform> _buildingTransforms = new List<Transform>();

    private void OnDrawGizmos()
    {
        if (playerMove == null)
            return;

        float[] lanes = playerMove.laneZPositions;
        if (lanes == null || lanes.Length == 0)
            return;

        CollectBuildings();

        DrawLaneZLines(lanes);
        DrawBuildingXLines();
    }

    private void CollectBuildings()
    {
        _buildingTransforms.Clear();

        if (buildingsRoot != null)
        {
            CollectFromRoot(buildingsRoot);
            return;
        }

        if (string.IsNullOrWhiteSpace(buildingTag))
            return;

        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(buildingTag);
        if (taggedObjects == null || taggedObjects.Length == 0)
            return;

        for (int i = 0; i < taggedObjects.Length; i++)
        {
            GameObject go = taggedObjects[i];
            if (go == null)
                continue;

            _buildingTransforms.Add(go.transform);
        }
    }

    private void CollectFromRoot(Transform root)
    {
        if (root == null)
            return;

        Transform[] children = root.GetComponentsInChildren<Transform>(includeInactiveChildren);

        for (int i = 0; i < children.Length; i++)
        {
            Transform current = children[i];

            if (current == null || current == root)
                continue;

            _buildingTransforms.Add(current);
        }
    }

    private void DrawLaneZLines(float[] lanes)
    {
#if UNITY_EDITOR
        Handles.color = laneColor;
#endif

        for (int i = 0; i < lanes.Length; i++)
        {
            float z = lanes[i];

            Vector3 p1 = new Vector3(-lineLength * 0.5f, 0f, z);
            Vector3 p2 = new Vector3(+lineLength * 0.5f, 0f, z);

#if UNITY_EDITOR
            Handles.DrawAAPolyLine(lineThickness, p1, p2);
#else
            Gizmos.color = laneColor;
            Gizmos.DrawLine(p1, p2);
#endif

            if (showLabels)
            {
#if UNITY_EDITOR
                Handles.Label(new Vector3(0f, labelHeight, z), $"Lane Z = {z}");
#endif
            }
        }
    }

    private void DrawBuildingXLines()
    {
        if (_buildingTransforms.Count == 0)
            return;

#if UNITY_EDITOR
        Handles.color = xColor;
#endif

        for (int i = 0; i < _buildingTransforms.Count; i++)
        {
            Transform t = _buildingTransforms[i];
            if (t == null)
                continue;

            float x = t.position.x;

            Vector3 p1 = new Vector3(x, 0f, -lineLength * 0.5f);
            Vector3 p2 = new Vector3(x, 0f, +lineLength * 0.5f);

#if UNITY_EDITOR
            Handles.DrawAAPolyLine(lineThickness, p1, p2);
#else
            Gizmos.color = xColor;
            Gizmos.DrawLine(p1, p2);
#endif

            if (showLabels)
            {
#if UNITY_EDITOR
                string label = showBuildingNames ? $"{t.name} | X = {x:0.##}" : $"X = {x:0.##}";
                Handles.Label(new Vector3(x, labelHeight, 0f), label);
#endif
            }
        }
    }
}

