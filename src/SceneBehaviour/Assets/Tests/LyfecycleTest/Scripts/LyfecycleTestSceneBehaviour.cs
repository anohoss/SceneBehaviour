using Anoho.SceneBehaviour;
using UnityEngine;

public class LyfecycleTestSceneBehaviour : SceneBehaviour
{
    protected override void OnBeforeUpdate()
    {
        base.OnBeforeUpdate();

        Debug.Log("LyfecycleTest: SceneBehaviour.OnBeforeUpdate");
    }

    protected override void OnAfterUpdate()
    {
        base.OnAfterUpdate();

        Debug.Log("LyfecycleTest: SceneBehaviour.OnAfterUpdate");
    }
    protected override void OnBeforeLateUpdate()
    {
        base.OnBeforeLateUpdate();

        Debug.Log("LyfecycleTest: SceneBehaviour.OnBeforeLateUpdate");
    }

    protected override void OnAfterLateUpdate()
    {
        base.OnAfterLateUpdate();

        Debug.Log("LyfecycleTest: SceneBehaviour.OnAfterLateUpdate");
    }
}
