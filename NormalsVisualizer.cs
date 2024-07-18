using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshFilter))]
public class NormalsVisualizer : Editor
{
    private const string EDITOR_PREF_KEY = "normals_length";
    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private Vector3[] _verts;
    private Vector3[] _normals;
    private float _normalsLength = 1f;
    private bool _showIndices;

    private void OnEnable()
    {
        _meshFilter = target as MeshFilter;

        if (_meshFilter != null)
        {
            _mesh = _meshFilter.sharedMesh;
        }

        _normalsLength = EditorPrefs.GetFloat(EDITOR_PREF_KEY);
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
        
        for (int i = 0; i < _mesh.vertexCount; i++)
        {
            var pos = _verts[i] + _normals[i] * _normalsLength;
            Handles.DrawLine(_verts[i], pos);

            if (_showIndices)
            {
                Handles.Label(pos, i.ToString());
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUI.BeginChangeCheck();
        _normalsLength = EditorGUILayout.FloatField("Normals Length", _normalsLength);
        _showIndices = EditorGUILayout.Toggle("Show Indices", _showIndices);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetFloat(EDITOR_PREF_KEY, _normalsLength);
        }
    }
}