//~ #define NAVGEN_INCLUDE_TESTS

using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEditor.AI;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine.AI;
using UnityEngine;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;


namespace idbrii.navgen
{
    [CustomEditor(typeof(NavLinkGenerator), true)]
    public class NavLinkGenerator_Editor : Editor
    {
        const float k_DrawDuration = 1f;
        const string k_LinkRootName = "Generated NavLinks";

        [SerializeField] bool m_AttachDebugToLinks;
        [SerializeField] bool m_ShowCreatedLinks;
        [SerializeField] List<NavMeshLink> m_CreatedLinks = new List<NavMeshLink>();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var gen = target as NavLinkGenerator;

            m_AttachDebugToLinks = EditorGUILayout.Toggle("Attach Debug To Links", m_AttachDebugToLinks);

            EditorGUILayout.HelpBox("Workflow: click these buttons from left to right. See tooltips for more info.", MessageType.None);

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Clear All", "Delete generated Interior Volumes, NavMesh, and NavMeshLinks.")))
                {
                    NavNonWalkableCollection_Editor.ClearNonWalkableVolumes();
                    NavMeshAssetManager.instance.ClearSurfaces(NavEdUtil.GetAllInActiveScene<NavMeshSurface>());
                    RemoveLinks();
                    SceneView.RepaintAll();
                    Debug.Log($"Removed NavMesh and NavMeshLinks from all NavMeshSurfaces.");
                }

                if (GUILayout.Button(new GUIContent("Create Interior Volumes", "Create NonWalkable volumes to prevent navmesh generation inside of solid objects.")))
                {
                    NavNonWalkableCollection_Editor.CreateNonWalkableVolumes();
                }

                if (GUILayout.Button(new GUIContent("Bake NavMesh", "Build navmesh for all NavMeshSurface.")))
                {
                    var surfaces = NavEdUtil.GetAllInActiveScene<NavMeshSurface>();
                    NavMeshAssetManager.instance.StartBakingSurfaces(surfaces);
                    Debug.Log($"Baked NavMesh for {surfaces.Length} NavMeshSurfaces.");
                }

                if (GUILayout.Button(new GUIContent("Bake Links", "Create NavMeshLinks along your navmesh edges.")))
                {
                    GenerateLinks(gen);
                    Debug.Log($"Baked NavMeshLinks.");
                }

                if (GUILayout.Button(new GUIContent("Select NavMesh", "Selecting the navmesh makes it draw in the Scene view so you can evaluate the quality of the mesh and the links.")))
                {
                    Selection.objects = NavEdUtil.GetAllInActiveScene<NavMeshSurface>();
                }
            }

            EditorGUILayout.Space();
            m_ShowCreatedLinks = EditorGUILayout.Foldout(m_ShowCreatedLinks, "Created Links", toggleOnLabelClick: true);
            if (m_ShowCreatedLinks)
            {
                foreach (var entry in m_CreatedLinks)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.ObjectField(entry, typeof(NavMeshLink), allowSceneObjects: true);
                        using (new EditorGUI.DisabledScope(!m_AttachDebugToLinks))
                        {
                            if (GUILayout.Button("Draw"))
                            {
                                entry.GetComponent<NavLinkCreationReason>().Draw();

                                if (SceneView.lastActiveSceneView != null)
                                {
                                    // Prevent losing focus (we'd lose our state).
                                    ActiveEditorTracker.sharedTracker.isLocked = true;

                                    var activeGameObject = Selection.activeGameObject;
                                    activeGameObject = entry.gameObject;
                                    EditorGUIUtility.PingObject(activeGameObject);
                                    SceneView.lastActiveSceneView.FrameSelected();
                                    Selection.activeGameObject = activeGameObject;
                                }
                            }
                        }
                    }
                }
            }
        }

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
                    (   lhs.m_StartPos == rhs.m_StartPos && lhs.m_EndPos == rhs.m_EndPos)
                    || (lhs.m_StartPos == rhs.m_EndPos   && lhs.m_EndPos == rhs.m_StartPos);
            }

            public int GetHashCode(NavEdge e)
            {
                return e.m_StartPos.GetHashCode() ^ e.m_EndPos.GetHashCode();
            }
        }

#if NAVGEN_INCLUDE_TESTS
        [UnityEditor.MenuItem("Tools/Test/NavEdge Compare")]
