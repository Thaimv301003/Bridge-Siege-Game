using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class Rotate : MonoBehaviour
    {
        // Speed at which the object rotates (degrees per second)
        [SerializeField]
        private float rotateSpeed = 30f;

        // ---------- Update Rotation Every Frame ----------

        void Update()
        {
            // Rotate the object around its local up axis at the specified speed
            transform.Rotate(transform.up * rotateSpeed * Time.deltaTime);
        }
    }
}
