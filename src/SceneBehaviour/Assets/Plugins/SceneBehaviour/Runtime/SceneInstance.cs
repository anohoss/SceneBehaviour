using System.Linq;
using Anoho.Serializables;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using EditorSceneManagement = UnityEditor.SceneManagement;
#endif // UNITY_EDITOR

namespace Anoho.SceneBehaviour
{
    public sealed class SceneInstance : SceneBehaviour
    {
        private const int InvalidSceneHandle = -1;

        private const int InvalidSceneBuildIndex = -1;

        [SerializeField]
        private SerializableScene scene;

        private int sceneHandle = InvalidSceneHandle;

        public int GetSceneHandle()
        {
            return sceneHandle;
        }

        public string GetSceneAssetPath()
        {
            return scene != null ? scene.AssetPath : string.Empty;
        }

        public int GetSceneBuildIndex()
        {
            return scene != null ? scene.BuildIndex : InvalidSceneBuildIndex;
        }

        public bool IsSceneLoaded()
        {
            return sceneHandle is not InvalidSceneHandle;
        }

        public async Awaitable LoadSceneAsync()
        {
            if (IsSceneLoaded())
            {
                return;
            }

            if (scene.BuildIndex < 0)
            {
                Debug.LogError("Scene isn't set or added to scene list. Scene can't be loaded.");
                return;
            }

            var buildIndex = scene.BuildIndex;
            var loadOperation = SceneManager.LoadSceneAsync(scene.BuildIndex, LoadSceneMode.Additive);

            // SceneInstanceRegistryへの登録処理
            {
                // 読み込みシーン内のAwake関数の呼び出し前に登録処理を実行する。
                // SceneBehaviour.Awake関数内で、SceneBehaviourTreeにおける親を決定するために、SceneBehaviourRegistry.FindSceneInstance関数が実行されるため。
                loadOperation.allowSceneActivation = false;

                var loadedScene = SceneManager.GetSceneByBuildIndex(buildIndex);
                sceneHandle = loadedScene.handle;

                SceneInstanceRegistry.RegisterSceneInstance(this);

                loadOperation.allowSceneActivation = true;
            }

            await loadOperation;

            Debug.Log($"Scene: {scene.AssetPath} loaded successfully.");
        }

        public async Awaitable UnloadSceneAsync()
        {
            if (!IsSceneLoaded())
            {
                return;
            }

            if (scene.BuildIndex is InvalidSceneBuildIndex)
            {
                Debug.LogError("Scene isn't set or added to scene list. Scene can't be unloaded.");
                return;
            }

            SceneInstanceRegistry.UnregisterSceneInstance(this);

            sceneHandle = InvalidSceneHandle;

            var unloadOperation = SceneManager.UnloadSceneAsync(scene.BuildIndex);
           
            await unloadOperation;

            Debug.Log($"Scene: {scene.AssetPath} unloaded successfully.");
        }

        // Begin unity methods.

        protected override void OnDestroy()
        {
            // シーンが読み込まれていればアンロードする
            if (sceneHandle is not InvalidSceneHandle)
            {
                _ = UnloadSceneAsync();
            }

            base.OnDestroy();
        }

        // End unity methods.

#if UNITY_EDITOR
        // 編集モード中に開いたシーンをPIE後に復元するクラス
        // # Feature
        // - 再生前に、シーンをアンロードする
        // - 再生後に、再生前に開いていたシーンをロードする
        // - 再生前に、編集していたシーンを保存するかを確認するダイアログを表示する
        // 
        // # Bug
        // - OpenScene関数経由で開いていないシーンであっても復元の対象となる
        // - ダイアログで保存がキャンセルされた場合に、エディターが暗く表示される
        [InitializeOnLoad]
        private static class AutoSceneRestore
        {
            static AutoSceneRestore()
            {
                EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            }

            private static void HandlePlayModeStateChanged(PlayModeStateChange stateChange)
            {
                switch (stateChange)
                {
                    case PlayModeStateChange.EnteredEditMode:
                        {
                            var instances = FindObjectsByType<SceneInstance>(FindObjectsSortMode.None);

                            foreach (var instance in instances)
                            {
                                if (instance.hasSceneOpenedInEditMode)
                                {
                                    instance.hasSceneOpenedInEditMode = false;
                                    instance.OpenScene();
                                }
                            }

                            break;
                        }
                    case PlayModeStateChange.ExitingEditMode:   // ドメインのリロード前
                        {
                            var openInstances = FindObjectsByType<SceneInstance>(FindObjectsSortMode.None)
                                .Where(instance => instance.IsSceneOpened());

                            var openScenes = openInstances
                                .Select(instance => SceneManager.GetSceneByBuildIndex(instance.scene.BuildIndex))
                                .ToArray();

                            if (EditorSceneManagement.EditorSceneManager.SaveModifiedScenesIfUserWantsTo(openScenes))
                            {
                                foreach (var instance in openInstances)
                                {
                                    instance.hasSceneOpenedInEditMode = true;
                                    instance.RemoveScene();
                                }
                            }
                            else
                            {
                                EditorApplication.ExitPlaymode();
                            }

                            break;
                        }
                }
            }
        }

        // エディター内再生の前後でシーンが読み込まれていたか
        [SerializeField, HideInInspector]
        private bool hasSceneOpenedInEditMode;

        private bool IsSceneOpened()
        {
            if (scene == null || scene.BuildIndex is InvalidSceneBuildIndex)
            {
                return false;
            }

            return SceneManager.GetSceneByBuildIndex(scene.BuildIndex).isLoaded;
        }

        [ContextMenu("Open Scene")]
        private void OpenScene()
        {
            if (scene == null)
            {
                return;
            }

            if (IsSceneOpened())
            {
                return;
            }

            EditorSceneManagement.OpenSceneMode openMode = EditorSceneManagement.OpenSceneMode.Additive;
            EditorSceneManagement.EditorSceneManager.OpenScene(scene.AssetPath, openMode);
        }

        [ContextMenu("Open Scene", isValidateFunction: true)]
        private bool ValidateOpenScene()
        {
            if (scene == null)
            {
                return false;
            }

            return !EditorApplication.isPlayingOrWillChangePlaymode
                && !IsSceneOpened();
        }

        [ContextMenu("Remove Scene")]
        private void RemoveScene()
        {
            if (scene == null)
            {
                return;
            }

            if (!IsSceneOpened())
            {
                return;
            }

            Scene sceneToRemove = SceneManager.GetSceneByPath(scene.AssetPath);
            bool removeScene = true;
            EditorSceneManagement.EditorSceneManager.CloseScene(sceneToRemove, removeScene);
        }

        [ContextMenu("Remove Scene", isValidateFunction: true)]
        private bool ValidateRemoveScene()
        {
            if (scene == null)
            {
                return false;
            }

            return !EditorApplication.isPlayingOrWillChangePlaymode
                && IsSceneOpened();
        }
#endif // UNITY_EDITOR
    }
}