#endif
        static void Test_NavEdge_Compare()
        {
            var cmp = new NavEdgeEqualityComparer();
            var edge           = new NavEdge(new Vector3(10f, 20f, 30f), new Vector3(20f, 20f, 20f));
            var edge_identical = new NavEdge(edge.m_StartPos, edge.m_EndPos);
            var edge_reverse   = new NavEdge(edge.m_EndPos, edge.m_StartPos);

            Debug.Assert(cmp.Equals(edge, edge), "compare to self.");
            Debug.Assert(cmp.Equals(edge, edge_identical), "compare to identical.");
            Debug.Assert(cmp.Equals(edge, edge_reverse), "compare to mirrored.");

            var edge_list = new HashSet<NavEdge>(cmp);
            edge_list.Add(edge);
            Debug.Assert(edge_list.Add(edge) == false, "Add failed to find duplicate");
            Debug.Assert(edge_list.Remove(edge) == true, "Remove failed to find edge");
            Debug.Assert(edge_list.Count == 0, "Must be empty now.");

            AddIfUniqueAndRemoveIfNot(edge_list, edge);
            Debug.Assert(edge_list.Count == 1, "AddIfUniqueAndRemoveIfNot should add edge to empty set.");
            AddIfUniqueAndRemoveIfNot(edge_list, edge_identical);
            Debug.Assert(edge_list.Count == 0, "AddIfUniqueAndRemoveIfNot should remove identical edge.");

            AddIfUniqueAndRemoveIfNot(edge_list, edge);
            Debug.Assert(edge_list.Count == 1, "AddIfUniqueAndRemoveIfNot failed to add edge");
            AddIfUniqueAndRemoveIfNot(edge_list, edge_reverse);
            Debug.Assert(edge_list.Count == 0, "AddIfUniqueAndRemoveIfNot failed to find edge");

            Debug.Log("Test complete: NavEdge");
        }

        // Don't want inner edges (which match another existing edge).
        static void AddIfUniqueAndRemoveIfNot(HashSet<NavEdge> set, NavEdge edge)
        {
            bool had_edge = set.Remove(edge);
            if (!had_edge)
            {
                set.Add(edge);
            }
        }

        static float DistanceSqToPointOnLine(Vector3 a, Vector3 b, Vector3 p)
        {
            Vector3 ab = b - a;
            Vector3 pa = a - p;
            var mag = ab.magnitude;
            Vector3 c = ab * (Vector3.Dot( pa, ab ) / (mag * mag));
            Vector3 d = pa - c;
            return Vector3.Dot( d, d );
        }

#if NAVGEN_INCLUDE_TESTS
        [UnityEditor.MenuItem("Tools/Test/DistanceSqToPointOnLine")]
