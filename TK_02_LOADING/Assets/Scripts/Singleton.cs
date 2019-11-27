using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField]
        public static T instance;

        public static T Instance
        {
            get { return FindInstance(); }
        }

        private static T FindInstance()
        {
            if (System.Object.ReferenceEquals(null, instance))
                instance = (T)FindObjectOfType(typeof(T));

            if (System.Object.ReferenceEquals(null, instance))
            {
                instance = new GameObject(typeof(T).ToString()).AddComponent<T>();

                if (instance.transform)
                    instance.transform.position = Vector3.zero;
            }

            return instance;
        }

        public virtual void Awake()
        {
            FindInstance();
        }
    }
}
