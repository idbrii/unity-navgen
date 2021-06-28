using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class faceCamera : MonoBehaviour
{
    public string Text { set { textMesh.text = value; } }

    TextMesh textMesh;
    Camera camera;
    void Awake()
    {
        camera = Camera.main;
        textMesh = GetComponent<TextMesh>();
    }

    void Update()
    {
        transform.rotation = camera.transform.rotation;
        //transform.LookAt(camera.transform);
    }
}