#endif
        static void Test_DistanceSqToPointOnLine()
        {
            var a = new Vector3(0f, 0f, 0f);
            var b = new Vector3(1f, 1f, 0f);

            var not_on_line = new Vector3[]{
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 0f, 1f),
            };
            foreach (var t in not_on_line)
            {
                var dist = DistanceSqToPointOnLine(a, b, t);
                UnityEngine.Assertions.Assert.IsTrue(dist > 0.001f, $"Didn't expect {t} to be colinear, but result was {dist}.");
            }

            var are_on_line = new Vector3[]{
                new Vector3(0f, 0f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(0.75f, 0.75f, 0f),
                new Vector3(1f, 1f, 0f),
                new Vector3(2f, 2f, 0f), // line, not line segment
            };
            foreach (var t in are_on_line)
            {
                UnityEngine.Assertions.Assert.AreApproximatelyEqual(DistanceSqToPointOnLine(a, b, t), 0f, $"Expected {t} to be colinear");
            }

            Debug.Log("Test complete: DistanceSqToPointOnLine");
        }


        void GenerateLinks(NavLinkGenerator gen)
        {
            var tri = NavMesh.CalculateTriangulation();
            var edge_list = CreateEdges(tri);
            foreach (var edge in edge_list)
            {
                edge.ComputeDerivedData();
            }
            if (edge_list.Count() == 0)
            {
                return;
            }

            RemoveLinks();
            m_CreatedLinks.Clear();
            var parent = NavEdUtil.GetNamedRoot(k_LinkRootName);

            foreach (var edge in edge_list)
            {
                var mid = edge.GetMidpoint();
                var fwd = edge.m_Normal;
                var link = CreateNavLink(parent, gen, edge, mid, fwd);
                if (link != null)
                {
                    m_CreatedLinks.Add(link);
                }
            }
        }


        NavMeshLink CreateNavLink(Transform parent, NavLinkGenerator gen, NavEdge edge, Vector3 mid, Vector3 fwd)
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
            for (int i = 0; i < gen.m_Steps; ++i)
            {
                float scale = (float)i / (float)gen.m_Steps;

                var top = mid + (fwd * gen.m_MaxHorizontalJump * scale);
                var down = top + (Vector3.down * gen.m_MaxVerticalFall);
                bool hit = Physics.Linecast(top, down, out phys_hit, gen.m_PhysicsMask.value, QueryTriggerInteraction.Ignore);
                //~ Debug.DrawLine(mid, top, hit ? ground_found : ground_missing, k_DrawDuration);
                //~ Debug.DrawLine(top, down, hit ? ground_found : ground_missing, k_DrawDuration);
                if (hit)
                {
                    var max_distance = gen.m_MaxVerticalFall - phys_hit.distance;
                    hit = NavMesh.SamplePosition(phys_hit.point, out nav_hit, max_distance, (int)gen.m_NavMask);
                    // Only place downward links (to avoid back and forth double placement).
                    hit = hit && (nav_hit.position.y <= mid.y);
                    // Only accept 90 wedge in front of normal (prevent links
                    // that other edges are already handling).
                    hit = hit && Vector3.Dot(nav_hit.position - mid, edge.m_Normal) > Mathf.Cos(gen.m_MaxAngleFromEdgeNormal);
                    bool is_original_edge = edge.IsPointOnEdge(nav_hit.position);
                    hit &= !is_original_edge; // don't count self
                    //~ Debug.DrawLine(phys_hit.point, nav_hit.position, hit ? navmesh_found : navmesh_missing, k_DrawDuration);
                    if (hit)
                    {
                        var height_offset = Vector3.up * gen.m_AgentHeight;
                        var transit_start = mid + height_offset;
                        var transit_end = nav_hit.position + height_offset;
                        // Raycast both ways to ensure we're not inside a collider.

                        hit = Physics.Linecast(transit_start, transit_end, out ignored, gen.m_PhysicsMask.value, QueryTriggerInteraction.Ignore)
                            || Physics.Linecast(transit_end, transit_start, out ignored, gen.m_PhysicsMask.value, QueryTriggerInteraction.Ignore);
                        //~ Debug.DrawLine(transit_start, transit_end, hit ? traverse_clear : traverse_hit, k_DrawDuration);
                        if (hit)
                        {
                            // Agent can't jump through here.
                            continue;
                        }
                        var height_delta = nav_hit.position.y - mid.y;
                        var prefab = gen.m_JumpLinkPrefab;
                        if (height_delta > gen.m_MaxVerticalJump)
                        {
                            prefab = gen.m_FallLinkPrefab;
                        }
                        var t = PrefabUtility.InstantiatePrefab(prefab, parent.gameObject.scene) as Transform;
                        Debug.Assert(t != null, $"Failed to instantiate {prefab}");
                        t.SetParent(parent);
                        t.SetPositionAndRotation(mid, edge.m_Away);
                        var link = t.GetComponent<NavMeshLink>();

                        // Push endpoint out into the navmesh to ensure good
                        // connection. Necessary to prevent invalid links.
                        var inset = 0.05f;
                        link.startPoint = link.transform.InverseTransformPoint(mid - fwd * inset);
                        link.endPoint = link.transform.InverseTransformPoint(nav_hit.position) + (Vector3.forward * inset);
                        link.width = edge.m_Length;
                        link.UpdateLink();
                        Debug.Log("Created NavLink", link);
                        Undo.RegisterCompleteObjectUndo(link.gameObject, "Create NavMeshLink");

                        if (m_AttachDebugToLinks)
                        {
                            // Attach a component that has the information we
                            // used to decide how to create this navlink. Much
                            // easier to go back and inspect it like this than
                            // to try to examine the output as you generate
                            // navlinks. Mostly useful for debugging
                            // NavLinkGenerator.
                            var reason = link.gameObject.AddComponent<NavLinkCreationReason>();
                            reason.gen = gen;
                            reason.fwd = fwd;
                            reason.mid = mid;
                            reason.top = top;
                            reason.down = down;
                            reason.transit_start = transit_start;
                            reason.transit_end = transit_end;
                            reason.nav_hit_position = nav_hit.position;
                            reason.phys_hit_point = phys_hit.point;
                        }

                        return link;
                    }
                }
            }
            return null;
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

        void RemoveLinks()
        {
            var nav_links = NavEdUtil.GetNamedRoot(k_LinkRootName).GetComponentsInChildren<NavMeshLink>();
            foreach (var link in nav_links)
            {
                GameObject.DestroyImmediate(link.gameObject);
            }
        }

    }
}
