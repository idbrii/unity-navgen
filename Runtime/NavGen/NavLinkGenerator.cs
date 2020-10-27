using UnityEngine;
using UnityEngine.AI;

namespace idbrii.navgen
{

    [CreateAssetMenu(fileName = "NavLinkGenerator", menuName = "Navigation/NavLinkGenerator", order = 1)]
    public class NavLinkGenerator : ScriptableObject
    {
        public Transform m_JumpLinkPrefab;
        public Transform m_FallLinkPrefab;
        public float m_MaxHorizontalJump = 5f;
        [Tooltip("Max distance we can jump up.")]
        public float m_MaxVerticalJump = 3f;
        [Tooltip("Max distance we are allowed to fall. Usually higher than m_MaxVerticalJump.")]
        public float m_MaxVerticalFall = 5f;
        public int m_Steps = 10;
        public LayerMask m_PhysicsMask = -1;
        public NavMeshAreas m_NavMask = NavMeshAreas.All;
        public float m_AgentHeight = 1.5f;
        public float m_AgentRadius = 0.5f;
        [Tooltip("Maximum degrees away from the normal pointing horizontally out of a navmesh edge. Larger values allow more awkward links, but may result in redundant or inappropriate links.")]
        [Range(0f, 60f)]
        public float m_MaxAngleFromEdgeNormal = 45f;
    }
}
