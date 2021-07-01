///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Threading;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.runtime.@internal.kernel.thread
{
    /// <summary>
    ///     Thread factory for threading options.
    /// </summary>
    public class EngineThreadFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _prefix;
        private readonly string _runtimeURI;
        private readonly ThreadPriority _threadPriority;
        private int _currThreadCount;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="runtimeURI">runtime URI</param>
        /// <param name="prefix">prefix for thread names</param>
        /// <param name="threadPrio">priority to use</param>
        public EngineThreadFactory(
            string runtimeURI,
            string prefix,
            ThreadPriority threadPrio)
        {
            if (runtimeURI == null) {
                _runtimeURI = "default";
            }
            else {
                _runtimeURI = runtimeURI;
            }

            _prefix = prefix;
            _threadPriority = threadPrio;
        }

        public Thread NewThread(Runnable runnable)
        {
            var name = "com.espertech.esper." + _prefix + "-" + _runtimeURI + "-" + _currThreadCount;
            _currThreadCount++;

            var thread = new Thread(() => runnable.Invoke());
            thread.Name = name;
            thread.IsBackground = true;
            thread.Priority = _threadPriority;

            if (Log.IsDebugEnabled) {
                Log.Debug("Creating thread '" + name + "' : " + thread + " priority " + _threadPriority);
            }

            return thread;
        }
    }
} // end of namespace