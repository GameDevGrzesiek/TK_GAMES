using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AsyncOpSamples : MonoBehaviour
{
    [Serializable]
    public class SaveData
    {
        public List<int> Numbers = new List<int>();
    }

    private bool m_processCompleted = true;
    private bool m_loadCompleted = true;
    private string m_savePath;
    private SaveData LocalData = new SaveData();

    public Button StandardBtn;
    public Button CoroutineBtn;
    public Button AsyncBtn;
    public Button ThreadBtn;

    void Start()
    {
        m_savePath = Application.streamingAssetsPath + "/save.dat";

        StandardBtn.onClick.AddListener(delegate { OnStandardTest(); });
        CoroutineBtn.onClick.AddListener(delegate { OnCoroutineTest(); });
        AsyncBtn.onClick.AddListener(delegate { OnAsyncTest(); });
        ThreadBtn.onClick.AddListener(delegate { OnThreadTest(); });
    }

    void OnStandardTest()
    {
        StandardLoadFile();
        StandardProcess();
        LogProcessing();
    }

    void OnCoroutineTest()
    {
        m_loadCompleted = false;
        m_processCompleted = false;

        StartCoroutine(CoroutineLoadFile());
        StartCoroutine(CoroutineProcess());
        StartCoroutine(CoroutineLogProcessing());
    }

    async void OnAsyncTest()
    {
        try
        {
            LocalData = await AsyncLoadData();
            LocalData.Numbers = await AsyncProcessData();
            await AsyncLogProcessing();
        }
        catch
        {
            Debug.Log("Error occured");
        }
    }

    void OnThreadTest()
    {
        StandardLoadFile();
        ProcessJob processJob = new ProcessJob();
        processJob.inputList = LocalData.Numbers;
        processJob.Start();
        StartCoroutine(ThreadedLogProcessing(processJob));
    }

    private void LogProcessing()
    {
        //for (int i = 0; i < LocalData.Numbers.Count; ++i)
        for (int i = 0; i < 1000; ++i)
            Debug.Log(LocalData.Numbers[i]);
    }

    public void StandardSaveFile()
    {
        Debug.Log("Saving");
        FileStream file;

        if (File.Exists(m_savePath)) file = File.OpenWrite(m_savePath);
        else file = File.Create(m_savePath);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, LocalData);
        file.Close();

        Debug.Log("Save Completed");
    }

    public void StandardLoadFile()
    {
        Debug.Log("Loading File");

        float startLoading = Time.realtimeSinceStartup;

        FileStream file;

        if (File.Exists(m_savePath)) file = File.OpenRead(m_savePath);
        else
        {
            Debug.LogError("File not found");
            return;
        }

        BinaryFormatter bf = new BinaryFormatter();
        LocalData = (SaveData)bf.Deserialize(file);
        file.Close();
        m_loadCompleted = true;

        float endLoading = Time.realtimeSinceStartup;
    }

    private void StandardProcess()
    {
        List<int> processList = new List<int>();

        for (int j = 0; j < 100; ++j)
        {
            for (int i = 0; i < LocalData.Numbers.Count; ++i)
                processList.Add(LocalData.Numbers[i] + LocalData.Numbers[LocalData.Numbers.Count - 1 - i]);
        }

        LocalData.Numbers = processList;
    }

    IEnumerator CoroutineLoadFile()
    {
        Debug.Log("Loading File");

        var fileReader = new WWW("file://" + m_savePath);
        yield return fileReader;

        Stream fileStream = new MemoryStream(fileReader.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        LocalData = (SaveData)bf.Deserialize(fileStream);

        m_loadCompleted = true;

        Debug.Log("Data Loaded");
    }

    IEnumerator CoroutineProcess()
    {
        m_processCompleted = false;

        yield return new WaitUntil(() => m_loadCompleted == true);

        List<int> processList = new List<int>();
        float chunkStartTime = Time.realtimeSinceStartup;

        for (int j = 0; j < 100; ++j)
        {
            for (int i = 0; i < LocalData.Numbers.Count; ++i)
            {
                processList.Add(LocalData.Numbers[i] + LocalData.Numbers[LocalData.Numbers.Count - 1 - i]);

                if (Time.realtimeSinceStartup - chunkStartTime > 0.033f)
                {
                    yield return null;
                    chunkStartTime = Time.realtimeSinceStartup;
                }
            }
        }

        LocalData.Numbers = processList;

        m_processCompleted = true;
    }

    IEnumerator CoroutineLogProcessing()
    {
        yield return new WaitUntil(() => m_processCompleted == true);
        LogProcessing();
    }

    IEnumerator ThreadedLogProcessing(ProcessJob job)
    {
        yield return StartCoroutine(job.WaitFor());
        LocalData.Numbers = job.inputList;
        LogProcessing();
    }

    async Task<SaveData> AsyncLoadData()
    {
        var fileReader = new WWW("file://" + m_savePath);
        if (!string.IsNullOrEmpty(fileReader.error))
            throw new Exception();

        Stream fileStream = new MemoryStream(fileReader.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        SaveData data = (SaveData)bf.Deserialize(fileStream);
        return await Task.FromResult(data);
    }

    async Task<List<int>> AsyncProcessData()
    {
        List<int> processList = new List<int>();

        for (int j = 0; j < 100; ++j)
            for (int i = 0; i < LocalData.Numbers.Count; ++i)
                processList.Add(LocalData.Numbers[i] + LocalData.Numbers[LocalData.Numbers.Count - 1 - i]);

        return await Task.FromResult(processList);
    }

    async Task AsyncLogProcessing()
    {
        LogProcessing();
        await Task.FromResult(0);
    }
}
