///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.core.thread
{
    /// <summary>Thread factory for threading options.</summary>
    public class EngineThreadFactory : java.util.concurrent.ThreadFactory {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string engineURI;
        private readonly string prefix;
        private readonly ThreadGroup threadGroup;
        private readonly int threadPriority;
        private int currThreadCount;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="engineURI">engine URI</param>
        /// <param name="prefix">prefix for thread names</param>
        /// <param name="threadGroup">thread group</param>
        /// <param name="threadPrio">priority to use</param>
        public EngineThreadFactory(string engineURI, string prefix, ThreadGroup threadGroup, int threadPrio) {
            if (engineURI == null) {
                this.engineURI = "default";
            } else {
                this.engineURI = engineURI;
            }
            this.prefix = prefix;
            this.threadGroup = threadGroup;
            this.threadPriority = threadPrio;
        }
    
        public Thread NewThread(Runnable runnable) {
            string name = "com.espertech.esper." + prefix + "-" + engineURI + "-" + currThreadCount;
            currThreadCount++;
            var t = new Thread(threadGroup, runnable, name);
            t.Daemon = true;
            t.Priority = threadPriority;
    
            if (Log.IsDebugEnabled) {
                Log.Debug("Creating thread '" + name + "' : " + t + " priority " + threadPriority);
            }
            return t;
        }
    }
} // end of namespace
