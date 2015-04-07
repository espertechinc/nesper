///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.util
{
    /// <summary>
    /// Utility class for logging threading-related messages.
    /// <para>
    /// Prints thread information and lock-specific info.
    /// </para>
    /// </summary>
    public class ThreadLogUtil
    {
        /// <summary>Set trace log level.</summary>
        public static int TRACE = 0;

        /// <summary>Set info log level.</summary>
        public static int INFO = 1;

        /// <summary>Enable trace logging.</summary>
        public const bool ENABLED_TRACE = false;

        /// <summary>Enable info logging.</summary>
        public const Boolean ENABLED_INFO = false;

        /// <summary>If enabled, logs for trace level the given objects and text</summary>
        /// <param name="text">to log</param>
        /// <param name="objects">to write</param>
        public static void Trace(String text, params Object[] objects)
        {
            if (!ENABLED_TRACE)
            {
                return;
            }
            Write(text, objects);
        }

        /// <summary>If enabled, logs for info level the given objects and text</summary>
        /// <param name="text">to log</param>
        /// <param name="objects">to write</param>
        public static void Info(String text, params Object[] objects)
        {
            if (!ENABLED_INFO)
            {
                return;
            }
            Write(text, objects);
        }

        /// <summary>Logs the lock and action.</summary>
        /// <param name="lockAction">is the action towards the lock</param>
        /// <param name="lockObj">is the lock instance</param>
        public static void TraceLock(String lockAction, Object lockObj)
        {
            if (!ENABLED_TRACE)
            {
                return;
            }
            Write(lockAction + " " + GetLockInfo(lockObj));
        }

        /// <summary>Logs the lock and action.</summary>
        /// <param name="lockAction">is the action towards the lock</param>
        /// <param name="lockObj">is the lock instance</param>
        public static void TraceLock(String lockAction, IReaderWriterLock @lockObj)
        {
            if (!ENABLED_TRACE)
            {
                return;
            }
            Write(lockAction + " " + GetLockInfo(lockObj));
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

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // End of namespace
