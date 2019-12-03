using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadedJob
{
    private bool m_IsDone = false;
    private object m_Handle = new object();
    private System.Threading.Thread m_Thread = null;
    public bool IsDone
    {
        get
        {
            bool tmp;
            lock (m_Handle)
            {
                tmp = m_IsDone;
            }
            return tmp;
        }
        set
        {
            lock (m_Handle)
            {
                m_IsDone = value;
            }
        }
    }

    public virtual void Start()
    {
        m_Thread = new System.Threading.Thread(Run);
        m_Thread.Start();
    }
    public virtual void Abort()
    {
        m_Thread.Abort();
    }

    protected virtual void ThreadFunction() { }

    protected virtual void OnFinished() { }

    public virtual bool Update()
    {
        if (IsDone)
        {
            OnFinished();
            return true;
        }
        return false;
    }
    public IEnumerator WaitFor()
    {
        while (!Update())
        {
            yield return null;
        }
    }
    private void Run()
    {
        ThreadFunction();
        IsDone = true;
    }
}

public class ProcessJob : ThreadedJob
{
    public List<int> inputList = new List<int>();
    private List<int> processList = new List<int>();

    protected override void ThreadFunction()
    {
        for (int j = 0; j < 100; ++j)
        {
            for (int i = 0; i < inputList.Count; ++i)
                processList.Add(inputList[i] + inputList[inputList.Count - 1 - i]);
        }

        inputList = processList;
    }
}

public class LoadingJob : ThreadedJob
{
    public Vector3[] InData;  // arbitary job data
    public Vector3[] OutData; // arbitary job data

    protected override void ThreadFunction()
    {
        // Do your threaded task. DON'T use the Unity API here
        for (int i = 0; i < 100000000; i++)
        {
            OutData[i % OutData.Length] += InData[(i + 1) % InData.Length];
        }
    }
}