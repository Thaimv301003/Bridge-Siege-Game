#if BFUN_INSTALLED_TRUE
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bfun.LitMotion.Animation.Editor
{
    [CustomEditor(typeof(LitMotionAnimation))]
    public sealed class LitMotionAnimationEditor : UnityEditor.Editor
    {
        SerializedProperty componentsProperty;
        // --- THÊM MỚI: Property cho Play On Enable ---
        SerializedProperty playOnEnableProperty;
        // --------------------------------------------

        int prevArraySize;
        AddAnimationComponentDropdown dropdown;
        VisualElement componentRoot;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            componentRoot = new VisualElement();
            componentsProperty = serializedObject.FindProperty("components");
            // --- THÊM MỚI: Lấy property ---
            playOnEnableProperty = serializedObject.FindProperty("playOnEnable");
            // -----------------------------

            prevArraySize = componentsProperty.arraySize;

            dropdown = new AddAnimationComponentDropdown(new());
            dropdown.OnTypeSelected += type =>
            {
                var last = componentsProperty.arraySize;
                componentsProperty.InsertArrayElementAtIndex(componentsProperty.arraySize);
                var property = componentsProperty.GetArrayElementAtIndex(last);
                property.managedReferenceValue = ReflectionHelper.CreateDefaultInstance(type);
                serializedObject.ApplyModifiedProperties();
            };

            root.Add(CreateDataManagementPanel());

            // --- THÊM MỚI: Vẽ Panel Settings ---
            root.Add(CreateSettingsPanel());
            // ----------------------------------

            componentRoot.Add(CreateComponentsPanel());
            root.Add(componentRoot);
            root.Add(CreateDebugPanel());
            return root;
        }

        // --- THÊM MỚI: Hàm tạo Settings Panel ---
        VisualElement CreateSettingsPanel()
        {
            var box = CreateBox("Settings");
            var toggle = new PropertyField(playOnEnableProperty, "Play On Enable");
            box.Add(toggle);
            return box;
        }
        // ----------------------------------------

        VisualElement CreateComponentsPanel()
        {
            var box = CreateBox("Components");
            var views = new List<AnimationComponentView>();

            if (componentsProperty != null && componentsProperty.isArray)
            {
                for (int i = 0; i < componentsProperty.arraySize; i++)
                {
                    int index = i; // Lưu lại index cho Drag & Drop
                    var property = componentsProperty.GetArrayElementAtIndex(index);
                    if (property == null) continue;

                    var view = CreateComponentGUI(property.Copy());

                    // --- BẮT ĐẦU: Logic tạo Handle Kéo Thả ---
                    var itemContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 4, alignItems = Align.FlexStart } };

                    var handleContainer = new VisualElement { style = { width = 20, paddingTop = 4 }, tooltip = "Nắm vào đây để kéo thả đổi vị trí" };
                    var dragIcon = new Label("≡") { style = { fontSize = 16, color = Color.gray, unityTextAlign = TextAnchor.MiddleCenter } };
                    handleContainer.Add(dragIcon);

                    view.style.flexGrow = 1;
                    itemContainer.Add(handleContainer);
                    itemContainer.Add(view);

                    // 1. Khi nhấn chuột vào icon ≡ -> Bắt đầu kéo
                    handleContainer.RegisterCallback<PointerDownEvent>(evt => {
                        if (evt.button != 0) return; // Chỉ nhận chuột trái
                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.SetGenericData("LitMotionDragIndex", index);
                        DragAndDrop.paths = null;
                        DragAndDrop.objectReferences = new UnityEngine.Object[0];
                        DragAndDrop.StartDrag("Move Component");
                        evt.StopPropagation();
                    });

                    // 2. Khi lướt qua vùng của component khác -> Hiển thị highlight báo hiệu Drop
                    itemContainer.RegisterCallback<DragUpdatedEvent>(evt => {
                        var dragData = DragAndDrop.GetGenericData("LitMotionDragIndex");
                        if (dragData != null && (int)dragData != index)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                            itemContainer.style.backgroundColor = new Color(0.3f, 0.5f, 0.8f, 0.3f); // Đổi màu nền xanh nhạt
                        }
                    });

                    // 3. Khi chuột rời đi -> Xóa highlight
                    itemContainer.RegisterCallback<DragLeaveEvent>(evt => {
                        itemContainer.style.backgroundColor = StyleKeyword.Null;
                    });

                    // 4. Khi thả chuột -> Thực hiện đổi vị trí
                    itemContainer.RegisterCallback<DragPerformEvent>(evt => {
                        var dragData = DragAndDrop.GetGenericData("LitMotionDragIndex");
                        if (dragData != null)
                        {
                            int fromIndex = (int)dragData;
                            int toIndex = index;
                            if (fromIndex != toIndex)
                            {
                                DragAndDrop.AcceptDrag();
                                Undo.RecordObject(serializedObject.targetObject, "Reorder Component");
                                componentsProperty.MoveArrayElement(fromIndex, toIndex);
                                serializedObject.ApplyModifiedProperties();
                                RefleshComponentsView(true); // Cập nhật lại giao diện
                            }
                        }
                        itemContainer.style.backgroundColor = StyleKeyword.Null;
                    });
                    // --- KẾT THÚC: Logic Kéo Thả ---

                    // Gán logic cũ của bạn
                    CreateContextMenuManipulator(componentsProperty, index, false).target = view.Foldout.Q<Toggle>();
                    CreateContextMenuManipulator(componentsProperty, index, true).target = view.ContextMenuButton;
                    var enabledProperty = property.FindPropertyRelative("enabled");
                    if (enabledProperty != null) view.EnabledToggle.BindProperty(enabledProperty);

                    box.Add(itemContainer); // Add container chứa cả ≡ và view
                    views.Add(view);
                }
            }

            var addButton = new Button() { text = "Add...", style = { width = 200f, alignSelf = Align.Center } };
            addButton.clicked += () => dropdown.Show(addButton.worldBound);
            box.Add(addButton);

            box.schedule.Execute(() => { var enabled = IsActive(); foreach (var view in views) view.SetEnabled(enabled); addButton.SetEnabled(enabled); }).Every(100);
            box.schedule.Execute(() =>
            {
                if (componentsProperty == null) componentsProperty = serializedObject.FindProperty("components");
                if (componentsProperty != null && componentsProperty.isArray && componentsProperty.arraySize != prevArraySize) RefleshComponentsView(true);
                var components = ((LitMotionAnimation)target).Components;
                if (components == null) return;
                for (int i = 0; i < views.Count; i++) { if (components.Count <= i || components[i] == null) { views[i].Progress = 0f; continue; } var component = components[i]; var handle = component.TrackedHandle; if (handle.IsActive() && !double.IsInfinity(handle.TotalDuration)) views[i].Progress = Mathf.InverseLerp(0f, (float)handle.TotalDuration, (float)handle.Time); else views[i].Progress = 0f; }
            }).Every(20);

            return box;
        }

        // --- HÀM HELPER: Lấy danh sách Sound ---
        private List<string> GetSoundNamesFromProject()
        {
            var names = new List<string>();
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == "SoundName");

            if (type != null && type.IsEnum)
            {
                names.AddRange(Enum.GetNames(type));
            }

            if (names.Count == 0) names.Add("None");
            return names;
        }

        // --- HÀM VẼ COMPONENT ---
        AnimationComponentView CreateComponentGUI(SerializedProperty property)
        {
            var view = new AnimationComponentView();
            if (property == null) return view;

            if (string.IsNullOrEmpty(property.managedReferenceFullTypename))
            {
                view.Text = "(Missing)";
                view.Icon = (Texture2D)EditorGUIUtility.IconContent("Error").image;
                view.EnabledToggle.value = true;
                view.SetEnabled(true);
                view.EnabledToggle.Q("unity-checkmark").style.visibility = Visibility.Hidden;
                view.Add(new HelpBox("Missing Script Reference", HelpBoxMessageType.Error));
            }
            else
            {
                var displayNameProp = property.FindPropertyRelative("displayName");
                view.Text = displayNameProp != null ? displayNameProp.stringValue : "Component";
                var targetProperty = property.FindPropertyRelative("target");
                if (targetProperty != null) view.Icon = GUIHelper.GetComponentIcon(targetProperty.GetPropertyType());
                if (displayNameProp != null) view.TrackPropertyValue(displayNameProp, x => { view.Text = x.stringValue; });

                view.Foldout.BindProperty(property);

                var iterProp = property.Copy();
                var endProp = iterProp.GetEndProperty();
                iterProp.NextVisible(true);

                do
                {
                    if (SerializedProperty.EqualContents(iterProp, endProp)) break;
                    if (iterProp.name == "enabled" || iterProp.name == "displayName") continue;

                    // --- FIX LỖI Ở ĐÂY ---
                    if (iterProp.name == "soundName")
                    {
                        var choices = GetSoundNamesFromProject();

                        // QUAN TRỌNG: Lưu lại đường dẫn chính xác của thuộc tính này
                        string pathToProperty = iterProp.propertyPath;
                        var so = iterProp.serializedObject;

                        // Kiểm tra nếu chưa có giá trị thì gán mặc định là phần tử đầu
                        if (string.IsNullOrEmpty(iterProp.stringValue) && choices.Count > 0)
                        {
                            iterProp.stringValue = choices[0];
                            so.ApplyModifiedProperties();
                        }

                        var popup = new PopupField<string>("Sound Name", choices, iterProp.stringValue ?? choices[0]);
                        popup.style.marginTop = 2;
                        popup.style.marginBottom = 2;

                        popup.RegisterValueChangedCallback(evt => {
                            // Tìm lại đúng thuộc tính thông qua đường dẫn đã lưu
                            var targetProp = so.FindProperty(pathToProperty);
                            if (targetProp != null)
                            {
                                targetProp.stringValue = evt.newValue;
                                so.ApplyModifiedProperties();
                            }
                        });

                        // Cập nhật UI khi property thay đổi (Undo/Redo)
                        view.TrackPropertyValue(iterProp, p => {
                            popup.value = p.stringValue;
                        });

                        view.Add(popup);
                        continue;
                    }
                    // ---------------------

                    if (iterProp.name == "joinType" || iterProp.name == "joinDelay")
                    {
                        view.Add(new PropertyField(iterProp));
                        continue;
                    }

                    if (iterProp.name == "settings" && property.managedReferenceFullTypename.Contains("Slide"))
                    {
                        var settingsFoldout = new Foldout() { text = "Settings" };
                        settingsFoldout.BindProperty(iterProp);
                        var settingChild = iterProp.Copy();
                        var settingEnd = settingChild.GetEndProperty();
                        if (settingChild.NextVisible(true))
                        {
                            do
                            {
                                if (SerializedProperty.EqualContents(settingChild, settingEnd)) break;
                                if (settingChild.name.Equals("startValue", StringComparison.OrdinalIgnoreCase) || settingChild.name.Equals("endValue", StringComparison.OrdinalIgnoreCase)) continue;
                                settingsFoldout.Add(new PropertyField(settingChild));
                            } while (settingChild.NextVisible(false));
                        }
                        view.Add(settingsFoldout);
                    }
                    else
                    {
                        view.Add(new PropertyField(iterProp));
                    }
                } while (iterProp.NextVisible(false));
            }
            return view;
        }

        void OnEnable() { EditorApplication.playModeStateChanged += OnPlayModeStateChanged; EditorApplication.update += OnEditorUpdate; }
        void OnDisable() { if (!EditorApplication.isPlayingOrWillChangePlaymode && target != null) ((LitMotionAnimation)target).Stop(); EditorApplication.playModeStateChanged -= OnPlayModeStateChanged; EditorApplication.update -= OnEditorUpdate; }
        bool wasPlaying = false;
        void OnEditorUpdate()
        {
            if (Application.isPlaying) return;
            var animTarget = (LitMotionAnimation)target;
            if (animTarget == null) return;
            bool isPlaying = animTarget.IsPlaying;
            if (isPlaying || (wasPlaying && !isPlaying)) { EditorApplication.QueuePlayerLoopUpdate(); UnityEngine.Canvas.ForceUpdateCanvases(); UnityEditorInternal.InternalEditorUtility.RepaintAllViews(); }
            wasPlaying = isPlaying;
        }
        void OnPlayModeStateChanged(PlayModeStateChange state) { if (state == PlayModeStateChange.ExitingEditMode) ((LitMotionAnimation)target).Stop(); }
        VisualElement CreateBox(string label) { var box = new Box { style = { marginTop = 6f, marginBottom = 2f, paddingLeft = 4f, alignItems = Align.Stretch, flexDirection = FlexDirection.Column, flexGrow = 1f, } }; box.Add(new Label(label) { style = { marginTop = 5f, marginBottom = 3f, unityFontStyleAndWeight = FontStyle.Bold } }); return box; }
        VisualElement CreateDataManagementPanel() { var animTarget = (LitMotionAnimation)target; var box = CreateBox("Data Management"); var buttonGroup = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 4f, marginBottom = 4f } }; var saveAsButton = new Button(SaveNewDataAsset) { text = "Save As New...", style = { flexGrow = 1f } }; var loadButton = new Button(() => { LoadAnimationDataWindow.ShowWindow(selectedData => { if (selectedData == null) return; LoadDataToComponent(animTarget, selectedData); }); }) { text = "Load from Project...", style = { flexGrow = 1f } }; buttonGroup.Add(saveAsButton); buttonGroup.Add(loadButton); box.Add(buttonGroup); return box; }
        void LoadDataToComponent(LitMotionAnimation animTarget, LitMotionAnimationData sourceData) { Undo.RecordObject(animTarget, $"Load Animation Data ({sourceData.name})"); var newComponentsList = new List<LitMotionAnimationComponent>(); if (sourceData.components != null) { foreach (var sourceComp in sourceData.components) { if (sourceComp == null) continue; var json = JsonUtility.ToJson(sourceComp); var newInstance = (LitMotionAnimationComponent)Activator.CreateInstance(sourceComp.GetType()); JsonUtility.FromJsonOverwrite(json, newInstance); newComponentsList.Add(newInstance); } } animTarget.SetComponents(newComponentsList.ToArray()); EditorUtility.SetDirty(animTarget); serializedObject.Update(); RefleshComponentsView(false); Debug.Log($"Loaded animation data from: {sourceData.name}", sourceData); }
        void SaveNewDataAsset() { var animTarget = (LitMotionAnimation)target; if (animTarget.Components == null || animTarget.Components.Count == 0) { EditorUtility.DisplayDialog("Save Error", "Cannot save. The component list is empty.", "OK"); return; } string defaultPath = "Assets/Resources/LitmotionData"; ; if (!AssetDatabase.IsValidFolder(defaultPath)) AssetDatabase.CreateFolder("Assets/Resources", "LitmotionData"); string path = EditorUtility.SaveFilePanelInProject("Save New Animation Data", $"{animTarget.gameObject.name}LitmotionData", "asset", "Choose location to save the animation data.", defaultPath); if (string.IsNullOrEmpty(path)) return; var newData = CreateInstance<LitMotionAnimationData>(); var currentComponents = animTarget.Components; var compList = new List<LitMotionAnimationComponent>(); foreach (var comp in currentComponents) { if (comp == null) continue; var json = JsonUtility.ToJson(comp); var newInstance = (LitMotionAnimationComponent)Activator.CreateInstance(comp.GetType()); JsonUtility.FromJsonOverwrite(json, newInstance); compList.Add(newInstance); } newData.components = compList.ToArray(); AssetDatabase.CreateAsset(newData, path); AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); EditorUtility.FocusProjectWindow(); EditorGUIUtility.PingObject(newData); Debug.Log($"Animation Data saved to: {path}", newData); }
        VisualElement CreateDebugPanel() { var box = CreateBox("Debug"); var buttonGroup = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1f, } }; var playButton = new Button(() => ((LitMotionAnimation)target).Play()) { text = "Play", style = { flexGrow = 1f } }; var restartButton = new Button(() => ((LitMotionAnimation)target).Restart()) { text = "Restart", style = { flexGrow = 1f } }; var pauseButton = new Button(() => ((LitMotionAnimation)target).Pause()) { text = "Pause", style = { flexGrow = 1f } }; var stopButton = new Button(() => ((LitMotionAnimation)target).Stop()) { text = "Stop", style = { flexGrow = 1f } }; buttonGroup.Add(playButton); buttonGroup.Add(restartButton); buttonGroup.Add(pauseButton); buttonGroup.Add(stopButton); buttonGroup.schedule.Execute(() => { bool active = ((LitMotionAnimation)target).IsActive; playButton.SetEnabled(!active); restartButton.SetEnabled(active); pauseButton.SetEnabled(active); stopButton.SetEnabled(active); }).Every(100); box.Add(buttonGroup); return box; }
        void RefleshComponentsView(bool applyModifiedProperties) { if (applyModifiedProperties) serializedObject.ApplyModifiedProperties(); componentsProperty = serializedObject.FindProperty("components"); if (componentsProperty != null) prevArraySize = componentsProperty.arraySize; componentRoot.Clear(); componentRoot.Add(CreateComponentsPanel()); }
        ContextualMenuManipulator CreateContextMenuManipulator(SerializedProperty property, int arrayIndex, bool activeLeftClick) { var manipulator = new ContextualMenuManipulator(evt => { if (property == null || arrayIndex >= property.arraySize) return; evt.menu.AppendAction("Reset", x => { var elementProperty = property.GetArrayElementAtIndex(arrayIndex); if (elementProperty != null) { Undo.RecordObject(serializedObject.targetObject, "Reset LitMotionAnimation component"); elementProperty.managedReferenceValue = ReflectionHelper.CreateDefaultInstance(elementProperty.managedReferenceValue.GetType()); RefleshComponentsView(true); } }, (property.GetArrayElementAtIndex(arrayIndex) == null || string.IsNullOrEmpty(property.GetArrayElementAtIndex(arrayIndex).managedReferenceFullTypename)) ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal); evt.menu.AppendSeparator(); evt.menu.AppendAction("Remove Component", x => { Undo.RecordObject(serializedObject.targetObject, "Remove Component"); property.DeleteArrayElementAtIndex(arrayIndex); RefleshComponentsView(true); }); evt.menu.AppendAction("Move Up", x => { Undo.RecordObject(serializedObject.targetObject, "Move Component Up"); property.MoveArrayElement(arrayIndex, arrayIndex - 1); RefleshComponentsView(true); }, arrayIndex == 0 ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal); evt.menu.AppendAction("Move Down", x => { Undo.RecordObject(serializedObject.targetObject, "Move Component Down"); property.MoveArrayElement(arrayIndex, arrayIndex + 1); RefleshComponentsView(true); }, arrayIndex == property.arraySize - 1 ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal); }); if (activeLeftClick) manipulator.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, }); return manipulator; }
        bool IsActive() { var targetComponent = (LitMotionAnimation)target; return targetComponent != null && !targetComponent.IsActive; }
    }
}
#endif