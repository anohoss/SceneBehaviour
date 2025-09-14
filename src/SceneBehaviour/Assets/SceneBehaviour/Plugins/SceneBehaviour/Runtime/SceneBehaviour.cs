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

        private const float DefaultTimeScale = 1.0f;

        private const float MinTimeScale = 0f;

        [SerializeField]
        private float timeScale = DefaultTimeScale;

        public float GetRelativeTimeScale()
        {
            return timeScale;
        }

        public void SetRelativeTimeScale(float newTimeScale)
        {
            if (newTimeScale < MinTimeScale)
            {
                newTimeScale = MinTimeScale;
            }

            timeScale = newTimeScale;
        }

        public float GetTimeScale()
        {
            var timeScale = GetRelativeTimeScale();

            var root = GetRoot();
            if (root)
            {
                timeScale *= root.GetTimeScale();
            }
            else
            {
                var sceneInstance = GetSceneInstance();
                if (sceneInstance)
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

        #region Update

        protected virtual void OnBeforeUpdate() { }

        protected virtual void OnAfterUpdate() { }

        protected virtual void OnBeforeLateUpdate() { }

        protected virtual void OnAfterLateUpdate() { }

        internal void BeforeUpdateInternal()
        {
            OnBeforeUpdate();
        }

        internal void AfterUpdateInternal()
        {
            OnAfterUpdate();
        }

        internal void BeforeLateUpdateInternal()
        {
            OnBeforeLateUpdate();
        }

        internal void AfterLateUpdateInternal()
        {
            OnAfterLateUpdate();
        }

        #endregion Update

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

        internal void PauseInternal()
        {
            OnPause();
        }

        internal void UnpauseInternal()
        {
            OnUnpause();
        }

        protected virtual void OnPause() { }

        protected virtual void OnUnpause() { }

        #endregion Pause

        // Begin unity methods.

        protected virtual void Awake()
        {
            // Rootの初期化
            RefleshRoot();

            // Hierarchyへの追加
            SceneBehaviourHierarchy.Register(this);
        }

        protected virtual void Start() { }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() { }

        protected virtual void OnDestroy()
        {
            // Hierarchyからの削除
            SceneBehaviourHierarchy.Unregister(this);
        }

        // FixedUpdate は実行順が影響する処理を書かないため、
        protected virtual void FixedUpdate() { }

        protected virtual void OnTransformParentChanged()
        {
            // 親オブジェクトが変更されたので、Rootを更新する
            RefleshRoot();

            // Rootを更新したので、Hierarchy上での親を更新する
            SceneBehaviourHierarchy.RefleshParent(this);
        }

        protected virtual void OnTransformChildrenChanged() { }

        // Unity の更新イベントは SceneBehaviourでは利用しない

#pragma warning disable UNT0001 // Empty Unity message

        protected void Update() { }

        protected void LateUpdate() { }

#pragma warning restore UNT0001 // Empty Unity message

#if UNITY_EDITOR

        protected virtual void Reset()
        {
            timeScale = DefaultTimeScale;
        }

        protected virtual void OnValidate()
        {
            timeScale = Mathf.Max(timeScale, MinTimeScale);
        }

#endif // UNITY_EDITOR

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
