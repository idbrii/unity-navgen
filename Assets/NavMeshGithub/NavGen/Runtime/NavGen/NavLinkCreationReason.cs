using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.AI;
using UnityEngine;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace idbrii.navgen
{
    // Store the data we used to make decisions when generating NavMeshLinks.
    public class NavLinkCreationReason : MonoBehaviour
    {
        const float k_DrawDuration = 1f;

        public NavLinkGenerator gen;
        public Vector3 mid;
        public Vector3 fwd;
        public Vector3 top;
        public Vector3 down;
        public Vector3 transit_start;
        public Vector3 transit_end;
        public Vector3 nav_hit_position;
        public Vector3 phys_hit_point;

        //~ [NaughtyAttributes.Button]
        [ContextMenu("Draw")]
        public void Draw()
        {
            var ground_found = Color.Lerp(Color.green, Color.yellow, 0.75f);
            var ground_missing = Color.Lerp(Color.red, Color.white, 0.35f);
            var navmesh_found = Color.Lerp(Color.green, Color.cyan, 0.95f);
            var navmesh_missing = Color.Lerp(Color.red, Color.white, 0.65f);
            var traverse_clear = Color.green;
            var traverse_hit = Color.red;
            bool hit = true;

            // Find ground
            Debug.DrawLine(mid, top, hit ? ground_found : ground_missing, k_DrawDuration);
            Debug.DrawLine(top, down, hit ? ground_found : ground_missing, k_DrawDuration);
            // SamplePosition
            Debug.DrawLine(phys_hit_point, nav_hit_position, hit ? navmesh_found : navmesh_missing, k_DrawDuration);
            // Raycast both ways to ensure we're not inside a collider.
            Debug.DrawLine(transit_start, transit_end, hit ? traverse_clear : traverse_hit, k_DrawDuration);
        }


    }
}
