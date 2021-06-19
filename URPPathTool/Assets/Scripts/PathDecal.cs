using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class PathDecal : MonoBehaviour
{
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    [SerializeField] [Min(0.1f)] private float _width = 1.0f;
    [SerializeField] [Min(0.1f)] private float _height = 1.0f;
    [SerializeField] [Min(0.0f)] private float _radius = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] private float _heightInfluence = 1.0f;
    public List<Vector3> Positions;
    
    void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshFilter = GetComponent<MeshFilter>();
    }
    void Start() 
    {
        CreateMesh();
        UpdateShaderValues();
    }
    public void UpdateShaderValues()
    {
        var material = GetComponent<MeshRenderer>().sharedMaterial;
        Vector4 [] segments = new Vector4[256];
        for (int i = 0; i < Positions.Count ; i++)
        {
            segments[i] = new Vector4(Positions[i].x, Positions[i].y, Positions[i].z, 1.0f);
        }
        material.SetVectorArray("_Segments", segments);
        material.SetFloat("_Radius", _radius);
        material.SetFloat("_HeightInfluence", _heightInfluence);
        material.SetInt("_LinesCount", Positions.Count - 1);
    }

    public void CreateMesh()
    {
        if(Positions.Count <= 1) return;
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();
        List<int> newTriangles = new List<int>();
        Mesh mesh = new Mesh();
        var vertIndex = 0;
        for(int i = 0; i < Positions.Count; i++)
        {
            var nextDir =  i < Positions.Count - 1? Positions[i+1] - Positions[i]: Vector3.zero;
            var prevDir =  i > 0 ? Positions[i] - Positions[i-1]: Vector3.zero;
            var tangent = Vector3.Cross(Vector3.up, nextDir).normalized + Vector3.Cross(Vector3.up, prevDir).normalized;
            tangent.Normalize();
            var up = Vector3.up * _height * 0.5f;
            var v1 = Positions[i] + (tangent * _width * 0.5f) - up;
            var v2 = Positions[i] - (tangent * _width * 0.5f) - up;
            var v3 = Positions[i] + (tangent * _width * 0.5f) + up;
            var v4 = Positions[i] - (tangent * _width * 0.5f) + up;

            Debug.DrawLine(Positions[i], Positions[i] + tangent, Color.green, 0.25f);
            float modI = i % 2;
            var uv1 = new Vector2(modI, 1.0f - modI);
            var uv2 = new Vector2(1.0f - modI, modI);
            newVertices.Add(v1);
            newVertices.Add(v2);
            newVertices.Add(v3);
            newVertices.Add(v4);
            newUV.Add(uv1);
            newUV.Add(uv2);
            newUV.Add(uv2);
            newUV.Add(uv1);
            
            if(i == Positions.Count - 1)
            {
                int[] frontTriangles = new int[6]{
                    vertIndex+2,vertIndex+1,vertIndex,
                    vertIndex+2,vertIndex+3,vertIndex+1,
                };
                newTriangles.AddRange(frontTriangles);
                continue;
            }
            int[] triangles = new int[24]{
                vertIndex+4,vertIndex+1,vertIndex,
                vertIndex+4,vertIndex+5,vertIndex+1,
                vertIndex+6,vertIndex+2,vertIndex+3,
                vertIndex+6,vertIndex+3,vertIndex+7,
                vertIndex+5,vertIndex+3,vertIndex+1,
                vertIndex+7,vertIndex+3,vertIndex+5,
                vertIndex+4,vertIndex,vertIndex+2,
                vertIndex+6,vertIndex+4,vertIndex+2,

                };
            newTriangles.AddRange(triangles);
            if(i == 0)
            {
                int[] frontTriangles = new int[6]{
                    vertIndex+2,vertIndex,vertIndex+1,
                    vertIndex+2,vertIndex+1,vertIndex+3,
                };
                newTriangles.AddRange(frontTriangles);
            }

            vertIndex += 4;
        }
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = newVertices.ToArray();
        mesh.uv = newUV.ToArray();
        mesh.triangles = newTriangles.ToArray();
    }

}
