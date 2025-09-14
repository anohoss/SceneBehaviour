using System.Threading.Tasks;
using Anoho.Serializables;
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

        [SerializeField]
        private SerializableScene scene;

        [SerializeField]
        private bool loadOnPlay = false;

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
            return scene != null ? scene.BuildIndex : -1;
        }

        public async ValueTask LoadSceneAsync()
        {
            if (sceneHandle is not InvalidSceneHandle)
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
            loadOperation.allowSceneActivation = true;

            await loadOperation;

            Debug.Log($"Scene: {scene.AssetPath} loaded successfully.");

            var loadedScene = SceneManager.GetSceneByBuildIndex(buildIndex);
            sceneHandle = loadedScene.handle;

            SceneInstanceRegistry.RegisterSceneInstance(this);
        }

        public async ValueTask UnloadSceneAsync()
        {
            if (sceneHandle is InvalidSceneHandle)
            {
                return;
            }

            if (scene.BuildIndex < 0)
            {
                Debug.LogError("Scene isn't set or added to scene list. Scene can't be unloaded.");
                return;
            }

            SceneInstanceRegistry.UnregisterSceneInstance(this);

            sceneHandle = InvalidSceneHandle;

            var unloadOperation = SceneManager.UnloadSceneAsync(scene.BuildIndex);
            if (unloadOperation != null)
            {
                unloadOperation.allowSceneActivation = true;
                await unloadOperation;
            }

            Debug.Log($"Scene: {scene.AssetPath} unloaded successfully.");
        }

        // Begin unity methods.

        protected async override void Awake()
        {
            base.Awake();

            if (loadOnPlay)
            {
                await LoadSceneAsync();
            }
        }

        protected async override void OnDestroy()
        {
            if (sceneHandle is not InvalidSceneHandle)
            {
                await UnloadSceneAsync();
            }

            base.OnDestroy();
        }

        // End unity methods.

#if UNITY_EDITOR
        [ContextMenu("Load Scene")]
        private void LoadScene()
        {
            if (scene == null)
            {
                return;
            }

            EditorSceneManagement.OpenSceneMode openMode = EditorSceneManagement.OpenSceneMode.Additive;
            EditorSceneManagement.EditorSceneManager.OpenScene(scene.AssetPath, openMode);
        }

        [ContextMenu("Load Scene", isValidateFunction: true)]
        private bool ValidateLoadScene()
        {
            if (scene == null)
            {
                return false;
            }

            return !SceneManager.GetSceneByPath(scene.AssetPath).isLoaded;
        }

        [ContextMenu("Unload Scene")]
        private void UnloadScene()
        {
            if (scene == null)
            {
                return;
            }
            
            Scene sceneToUnload = SceneManager.GetSceneByPath(scene.AssetPath);
            if (sceneToUnload.isLoaded)
            {
                bool removeScene = true;
                EditorSceneManagement.EditorSceneManager.CloseScene(sceneToUnload, removeScene);
            }
        }

        [ContextMenu("Unload Scene", isValidateFunction: true)]
        private bool ValidateUnloadScene()
        {
            if (scene == null)
            {
                return false;
            }

            return SceneManager.GetSceneByPath(scene.AssetPath).isLoaded;
        }
#endif // UNITY_EDITOR
    }
}
