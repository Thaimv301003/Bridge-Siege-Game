using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    // Class representing a pool configuration
    [System.Serializable]
    public class Pools
    {
        public string tag;        // Tag used to identify the pool
        public GameObject prefab; // Prefab of the object to pool
        public int size;         // Number of objects in the pool
    }

    public class ObjectPooler : MonoBehaviour
    {
        #region Singleton

        public static ObjectPooler instance; // Singleton instance

        private void Awake()
        {
            instance = this; // Set the singleton instance to this object
        }

        #endregion

        [Header("Object Pool Configurations")]
        public List<Pools> pools; // List of pool configurations

        // Dictionary to hold the pooled objects categorized by tag
        public Dictionary<string, Queue<GameObject>> poolsDict;

        private void Start()
        {
            // Initialize the dictionary to hold the pools
            poolsDict = new Dictionary<string, Queue<GameObject>>();
            
            // Loop through each pool configuration
            foreach (Pools P in pools)
            {
                Queue<GameObject> objects = new Queue<GameObject>();

                // Instantiate the specified number of objects and add them to the queue
                for (int i = 0; i < P.size; i++)
                {
                    GameObject obj = Instantiate(P.prefab); // Instantiate the prefab
                    obj.SetActive(false); // Set the object to inactive
                    objects.Enqueue(obj); // Add the object to the queue
                }

                poolsDict.Add(P.tag, objects); // Add the queue to the dictionary with its tag
            }
        }

        // Method to spawn an object from the pool
        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            // Dequeue an object from the specified pool
            GameObject obj = poolsDict[tag].Dequeue();

            obj.SetActive(true); // Activate the object
            obj.transform.position = position; // Set its position
            obj.transform.rotation = rotation; // Set its rotation
            poolsDict[tag].Enqueue(obj); // Re-enqueue the object for future use

            return obj; // Return the activated object
        }

        // Method to return an object back to the pool
        public void BackToQueue(string tag, GameObject obj)
        {
            obj.SetActive(false); // Deactivate the object
        }
    }
}
