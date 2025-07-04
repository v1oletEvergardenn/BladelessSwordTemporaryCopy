using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternalObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    // Start is called before the first frame update
    private void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            GameObject parent = new GameObject(pool.tag + " Pool");
            parent.transform.SetParent(transform, false);
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, parent.transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    /// <summary>
    /// spawn the object in given position with given rotation.
    /// </summary>
    /// <param name="tag"> type/tag of the object you want spawn</param>
    /// <param name="position"> postion to spawn at</param>
    /// <param name="rotation"> rotation to spawn with</param>
    /// <returns>return the first object in pool</returns>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            return null;
        }
        GameObject obj = poolDictionary[tag].Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(false);
        obj.SetActive(true);

        poolDictionary[tag].Enqueue(obj);
        return obj;
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, bool randomRot = false)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            return null;
        }
        GameObject obj = poolDictionary[tag].Dequeue();
        obj.transform.position = position;
        obj.SetActive(false);
        obj.SetActive(true);
        if (randomRot)
        {
            float i = Random.Range(0f, 360f);
            obj.transform.eulerAngles = new Vector3(0, 0, i);
        }
        poolDictionary[tag].Enqueue(obj);
        return obj;
    }
}