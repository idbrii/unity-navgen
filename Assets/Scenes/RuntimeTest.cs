using idbrii.navgen.runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RuntimeTest : MonoBehaviour
{
    public NavMeshGenerator_Runtime navMeshGenerator_Runtime;
    public NavLinkGenerator_Runtime navLinkGenerator_Runtime;

    private void Awake()
    {
        Debug.Assert(navMeshGenerator_Runtime != null);
        Debug.Assert(navLinkGenerator_Runtime != null);
    }

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        navMeshGenerator_Runtime.RebuildNavMesh();
        navLinkGenerator_Runtime.RebuildLinks();
    }
}
