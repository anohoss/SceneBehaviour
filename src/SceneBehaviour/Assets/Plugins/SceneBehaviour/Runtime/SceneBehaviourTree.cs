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

            var wasPaused = ExistsPausedNodeInAncestors(node, true);

            var oldParentNode = node.GetParent();
            if (oldParentNode != null)
            {
                node.SetParent(null);
                oldParentNode.RemoveChild(node);
            }

            var newParentNode = FindOrCreateParentNode(behaviour);
            node.SetParent(newParentNode);
            newParentNode.AddChild(node);

            var willBePaused = ExistsPausedNodeInAncestors(node, true);

            if (wasPaused != willBePaused)
            {
                if (willBePaused)
                {
                    node.Pause();
                }
                else
                {
                    node.Unpause();
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
            
            return ExistsPausedNodeInAncestors(node, true);
        }

        private bool ExistsPausedNodeInAncestors(SceneBehaviourTreeNode node, bool includeSelf)
        {
            var parent = includeSelf ? node : node.GetParent();
            while (parent != null)
            {
                if (parent.IsPaused())
                {
                    return true;
                }

                parent = parent.GetParent();
            }

            return false;
        }
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
            if (children is null)
            {
                return;
            }

            children.Clear();
            children.AddRange(this.children);
        }

        public void AddChild(SceneBehaviourTreeNode child)
        {
            if (child is null)
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
            if (child is null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            if (!children.Contains(child))
            {
                return;
            }

            children.Remove(child);
        }

        private bool isPaused;

        public bool IsPaused() => isPaused;

        public void Pause()
        {
            isPaused = true;
            
            if (behaviour != null)
            {
                behaviour.OnPause();
            }

            foreach (var child in children)
            {
                child.CallOnPauseOnDescendants();
            }
        }

        private void CallOnPauseOnDescendants()
        {
            if (behaviour != null)
            {
                behaviour.OnPause();
            }

            foreach (var child in children)
            {
                child.CallOnPauseOnDescendants();
            }
        }

        public void Unpause()
        {
            if (!isPaused)
            {
                return;
            }
            
            isPaused = false;
            
            if (behaviour != null)
            {
                behaviour.OnUnpause();
            }

            foreach (var child in children)
            {
                child.CallOnUnpauseOnDescendants();
            }
        }

        private void CallOnUnpauseOnDescendants()
        {
            if (behaviour != null)
            {
                behaviour.OnUnpause();
            }

            foreach (var child in children)
            {
                child.CallOnUnpauseOnDescendants();
            }
        }
    }
}
