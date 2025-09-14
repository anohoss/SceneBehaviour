using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Anoho.SceneBehaviour
{
    internal struct BeforeUpdate { }

    internal struct BeforeLateUpdate { }

    internal struct AfterLateUpdate { }

    internal struct AfterUpdate { }
    
    internal static class SceneBehaviourHierarchy
    {
        private static readonly SceneBehaviourTree tree = new();

        #region PlayerLoop

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitializeOnLoad()
        {
            var playerloop = PlayerLoop.GetDefaultPlayerLoop();

            for (var i = 0; i < playerloop.subSystemList.Length; i++)
            {
                var isUnityUpdateSystem = playerloop.subSystemList[i].type == typeof(Update);
                if (isUnityUpdateSystem)
                {
                    var unityUpdateSystem = playerloop.subSystemList[i];

                    var subSystemNum = unityUpdateSystem.subSystemList.Length;
                    var newSubsystemList = new PlayerLoopSystem[subSystemNum + 2];

                    int shiftIdx = 0;
                    // MonoBehaviour の Update 呼び出しの前後に SceneBehaviour の更新処理を差し込む
                    for (int j = 0; j < subSystemNum; j++)
                    {
                        var subSystem = unityUpdateSystem.subSystemList[j];

                        // SceneBehaviour.OnBeforeUpdate は MonoBehaviour.Update よりも先に実行する
                        if (subSystem.type == typeof(Update.ScriptRunBehaviourUpdate))
                        {
                            newSubsystemList[j] = new PlayerLoopSystem
                            {
                                type = typeof(BeforeUpdate),
                                updateDelegate = OnBeforeUpdate,
                            };

                            shiftIdx += 1;
                        }

                        newSubsystemList[j + shiftIdx] = unityUpdateSystem.subSystemList[j];

                        // SceneBehaviour.OnAfterUpdate は MonoBehaviour.Update よりも後に実行する
                        if (subSystem.type == typeof(Update.ScriptRunBehaviourUpdate))
                        {
                            newSubsystemList[j + shiftIdx + 1] = new PlayerLoopSystem
                            {
                                type = typeof(AfterUpdate),
                                updateDelegate = OnAfterUpdate,
                            };

                            shiftIdx += 1;
                        }
                    }

                    playerloop.subSystemList[i] = new PlayerLoopSystem
                    {
                        type = unityUpdateSystem.type,
                        updateDelegate = unityUpdateSystem.updateDelegate,
                        subSystemList = newSubsystemList,
                        updateFunction = unityUpdateSystem.updateFunction,
                        loopConditionFunction = unityUpdateSystem.loopConditionFunction,
                    };

                    continue;
                }

                var isUnityPreLateUpdateSystem = playerloop.subSystemList[i].type == typeof(PreLateUpdate);
                if (isUnityPreLateUpdateSystem)
                {
                    var subsystem = playerloop.subSystemList[i];

                    var subSystemNum = subsystem.subSystemList.Length;
                    var newSubsystemList = new PlayerLoopSystem[subSystemNum + 2];

                    int shiftIdx = 0;
                    // MonoBehaviour の LateUpate 呼び出しの前後に SceneBehaviour の更新処理を差し込む
                    for (int j = 0; j < subSystemNum; j++)
                    {
                        var subSystem = subsystem.subSystemList[j];

                        // SceneBehaviour.OnBeforeLateUpdate は MonoBehaviour.LateUpdate よりも先に実行する
                        if (subSystem.type == typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate))
                        {
                            newSubsystemList[j] = new PlayerLoopSystem
                            {
                                type = typeof(BeforeLateUpdate),
                                updateDelegate = OnBeforeLateUpdate,
                            };

                            shiftIdx += 1;
                        }

                        newSubsystemList[j + shiftIdx] = subsystem.subSystemList[j];

                        // SceneBehaviour.OnAfterLateUpdate は MonoBehaviour.LateUpdate よりも後に実行する
                        if (subSystem.type == typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate))
                        {
                            newSubsystemList[j + shiftIdx + 1] = new PlayerLoopSystem
                            {
                                type = typeof(AfterLateUpdate),
                                updateDelegate = OnAfterLateUpdate,
                            };

                            shiftIdx += 1;
                        }
                    }

                    playerloop.subSystemList[i] = new PlayerLoopSystem
                    {
                        type = subsystem.type,
                        updateDelegate = subsystem.updateDelegate,
                        subSystemList = newSubsystemList,
                        updateFunction = subsystem.updateFunction,
                        loopConditionFunction = subsystem.loopConditionFunction,
                    };

                    continue;
                }
            }

            PlayerLoop.SetPlayerLoop(playerloop);
        }

        private static void OnBeforeUpdate()
        {
            tree.BeforeUpdate();
        }

        private static void OnAfterUpdate()
        {
            tree.AfterUpdate();
        }

        private static void OnBeforeLateUpdate()
        {
            tree.BeforeLateUpdate();
        }

        private static void OnAfterLateUpdate()
        {
            tree.AfterLateUpdate();
        }

        #endregion PlayerLoop

        public static void Register(SceneBehaviour behaviour)
        {
            tree.AddNode(behaviour);
        }

        public static void Unregister(SceneBehaviour behaviour)
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
