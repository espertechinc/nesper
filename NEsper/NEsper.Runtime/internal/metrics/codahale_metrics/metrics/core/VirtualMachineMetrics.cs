///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    /// A collection of Java Virtual Machine metrics.
    /// </summary>
    public class VirtualMachineMetrics
    {
        private const int MAX_STACK_TRACE_DEPTH = 100;

        private static readonly VirtualMachineMetrics INSTANCE = new VirtualMachineMetrics(
                ManagementFactory.MemoryMXBean,
                ManagementFactory.MemoryPoolMXBeans,
                ManagementFactory.OperatingSystemMXBean,
                ManagementFactory.ThreadMXBean,
                ManagementFactory.GarbageCollectorMXBeans,
                ManagementFactory.RuntimeMXBean,
                ManagementFactory.PlatformMBeanServer);

        /// <summary>
        /// The default instance of <seealso cref="VirtualMachineMetrics" />.
        /// </summary>
        /// <returns>the default {@link VirtualMachineMetrics instance}</returns>
        public static VirtualMachineMetrics GetInstance()
        {
            return INSTANCE;
        }

        /// <summary>
        /// Per-GC statistics.
        /// </summary>
        public class GarbageCollectorStats
        {
            private readonly long runs, timeMS;

            private GarbageCollectorStats(long runs, long timeMS)
            {
                this.runs = runs;
                this.timeMS = timeMS;
            }

            /// <summary>
            /// Returns the number of times the garbage collector has run.
            /// </summary>
            /// <returns>the number of times the garbage collector has run</returns>
            public long Runs
            {
                get => runs;
            }

            /// <summary>
            /// Returns the amount of time in the given unit the garbage collector has taken in total.
            /// </summary>
            /// <param name="unit">the time unit for the return value</param>
            /// <returns>the amount of time in the given unit the garbage collector</returns>
            public long GetTime(TimeUnit unit)
            {
                return unit.Convert(timeMS, TimeUnit.MILLISECONDS);
            }
        }

        /// <summary>
        /// The management interface for a buffer pool, for example a pool of {@link
        /// java.nio.ByteBuffer#allocateDirect direct} or {@link java.nio.MappedByteBuffer mapped}
        /// buffers.
        /// </summary>
        public class BufferPoolStats
        {
            private readonly long count, memoryUsed, totalCapacity;

            private BufferPoolStats(long count, long memoryUsed, long totalCapacity)
            {
                this.count = count;
                this.memoryUsed = memoryUsed;
                this.totalCapacity = totalCapacity;
            }

            /// <summary>
            /// Returns an estimate of the number of buffers in the pool.
            /// </summary>
            /// <returns>An estimate of the number of buffers in this pool</returns>
            public long Count
            {
                get => count;
            }

            /// <summary>
            /// Returns an estimate of the memory that the Java virtual machine is using for this buffer
            /// pool. The value returned by this method may differ from the estimate of the total {@link
            /// #getTotalCapacity capacity} of the buffers in this pool. This difference is explained by
            /// alignment, memory allocator, and other implementation specific reasons.
            /// </summary>
            /// <returns>An estimate of the memory that the Java virtual machine is using for this bufferpool in bytes, or {@code -1L} if an estimate of the memory usage is not
            /// available
            /// </returns>
            public long MemoryUsed
            {
                get => memoryUsed;
            }

            /// <summary>
            /// Returns an estimate of the total capacity of the buffers in this pool. A buffer's
            /// capacity is the number of elements it contains and the value returned by this method is
            /// an estimate of the total capacity of buffers in the pool in bytes.
            /// </summary>
            /// <returns>An estimate of the total capacity of the buffers in this pool in bytes</returns>
            public long TotalCapacity
            {
                get => totalCapacity;
            }
        }

        private readonly MemoryMXBean memory;
        private readonly IList<MemoryPoolMXBean> memoryPools;
        private readonly OperatingSystemMXBean os;
        private readonly ThreadMXBean threads;
        private readonly IList<GarbageCollectorMXBean> garbageCollectors;
        private readonly RuntimeMXBean runtime;
        private readonly MBeanServer mBeanServer;

        private VirtualMachineMetrics(MemoryMXBean memory,
                              IList<MemoryPoolMXBean> memoryPools,
                              OperatingSystemMXBean os,
                              ThreadMXBean threads,
                              IList<GarbageCollectorMXBean> garbageCollectors,
                              RuntimeMXBean runtime, MBeanServer mBeanServer)
        {
            this.memory = memory;
            this.memoryPools = memoryPools;
            this.os = os;
            this.threads = threads;
            this.garbageCollectors = garbageCollectors;
            this.runtime = runtime;
            this.mBeanServer = mBeanServer;
        }

        /// <summary>
        /// Returns the total initial memory of the current JVM.
        /// </summary>
        /// <returns>total Heap and non-heap initial JVM memory in bytes.</returns>
        public double TotalInit()
        {
            return memory.HeapMemoryUsage.Init +
                    memory.NonHeapMemoryUsage.Init;
        }

        /// <summary>
        /// Returns the total memory currently used by the current JVM.
        /// </summary>
        /// <returns>total Heap and non-heap memory currently used by JVM in bytes.</returns>
        public double TotalUsed()
        {
            return memory.HeapMemoryUsage.Used +
                    memory.NonHeapMemoryUsage.Used;
        }

        /// <summary>
        /// Returns the total memory currently used by the current JVM.
        /// </summary>
        /// <returns>total Heap and non-heap memory currently used by JVM in bytes.</returns>
        public double TotalMax()
        {
            return memory.HeapMemoryUsage.Max +
                    memory.NonHeapMemoryUsage.Max;
        }

        /// <summary>
        /// Returns the total memory committed to the JVM.
        /// </summary>
        /// <returns>total Heap and non-heap memory currently committed to the JVM in bytes.</returns>
        public double TotalCommitted()
        {
            return memory.HeapMemoryUsage.Committed +
                    memory.NonHeapMemoryUsage.Committed;
        }

        /// <summary>
        /// Returns the heap initial memory of the current JVM.
        /// </summary>
        /// <returns>Heap initial JVM memory in bytes.</returns>
        public double HeapInit()
        {
            return memory.HeapMemoryUsage.Init;
        }

        /// <summary>
        /// Returns the heap memory currently used by the current JVM.
        /// </summary>
        /// <returns>Heap memory currently used by JVM in bytes.</returns>
        public double HeapUsed()
        {
            return memory.HeapMemoryUsage.Used;
        }

        /// <summary>
        /// Returns the heap memory currently used by the current JVM.
        /// </summary>
        /// <returns>Heap memory currently used by JVM in bytes.</returns>
        public double HeapMax()
        {
            return memory.HeapMemoryUsage.Max;
        }

        /// <summary>
        /// Returns the heap memory committed to the JVM.
        /// </summary>
        /// <returns>Heap memory currently committed to the JVM in bytes.</returns>
        public double HeapCommitted()
        {
            return memory.HeapMemoryUsage.Committed;
        }

        /// <summary>
        /// Returns the percentage of the JVM's heap which is being used.
        /// </summary>
        /// <returns>the percentage of the JVM's heap which is being used</returns>
        public double HeapUsage()
        {
            MemoryUsage usage = memory.HeapMemoryUsage;
            return usage.Used / (double) usage.Max;
        }

        /// <summary>
        /// Returns the percentage of the JVM's non-heap memory (e.g., direct buffers) which is being
        /// used.
        /// </summary>
        /// <returns>the percentage of the JVM's non-heap memory which is being used</returns>
        public double NonHeapUsage()
        {
            MemoryUsage usage = memory.NonHeapMemoryUsage;
            return usage.Used / (double) usage.Max;
        }

        /// <summary>
        /// Returns a map of memory pool names to the percentage of that pool which is being used.
        /// </summary>
        /// <returns>a map of memory pool names to the percentage of that pool which is being used</returns>
        public IDictionary<string, Double> MemoryPoolUsage()
        {
            IDictionary<string, Double> pools = new SortedDictionary<string, Double>();
            foreach (MemoryPoolMXBean pool in memoryPools)
            {
                double max = pool.Usage.Max == -1 ?
                        pool.Usage.Committed :
                        pool.Usage.Max;
                pools.Put(pool.Name, pool.Usage.Used / max);
            }
            return Collections.UnmodifiableMap(pools);
        }

        /// <summary>
        /// Returns the percentage of available file descriptors which are currently in use.
        /// </summary>
        /// <returns>the percentage of available file descriptors which are currently in use, or {@codeNaN} if the running JVM does not have access to this information
        /// </returns>
        public double FileDescriptorUsage()
        {
            try
            {
                Method getOpenFileDescriptorCount = os.GetType().GetDeclaredMethod("getOpenFileDescriptorCount");
                getOpenFileDescriptorCount.Accessible = true;
                long? openFds = (long?) getOpenFileDescriptorCount.Invoke(os);
                Method getMaxFileDescriptorCount = os.GetType().GetDeclaredMethod("getMaxFileDescriptorCount");
                getMaxFileDescriptorCount.Accessible = true;
                long? maxFds = (long?) getMaxFileDescriptorCount.Invoke(os);
                return openFds.DoubleValue() / maxFds.DoubleValue();
            }
            catch (NoSuchMethodException e)
            {
                return Double.NaN;
            }
            catch (IllegalAccessException e)
            {
                return Double.NaN;
            }
            catch (InvocationTargetException e)
            {
                return Double.NaN;
            }
        }

        /// <summary>
        /// Returns the version of the currently-running jvm.
        /// </summary>
        /// <returns>the version of the currently-running jvm, eg "1.6.0_24"</returns>
        /// <unknown>@see &lt;a href="http://java.sun.com/j2se/versioning_naming.html"&gt;J2SE SDK/JRE RuntimeVersion String</unknown>
        public string Version()
        {
            return System.GetProperty("java.runtime.version");
        }

        /// <summary>
        /// Returns the name of the currently-running jvm.
        /// </summary>
        /// <returns>the name of the currently-running jvm, eg  "Java HotSpot(TM) Client VM"</returns>
        /// <unknown>@see &lt;a href="http://download.oracle.com/javase/6/docs/api/java/lang/System.html#getProperties()"&gt;System.getProperties()&lt;/a&gt;</unknown>
        public string Name()
        {
            return System.GetProperty("java.vm.name");
        }

        /// <summary>
        /// Returns the number of seconds the JVM process has been running.
        /// </summary>
        /// <returns>the number of seconds the JVM process has been running</returns>
        public long Uptime()
        {
            return TimeUnit.MILLISECONDS.ToSeconds(runtime.Uptime);
        }

        /// <summary>
        /// Returns the number of live threads (includes {@link #daemonThreadCount()}.
        /// </summary>
        /// <returns>the number of live threads</returns>
        public int ThreadCount()
        {
            return threads.ThreadCount;
        }

        /// <summary>
        /// Returns the number of live daemon threads.
        /// </summary>
        /// <returns>the number of live daemon threads</returns>
        public int DaemonThreadCount()
        {
            return threads.DaemonThreadCount;
        }

        /// <summary>
        /// Returns a map of garbage collector names to garbage collector information.
        /// </summary>
        /// <returns>a map of garbage collector names to garbage collector information</returns>
        public IDictionary<string, GarbageCollectorStats> GarbageCollectors()
        {
            IDictionary<string, GarbageCollectorStats> stats = new Dictionary<string, GarbageCollectorStats>();
            foreach (GarbageCollectorMXBean gc in garbageCollectors)
            {
                stats.Put(gc.Name,
                        new GarbageCollectorStats(gc.CollectionCount,
                                gc.CollectionTime));
            }
            return Collections.UnmodifiableMap(stats);
        }

        /// <summary>
        /// Returns a set of strings describing deadlocked threads, if any are deadlocked.
        /// </summary>
        /// <returns>a set of any deadlocked threads</returns>
        public ISet<string> DeadlockedThreads()
        {
            long[] threadIds = threads.FindDeadlockedThreads();
            if (threadIds != null)
            {
                ISet<string> threads = new HashSet<string>();
                foreach (ThreadInfo info in this.threads.GetThreadInfo(threadIds, MAX_STACK_TRACE_DEPTH))
                {
                    StringBuilder stackTrace = new StringBuilder();
                    foreach (StackTraceElement element in info.StackTrace)
                    {
                        stackTrace.Append("\t at ").Append(element.ToString()).Append('\n');
                    }

                    threads.Add(
                            string.Format(
                                    "{0} locked on {1} (owned by {2}):\n{3}",
                                    info.ThreadName, info.LockName,
                                    info.LockOwnerName,
                                    stackTrace.ToString()
                            )
                    );
                }
                return Collections.UnmodifiableSet(threads);
            }
            return new EmptySet<string>();
        }

        /// <summary>
        /// Returns a map of thread states to the percentage of all threads which are in that state.
        /// </summary>
        /// <returns>a map of thread states to percentages</returns>
        public IDictionary<State, Double> ThreadStatePercentages()
        {
            IDictionary<State, Double> conditions = new Dictionary<State, Double>();
            foreach (State state in State.Values())
            {
                conditions.Put(state, 0.0);
            }

            long[] allThreadIds = threads.AllThreadIds;
            ThreadInfo[] allThreads = threads.GetThreadInfo(allThreadIds);
            int liveCount = 0;
            foreach (ThreadInfo info in allThreads)
            {
                if (info != null)
                {
                    State state = info.ThreadState;
                    conditions.Put(state, conditions.Get(state) + 1);
                    liveCount++;
                }
            }
            foreach (State state in new List<State>(conditions.KeySet()))
            {
                conditions.Put(state, conditions.Get(state) / liveCount);
            }

            return Collections.UnmodifiableMap(conditions);
        }

        /// <summary>
        /// Dumps all of the threads' current information to an output stream.
        /// </summary>
        /// <param name="out">an output stream</param>
        public void ThreadDump(Stream @out)
        {
            ThreadInfo[] threads = this.threads.DumpAllThreads(true, true);
            TextWriter writer = new StreamWriter(@out);

            for (int ti = threads.Length - 1; ti >= 0; ti--)
            {
                ThreadInfo t = threads[ti];
                writer.Write("{0} id={1} state={2}",
                        t.ThreadName,
                        t.ThreadId,
                        t.ThreadState);
                LockInfo @lock = t.LockInfo;
                if (@lock != null && t.ThreadState != State.BLOCKED)
                {
                    writer.Write("\n    - waiting on <0x%08x> (a %s)",
                            @lock.IdentityHashCode,
                            @lock.ClassName);
                    writer.Write("\n    - @locked <0x%08x> (a %s)",
                            @lock.IdentityHashCode,
                            @lock.ClassName);
                }
                else if (@lock != null && t.ThreadState == State.BLOCKED)
                {
                    writer.Write("\n    - waiting to @lock <0x%08x> (a %s)",
                            @lock.IdentityHashCode,
                            @lock.ClassName);
                }

                if (t.IsSuspended)
                {
                    writer.Write(" (suspended)");
                }

                if (t.IsInNative)
                {
                    writer.Write(" (running in native)");
                }

                writer.WriteLine();
                if (t.LockOwnerName != null)
                {
                    writer.Write("     owned by {0} id={1}\n", t.LockOwnerName, t.LockOwnerId);
                }

                StackTraceElement[] elements = t.StackTrace;
                MonitorInfo[] monitors = t.LockedMonitors;

                for (int i = 0; i < elements.Length; i++)
                {
                    StackTraceElement element = elements[i];
                    writer.Printf("    at %s\n", element);
                    for (int j = 1; j < monitors.Length; j++)
                    {
                        MonitorInfo monitor = monitors[j];
                        if (monitor.LockedStackDepth == i) {
                            writer.Write("      - locked {0}\n", monitor);
                        }
                    }
                }
                writer.WriteLine();

                LockInfo[] locks = t.LockedSynchronizers;
                if (locks.Length > 0)
                {
                    writer.Write("    Locked synchronizers: count = %d\n", locks.Length);
                    foreach (LockInfo l in locks)
                    {
                        writer.Write("      - %s\n", l);
                    }
                    writer.WriteLine();
                }
            }

            writer.WriteLine();
            writer.Flush();
        }

        public IDictionary<string, BufferPoolStats> GetBufferPoolStats()
        {
            try
            {
                string[] attributes = { "Count", "MemoryUsed", "TotalCapacity" };

                ObjectName direct = new ObjectName("java.nio:type=BufferPool,name=direct");
                ObjectName mapped = new ObjectName("java.nio:type=BufferPool,name=mapped");

                AttributeList directAttributes = mBeanServer.GetAttributes(direct, attributes);
                AttributeList mappedAttributes = mBeanServer.GetAttributes(mapped, attributes);

                IDictionary<string, BufferPoolStats> stats = new SortedDictionary<string, BufferPoolStats>();

                BufferPoolStats directStats = new BufferPoolStats((long?) ((Attribute) directAttributes.Get(0)).Value,
                        (long?) ((Attribute) directAttributes.Get(1)).Value,
                        (long?) ((Attribute) directAttributes.Get(2)).Value);

                stats.Put("direct", directStats);

                BufferPoolStats mappedStats = new BufferPoolStats((long?) ((Attribute) mappedAttributes.Get(0)).Value,
                        (long?) ((Attribute) mappedAttributes.Get(1)).Value,
                        (long?) ((Attribute) mappedAttributes.Get(2)).Value);

                stats.Put("mapped", mappedStats);

                return Collections.UnmodifiableMap(stats);
            }
            catch (JMException e)
            {
                return Collections.EmptyMap();
            }
        }
    }
} // end of namespace