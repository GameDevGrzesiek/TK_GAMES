using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    [Serializable]
    public class ObjectPool
    {
        public bool IsExpandable = true;
        public MonoBehaviour PoolTemplate;
        public int PoolCount = 0;

        private List<MonoBehaviour> m_pool = new List<MonoBehaviour>();
        private List<bool> m_used = new List<bool>();

        public void Init()
        {
            AddInstances(PoolCount);
        }

        public void Expand(int i_cnt)
        {
            PoolCount += i_cnt;

            int changeAmount = i_cnt;

            if (PoolCount < 0)
            {
                changeAmount = Math.Abs(PoolCount);
                PoolCount = 0;
            }

            if (i_cnt > 0)
                AddInstances(changeAmount);
            else
                RemoveInstances(changeAmount);
        }

        private void AddInstances(int i_cnt)
        {
            for (int i = 0; i < i_cnt; ++i)
            {
                var pooledObj = Instantiate(PoolTemplate, PoolManager.Instance.gameObject.transform);
                pooledObj.gameObject.SetActive(false);
                m_pool.Add(pooledObj);
                m_used.Add(false);
            }

            UIManager.Instance.RefreshPoolCount();
        }

        private void RemoveInstances(int i_cnt)
        {
            int itemCnt = Math.Abs(i_cnt);

            for (int i = 0; i < itemCnt; ++i)
            {
                m_pool[m_pool.Count - 1].StopAllCoroutines();
                m_pool[m_pool.Count - 1].gameObject.SetActive(false);
                GameObject.Destroy(m_pool[m_pool.Count - 1].gameObject);
                m_used.RemoveAt(m_pool.Count - 1);
                m_pool.RemoveAt(m_pool.Count - 1);
            }

            UIManager.Instance.RefreshPoolCount();
        }

        public MonoBehaviour SpawnObject(Vector3 i_position, Quaternion i_rotation, Transform parent = null)
        {
            int index = m_used.IndexOf(false);
            if (index >= 0 && index < m_pool.Count)
            {
                m_used[index] = true;
                var objToReturn = m_pool[index];

                if (parent != null)
                    objToReturn.transform.SetParent(parent);

                objToReturn.gameObject.SetActive(true);
                objToReturn.transform.position = i_position;
                objToReturn.transform.rotation = i_rotation;
                
                return objToReturn;
            }
            else
            {
                if (IsExpandable)
                {
                    Expand(1);
                    return SpawnObject(i_position, i_rotation, parent);
                }
            }

            return null;
        }

        public void ReturnToPool(MonoBehaviour i_obj)
        {
            i_obj.StopAllCoroutines();

            if (i_obj.transform.parent != PoolManager.Instance.gameObject.transform)
                i_obj.transform.SetParent(PoolManager.Instance.gameObject.transform);

            i_obj.gameObject.SetActive(false);
            int index = m_pool.IndexOf(i_obj);
            if (index >= 0 && index < m_used.Count)
            {
                m_used[index] = false;
            }
            else
            {
                Debug.LogWarning("Returning to the wrong pool!");
            }
        }
    }

    public ObjectPool MobPool;
    public ObjectPool SpearPool;

    public void Start()
    {
        MobPool.Init();
        SpearPool.Init();
    }
}