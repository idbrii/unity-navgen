using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace idbrii.navgen
{
    static class NavEdUtil
    {

        public static Object[] GetAllInActiveScene<T>() where T : Component
        {
            var scene = EditorSceneManager.GetActiveScene();
            return Resources.FindObjectsOfTypeAll<T>()
                .Where(comp => comp.gameObject.scene == scene)
                .Select(comp => comp as Object)
                .ToArray();
        }

        public static Transform GetNamedRoot(string root_name)
        {
            var root_objects = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var obj in root_objects)
            {
                if (obj.name == root_name)
                {
                    return obj.transform;
                }
            }
            return new GameObject(root_name).transform;
        }

    }
}
