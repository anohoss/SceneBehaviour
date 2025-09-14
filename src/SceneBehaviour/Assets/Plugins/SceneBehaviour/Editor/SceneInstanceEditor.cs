using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using System.Linq;
using UnityEditor.UIElements;

namespace Anoho.SceneBehaviour.Editor
{
    [CustomEditor(typeof(SceneInstance))]
    public sealed class SceneInstanceEditor : UnityEditor.Editor
    {
        private const string PropertyFieldName_Scene = "Prop_Scene";

        private const string LabelName_AddedToSceneList = "Label_AddedToSceneList";

        private const string ButtonName_AddToSceneList = "Btn_AddToSceneList";

        private const string LabelName_BuildIndex = "Label_BuildIndex";

        private const string LabelName_AssetPath = "Label_AssetPath";

        // スクリプトのデフォルト参照で設定する
        [SerializeField]
        private VisualTreeAsset inspectorUxml;

        private string cachedInspectorUxmlPath;

        private VisualElement cachedInspector;

        // Begin unity methods

        private void OnValidate()
        {
            if (inspectorUxml == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(cachedInspectorUxmlPath))
            {
                // スクリプトのデフォルト参照はPlayInEditorではnullになるため、Uxmlアセットのパスをキャッシュする
                cachedInspectorUxmlPath = AssetDatabase.GetAssetPath(inspectorUxml);
            }
        }

        // End unity methods
        
        // Begin Editor immplementation

        public override VisualElement CreateInspectorGUI()
        {
            var inspector = new VisualElement();

            if (inspectorUxml == null)
            {
                if (!string.IsNullOrEmpty(cachedInspectorUxmlPath))
                {
                    inspectorUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(cachedInspectorUxmlPath);

                    if (inspectorUxml == null)
                    {
                        Debug.LogError($"Failed to load inspector UXML from path: {cachedInspectorUxmlPath}");
                    }
                }
            }

            if (inspectorUxml != null)
            {
                inspectorUxml.CloneTree(inspector);

                inspector.Q<Button>(ButtonName_AddToSceneList).clicked += () => OnButton_AddToSceneListClicked(inspector);
                inspector.Q<PropertyField>(PropertyFieldName_Scene).RegisterValueChangeCallback(_ => OnPropertyField_SceneChanged(inspector));

                UpdateInspectorGUI(inspector);
            }

            cachedInspector = inspector;

            return cachedInspector;
        }

        // End Editor immplementation

        private void OnButton_AddToSceneListClicked(VisualElement inspector)
        {
            if (target is not SceneInstance sceneInstance)
            {
                return;
            }

            var sceneList = EditorBuildSettings.scenes.ToList();
            var indexInSceneList = sceneList.FindIndex(scene => scene.path == sceneInstance.GetSceneAssetPath());

            if (indexInSceneList < 0)
            {
                var newScene = new EditorBuildSettingsScene(sceneInstance.GetSceneAssetPath(), true);
                sceneList.Add(newScene);
            }
            else
            {
                sceneList.RemoveAll(scene => scene.path == sceneInstance.GetSceneAssetPath());
            }

            EditorBuildSettings.scenes = sceneList.ToArray();

            UpdateInspectorGUI(inspector);
        }

        private void OnPropertyField_SceneChanged(VisualElement inspector)
        {
            UpdateInspectorGUI(inspector);
        }

        private void UpdateButton_AddToSceneList(VisualElement inspector)
        {
            var button = inspector.Q<Button>(ButtonName_AddToSceneList);

            if (button == null)
            {
                Debug.LogError($"Button: {ButtonName_AddToSceneList} not found in the inspector.");
                return;
            }

            if (IsAddedToSceneList())
            {
                button.text = "Remove from Scene List";
            }
            else
            {
                button.text = "Add to Scene List";
            }
        }

        private void UpdateLabel_AddedToSceneList(VisualElement inspector)
        {
            var label = inspector.Q<Label>(LabelName_AddedToSceneList);

            if (label == null)
            {
                Debug.LogError($"Label: {LabelName_AddedToSceneList} not found in the inspector.");
                return;
            }

            var sceneInstance = target as SceneInstance;
            var sceneName = Path.GetFileNameWithoutExtension(sceneInstance.GetSceneAssetPath());

            if (IsAddedToSceneList())
            {
                label.text = $"{sceneName} is added to the scene list.";
            }
            else
            {
                label.text = $"{sceneName} is not added to the scene list.\nIf you want to load {sceneName} at runtime, add to the scene list.";
            }
        }

        private void UpdateLabel_AssetPath(VisualElement inspector)
        {
            var label = inspector.Q<Label>(LabelName_AssetPath);

            if (label == null)
            {
                Debug.LogError($"Label: {LabelName_AssetPath} not found in the inspector.");
                return;
            }

            var sceneInstance = target as SceneInstance;
            label.text = sceneInstance != null ? sceneInstance.GetSceneAssetPath() : "None";
        }

        private void UpdateLabel_BuildIndex(VisualElement inspector)
        {
            var label = inspector.Q<Label>(LabelName_BuildIndex);

            if (label == null)
            {
                Debug.LogError($"Label: {LabelName_BuildIndex} not found in the inspector.");
                return;
            }

            var sceneInstance = target as SceneInstance;
            label.text = sceneInstance != null ? sceneInstance.GetSceneBuildIndex().ToString() : "None";
        }

        private void UpdateInspectorGUI(VisualElement inspector)
        {
            UpdateButton_AddToSceneList(inspector);
            UpdateLabel_AddedToSceneList(inspector);
            UpdateLabel_AssetPath(inspector);
            UpdateLabel_BuildIndex(inspector);
        }

        private bool IsAddedToSceneList() => target is SceneInstance sceneInstance && sceneInstance.GetSceneBuildIndex() >= 0;
    }
}
