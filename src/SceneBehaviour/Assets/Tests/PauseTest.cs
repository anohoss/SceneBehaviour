using Anoho.SceneBehaviour;
using NUnit.Framework;
using UnityEngine;

internal class PauseTest
{
    /// <summary>
    /// 単一のSceneBehaviourRootのポーズの可否をテストする
    /// </summary>
    [Test]
    public void PauseRootOnly()
    {
        var root = new GameObject("Root").AddComponent<SceneBehaviourRoot>();

        root.Pause();

        Assert.That(root.IsPaused());
    }

    /// <summary>
    /// 階層化されたSceneBehaviourのポーズの可否をテストする
    /// </summary>
    [Test]
    public void PauseRootAndChildren()
    {
        var root = new GameObject("Root").AddComponent<SceneBehaviourRoot>();

        var c1 = new GameObject("Child_1").AddComponent<SceneBehaviourRoot>();
        var c2 = new GameObject("Child_2").AddComponent<SceneBehaviourRoot>();

        c1.transform.SetParent(root.transform);
        c2.transform.SetParent(root.transform);

        root.Pause();

        Assert.That(root.IsPaused()
         && c1.IsPaused()
         && c2.IsPaused());
    }

    /// <summary>
    /// 階層化されたSceneBehaviourの中で、単一のSceneBehaviourのポーズの可否をテストする
    /// </summary>
    [Test]
    public void PauseChild()
    {
        var root = new GameObject("Root").AddComponent<SceneBehaviourRoot>();

        var c1 = new GameObject("Child_1").AddComponent<SceneBehaviourRoot>();
        var c2 = new GameObject("Child_2").AddComponent<SceneBehaviourRoot>();

        c1.transform.SetParent(root.transform);
        c2.transform.SetParent(root.transform);

        c1.Pause();

        Assert.That(!root.IsPaused()
            && c1.IsPaused()
            && !c2.IsPaused()
        );
    }

    /// <summary>
    /// 階層化されたSceneBehaviourの中で、単一のSceneBehaviourのポーズの可否をテストする
    /// </summary>
    [Test]
    public void PauseChildAndGrandchildren()
    {
        var root = new GameObject("Root").AddComponent<SceneBehaviourRoot>();

        var c1 = new GameObject("Child_1").AddComponent<SceneBehaviourRoot>();
        var c2 = new GameObject("Child_2").AddComponent<SceneBehaviourRoot>();

        var gc1 = new GameObject("Grandchild_1").AddComponent<SceneBehaviourRoot>();
        var gc2 = new GameObject("Grandchild_2").AddComponent<SceneBehaviourRoot>();

        c1.transform.SetParent(root.transform);
        c2.transform.SetParent(root.transform);

        gc1.transform.SetParent(c1.transform);
        gc2.transform.SetParent(c1.transform);

        c1.Pause();

        Assert.That(!root.IsPaused()
            && c1.IsPaused()
            && !c2.IsPaused()
            && gc1.IsPaused()
            && gc2.IsPaused()
        );
    }

    /// <summary>
    /// 単一のSceneBehaviourRootのポーズの解除の可否をテストする
    /// </summary>
    [Test]
    public void UnpauseRootOnly()
    {
        var root = new GameObject("Root");
        var sceneBehaviourRoot = root.AddComponent<SceneBehaviourRoot>();

        sceneBehaviourRoot.Pause();
        sceneBehaviourRoot.Unpause();

        Assert.That(!sceneBehaviourRoot.IsPaused());
    }

    /// <summary>
    /// 階層化されたSceneBehaviourのポーズの解除可否をテストする
    /// </summary>
    [Test]
    public void UnpauseRootAndChildren()
    {
        var root = new GameObject("Root").AddComponent<SceneBehaviourRoot>();

        var c1 = new GameObject("Child_1").AddComponent<SceneBehaviourRoot>();
        var c2 = new GameObject("Child_2").AddComponent<SceneBehaviourRoot>();

        c1.transform.SetParent(root.transform);
        c2.transform.SetParent(root.transform);

        root.Pause();
        root.Unpause();

        Assert.That(!root.IsPaused()
         && !c1.IsPaused()
         && !c2.IsPaused());
    }

    /// <summary>
    /// 階層化されたSceneBehaviourの中で、単一のSceneBehaviourのポーズの解除の可否をテストする
    /// </summary>
    [Test]
    public void UnpauseChild()
    {
        var root = new GameObject("Root").AddComponent<SceneBehaviourRoot>();

        var c1 = new GameObject("Child_1").AddComponent<SceneBehaviourRoot>();
        var c2 = new GameObject("Child_2").AddComponent<SceneBehaviourRoot>();

        c1.transform.SetParent(root.transform);
        c2.transform.SetParent(root.transform);

        c1.Pause();
        c1.Unpause();

        Assert.That(!c1.IsPaused());
    }

    /// <summary>
    /// 階層化されたSceneBehaviourの中で、単一のSceneBehaviourのポーズの可否をテストする
    /// </summary>
    [Test]
    public void UnpauseChildAndGrandchildren()
    {
        var root = new GameObject("Root").AddComponent<SceneBehaviourRoot>();

        var c1 = new GameObject("Child_1").AddComponent<SceneBehaviourRoot>();
        var c2 = new GameObject("Child_2").AddComponent<SceneBehaviourRoot>();

        var gc1 = new GameObject("Grandchild_1").AddComponent<SceneBehaviourRoot>();
        var gc2 = new GameObject("Grandchild_2").AddComponent<SceneBehaviourRoot>();

        c1.transform.SetParent(root.transform);
        c2.transform.SetParent(root.transform);

        gc1.transform.SetParent(c1.transform);
        gc2.transform.SetParent(c1.transform);

        c1.Pause();
        c1.Unpause();

        Assert.That(!root.IsPaused()
            && !c1.IsPaused()
            && !c2.IsPaused()
            && !gc1.IsPaused()
            && !gc2.IsPaused()
        );
    }

    /// <summary>
    /// ポーズされたSceneBehaviourに子オブジェクトをアタッチしたときのポーズの可否をテストする
    /// </summary>
    [Test]
    public void ReparentToPausedRoot()
    {
        var root = new GameObject("Root").AddComponent<SceneBehaviourRoot>();

        root.Pause();

        var child = new GameObject("Child").AddComponent<SceneBehaviourRoot>();

        child.transform.SetParent(root.transform);

        Assert.That(root.IsPaused() && child.IsPaused());
    }
}
