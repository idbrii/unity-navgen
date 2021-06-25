using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine;

//public static NavLinkGenerator_Runtime Singleton;
//NavLinkGenerator_Runtime()
//{
//    Singleton = this;
//}

namespace idbrii.navgen.runtime
{
    public class NavLinkGenerator_Runtime : MonoBehaviour
    {
        //parameter
        public Transform OffMeshLinkPrefab;
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

        //generate
        Transform _LinksRoot;
        public Transform LinksRoot
        {
            get
            {
                if (_LinksRoot == null)
                    _LinksRoot = new GameObject().transform;
                return _LinksRoot;
            }
        }
        public bool m_UseOffMeshLink;
        //public bool m_AttachDebugToLinks;
        //public bool m_ShowCreatedLinks;
        public List<GameObject> m_CreatedLinks = new List<GameObject>();

        

        void Awake()
        {

        }

        void OnDestroy()
        {
            CleanLinks();
        }

        public int RebuildLinks()
        {
            var tri = NavMesh.CalculateTriangulation();
            var edge_list = CreateEdges(tri);
            foreach (var edge in edge_list)
                edge.ComputeDerivedData();

            if (edge_list.Count() == 0)
                return 0;

            CleanLinks();
            m_CreatedLinks.Clear();

            foreach (var edge in edge_list)
            {
                var mid = edge.GetMidpoint();
                var fwd = edge.m_Normal;
                var link = CreateLink(LinksRoot, edge, mid, fwd, m_UseOffMeshLink);
                if (link != null)
                    m_CreatedLinks.Add(link);
            }

            if (m_UseOffMeshLink)
                Debug.Log($"Baked OffMeshLinks:" + m_CreatedLinks.Count);
            else
                Debug.Log($"Baked NavMeshLinks:" + m_CreatedLinks.Count);

            return m_CreatedLinks.Count;
        }

