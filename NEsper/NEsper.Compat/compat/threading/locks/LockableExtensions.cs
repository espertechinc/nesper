using System;

namespace com.espertech.esper.compat.threading.locks
{
    public static class LockableExtensions
    {
        /// <summary>
        /// Executes an observable call within the scope of the lock.
        /// </summary>
        /// <param name="lockable">The lockable.</param>
        /// <param name="observableCall">The observable call.</param>
        public static void Call(this ILockable lockable, Action observableCall)
        {
            using(lockable.Acquire()) {
                observableCall.Invoke();
            }
        }

        /// <summary>
        /// Executes a function within the scope of the lock.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lockable">The lockable.</param>
        /// <param name="function">The function.</param>
        /// <returns></returns>
        public static T Call<T>(this ILockable lockable, Func<T> function)
        {
            using (lockable.Acquire()) {
                return function.Invoke();
            }
        }
    }
}
