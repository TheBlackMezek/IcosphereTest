using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcosphereMaker : MonoBehaviour {

    public int recusionLevel = 1;

    public MeshFilter meshFilter;
    public MeshCollider meshCollider;

    private Mesh icoMesh;
    List<Vector3> vertices;

    private int index;
    private Dictionary<long, int> middlePointIndexCache;




    // Use this for initialization
    void Start () {
        MakeIcoMesh();
	}
	
	public void MakeIcoMesh()
    {
        icoMesh = new Mesh();
        List<Vector3> preVertices = new List<Vector3>();
        vertices = new List<Vector3>();
        List<Vector3> faces = new List<Vector3>();
        middlePointIndexCache = new Dictionary<long, int>();
        //int[] tri = new int[60];

        index = 0;

        //Changing this produces strange results
        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;


        preVertices.Add(new Vector3(-1, t, 0));
        preVertices.Add(new Vector3(1, t, 0));
        preVertices.Add(new Vector3(-1, -t, 0));
        preVertices.Add(new Vector3(1, -t, 0));

        preVertices.Add(new Vector3(0, -1, t));
        preVertices.Add(new Vector3(0, 1, t));
        preVertices.Add(new Vector3(0, -1, -t));
        preVertices.Add(new Vector3(0, 1, -t));

        preVertices.Add(new Vector3(t, 0, -1));
        preVertices.Add(new Vector3(t, 0, 1));
        preVertices.Add(new Vector3(-t, 0, -1));
        preVertices.Add(new Vector3(-t, 0, 1));

        foreach(Vector3 vert in preVertices)
        {
            addVertex(vert);
        }


        faces.Add(new Vector3(0, 11, 5));
        faces.Add(new Vector3(0, 5, 1));
        faces.Add(new Vector3(0, 1, 7));
        faces.Add(new Vector3(0, 7, 10));
        faces.Add(new Vector3(0, 10, 11));
        
        faces.Add(new Vector3(1, 5, 9));
        faces.Add(new Vector3(5, 11, 4));
        faces.Add(new Vector3(11, 10, 2));
        faces.Add(new Vector3(10, 7, 6));
        faces.Add(new Vector3(7, 1, 8));
        
        faces.Add(new Vector3(3, 9, 4));
        faces.Add(new Vector3(3, 4, 2));
        faces.Add(new Vector3(3, 2, 6));
        faces.Add(new Vector3(3, 6, 8));
        faces.Add(new Vector3(3, 8, 9));
        
        faces.Add(new Vector3(4, 9, 5));
        faces.Add(new Vector3(2, 4, 11));
        faces.Add(new Vector3(6, 2, 10));
        faces.Add(new Vector3(8, 6, 7));
        faces.Add(new Vector3(9, 8, 1));
        
        for (int i = 0; i < recusionLevel; ++i)
        {
            List<Vector3> faces2 = new List<Vector3>();

            foreach(Vector3 tri in faces)
            {
                int a = getMiddlePoint((int)tri.x, (int)tri.y);
                int b = getMiddlePoint((int)tri.y, (int)tri.z);
                int c = getMiddlePoint((int)tri.z, (int)tri.x);

                faces2.Add(new Vector3(tri.x, a, c));
                faces2.Add(new Vector3(tri.y, b, a));
                faces2.Add(new Vector3(tri.z, c, b));
                faces2.Add(new Vector3(a, b, c));
            }
            faces = faces2;
        }

        int[] triangles = new int[faces.Count * 3];
        int triIdx = 0;
        foreach(Vector3 tri in faces)
        {
            triangles[triIdx]   = (int)tri.x;
            triangles[triIdx+1] = (int)tri.y;
            triangles[triIdx+2] = (int)tri.z;
            triIdx += 3;
        }


        icoMesh.SetVertices(vertices);

        icoMesh.triangles = triangles;

        icoMesh.RecalculateNormals();

        //icoMesh.triangles = new int[]{
        //    0, 11, 5,
        //    0, 5, 1,
        //    0, 1, 7,
        //    0, 7, 10,
        //    0, 10, 11,
        //
        //    1, 5, 9,
        //    5, 11, 4,
        //    11, 10, 2,
        //    10, 7, 6,
        //    7, 1, 8,
        //
        //    3, 9, 4,
        //    3, 4, 2,
        //    3, 2, 6,
        //    3, 6, 8,
        //    3, 8, 9,
        //
        //    4, 9, 5,
        //    2, 4, 11,
        //    6, 2, 10,
        //    8, 6, 7,
        //    9, 8, 1
        //    };


        meshFilter.mesh = icoMesh;
        if(meshCollider)
        {
            meshCollider.sharedMesh = icoMesh;
        }
    }


    //Taken from http://blog.andreaskahler.com/2009/06/creating-icosphere-mesh-in-code.html
    // return index of point in the middle of p1 and p2
    private int getMiddlePoint(int p1, int p2)
    {
        // first check if we have it already
        bool firstIsSmaller = p1 < p2;
        long smallerIndex = firstIsSmaller ? p1 : p2;
        long greaterIndex = firstIsSmaller ? p2 : p1;
        long key = (smallerIndex << 32) + greaterIndex;
        
        int ret;
        if (this.middlePointIndexCache.TryGetValue(key, out ret))
        {
            return ret;
        }

        // not in cache, calculate it
        Vector3 point1 = vertices[p1];
        Vector3 point2 = vertices[p2];
        Vector3 middle = new Vector3(
            (point1.x + point2.x) / 2.0f,
            (point1.y + point2.y) / 2.0f,
            (point1.z + point2.z) / 2.0f);

        // add vertex makes sure point is on unit sphere
        int i = addVertex(middle);

        // store it, return index
        this.middlePointIndexCache.Add(key, i);
        return i;
    }

    //Taken from http://blog.andreaskahler.com/2009/06/creating-icosphere-mesh-in-code.html
    // add vertex to mesh, fix position to be on unit sphere, return index
    private int addVertex(Vector3 p)
    {
        float length = Mathf.Sqrt(p.x * p.x + p.y * p.y + p.z * p.z);
        vertices.Add(new Vector3(p.x / length, p.y / length, p.z / length));
        return index++;
    }


}
