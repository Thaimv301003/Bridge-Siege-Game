using System;
using BFunCoreKit;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BFunCoreKit
{
    public class GameManager : Singleton<GameManager>
    {
        public BFunManagerData bfunManagerData;
        [ShowInInspector, ReadOnly] public Camera cameraMain;

        public const string BFUN_VERSION = "0.0.7";
        public Action OnInitAplication;

        public static bool IsInLoadingScene
        {
            get { return SceneManager.GetActiveScene().name == Instance.bfunManagerData.LoadingScene; }
        }

        public static bool IsInHomeScene
        {
            get { return SceneManager.GetActiveScene().name == Instance.bfunManagerData.HomeScene; }
        }

        public static bool IsInGameScene
        {
            get { return SceneManager.GetActiveScene().name != Instance.bfunManagerData.HomeScene; }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public override void Awake()
        {
            base.Awake();
            Input.multiTouchEnabled = true;
            //bfunManagerData = AssetDatabase.LoadAssetAtPath<BFunManagerData>(GlobalConst.SettingFolder + "/Project.asset");
            DontDestroyOnLoad(transform.parent.gameObject);
        }
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            cameraMain = Camera.main;

            if (scene.name == bfunManagerData.LoadingScene)
            {
                OnInitAplication?.Invoke();
            }
        }
    }
}
