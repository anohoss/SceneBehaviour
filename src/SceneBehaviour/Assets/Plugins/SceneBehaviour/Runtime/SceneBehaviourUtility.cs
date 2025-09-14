using System.Collections.Generic;

namespace Anoho.SceneBehaviour
{
    internal static class SceneBehaviourUtility
    {
        public static void GetAncestors(SceneBehaviour behaviour, List<SceneBehaviour> results)
        {
            var parent = GetParent(behaviour);

            while (parent != null)
            {
                results.Add(parent);
                parent = GetParent(parent);
            }
        }

        public static SceneBehaviour GetParent(SceneBehaviour behaviour)
        {
            SceneBehaviour result = behaviour.GetRoot();

            if (result == null)
            {
                result = behaviour.GetSceneInstance();
            }

            return result;
        }
    }
}
