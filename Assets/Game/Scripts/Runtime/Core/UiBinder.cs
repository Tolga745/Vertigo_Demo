using UnityEngine;

namespace VertigoWheel.Core
{
    /// <summary>
    /// Small reflection-free helper used by view components inside <c>OnValidate</c> to wire their
    /// child references automatically. The brief requires that "button references should be
    /// automatically set from OnValidate" and that we never wire references from the Editor by hand,
    /// so every view resolves its children by name through this binder.
    /// </summary>
    public static class UiBinder
    {
        /// <summary>
        /// Finds a child (recursively) whose GameObject name matches <paramref name="childName"/> and
        /// returns the requested component on it. Returns null when not found, which keeps OnValidate
        /// safe to run on partially built hierarchies.
        /// </summary>
        public static T FindByName<T>(Transform root, string childName) where T : Component
        {
            if (root == null) return null;

            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t != root && t.name == childName)
                    return t.GetComponent<T>();
            }
            return null;
        }

        /// <summary>Returns the component on the first descendant carrying it, ignoring the root.</summary>
        public static T FindInChildren<T>(Transform root) where T : Component
        {
            if (root == null) return null;

            foreach (var c in root.GetComponentsInChildren<T>(true))
            {
                if (c.transform != root)
                    return c;
            }
            return null;
        }
    }
}
