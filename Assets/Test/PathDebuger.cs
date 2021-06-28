using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//[RequireComponent(typeof(LineRenderer))]
public class PathDebuger : MonoBehaviour
{
    public static PathDebuger main;
    public PathDebuger()
    {
        main = this;
    }
    
    //=====================DebugLine=================
    LineRenderer lineRenderer;
    
    //================3DText===================
    public GameObject Text3DPerfab;

    Transform _DebugRoot;
    public Transform DebugRoot
    {
        get
        {
            if (_DebugRoot == null)
            {
                var linkroot = new GameObject();
                linkroot.name = "DebugPathRoot"+this.GetHashCode();
                _DebugRoot = linkroot.transform;
            }
            return _DebugRoot;
        }
        set {
            if (value == null && _DebugRoot != null)
            {
                Destroy(_DebugRoot.gameObject);
                _DebugRoot = null;
            }
            else
                _DebugRoot = value;
        }
    }


    private void Awake()
    {
        Text3DPerfab = Resources.Load<GameObject>("Prefab/DebugText");
        Debug.Assert(Text3DPerfab != null);

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.widthMultiplier = 0.5f;
        }
        lineRenderer.startColor = Color.blue;
        Debug.Assert(lineRenderer != null);
    }
    public void OnDestroy()
    {
        lineRenderer.positionCount = 0;
        DebugRoot = null;
    }
    public void DebugPath(NavMeshPath p)
    {
        //Debug.Log("DebugPath:" + p.corners.Length);
        DebugRoot = null;

        //CreatPath
        for (int i = 0; i < p.corners.Length; i++)
        {
            var text3d = Instantiate(Text3DPerfab, DebugRoot);
            text3d.transform.position = p.corners[i];
            faceCamera faceCamera = text3d.GetComponent<faceCamera>();
            faceCamera.Text = i.ToString();
        }
        lineRenderer.positionCount = p.corners.Length;
        lineRenderer.SetPositions(p.corners);
        //Debug.Log(p.corners.Length);
    }
    
}
