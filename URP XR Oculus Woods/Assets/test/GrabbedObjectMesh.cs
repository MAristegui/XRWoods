using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbedObjectMesh : MonoBehaviour
{
    public Collider[] Colliders;

    Dictionary<Collider, int> _dictionary;

    int prevLayer;

    private void Start()
    {
        if (Colliders.Length == 0)
        {
            Colliders = GetComponentsInChildren<Collider>();
        }
        _dictionary = new Dictionary<Collider, int>();
        foreach(var c in Colliders)
        {
            if(!c.isTrigger)
                _dictionary.Add(c, c.gameObject.layer);
        }
    }
    [SerializeField]bool changed = false;
    public void ChangeLayer(int layer)
    {
        Colliders = GetComponentsInChildren<Collider>();
        if (!changed)
        {
            prevLayer = gameObject.layer;

            _dictionary.Clear();
            foreach (var c in Colliders)
            {
                if (!c.isTrigger)
                    _dictionary.Add(c, c.gameObject.layer);
            }
        }
        changed = true;
        foreach (var c in _dictionary.Keys)
        {
            c.gameObject.layer = layer;
        }
        gameObject.layer = layer;
        
    }

    public void ResetLayers()
    {

        changed = false;
        foreach (var c in _dictionary.Keys)
        {
            c.gameObject.layer = _dictionary[c];
        }
        gameObject.layer = prevLayer;
    }
}