        GameObject CreateLink(Transform parent, NavEdge edge, Vector3 mid, Vector3 fwd,bool useOffMeshLink)
        {
            RaycastHit phys_hit;
            RaycastHit ignored;
            NavMeshHit nav_hit;
            var ground_found = Color.Lerp(Color.red, Color.white, 0.75f);
            var ground_missing = Color.Lerp(Color.red, Color.white, 0.35f);
            var navmesh_found = Color.Lerp(Color.cyan, Color.white, 0.75f);
            var navmesh_missing = Color.Lerp(Color.red, Color.white, 0.65f);
            var traverse_clear = Color.green;
            var traverse_hit = Color.red;
            for (int i = 0; i < m_Steps; ++i)
            {
                float scale = (float)i / (float)m_Steps;

                var top = mid + (fwd * m_MaxHorizontalJump * scale);
                var down = top + (Vector3.down * m_MaxVerticalFall);
                bool hit = Physics.Linecast(top, down, out phys_hit, m_PhysicsMask.value, QueryTriggerInteraction.Ignore);
                //~ Debug.DrawLine(mid, top, hit ? ground_found : ground_missing, k_DrawDuration);
                //~ Debug.DrawLine(top, down, hit ? ground_found : ground_missing, k_DrawDuration);
                if (hit)
                {
                    var max_distance = m_MaxVerticalFall - phys_hit.distance;
                    hit = NavMesh.SamplePosition(phys_hit.point, out nav_hit, max_distance, (int)m_NavMask);
                    // Only place downward links (to avoid double placement).
                    hit = hit && (nav_hit.position.y <= mid.y);
                    bool is_original_edge = edge.IsPointOnEdge(nav_hit.position);
                    hit &= !is_original_edge; // don't count self
                    //~ Debug.DrawLine(phys_hit.point, nav_hit.position, hit ? navmesh_found : navmesh_missing, k_DrawDuration);
                    if (hit)
                    {
                        var height_offset = Vector3.up * m_AgentHeight;
                        var transit_start = mid + height_offset;
                        var transit_end = nav_hit.position + height_offset;
                        // Raycast both ways to ensure we're not inside a collider.

                        hit = Physics.Linecast(transit_start, transit_end, out ignored, m_PhysicsMask.value, QueryTriggerInteraction.Ignore)
                            || Physics.Linecast(transit_end, transit_start, out ignored, m_PhysicsMask.value, QueryTriggerInteraction.Ignore);
                        //~ Debug.DrawLine(transit_start, transit_end, hit ? traverse_clear : traverse_hit, k_DrawDuration);
                        if (hit)
                        {
                            // Agent can't jump through here.
                            continue;
                        }
                        var height_delta = nav_hit.position.y - mid.y;

                        GameObject linkgameObject = null;
                        if (useOffMeshLink)
                        {
                            var t = Instantiate(OffMeshLinkPrefab, parent);
                            Debug.Assert(t != null, $"Failed to instantiate {OffMeshLinkPrefab}");
                            t.SetParent(parent);
                            t.SetPositionAndRotation(mid, edge.m_Away);
                            OffMeshLink link = t.GetComponent<OffMeshLink>();
                            linkgameObject = link.gameObject;

                            // Push endpoint out into the navmesh to ensure good
                            // connection. Necessary to prevent invalid links.
                            var inset = 0.05f;
                            link.startTransform.localPosition = link.transform.InverseTransformPoint(mid - fwd * inset);
                            link.endTransform.localPosition = link.transform.InverseTransformPoint(nav_hit.position) + (Vector3.forward * inset);
                            //link.w = edge.m_Length;
                            //link.UpdateLink();
                            //Debug.Log("Created OffMeshLink");
                        }
                        else {
                            var prefab = m_JumpLinkPrefab;
                            if (height_delta > m_MaxVerticalJump)
                            {
                                prefab = m_FallLinkPrefab;
                            }
                            var t = Instantiate(prefab, parent);
                            Debug.Assert(t != null, $"Failed to instantiate {prefab}");
                            t.SetParent(parent);
                            t.SetPositionAndRotation(mid, edge.m_Away);
                            NavMeshLink link = t.GetComponent<NavMeshLink>();
                            linkgameObject = link.gameObject;

                            // Push endpoint out into the navmesh to ensure good
                            // connection. Necessary to prevent invalid links.
                            var inset = 0.05f;
                            link.startPoint = link.transform.InverseTransformPoint(mid - fwd * inset);
                            link.endPoint = link.transform.InverseTransformPoint(nav_hit.position) + (Vector3.forward * inset);
                            link.width = edge.m_Length;
                            link.UpdateLink();
                            //Debug.Log("Created OffMeshLink");
                        }
                        linkgameObject.name = linkgameObject.name+":"+edge.m_StartPos;

                        //if (m_AttachDebugToLinks)
                        //{
                        //    // Attach a component that has the information we
                        //    // used to decide how to create this navlink. Much
                        //    // easier to go back and inspect it like this than
                        //    // to try to examine the output as you generate
                        //    // navlinks. Mostly useful for debugging
                        //    // NavLinkGenerator.
                        //    var reason = linkgameObject.AddComponent<NavLinkCreationReason>();
                        //    //reason.gen = gen;
                        //    reason.fwd = fwd;
                        //    reason.mid = mid;
                        //    reason.top = top;
                        //    reason.down = down;
                        //    reason.transit_start = transit_start;
                        //    reason.transit_end = transit_end;
                        //    reason.nav_hit_position = nav_hit.position;
                        //    reason.phys_hit_point = phys_hit.point;
                        //}

                        return linkgameObject;
                    }
                }
            }
            return null;
        }



        public void CleanLinks()
        {
            if(LinksRoot != null)
                GameObject.DestroyImmediate(LinksRoot.gameObject);
        }

        //public static T[] GetAllInActiveScene<T>() where T : Component
        //{
        //    return Resources.FindObjectsOfTypeAll<T>();
        //}

        //private static void StartBakingSurfaces(NavMeshSurface[] surfaces)
        //{
        //    throw new System.NotImplementedException();
        //}
        //public static void ClearSurfaces(NavMeshSurface[] surfaces)
        //{
        //    foreach (NavMeshSurface s in surfaces)
        //        ClearSurface(s);
        //}
        //public static void ClearSurface(NavMeshSurface navSurface)
        //{
        //    throw new System.NotImplementedException();
        //}

