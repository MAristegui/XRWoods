using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodsTerrain : MonoBehaviour
{
    static WoodsTerrain _instance;
    public static WoodsTerrain Instance { get { return _instance; } }
    void Awake()
    {
        _instance = this;
    }

}
