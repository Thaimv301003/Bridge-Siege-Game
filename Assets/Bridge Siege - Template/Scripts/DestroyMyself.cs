using System.Collections;
using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class DestroyMyself : MonoBehaviour
    {
        // Time (in seconds) before the object is returned to the pool
        public float destroyTimer = 2f;

        // Tag used by the ObjectPooler to identify and recycle this object
        public string myTag;

        // ---------- Initialization ----------

        private void Start()
        {
            // Start the coroutine to return the object to the pool after a delay
            StartCoroutine(BackToQueue());
        }

        // Coroutine to handle the delayed return to the object pool
        IEnumerator BackToQueue()
        {
            // Wait for the specified amount of time
            yield return new WaitForSeconds(destroyTimer);

            // Return this object to the pool using the specified tag
            ObjectPooler.instance.BackToQueue(myTag, gameObject);
        }
    }
}
