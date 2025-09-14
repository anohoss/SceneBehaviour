using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Anoho.SceneBehaviour
{
    internal struct BeforeUpdate { }

    internal struct AfterUpdate { }
    
    internal static class SceneBehaviourHierarchy
    {
        private static readonly SceneBehaviourTree tree = new();

        #region PlayerLoop

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitializeOnLoad()
        {
            var beforeUpdateSystem = new PlayerLoopSystem
            {
                type = typeof(BeforeUpdate),
                updateDelegate = OnBeforeUpdate,
            };

            var afterUpdateSystem = new PlayerLoopSystem
            {
                type = typeof(AfterUpdate),
                updateDelegate = OnAfterUpdate,
            };

            var playerloop = PlayerLoop.GetDefaultPlayerLoop();

            for (var i = 0; i < playerloop.subSystemList.Length; i++)
            {
                var isUnityUpdateSystem = playerloop.subSystemList[i].type == typeof(Update);
                if (isUnityUpdateSystem)
                {
                    var unityUpdateSystem = playerloop.subSystemList[i];

                    var subSystemNum = unityUpdateSystem.subSystemList.Length;
                    var newSubsystemList = new PlayerLoopSystem[subSystemNum + 2];
                    newSubsystemList[0] = beforeUpdateSystem; // BeforeUpdateはMonoBehaviourよりも先に実行する
                    for (int j = 0; j < subSystemNum; j++)
                    {
                        newSubsystemList[j + 1] = unityUpdateSystem.subSystemList[j];
                    }
                    newSubsystemList[subSystemNum + 1] = afterUpdateSystem; // AfterUpdateはMonoBehaviourよりも後に実行する

                    playerloop.subSystemList[i] = new PlayerLoopSystem
                    {
                        type = unityUpdateSystem.type,
                        updateDelegate = unityUpdateSystem.updateDelegate,
                        subSystemList = newSubsystemList,
                        updateFunction = unityUpdateSystem.updateFunction,
                        loopConditionFunction = unityUpdateSystem.loopConditionFunction,
                    };

                    break;
                }
            }

            PlayerLoop.SetPlayerLoop(playerloop);
        }

        private static void OnBeforeUpdate()
        {

        }

        private static void OnAfterUpdate()
        {

        }

        #endregion PlayerLoop

        public static void Add(SceneBehaviour behaviour)
        {
            tree.AddNode(behaviour);
        }

        public static void Remove(SceneBehaviour behaviour)
        {
            tree.RemoveNode(behaviour);
        }

        public static void RefleshParent(SceneBehaviour behaviour)
        {
            tree.RefleshParentNode(behaviour);
        }

        #region Pause

        public static bool IsPaused(SceneBehaviour behaviour)
        {
            return tree.IsPaused(behaviour);
        }

        public static void Pause(SceneBehaviour behaviour)
        {
            tree.PauseNode(behaviour);
        }

        public static void Unpause(SceneBehaviour behaviour)
        {
            tree.UnpauseNode(behaviour);
        }

        #endregion
    }
}
