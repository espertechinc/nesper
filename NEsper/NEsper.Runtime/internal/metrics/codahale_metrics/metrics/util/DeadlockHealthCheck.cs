///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.util
{
    /// <summary>
    ///     A <seealso cref="HealthCheck" /> implementation which returns a list of deadlocked threads, if any.
    /// </summary>
    public class DeadlockHealthCheck : HealthCheck
    {
        private readonly VirtualMachineMetrics vm;

        /// <summary>
        ///     Creates a new <seealso cref="DeadlockHealthCheck" /> with the given <seealso cref="VirtualMachineMetrics" />
        ///     instance.
        /// </summary>
        /// <param name="vm">a <seealso cref="VirtualMachineMetrics" /> instance</param>
        public DeadlockHealthCheck(VirtualMachineMetrics vm)
            : base("deadlocks")
        {
            this.vm = vm;
        }

        /// <summary>
        ///     Creates a new <seealso cref="DeadlockHealthCheck" />.
        /// </summary>
        public DeadlockHealthCheck()
            : this(VirtualMachineMetrics.Instance)
        {
        }

        protected override Result Check()
        {
            var threads = vm.DeadlockedThreads();
            if (threads.IsEmpty()) {
                return Result.Healthy();
            }

            var builder = new StringBuilder("Deadlocked threads detected:\n");
            foreach (var thread in threads) {
                builder.Append(thread).Append('\n');
            }

            return Result.Unhealthy(builder.ToString());
        }
    }
} // end of namespace