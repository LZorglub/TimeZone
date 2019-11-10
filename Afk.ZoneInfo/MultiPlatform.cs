using System.Reflection;

namespace Afk.ZoneInfo
{
    /// <summary>
    /// Provides functions for multiplatform
    /// </summary>
    class MultiPlatform
    {
        /// <summary>
        /// Gets the current assembly
        /// </summary>
        /// <returns></returns>
        public static Assembly GetCurrentAssembly()
        {
#if NETSTANDARD
            return typeof(MultiPlatform).GetTypeInfo().Assembly;
#else
            return Assembly.GetExecutingAssembly();
#endif
        }
    }
}
