using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameObject MobPrefab;
    public GameObject SpearPrefab;

    public TargetComponent Target;
    public static readonly System.Random RNG = new System.Random();
    static readonly Vector3 m_defaultSpawnPos = new Vector3(0, 1, 0);

    public int MobCnt = 0;
    public List<MobFightComponent> Mobs = new List<MobFightComponent>();

    void Start()
    {
        MobCnt = 0;
    }

    void Update()
    {
        UIManager.Instance.UpdateFPS(1.0f / Time.deltaTime);
    }

    public Vector3 GetSpawnPosFromStart(Vector3 startPos, int index, float scale = 1.0f)
    {
        Vector3 returnPos = startPos;

        float k = Mathf.Ceil( (Mathf.Sqrt(index) - 1.0f) / 2.0f);
        float t = 2.0f * k;
        float m = (t + 1f) * (t + 1f);
        
        if (index >= m - t)
            return new Vector3(k - (m - index), 0f, -k) * scale + startPos;
        else
            m -= t;

        if (index >= m - t)
            return new Vector3(-k, 0f, -k + (m - index)) * scale + startPos;
        else
            m -= t;

        if (index >= m - t)
            return new Vector3(-k + (m - index), 0f, k) * scale + startPos;
        else
            return new Vector3(k, 0f, k - (m - index - t)) * scale + startPos;
    }

    public void SpawnMobs(int mobCnt)
    {
        for (int i = 0; i < mobCnt; ++i)
            SpawnMob();
    }

    private void SpawnMob()
    {
        Vector3 startPos = GetSpawnPosFromStart(m_defaultSpawnPos, MobCnt, 2f);
        var mobGO = GameObject.Instantiate(GameManager.Instance.MobPrefab);
        var mob = mobGO.GetComponent<MobFightComponent>();
        mob.transform.position = startPos;
        mob.StartPos = startPos;
        mob.TargetPos = new Vector3(startPos.x, startPos.y, Target.transform.position.z);
        ++MobCnt;
        Mobs.Add(mob);

        UIManager.Instance.RefreshPoolCount();
    }

    public void DespawnMobs(int mobCnt)
    {
        for (int i = 0; i < mobCnt; ++i)
        {
            var mob = Mobs[Mobs.Count - 1];
            Mobs.RemoveAt(Mobs.Count - 1);
            Object.Destroy(mob.gameObject);
        }
    }
}
