using UnityEngine.AI;
using UnityEngine;

namespace idbrii
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(LineRenderer))]
    public class NavMeshDrawPath : MonoBehaviour
    {

        NavMeshAgent m_Agent;
        private LineRenderer m_Line;

        void Awake()
        {
            m_Agent = GetComponent<NavMeshAgent>();
            m_Line = GetComponent<LineRenderer>();
        }

        void Update()
        {
            var prev = m_Agent.transform.position;
            var corners = m_Agent.path.corners;

            m_Line.positionCount = corners.Length;
            var half_height = m_Agent.height * 0.5f;
            for (int i = 0; i < corners.Length; ++i)
            {
                var c = corners[i];
                c.y += half_height;
                corners[i] = c;
            }

            m_Line.SetPositions(corners);
        }

    }
}
