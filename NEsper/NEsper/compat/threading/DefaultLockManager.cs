///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.compat.threading
{
    public class DefaultLockManager : ILockManager
    {
        private readonly object _categoryFactoryTableLock = new object();
        private readonly IDictionary<string, Func<int, ILockable>> _categoryFactoryTable =
            new Dictionary<string, Func<int, ILockable>>();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is telemetry enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is telemetry enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsTelemetryEnabled { get; set; }

        /// <summary>
        /// Gets or sets the default lock factory.
        /// </summary>
        /// <value>The default lock factory.</value>
        public Func<int, ILockable> DefaultLockFactory { get; set; }

        /// <summary>
        /// Gets or sets the default lock timeout.
        /// </summary>
        /// <value>
        /// The default lock timeout.
        /// </value>
        public int DefaultLockTimeout { get; set; }

        /// <summary>
        /// Initializes the <see cref="DefaultLockManager" /> class.
        /// </summary>
        /// <param name="defaultLockFactory">The default lock factory.</param>
        public DefaultLockManager(Func<int, ILockable> defaultLockFactory)
        {
            DefaultLockTimeout = 60000;
            DefaultLockFactory = defaultLockFactory;
            IsTelemetryEnabled = false;
        }

        /// <summary>
        /// Registers the category lock.
        /// </summary>
        /// <param name="typeCategory">The type category.</param>
        /// <param name="lockFactory">The lock factory.</param>
        public void RegisterCategoryLock(Type typeCategory, Func<int, ILockable> lockFactory)
        {
            RegisterCategoryLock(typeCategory.FullName, lockFactory);
        }

        /// <summary>
        /// Registers the category lock.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="lockFactory">The lock factory.</param>
        public void RegisterCategoryLock(string category, Func<int, ILockable> lockFactory)
        {
            lock( _categoryFactoryTableLock ) {
                _categoryFactoryTable[category] = lockFactory;
            }
        }

        /// <summary>
        /// Creates a lock for the category defined by the type.
        /// </summary>
        /// <param name="typeCategory">The type category.</param>
        /// <returns></returns>
        public ILockable CreateLock(Type typeCategory)
        {
            return CreateLock(typeCategory.FullName);
        }

        /// <summary>
        /// Creates a specific type of lock.
        /// </summary>
        /// <param name="lockFactory">The lock factory.</param>
        /// <returns></returns>
        public ILockable CreateLock(Func<int, ILockable> lockFactory)
        {
            return WrapLock(lockFactory.Invoke(DefaultLockTimeout));
        }

        /// <summary>
        /// Wraps the lock.
        /// </summary>
        /// <returns></returns>
        private ILockable WrapLock(ILockable @lock)
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
        public ILockable CreateLock(string category)
        {
            if (category != null) {
                lock (_categoryFactoryTableLock) {
                    category = category.TrimEnd('.');

                    while( category != String.Empty ) {
                        Func<int, ILockable> lockFactory;
                        // Lookup a factory for the category
                        if (_categoryFactoryTable.TryGetValue(category, out lockFactory)) {
                            return WrapLock(lockFactory.Invoke(DefaultLockTimeout));
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
        public ILockable CreateDefaultLock()
        {
            var lockFactory = DefaultLockFactory;
            if (lockFactory == null) {
                throw new ApplicationException("default lock factory is not set");
            }

            return WrapLock(lockFactory.Invoke(DefaultLockTimeout));
        }
    }
}
