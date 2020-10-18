using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.AI;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class DungeonManager : MonoBehaviour
{
    public int m_Width = 10;
    public int m_Height = 10;
    public float m_Spacing = 4.0f;
    public GameObject[] m_Tiles = new GameObject[16];

    List<GameObject> m_Instances = new List<GameObject>(16);

    void Awake()
    {
        CreateDungeon(23431);
    }

    void CreateDungeon(int seed)
    {
        Random.InitState(seed);
        var map = new int[m_Width * m_Height];
        for (int y = 0; y < m_Height; y++)
        {
            for (int x = 0; x < m_Width; x++)
            {
                bool px = false;
                bool py = false;
                if (x > 0)
                    px = (map[(x - 1) + y * m_Width] & 1) != 0;
                if (y > 0)
                    py = (map[x + (y - 1) * m_Width] & 2) != 0;

                int tile = 0;
                if (px)
                    tile |= 4;
                if (py)
                    tile |= 8;
                if (x + 1 < m_Width && Random.value > 0.5f)
                    tile |= 1;
                if (y + 1 < m_Height && Random.value > 0.5f)
                    tile |= 2;

                map[x + y * m_Width] = tile;
            }
        }

        for (int y = 0; y < m_Height; y++)
        {
            for (int x = 0; x < m_Width; x++)
            {
                var pos = new Vector3(x * m_Spacing, 0, y * m_Spacing);
                if (m_Tiles[map[x + y * m_Width]] != null)
                    m_Instances.Add(Instantiate(m_Tiles[map[x + y * m_Width]], pos, Quaternion.identity));
            }
        }
    }

    [NaughtyAttributes.Button]
    void CreateDisconnectedDungeon()
    {
        foreach (var entry in m_Instances)
        {
            GameObject.DestroyImmediate(entry.gameObject);
        }
        m_Instances.Clear();

        CreateDungeon(Mathf.RoundToInt(Random.value * 10000000f));
        var root = GameObject.Find("Dungeon Root") ?? new GameObject("Dungeon Root");
        var parent = root.transform;
        foreach (var obj in m_Instances)
        {
            obj.transform.SetParent(parent);
            var position = obj.transform.position;
            position.y += Random.Range(-5f, 5f);
            obj.transform.position = position;
            UnityEditor.Undo.RegisterCreatedObjectUndo(obj, "CreateDisconnectedDungeon");
        }
        foreach (var entry in parent.GetComponentsInChildren<NavMeshLink>())
        {
            Component.DestroyImmediate(entry);
        }
        UnityEditor.Undo.RegisterCreatedObjectUndo(parent.gameObject, "CreateDisconnectedDungeon");
    }

}
