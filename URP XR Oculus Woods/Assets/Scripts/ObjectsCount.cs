using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsCount : MonoBehaviour
{
    string oldname;
    [SerializeField]
    List<Transform> list = new List<Transform>();
    // Start is called before the first frame update
    void Start()
    {
        oldname = gameObject.name;
        var c = GetComponentsInChildren<MonoBehaviour>();
        gameObject.name = oldname + " c: " + c.Length;
        var co = GetComponentsInChildren<MeshCollider>();
        gameObject.name = gameObject.name + " mc: " + co.Length;

        list.Add(transform);
        Transform tr = transform;

        int i = 0;
        while (i < list.Count)
        {
            tr = list[i];

            for(int j=0; j < tr.childCount; j++)
            {
                list.Add(tr.GetChild(j));
            }

            i++;
        }

        gameObject.name = gameObject.name + " o: " + list.Count;


    }


}
