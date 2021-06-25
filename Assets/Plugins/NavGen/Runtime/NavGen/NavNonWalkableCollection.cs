using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

namespace idbrii.navgen
{
    public class NavNonWalkableCollection : MonoBehaviour
    {
        [Tooltip("Volumes managed by NavNonWalkableCollection. Remove volumes from this list to avoid rebuilding them automatically.")]
        public List<NavMeshModifierVolume> m_Volumes;
    }
}
