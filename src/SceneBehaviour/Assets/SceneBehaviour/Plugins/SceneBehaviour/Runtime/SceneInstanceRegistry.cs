using System;
using System.Collections.Generic;
using UnityEngine;

namespace Anoho.SceneBehaviour
{
    internal static class SceneInstanceRegistry
    {
        private static Dictionary<int, SceneInstance> sceneHandleToInstance = new();

        public static void GetAllSceneInstances(List<SceneInstance> results)
        {
            if (results is null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            results.Clear();
            
            foreach (var pair in sceneHandleToInstance)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                results.Add(pair.Value);
            }
        }

        public static SceneInstance FindSceneInstance(int sceneHandle)
        {
            return sceneHandleToInstance.TryGetValue(sceneHandle, out var sceneInstance) ? sceneInstance : null;
        }

        internal static void RegisterSceneInstance(SceneInstance sceneInstance)
        {
            if (sceneInstance is null)
            {
                throw new ArgumentNullException(nameof(sceneInstance));
            }

            var sceneHandle = sceneInstance.GetSceneHandle();
            sceneHandleToInstance.Add(sceneHandle, sceneInstance);

            Debug.Log($"Registered SceneInstance: {sceneInstance.name}");
        }

        internal static void UnregisterSceneInstance(SceneInstance sceneInstance)
        {
            if (sceneInstance is null)
            {
                throw new ArgumentNullException(nameof(sceneInstance));
            }
            
            var sceneHandle = sceneInstance.GetSceneHandle();
            if (sceneHandleToInstance.Remove(sceneHandle))
            {
                Debug.Log($"Unregistered SceneInstance: {sceneInstance.name}");
            }
            else
            {
                Debug.LogWarning($"SceneInstance not found: {sceneInstance.name}");
            }
        }
    }
}
