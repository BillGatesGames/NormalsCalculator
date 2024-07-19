using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshFilter))]
public class NormalsVisualizer : Editor
{
    [SerializeField] private float _normalsLength = 1f;
    [SerializeField] private bool _showIndices;
    [SerializeField] private bool _showNormalsOnlyForSplitVertices;

    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private Vector3[] _verts;
    private Vector3[] _normals;
    private Dictionary<Vector3, int> _splittedVertices;

    private void OnDisable()
    {
        EditorPrefs.SetString(typeof(NormalsVisualizer).ToString(), EditorJsonUtility.ToJson(this));
    }

    private void OnEnable()
    {
        _meshFilter = target as MeshFilter;

        if (_meshFilter != null)
        {
            _mesh = _meshFilter.sharedMesh;
        }

        EditorJsonUtility.FromJsonOverwrite(EditorPrefs.GetString(typeof(NormalsVisualizer).ToString()), this);
    }

    private void OnSceneGUI()
    {
        if (_mesh == null)
        {
            return;
        }

        Handles.matrix = _meshFilter.transform.localToWorldMatrix;
        Handles.color = Color.yellow;
        
        _verts = _mesh.vertices;
        _normals = _mesh.normals;

        _splittedVertices = new Dictionary<Vector3, int>();

        if (_showNormalsOnlyForSplitVertices)
        {
            for (int i = 0; i < _mesh.vertexCount; i++)
            {
                if (!_splittedVertices.ContainsKey(_verts[i]))
                {
                    _splittedVertices.Add(_verts[i], 0);
                }

                _splittedVertices[_verts[i]]++;
            }
        }

        for (int i = 0; i < _mesh.vertexCount; i++)
        {
            bool show = true;

            if (_showNormalsOnlyForSplitVertices)
            {
                if (_splittedVertices[_verts[i]] < 2)
                {
                    show = false;
                }
            }

            if (show)
            {
                var pos = _verts[i] + _normals[i] * _normalsLength;
                Handles.DrawLine(_verts[i], pos);

                if (_showIndices)
                {
                    Handles.Label(pos, i.ToString());
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();

        _normalsLength = EditorGUILayout.FloatField("Normals Length", _normalsLength);
        _showIndices = EditorGUILayout.Toggle("Show Indices", _showIndices);
        _showNormalsOnlyForSplitVertices = EditorGUILayout.Toggle("Show Normals Only For Split Vertices", _showNormalsOnlyForSplitVertices);

        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetString(typeof(NormalsVisualizer).ToString(), EditorJsonUtility.ToJson(this));
        }
    }
}