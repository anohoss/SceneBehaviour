using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Anoho.SceneBehaviour
{
    internal sealed class SceneBehaviourTree
    {
        private readonly Dictionary<int, SceneBehaviourTreeNode> instanceIdToNode = new();

        // NOTE: ルートノードは SceneBehaviour を保持しないダミーノードとして初期化される。
        // シーン上に配置するSceneBehaviourのルートとなるSceneBehaviourは複数存在する可能性があるため、
        // ルートをダミーノードとして初期化し、子ノードが実質的なルートとなる。
        /// <summary>
        /// ルートノード
        /// </summary>
        private readonly SceneBehaviourTreeNode rootNode = new(null, null);

        public void AddNode(SceneBehaviour behaviour)
        {
            if (behaviour == null)
            {
                throw new ArgumentNullException(nameof(behaviour));
            }

            var instanceId = behaviour.GetInstanceID();
            if (instanceIdToNode.ContainsKey(instanceId))
            {
                return;
            }

            var parentNode = FindOrCreateParentNode(behaviour);
            var newNode = new SceneBehaviourTreeNode(behaviour, parentNode);
            parentNode.AddChild(newNode);

            instanceIdToNode.Add(behaviour.GetInstanceID(), newNode);
        }

        public void RemoveNode(SceneBehaviour behaviour)
        {
            if (behaviour == null)
            {
                throw new ArgumentNullException(nameof(behaviour));
            }

            var instanceId = behaviour.GetInstanceID();
            if (!instanceIdToNode.ContainsKey(instanceId))
            {
                return;
            }

            var node = instanceIdToNode[instanceId];
            var parent = node.GetParent();
            parent.RemoveChild(node);

            instanceIdToNode.Remove(instanceId);
        }

        public SceneBehaviourTreeNode FindNode(SceneBehaviour behaviour)
        {
            if (behaviour == null)
            {
                throw new ArgumentNullException(nameof(behaviour));
            }

            var instanceID = behaviour.GetInstanceID();
            return instanceIdToNode.ContainsKey(instanceID)
                ? instanceIdToNode[instanceID]
                : null;
        }

        public void RefleshParentNode(SceneBehaviour behaviour)
        {
            if (behaviour == null)
            {
                throw new ArgumentNullException(nameof(Behaviour));
            }

            var node = FindNode(behaviour);

            if (node == null)
            {
                return;
            }

            var wasPausedInAncestors = node.IsPausedInAncestors(false);

            var oldParentNode = node.GetParent();
            if (oldParentNode != null)
            {
                node.SetParent(null);
                oldParentNode.RemoveChild(node);
            }

            var newParentNode = FindOrCreateParentNode(behaviour);
            node.SetParent(newParentNode);
            newParentNode.AddChild(node);

            var willBePausedInAncestors = node.IsPausedInAncestors(false);

            if (wasPausedInAncestors != willBePausedInAncestors)
            {
                if (willBePausedInAncestors)
                {
                    node.OnAncestorPaused();
                }
                else
                {
                    node.OnAncestorUnpaused();
                }
            }
        }

        private SceneBehaviourTreeNode FindOrCreateParentNode(SceneBehaviour behaviour)
        {
            var parent = SceneBehaviourUtility.GetParent(behaviour);

            // 親ノードが既に存在する場合はそのノードを返す
            if (parent != null)
            {
                var parentId = parent.GetInstanceID();
                if (instanceIdToNode.ContainsKey(parentId))
                {
                    return instanceIdToNode[parentId];
                }
            }

            var ancestors = ListPool<SceneBehaviour>.Get();
            SceneBehaviourUtility.GetAncestors(behaviour, ancestors);

            var num = ancestors.Count;
            var parentNode = rootNode;
            for (int i = 0; i < num; i++)
            {
                var ancestor = ancestors[i];
                var instanceId = ancestor.GetInstanceID();
                if (instanceIdToNode.ContainsKey(instanceId))
                {
                    parentNode = instanceIdToNode[instanceId];
                }
                else
                {
                    var newParentNode = new SceneBehaviourTreeNode(ancestor, parentNode);
                    parentNode.AddChild(newParentNode);

                    instanceIdToNode.Add(instanceId, newParentNode);

                    parentNode = newParentNode;
                }
            }

            return parentNode;
        }

        #region Pause

        public void PauseNode(SceneBehaviour behaviour)
        {
            if (behaviour == null)
            {
                return;
            }

            var node = FindNode(behaviour);
            if (node == null)
            {
                return;
            }

            node.Pause();
        }

        public void UnpauseNode(SceneBehaviour behaviour)
        {
            if (behaviour == null)
            {
                return;
            }

            var node = FindNode(behaviour);
            if (node == null)
            {
                return;
            }

            node.Unpause();
        }

        public bool IsPaused(SceneBehaviour behaviour)
        {
            var node = FindNode(behaviour);
            if (node == null)
            {
                return false;
            }

            return node.IsPaused();
        }

        #endregion Pause

        #region Update

        public void BeforeUpdate()
        {
            var nodes = instanceIdToNode.Values;

            foreach (var node in nodes)
            {
                node.BeforeUpdate();
            }
        }

        public void AfterUpdate()
        {
            var nodes = instanceIdToNode.Values;

            foreach (var node in nodes)
            {
                node.AfterUpdate();
            }
        }

        public void BeforeLateUpdate()
        {
            var nodes = instanceIdToNode.Values;

            foreach (var node in nodes)
            {
                node.BeforeLateUpdate();
            }
        }

        public void AfterLateUpdate()
        {
            var nodes = instanceIdToNode.Values;

            foreach (var node in nodes)
            {
                node.AfterLateUpdate();
            }
        }

        #endregion Update
    }

    internal sealed class SceneBehaviourTreeNode
    {
        public SceneBehaviourTreeNode(SceneBehaviour behaviour, SceneBehaviourTreeNode parent, Span<SceneBehaviourTreeNode> children)
        {
            this.behaviour = behaviour;
            this.parent = parent;

            foreach (var child in children)
            {
                this.children.Add(child);
            }
        }

        public SceneBehaviourTreeNode(SceneBehaviour behaviour, SceneBehaviourTreeNode parent)
            : this(behaviour, parent, Array.Empty<SceneBehaviourTreeNode>())
        {

        }

        private readonly SceneBehaviour behaviour;

        private SceneBehaviourTreeNode parent;

        private readonly HashSet<SceneBehaviourTreeNode> children = new();

        public SceneBehaviour GetSceneBehaviour() => behaviour;

        public SceneBehaviourTreeNode GetParent() => parent;

        public void SetParent(SceneBehaviourTreeNode newParent)
        {
            parent = newParent;
        }

        public void GetChildren(List<SceneBehaviourTreeNode> children)
        {
            if (children == null)
            {
                return;
            }

            children.Clear();
            children.AddRange(this.children);
        }

        public void AddChild(SceneBehaviourTreeNode child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            if (children.Contains(child))
            {
                return;
            }

            children.Add(child);
        }

        public void RemoveChild(SceneBehaviourTreeNode child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            if (!children.Contains(child))
            {
                return;
            }

            children.Remove(child);
        }

        public void BeforeUpdate()
        {
            if (isPaused)
            {
                return;
            }
            
            if (!behaviour)
            {
                return;
            }

            if (!behaviour.isActiveAndEnabled)
            {
                return;
            }

            behaviour.BeforeUpdateInternal();
        }

        public void AfterUpdate()
        {
            if (isPaused)
            {
                return;
            }
            
            if (!behaviour)
            {
                return;
            }

            if (!behaviour.isActiveAndEnabled)
            {
                return;
            }

            behaviour.AfterUpdateInternal();
        }

        public void BeforeLateUpdate()
        {
            if (isPaused)
            {
                return;
            }
            
            if (!behaviour)
            {
                return;
            }

            if (!behaviour.isActiveAndEnabled)
            {
                return;
            }

            behaviour.BeforeLateUpdateInternal();
        }

        public void AfterLateUpdate()
        {
            if (isPaused)
            {
                return;
            }
            
            if (!behaviour)
            {
                return;
            }

            if (!behaviour.isActiveAndEnabled)
            {
                return;
            }

            behaviour.AfterLateUpdateInternal();
        }

        private bool isPaused;

        // NOTE: 親ノードをさかのぼることで判定可能だが、毎フレームの判定には相応の負荷が発生することが予想されるためキャッシュ
        /// <summary>
        /// 先祖がポーズされているかどうか
        /// </summary>
        private bool isPausedInAncestors;

        public bool IsPaused() => isPaused || isPausedInAncestors;

        public bool IsPausedInAncestors(bool includeSelf)
        {
            var parent = includeSelf ? this : GetParent();
            while (parent != null)
            {
                if (parent.isPaused)
                {
                    return true;
                }

                parent = parent.GetParent();
            }

            return false;
        }

        public void Pause()
        {
            if (isPaused)
            {
                return;
            }

            isPaused = true;

            // 先祖がポーズされていた場合、既にポーズ関数は実行済みなので何もしない
            if (!isPausedInAncestors)
            {
                if (behaviour != null)
                {
                    behaviour.PauseInternal();
                }

                foreach (var child in children)
                {
                    child.OnAncestorPaused();
                }
            }
        }

        public void OnAncestorPaused()
        {
            if (isPausedInAncestors)
            {
                return;
            }

            isPausedInAncestors = true;

            // 元々ポーズしていた場合は何もしない（孫までポーズされている）
            if (!isPaused)
            {
                // 先祖がポーズされた場合、自身もポーズする
                if (behaviour != null)
                {
                    behaviour.PauseInternal();
                }

                // 子ノードにもポーズを伝播
                foreach (var child in children)
                {
                    child.OnAncestorPaused();
                }
            }
        }

        public void Unpause()
        {
            if (!isPaused)
            {
                return;
            }

            isPaused = false;

            if (!isPausedInAncestors)
            {
                if (behaviour != null)
                {
                    behaviour.UnpauseInternal();
                }

                foreach (var child in children)
                {
                    child.OnAncestorUnpaused();
                }
            }
        }

        public void OnAncestorUnpaused()
        {
            if (!isPausedInAncestors)
            {
                return;
            }

            // 先祖が複数ポーズされていた場合はポーズ状態を継続
            isPausedInAncestors = IsPausedInAncestors(false);

            if (!isPaused && !isPausedInAncestors)
            {
                if (behaviour != null)
                {
                    behaviour.UnpauseInternal();
                }

                foreach (var child in children)
                {
                    child.OnAncestorUnpaused();
                }
            }
        }
    }
}
