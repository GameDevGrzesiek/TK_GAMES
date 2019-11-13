using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T m_instance;

    public static T Instance
    {
        get { return ReturnInstance(); }
    }

    private static T ReturnInstance()
    {
        if (m_instance == null)
        {
            m_instance = (T)FindObjectOfType(typeof(T));
            if (m_instance == null)
            {
                m_instance = new GameObject(typeof(T).ToString()).AddComponent<T>();

                if (m_instance.transform)
                    m_instance.transform.position = Vector3.zero;
            }
        }

        return m_instance;
    }


    // niekonieczne, ale przydatne jeżeli chcemy 
    // automatycznej inicjalizacji na początku sceny
    protected void Awake()
    {
        ReturnInstance();
        DontDestroyOnLoad(this);
    }
}