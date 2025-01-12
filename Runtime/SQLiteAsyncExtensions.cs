using System.Threading.Tasks;
using UnityEngine;

namespace SQLite
{
    public static class SQLiteAsyncExtensions
    {
#if UNITY_WEBGL
        // WebGL builds cannot use background threads, so use a
        // TaskScheduler that executes tasks on Unity's main thread.
        public static TaskScheduler TaskScheduler { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeTaskScheduler()
        {
            TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }
#else
        // On all other platforms, use the default TaskScheduler
        public static TaskScheduler TaskScheduler => TaskScheduler.Default;
#endif
    }
}
