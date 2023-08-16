using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeLogFragment : MonoBehaviour
{

    public bool Visited = false;

    [SerializeField]private List<NodeLogFragment> _neighbors;

    public LogFragment LogFragment;

    public List<NodeLogFragment> Neighbors 
    { 
        get 
        { 
            CheckListIntegrity(); 
            return _neighbors; 
        } 
    }


    // Start is called before the first frame update
    void Awake()
    {
        _neighbors = new List<NodeLogFragment>();
        //LogFragment = GetComponent<LogFragment>();

    }

    private void OnTriggerEnter(Collider other)
    {
        NodeLogFragment node = other.GetComponent<NodeLogFragment>();

        if (node)
        {
            if (!_neighbors.Contains(node) && node.LogFragment.Parent == LogFragment.Parent)
            {
                _neighbors.Add(node);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        NodeLogFragment node = other.GetComponent<NodeLogFragment>();

        if (node)
        {
            if (_neighbors.Contains(node))
            {
                _neighbors.Remove(node);
            }
        }
    }


    private void CheckListIntegrity()
    {
        List<NodeLogFragment> toRemove = new List<NodeLogFragment>();

        foreach(var n in _neighbors)
        {
            if(!n || !n.isActiveAndEnabled)
            {
                toRemove.Add(n);
            }
        }

        foreach (var n in toRemove)
        {
            _neighbors.Remove(n);
        }

        
    }


}
