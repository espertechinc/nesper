///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading.Tasks;

namespace com.espertech.esper.compat.concurrency
{
    public class DisposableTaskFactory : TaskFactory, IDisposable
    {
        public DisposableTaskFactory(TaskScheduler scheduler) : base(scheduler)
        {
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Scheduler is IDisposable disposableScheduler)
            {
                disposableScheduler.Dispose();
            }
        }
    }
}
