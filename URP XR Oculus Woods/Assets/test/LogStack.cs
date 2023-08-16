using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LogStack : MonoBehaviour
{

    protected static int IndexSelected = -1;

    [SerializeField] List<Log> Logs;
    [SerializeField] Material LogsMaterial;
    [SerializeField] LogStack Prefab;
    [SerializeField] float TimeForRespawn;
    [SerializeField] int MaxLogCount = 50;
    [SerializeField] int CurrentLogCount_Test;
    public static int LogCount = 0;
    public static void AddLog() { LogCount++; }
    public static void RemoveLog() { LogCount--; }

    public static LogStack Instance { get; protected set; }

    [SerializeField]Log[] LogsPrefab;
    public Log GetPrefab(int index)
    {
        return LogsPrefab[index];
    }

    public static bool OverLogCount()
    {
        return LogCount > 100;
    }
   
    void Awake()
    {
        Instance = this;
        Logs = GetComponentsInChildren<Log>().ToList<Log>();
        LogsPrefab = new Log[Logs.Count];

        if (Logs!=null)
        {
            for(int i=0; i<Logs.Count; i++)
            {
                var log = Logs[i];
                log.SetStack(this);
                log.Index = i;
                LogsPrefab[i] = log.Prefab;
            }
        }

        SetSpecialLog();

    }


    void SetSpecialLog()
    {
        if (IndexSelected < 0)
        {
            IndexSelected = (int)(UnityEngine.Random.value * Logs.Count);

            IndexSelected = IndexSelected % Logs.Count;

            Logs[IndexSelected].SetSpecial();
        }
        else if (IsOnList(IndexSelected))
        {
            Logs[IndexSelected].SetSpecial();
        }
    }

    bool _respawn = false;
    float _timer = 0;
    public void Init(LogStack prefab)
    {
        Prefab = prefab;
    }
    // Update is called once per frame
    void Update()
    {
        CurrentLogCount_Test = LogCount;
        // Revisar esto, el problema es que falta WoodsPlayer
        /*if (_respawn)
        {
            if(_timer > TimeForRespawn)
            {
                if (!WoodsPlayer.Player.Camera.LookToPosition(transform) && LogCount < MaxLogCount)
                {
                    Destroy(this.gameObject);
                    var st = Instantiate<LogStack>(Prefab, this.transform.position, this.transform.rotation);
                    st.Init(Prefab);
                }
            }
            else
            {
                _timer += Time.deltaTime;
            }
        }*/
    }

    public void RemoveLog(Log log)
    {
        Logs.Remove(log);
    }

    internal void Respawn()
    {
        _respawn = true;
    }

    public void Save(SaveDataSlot slot)
    {
        slot.LogsOutStack = new int[LogsPrefab.Length - Logs.Count];
        int j = 0;
        for(int i = 0; i < LogsPrefab.Length; i++)
        {
            if (!IsOnList(i))
            {
                slot.LogsOutStack[j] = i;
                j++;
            }
        }

        slot.SpecialIndex = IndexSelected;
    }

    bool IsOnList(int i)
    {
        foreach(var log in Logs)
        {
            if (log.Index == i)
                return true;
        }
        return false;
    }

    public void Load(SaveDataSlot slot)
    {
        if (IndexSelected >= 0 && IndexSelected < Logs.Count)
        {
            Logs[IndexSelected].NotSpecial(LogsMaterial);
        }

        IndexSelected = slot.SpecialIndex;

        SetSpecialLog();
        List<Log> toRem = new List<Log>();
        foreach(var i in slot.LogsOutStack)
        {
            toRem.Add(Logs[i]);
        }
        foreach(var l in toRem)
        {
            RemoveLog(l);
            Destroy(l.gameObject);
            RemoveLog();
        }

        

    }


}
