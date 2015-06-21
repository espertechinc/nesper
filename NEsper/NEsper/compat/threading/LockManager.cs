///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.compat.threading
{
    public class LockManager
    {
        private static readonly Object CategoryFactoryTableLock = new object();
        private static readonly IDictionary<string, Func<ILockable>> CategoryFactoryTable =
            new Dictionary<string, Func<ILockable>>();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is telemetry enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is telemetry enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsTelemetryEnabled { get; set; }

        /// <summary>
        /// Gets or sets the default lock factory.
        /// </summary>
        /// <value>The default lock factory.</value>
        public static Func<ILockable> DefaultLockFactory { get; set; }

        /// <summary>
        /// Initializes the <see cref="LockManager"/> class.
        /// </summary>
        static LockManager()
        {
            DefaultLockFactory = CreateMonitorLock;
            IsTelemetryEnabled = false;

            // Establishes the default locking style
            var defaultLockTypeName = CompatSettings.Default.DefaultLockType;
            if (String.IsNullOrEmpty(defaultLockTypeName)) {
                return;
            }

            switch( defaultLockTypeName.ToUpper() ) {
                case "MONITOR":
                case "MONITORLOCK":
                    DefaultLockFactory = CreateMonitorLock;
                    break;
                case "SPIN":
                case "MONITORSPIN":
                case "MONITORSPINLOCK":
                    DefaultLockFactory = CreateMonitorSpinLock;
                    break;
                case "SLIM":
                case "MONITORSLIM":
                case "MONITORSLIMLOCK":
                    DefaultLockFactory = CreateMonitorSlimLock;
                    break;
                case "VOID":
                    DefaultLockFactory = CreateVoidLock;
                    break;
                default:
                    throw new ArgumentException("unknown lock type '" + defaultLockTypeName + "'");
            }
        }

        /// <summary>
        /// Registers the category lock.
        /// </summary>
        /// <param name="typeCategory">The type category.</param>
        /// <param name="lockFactory">The lock factory.</param>
        public static void RegisterCategoryLock(Type typeCategory, Func<ILockable> lockFactory)
        {
            RegisterCategoryLock(typeCategory.FullName, lockFactory);
        }

        /// <summary>
        /// Registers the category lock.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="lockFactory">The lock factory.</param>
        public static void RegisterCategoryLock(string category, Func<ILockable> lockFactory)
        {
            lock( CategoryFactoryTableLock ) {
                CategoryFactoryTable[category] = lockFactory;
            }
        }

        /// <summary>
        /// Creates a lock for the category defined by the type.
        /// </summary>
        /// <param name="typeCategory">The type category.</param>
        /// <returns></returns>
        public static ILockable CreateLock(Type typeCategory)
        {
            return CreateLock(typeCategory.FullName);
        }

        /// <summary>
        /// Wraps the lock.
        /// </summary>
        /// <returns></returns>
        private static ILockable WrapLock(ILockable @lock)
        {
            if (IsTelemetryEnabled) {
                return new TelemetryLock(@lock);
            }

            return @lock;
        }

        /// <summary>
        /// Creates a lock for the category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        public static ILockable CreateLock(string category)
        {
            if (category != null) {
                lock (CategoryFactoryTableLock) {
                    category = category.TrimEnd('.');

                    while( category != String.Empty ) {
                        Func<ILockable> lockFactory;
                        // Lookup a factory for the category
                        if (CategoryFactoryTable.TryGetValue(category, out lockFactory)) {
                            return WrapLock(lockFactory.Invoke());
                        }
                        // Lock factory not found, back-up one segment of the category
                        int lastIndex = category.LastIndexOf('.');
                        if (lastIndex == -1) {
                            break;
                        }

                        category = category.Substring(0, lastIndex).TrimEnd('.');
                    }
                }
            }

            return CreateDefaultLock();
        }

        /// <summary>
        /// Creates the default lock.
        /// </summary>
        /// <returns></returns>
        public static ILockable CreateDefaultLock()
        {
            var lockFactory = DefaultLockFactory;
            if (lockFactory == null) {
                throw new ApplicationException("default lock factory is not set");
            }

            return WrapLock(lockFactory.Invoke());
        }

        /// <summary>
        /// Creates the monitor lock.
        /// </summary>
        /// <returns></returns>
        public static ILockable CreateMonitorLock()
        {
            return new MonitorLock();
        }

        /// <summary>
        /// Creates the monitor spin lock.
        /// </summary>
        /// <returns></returns>
        public static ILockable CreateMonitorSpinLock()
        {
            return new MonitorSpinLock();
        }

        /// <summary>
        /// Creates the monitor slim lock.
        /// </summary>
        /// <returns></returns>
        public static ILockable CreateMonitorSlimLock()
        {
            return new MonitorSlimLock();
        }

        /// <summary>
        /// Creates a void lock.
        /// </summary>
        /// <returns></returns>
        public static ILockable CreateVoidLock()
        {
            return new VoidLock();
        }
    }
}
