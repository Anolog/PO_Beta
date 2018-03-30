using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{

    public List<GameObject> SpawnPoints;
    /// <summary>
    /// Key is Item, Value is Spawn Point
    /// </summary>
    Dictionary<GameObject, GameObject> m_OccupiedSpawnPoints;
    GameObject m_LastSpawnPoint;

    public float SpawnInterval = 20;
    public float SpawnIntervalVariance = 5;

    private float m_AmountOfPlayers;

    float m_NextSpawnTime;

    public List<GameObject> Items;

    // Use this for initialization
    void Start()
    {
        m_AmountOfPlayers = this.transform.GetComponent<Game>().GetAmountOfPlayersInGame();

        if (m_AmountOfPlayers == 1)
        {
            SpawnInterval = 24;
        }

        else if (m_AmountOfPlayers == 2)
        {
            SpawnInterval = 20;
        }

        else if (m_AmountOfPlayers == 3)
        {
            SpawnInterval = 16;
        }

        else if (m_AmountOfPlayers == 4)
        {
            SpawnInterval = 12;
        }

        m_OccupiedSpawnPoints = new Dictionary<GameObject, GameObject>();
        m_NextSpawnTime = Time.time + SpawnInterval + Random.Range(0, SpawnIntervalVariance);
        

        for(int i = 0; i < Items.Count; i++)
        {
            ItemPickup itemScript = Items[i].GetComponent<ItemPickup>();

            if (itemScript != null)
            {
                itemScript.ItemSpawner = this;
            }
            else
            {
                Debug.Log(Items[i].ToString() + "doesn't have a Item Pickup attached, it has been removed from items");
                Items.RemoveAt(i);
                i--;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Time.time > m_NextSpawnTime)
        {
            m_NextSpawnTime += SpawnInterval + Random.Range(0, SpawnIntervalVariance);

            if (Items.Count == 0)
            {
                Debug.Log("No items have been added to Item Manager");
                return;
            }
            //either no spawn points are assigned, or they are all occupied with items
            if (SpawnPoints.Count == 0)
                return;

            //don't spawn in the same spot twice in a row if there are any spawn points available
            bool lastSpawnRemoved = false;

            if (m_LastSpawnPoint != null && SpawnPoints.Count > 1)
            {
                lastSpawnRemoved = SpawnPoints.Remove(m_LastSpawnPoint);
            }

            int itemIndex = Random.Range(0, Items.Count);
            GameObject obj = Items[itemIndex];

            int posIndex = Random.Range(0, SpawnPoints.Count);
            GameObject pos = SpawnPoints[posIndex];

            //if we took the last spawn point out of the list while picking, put it back
            if (lastSpawnRemoved)
                SpawnPoints.Add(m_LastSpawnPoint);

            m_LastSpawnPoint = pos;

            GameObject item = Instantiate(obj, pos.transform.position, pos.transform.rotation);

            m_OccupiedSpawnPoints.Add(item, pos);
            SpawnPoints.RemoveAt(posIndex);
        }
    }

    public void ItemPickedUp(GameObject item)
    {
        

        SpawnPoints.Add(m_OccupiedSpawnPoints[item]);
        m_OccupiedSpawnPoints.Remove(item);
    }
}

