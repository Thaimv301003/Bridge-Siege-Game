using System.Collections;
using UnityEngine;
using BFunCoreKit;

public class GameState : MonoBehaviour
{
    private void Start()
    {
        LoadHomeScene();
    }

    public void LoadHomeScene()
    {
        StartCoroutine(LoadHomeCoroutine());
    }

    private IEnumerator LoadHomeCoroutine()
    {
        // 1. Load scene Home
        yield return LoadManager.LoadScene(BFunCoreKit.GameManager.Instance.bfunManagerData.HomeScene);

        // 2. Chuyển Canvas sang Home trong GUIManager
        yield return GUIManager.Instance.SwitchCanvas(CanvasName.CanvasHome);

        // 3. Tắt Background trong GUIManager
        yield return GUIManager.Instance.CloseBackGround();
        TheLegends.Base.Ads.AdsManager.Instance.ShowInterstitial(TheLegends.Base.Ads.AdsType.Interstitial, TheLegends.Base.Ads.PlacementOrder.One, "inter_level");
    }
}
