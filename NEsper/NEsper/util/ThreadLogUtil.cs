///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.util
{
    /// <summary>
    /// Utility class for logging threading-related messages.
    /// <para>
    /// Prints thread information and lock-specific INFO.
    /// </para>
    /// </summary>
    public class ThreadLogUtil
    {
        /// <summary>Enable TRACE logging.</summary>
        public static readonly bool ENABLED_TRACE = false;

        /// <summary>Enable INFO logging.</summary>
        public static readonly bool ENABLED_INFO = false;

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>Set TRACE log level.</summary>
        public static int TRACE = 0;

        /// <summary>Set INFO log level.</summary>
        public static int INFO = 1;

        /// <summary>
        /// If enabled, logs for TRACE level the given objects and text
        /// </summary>
        /// <param name="text">to log</param>
        /// <param name="objects">to write</param>
        public static void Trace(string text, params Object[] objects)
        {
            if (!ENABLED_TRACE)
            {
                return;
            }
            Write(text, objects);
        }

        /// <summary>
        /// If enabled, logs for INFO level the given objects and text
        /// </summary>
        /// <param name="text">to log</param>
        /// <param name="objects">to write</param>
        public static void Info(string text, params Object[] objects)
        {
            if (!ENABLED_INFO)
            {
                return;
            }
            Write(text, objects);
        }

        /// <summary>
        /// Logs the lock and action.
        /// </summary>
        /// <param name="lockAction">is the action towards the lock</param>
        /// <param name="lock">is the lock instance</param>
        public static void TraceLock(string lockAction, IReaderWriterLock @lock)
        {
            if (!ENABLED_TRACE)
            {
                return;
            }
            Write(lockAction + " " + GetLockInfo(@lock));
        }

        private static String GetLockInfo(Object lockObj)
        {
            String lockid = String.Format("Lock@{0:X8}", Marshal.GetIUnknownForObject(lockObj).ToInt64());
            return "lock " + lockid;
            //return "lock " + lockid + " held=" + lockObj.HoldCount + " isHeldMe=" + lockObj.IsHeldByCurrentThread() +
            //        " hasQueueThreads=" + lockObj.HasQueuedThreads();
        }

        private static String GetLockInfo(IReaderWriterLock @lockObj)
        {
            String lockid = String.Format("RWLock@{0:X}", lockObj.GetHashCode());
            return lockid +
                   //" readLockCount=" + lockObj.ReadLockCount +
                   " isWriteLocked=" + lockObj.IsWriterLockHeld;
        }

        private static void Write(String text, params Object[] objects)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(text);
            buf.Append(' ');
            foreach (Object obj in objects)
            {
                if ((obj is String) || (obj is ValueType))
                {
                    buf.Append(obj.ToString());
                }
                else
                {
                    buf.Append(obj.GetType().FullName);
                    buf.Append('@');
                    buf.Append(String.Format("{0:X2}", obj.GetHashCode()));
                }
                buf.Append(' ');
            }
            Write(buf.ToString());
        }

        private static void Write(String text)
        {
            Log.Info(".write Thread " + Thread.CurrentThread.ManagedThreadId + " " + text);
        }
    }
} // end of namespace
