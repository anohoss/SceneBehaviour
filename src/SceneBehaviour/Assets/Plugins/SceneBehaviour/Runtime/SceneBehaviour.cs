using UnityEngine;

namespace Anoho.SceneBehaviour
{
    public abstract class SceneBehaviour : MonoBehaviour
    {
        private SceneBehaviourRoot root;

        public SceneBehaviourRoot GetRoot()
        {
            return root;
        }

        public SceneInstance GetSceneInstance()
        {
            return SceneInstanceRegistry.FindSceneInstance(gameObject.scene.handle);
        }

        [SerializeField]
        private float timeScale = 1.0f;

        public float GetRelativeTimeScale()
        {
            return timeScale;
        }

        public void SetRelativeTimeScale(float newTimeScale)
        {
            if (newTimeScale < Mathf.Epsilon)
            {
                newTimeScale = Mathf.Epsilon;
            }

            timeScale = newTimeScale;
        }

        public float GetTimeScale()
        {
            var timeScale = GetRelativeTimeScale();

            var root = GetRoot();
            if (root != null)
            {
                timeScale *= root.GetTimeScale();
            }
            else
            {
                var sceneInstance = GetSceneInstance();
                if (sceneInstance != null)
                {
                    timeScale *= sceneInstance.GetTimeScale();
                }
            }

            return timeScale;
        }

        public float GetDeltaTime()
        {
            return Time.deltaTime * GetTimeScale();
        }

        public virtual void OnBeforeUpdate() { }

        #region Pause

        public void Pause()
        {
            SceneBehaviourHierarchy.Pause(this);
        }

        public void Unpause()
        {
            SceneBehaviourHierarchy.Unpause(this);
        }

        public bool IsPaused()
        {
            return SceneBehaviourHierarchy.IsPaused(this);
        }

        public virtual void OnPause() { Debug.Log($"{gameObject.name} is paused."); }

        public virtual void OnUnpause() { Debug.Log($"{gameObject.name} is unpaused.");}

        #endregion Pause

        // Begin unity methods.

        protected virtual void Awake()
        {
            // Rootの初期化
            RefleshRoot();

            // Hierarchyへの追加
            SceneBehaviourHierarchy.Add(this);
        }

        protected virtual void OnDestroy()
        {
            // Hierarchyからの削除
            SceneBehaviourHierarchy.Remove(this);
        }

        protected virtual void OnTransformParentChanged()
        {
            // 親オブジェクトが変更されたので、Rootを更新する
            RefleshRoot();

            // Rootが変更されたので、Hierarchy上での親を更新する
            SceneBehaviourHierarchy.RefleshParent(this);
        }

        // End unity methods.

        private void RefleshRoot()
        {
            var parent = transform.parent;

            if (root != null && parent == root.transform)
            {
                return;
            }

            if (parent == null)
            {
                root = null;
            }
            else
            {
                if (parent.TryGetComponent<SceneBehaviourRoot>(out var newRoot))
                {
                    root = newRoot;
                }
                else // If the parent is not a SceneBehaviourRoot, search upwards in the hierarchy
                {
                    bool includeInactive = true;
                    root = parent.GetComponentInParent<SceneBehaviourRoot>(includeInactive);
                }
            }
        }
    }
}