        void DrawEdges(float duration)
        {
            var tri = NavMesh.CalculateTriangulation();
            var edge_list = CreateEdges(tri);
            foreach (var edge in edge_list)
            {
                edge.ComputeDerivedData();
                Debug.DrawLine(edge.m_StartPos, edge.m_EndPos, Color.magenta, duration);
                var mid = edge.GetMidpoint();
                Debug.DrawLine(mid, mid + edge.m_Normal, Color.blue, duration);
            }
        }
        // Don't want inner edges (which match another existing edge).
        static void AddIfUniqueAndRemoveIfNot(HashSet<NavEdge> set, NavEdge edge)
        {
            bool had_edge = set.Remove(edge);
            if (!had_edge)
                set.Add(edge);
        }

        static float DistanceSqToPointOnLine(Vector3 a, Vector3 b, Vector3 p)
        {
            Vector3 ab = b - a;
            Vector3 pa = a - p;
            var mag = ab.magnitude;
            Vector3 c = ab * (Vector3.Dot(pa, ab) / (mag * mag));
            Vector3 d = pa - c;
            return Vector3.Dot(d, d);
        }
        static IEnumerable<NavEdge> CreateEdges(NavMeshTriangulation tri)
        {
            // use HashSet to ignore duplicate edges.
            var edges = new HashSet<NavEdge>(new NavEdgeEqualityComparer());
            for (int i = 0; i < tri.indices.Length - 1; i += 3)
            {
                AddIfUniqueAndRemoveIfNot(edges, TriangleToEdge(tri, i, i + 1));
                AddIfUniqueAndRemoveIfNot(edges, TriangleToEdge(tri, i + 1, i + 2));
                AddIfUniqueAndRemoveIfNot(edges, TriangleToEdge(tri, i + 2, i));
            }
            return edges;
        }

        static NavEdge TriangleToEdge(NavMeshTriangulation tri, int start, int end)
        {
            var v1 = tri.vertices[tri.indices[start]];
            var v2 = tri.vertices[tri.indices[end]];
            return new NavEdge(v1, v2);
        }

        class NavEdge
        {
            public Vector3 m_StartPos;
            public Vector3 m_EndPos;

            // Derived data
            public float m_Length;
            public Vector3 m_Normal;
            public Quaternion m_Away;

            public NavEdge(Vector3 start, Vector3 end)
            {
                m_StartPos = start;
                m_EndPos = end;
            }

            public Vector3 GetMidpoint()
            {
                return Vector3.Lerp(m_StartPos, m_EndPos, 0.5f);
            }

            public void ComputeDerivedData()
            {
                m_Length = Vector3.Distance(m_StartPos, m_EndPos);
                var normal = Vector3.Cross(m_EndPos - m_StartPos, Vector3.up).normalized;

                // Point it outside the nav poly.
                NavMeshHit nav_hit;
                var mid = GetMidpoint();
                var end = mid - normal * 0.3f;
                bool hit = NavMesh.SamplePosition(end, out nav_hit, 0.2f, NavMesh.AllAreas);
                //~ Debug.DrawLine(mid, end, hit ? Color.red : Color.white);
                if (!hit)
                {
                    normal *= -1f;
                }
                m_Normal = normal;
                m_Away = Quaternion.LookRotation(normal);
            }
            public bool IsPointOnEdge(Vector3 point)
            {
                return DistanceSqToPointOnLine(m_StartPos, m_EndPos, point) < 0.001f;
            }
        }

        // Using EqualityComparer on NavEdge didn't work, so use a comparer.
        class NavEdgeEqualityComparer : IEqualityComparer<NavEdge>
        {
            public bool Equals(NavEdge lhs, NavEdge rhs)
            {
                return
                    (lhs.m_StartPos == rhs.m_StartPos && lhs.m_EndPos == rhs.m_EndPos)
                    || (lhs.m_StartPos == rhs.m_EndPos && lhs.m_EndPos == rhs.m_StartPos);
            }

            public int GetHashCode(NavEdge e)
            {
                return e.m_StartPos.GetHashCode() ^ e.m_EndPos.GetHashCode();
            }
        }
    }
}
