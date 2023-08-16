using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField] List<NodeLogFragment> List;

    private void Awake()
    {
        List = new List<NodeLogFragment>();
    }

    public void AddNode(NodeLogFragment node)
    {
        if (!List.Contains(node))
        {
            List.Add(node);
        }
    }

    public void RemoveNode(NodeLogFragment node)
    {
        if (List.Contains(node))
            List.Remove(node);
    }


    static int index = 0;
    public void CheckUnconnectedGraph(Log log)
    {
        if (List.Count == 0) return;

        Stack<NodeLogFragment> Stack = new Stack<NodeLogFragment>();

        bool f = false;

        foreach(var node in List)
        {
            if (!node.Visited)
            {
                Stack.Push(node);

                List<NodeLogFragment> auxList = new List<NodeLogFragment>();

                while (Stack.Count > 0)
                {
                    NodeLogFragment nd = Stack.Pop();

                    nd.Visited = true;
                    auxList.Add(nd);

                    foreach(var n in nd.Neighbors)
                    {
                        if(!n.Visited && !Stack.Contains(n))
                            Stack.Push(n);
                    }

                }

                if(auxList.Count>0 && auxList.Count < List.Count)
                {
                    CreateNewLog(auxList, log);
                    f = true;
                }
            }
        }

        foreach(var node in List)
        {
            node.Visited = false;
        }

        if (f)
            Destroy(this.gameObject);

    }

    private void CreateNewLog(List<NodeLogFragment> list, Log log)
    {
        Log NewLog = Instantiate<Log>(log.Prefab, log.transform.position,log.transform.rotation);
        NewLog.Index = log.Index;
        NewLog.transform.localScale = log.transform.localScale;

        NewLog.Prefab = log.Prefab;
        
        NewLog.Radius = log.Radius;

        NewLog.name = log.name+" - GL"+ index;
        index++;
        foreach (var node in list)
        {
            node.LogFragment.transform.parent = NewLog.transform;

            node.LogFragment.Parent = NewLog;

            node.LogFragment.transform.localPosition = Vector3.zero;

            NewLog.AddFragment(node.LogFragment);

        }

        NewLog.RecalculateMass();
        // Falta Axecutter
        //NewLog.CheckMinVolume(AxeCutter.MinVolume);

        NewLog.MassCenter = log.MassCenter;
        if (list.Count == 1)
        {
            //Vector3 center = NewLog.GetFragmentList()[0].CenterFragment(log.MassCenter);
            NewLog.MassCenter = NewLog.GetFragmentList()[0].MassCenter;

            //center = log.transform.TransformPoint(center);

            //NewLog.transform.position = 2 * NewLog.transform.position - center;

        }
        //AxeCutter.GenerateMeshCollider(NewLog);
    }



}
