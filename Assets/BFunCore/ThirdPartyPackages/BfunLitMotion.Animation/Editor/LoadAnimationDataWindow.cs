#if BFUN_INSTALLED_TRUE
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Bfun.LitMotion.Animation.Editor
{
    public class LoadAnimationDataWindow : EditorWindow
    {
        private List<LitMotionAnimationData> dataAssets;
        private Action<LitMotionAnimationData> onDataSelected;
        private Vector2 scrollPosition;

        // Phương thức tĩnh để mở cửa sổ và truyền vào một hành động callback
        public static void ShowWindow(Action<LitMotionAnimationData> onSelect)
        {
            var window = GetWindow<LoadAnimationDataWindow>("Load Animation Data");
            window.onDataSelected = onSelect;
            window.minSize = new Vector2(300, 400);
        }

        private void OnEnable()
        {
            // Tìm tất cả các tài sản LitMotionAnimationData trong dự án
            dataAssets = new List<LitMotionAnimationData>();
            string[] guids = AssetDatabase.FindAssets("t:LitMotionAnimationData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<LitMotionAnimationData>(path);
                if (data != null)
                {
                    dataAssets.Add(data);
                }
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Select an Animation Data asset:", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (dataAssets.Count == 0)
            {
                EditorGUILayout.HelpBox("No LitMotionAnimationData assets found in the project.", MessageType.Info);
            }
            else
            {
                foreach (var data in dataAssets)
                {
                    if (GUILayout.Button(data.name))
                    {
                        // Khi một nút được nhấn, gọi callback và đóng cửa sổ
                        onDataSelected?.Invoke(data);
                        Close();
                    }
                }
            }
            
            EditorGUILayout.EndScrollView();
        }
    }
}
#endif