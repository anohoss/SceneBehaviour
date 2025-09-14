using UnityEngine;

public class UpdateTestMonoBehaviour : MonoBehaviour
{
    void Update()
    {
        Debug.Log("LyfecycleTest: MonoBehaviour.Update");
    }

    void LateUpdate()
    {
        Debug.Log("LyfecycleTest: MonoBehaviour.LateUpdate");
    }
}
