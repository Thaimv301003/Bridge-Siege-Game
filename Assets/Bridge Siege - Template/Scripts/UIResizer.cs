using IndianOceanAssets.BridgeSiege;
using UnityEngine;

public class UIResizer : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.instance.gamePlatform == GameManager.GamePlatform.PC)
            GetComponent<Transform>().localScale = new Vector3(.5f, .5f, .5f);
    }
}