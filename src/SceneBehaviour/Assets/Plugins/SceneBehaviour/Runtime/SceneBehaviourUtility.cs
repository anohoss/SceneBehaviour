using System.Collections.Generic;

namespace Anoho.SceneBehaviour
{
    internal static class SceneBehaviourUtility
    {
        /// <summary>
        /// SceneBehaviour の先祖を取得する
        /// </summary>
        /// <param name="behaviour"></param>
        /// <param name="results"><paramref name="behaviour"/> の先祖</param>
        public static void GetAncestors(SceneBehaviour behaviour, List<SceneBehaviour> results)
        {
            var parent = GetParent(behaviour);

            while (parent != null)
            {
                results.Add(parent);
                parent = GetParent(parent);
            }
        }

        /// <summary>
        /// SceneBehaviour の親を取得する
        /// </summary>
        /// <param name="behaviour"></param>
        /// <returns><paramref name="behaviour"/> の親</returns>
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
