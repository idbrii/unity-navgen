﻿using System.Collections.Generic;
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
    [CustomEditor(typeof(NavNonWalkableCollection), true)]
    public class NavNonWalkableCollection_Editor : Editor
    {
        const float k_DrawDuration = 1f;

        internal const string k_NoMeshVolumeRootName = "NonWalkable Collection";


        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var collection = target as NavNonWalkableCollection;

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Clear Interior Volumes"))
                {
                    ClearVolumes(collection);
                }

                if (GUILayout.Button("Create Interior Volumes"))
                {
                    CreateNonWalkableVolumes(collection);
                }

                if (GUILayout.Button("Select NavMesh"))
                {
                    Selection.objects = NavEdUtil.GetAllInActiveScene<NavMeshSurface>();
                }
            }
        }

        static NavNonWalkableCollection Get()
        {
            var root = NavEdUtil.GetNamedRoot(k_NoMeshVolumeRootName);
            if (root == null)
            {
                root = new GameObject(k_NoMeshVolumeRootName).transform;
            }
            var collection = root.GetComponent<NavNonWalkableCollection>();
            if (collection == null)
            {
                collection = root.gameObject.AddComponent<NavNonWalkableCollection>();
            }
            return collection;
        }

        public static void CreateNonWalkableVolumes()
        {
            var collection = Get();
            if (collection == null)
            {
                return;
            }
            CreateNonWalkableVolumes(collection);
        }

        public static void ClearNonWalkableVolumes()
        {
            var collection = Get();
            if (collection == null)
            {
                return;
            }
            ClearVolumes(collection);
        }

        static void ClearVolumes(NavNonWalkableCollection collection)
        {
            foreach (var entry in collection.m_Volumes)
            {
                if (entry != null)
                {
                    GameObject.DestroyImmediate(entry.gameObject);
                }
            }
            collection.m_Volumes.Clear();
        }

        static void CreateNonWalkableVolumes(NavNonWalkableCollection collection)
        {
            if (collection.m_Volumes == null)
            {
                collection.m_Volumes = new List<NavMeshModifierVolume>();
            }

            ClearVolumes(collection);

            var surfaces = NavEdUtil.GetAllInActiveScene<NavMeshSurface>();
            var colliders = surfaces
                .SelectMany(s => (s as NavMeshSurface).GetComponentsInChildren<Collider>());
            var threshold_sqr = 1.5f * 1.5f;
            foreach (Collider c in colliders)
            {
                if (c.GetComponentInChildren<NavMeshModifierVolume>() != null)
                {
                    // Skip so we can move them out of m_Volumes to make them persistent.
                    Debug.Log($"[NavMesh] Skipping NonWalkable generation on {c.name} because it already has a NavMeshModifierVolume child.", c);
                    continue;
                }
                // Get unrotated bounds. Hopefully these are tighter.
                var t = c.transform;
                var rot = t.rotation;
                var pos = t.position;
                t.position = Vector3.zero;
                t.rotation = Quaternion.identity;
                Physics.SyncTransforms();
                var b = c.bounds;
                t.rotation = rot;
                t.position = pos;

                if (b.size.sqrMagnitude > threshold_sqr)
                {
                    var obj = new GameObject("Block NavMesh - "+ t.name);
                    obj.transform.SetParent(t);
                    obj.transform.SetPositionAndRotation(pos, rot);
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;

                    var vol = obj.AddComponent<NavMeshModifierVolume>();
                    vol.area = (int)NavMeshAreaIndex.NotWalkable;

                    var offset = 0.2f;
                    var size = b.size;
                    size -= Vector3.one * offset;
                    vol.size = size;
                    vol.center = b.center + Vector3.down * offset;
                    Undo.RegisterCreatedObjectUndo(obj, "Create No Walk Volumes");
                    collection.m_Volumes.Add(vol);
                }
            }
            Undo.RecordObject(collection, "Create No Walk Volumes");
        }

    }
}
