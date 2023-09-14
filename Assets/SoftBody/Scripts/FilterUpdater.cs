using System.Collections;
using System.Collections.Generic;
using SoftBody.Scripts;
using UnityEngine;

public class FilterUpdater : MonoBehaviour
{
    private ButterworthFilter _filter;
    private Mesh _meshFiltered;
    private Mesh _meshUnFiltered;
    
    public GameObject defKitObj;
    public bool isFiltering;
    public int filterOrder;
    public int cutoffFrequency;
    
    // Start is called before the first frame update
    void Start()
    {
        _meshFiltered = GetComponent<MeshFilter>().mesh;
        _meshUnFiltered = defKitObj.GetComponent<MeshFilter>().mesh;
        _filter = new ButterworthFilter(filterOrder,cutoffFrequency,_meshFiltered.vertices);
        print(_meshFiltered.vertices.Length);

    }

    void FixedUpdate()
    {
        if (isFiltering)
        {
            _meshFiltered.vertices = _filter.Filter(_meshUnFiltered.vertices);
        }
        else
        {
            _meshFiltered.vertices = _meshUnFiltered.vertices;
        }
        

        _meshFiltered.RecalculateNormals();
        _meshFiltered.RecalculateBounds();
        
    }
}
