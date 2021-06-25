using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace idbrii.navgen.runtime
{
    public class NavMeshGenerator_Runtime : MonoBehaviour
    {
        public NavMeshSurface navMeshSurface;

        void Awake()
        {
            NavMeshSurface[] navMeshSurfaces = Resources.FindObjectsOfTypeAll<NavMeshSurface>();

            Debug.Assert(navMeshSurfaces != null, "NavMeshSurface can't find in scene");
            Debug.Assert(navMeshSurfaces.Length == 1, "NavMeshSurface config not only in scene:" + navMeshSurfaces.Length);

            navMeshSurface = navMeshSurfaces[0];
        }

        public void RebuildNavMesh() {
            navMeshSurface.BuildNavMesh();
        }
    }
}