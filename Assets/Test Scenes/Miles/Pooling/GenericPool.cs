using UnityEngine;

public class GenericPool : MonoBehaviour {

    public GameObject m_ObjectToPool;

    public int m_MaxPoolSize = 20;
    public float m_ObjectLife = 5.0f;

    private PoolObject[] m_Pool;

    private int m_CurrentLiveObjects = 0;
    private PoolObject m_HelperPointer;

    public string m_ObjectNames = "Object";

    public Transform m_Parent;

    public bool m_RenameSelf = true;

    #region Pool Class
        private class PoolObject
        {
            public GameObject m_Object;
            public float m_LifeTimer;

            public PoolObject(GameObject gameObject)
            {
                m_Object = gameObject;
            }
        }
    #endregion

    void Start()
    {
        m_Pool = new PoolObject[m_MaxPoolSize];

        if (m_Parent == null) m_Parent = transform;

        for (int i = 0; i < m_MaxPoolSize; i++)
        {
            GameObject temp = Instantiate<GameObject>(m_ObjectToPool, m_Parent);
            temp.name = m_ObjectNames + " : " + i;
            temp.SetActive(false);
            m_Pool[i] = new PoolObject(temp);
        }

        if (m_RenameSelf) name = "Container of " + m_ObjectNames;

        for(int i = 0; i < 5; i++)
        {
            SpawnObject(Vector3.zero);
        }
    }

    void Update()
    {
        for (int i = 0; i < m_CurrentLiveObjects; i++)
        {
            if (m_Pool[i].m_LifeTimer < Time.time || !m_Pool[i].m_Object.activeSelf)
            {
                DeactivateObject(i);
            }
        }
    }



    public void SpawnObject(Vector3 position)
    {
        if (m_CurrentLiveObjects == m_MaxPoolSize)
        {
            //Recycle first in list, might not be youngest
            //ActivateObject(0);

            //or

            //Debug and not add
            Debug.Log("No free objects to use");
        }
        else
        {
            ActivateObject(m_CurrentLiveObjects++);
        }
    }

    private void ActivateObject(int index)
    {
        m_Pool[index].m_LifeTimer = Time.time + m_ObjectLife;
        m_Pool[index].m_Object.SetActive(true);
    }

    private void DeactivateObject(int index)
    {
        m_CurrentLiveObjects--;
        m_Pool[index].m_Object.SetActive(false);
        m_HelperPointer = m_Pool[index];
        m_Pool[index] = m_Pool[m_CurrentLiveObjects];
        m_Pool[m_CurrentLiveObjects] = m_HelperPointer;
    }
}
