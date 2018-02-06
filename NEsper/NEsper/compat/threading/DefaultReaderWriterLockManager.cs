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
    public class DefaultReaderWriterLockManager : IReaderWriterLockManager
    {
        private readonly Object _categoryFactoryTableLock = new object();
        private readonly IDictionary<string, Func<int, IReaderWriterLock>> _categoryFactoryTable =
            new Dictionary<string, Func<int, IReaderWriterLock>>();

        /// <summary>
        /// Engine that captures telemetry data.
        /// </summary>
        public readonly TelemetryEngine TelemetryEngine = new TelemetryEngine();

        /// <summary>
        /// Gets or sets a value indicating whether lock telemetry is enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is telemetry enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsTelemetryEnabled { get; set; }

        /// <summary>
        /// Gets or sets the default lock factory.
        /// </summary>
        /// <value>The default lock factory.</value>
        public Func<int, IReaderWriterLock> DefaultLockFactory { get; set; }

        /// <summary>
        /// Gets or sets the default lock timeout.
        /// </summary>
        /// <value>
        /// The default lock timeout.
        /// </value>
        public int DefaultLockTimeout { get; set; }

        /// <summary>
        /// Initializes the <see cref="DefaultLockManager"/> class.
        /// </summary>
        public DefaultReaderWriterLockManager(Func<int, IReaderWriterLock> defaultLockFactory)
        {
            DefaultLockTimeout = 60000;
            DefaultLockFactory = defaultLockFactory;
            IsTelemetryEnabled = false;
        }
        
        /// <summary>
        /// Registers the category lock.
        /// </summary>
        /// <param name="lockFactory">The lock factory.</param>
        public void RegisterCategoryLock<T>(Func<int, IReaderWriterLock> lockFactory)
        {
        	RegisterCategoryLock(typeof(T).FullName, lockFactory);
        }

        /// <summary>
        /// Registers the category lock.
        /// </summary>
        /// <param name="typeCategory">The type category.</param>
        /// <param name="lockFactory">The lock factory.</param>
        public void RegisterCategoryLock(Type typeCategory, Func<int, IReaderWriterLock> lockFactory)
        {
            RegisterCategoryLock(typeCategory.FullName, lockFactory);
        }

        /// <summary>
        /// Registers the category lock.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="lockFactory">The lock factory.</param>
        public void RegisterCategoryLock(string category, Func<int, IReaderWriterLock> lockFactory)
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
        public IReaderWriterLock CreateLock(Type typeCategory)
        {
            var typeName = typeCategory.FullName;
            if (typeName != null) {
                var typeNameIndex = typeName.IndexOf('`');
                if (typeNameIndex != -1) {
                    typeName = typeName.Substring(0, typeNameIndex);
                }
            }

            return CreateLock(typeName);
        }

        /// <summary>
        /// Creates a specific type of lock.
        /// </summary>
        /// <param name="lockFactory">The lock factory.</param>
        /// <returns></returns>
        public IReaderWriterLock CreateLock(Func<int, IReaderWriterLock> lockFactory)
        {
            return WrapLock(lockFactory.Invoke(DefaultLockTimeout), string.Empty);
        }

        /// <summary>
        /// Wraps the lock.
        /// </summary>
        /// <param name="readerWriterLock">The reader writer lock.</param>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        private IReaderWriterLock WrapLock(IReaderWriterLock readerWriterLock, string category)
        {
            if (IsTelemetryEnabled) {
                var rLockCategory = TelemetryEngine.GetCategory(string.Format("{0}.Read", category));
                var wLockCategory = TelemetryEngine.GetCategory(string.Format("{0}.Write", category));
                var lockObject = new TelemetryReaderWriterLock(readerWriterLock);
                lockObject.ReadLockReleased += rLockCategory.OnLockReleased;
                lockObject.WriteLockReleased += wLockCategory.OnLockReleased;
                return lockObject;
            }

            return readerWriterLock;
        }

        /// <summary>
        /// Creates a lock for the category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        public IReaderWriterLock CreateLock(string category)
        {
            var trueCategory = category;

            if (category != null) {
                lock (_categoryFactoryTableLock) {
                    trueCategory = category = category.TrimEnd('.');

                    while( category != String.Empty ) {
                        Func<int, IReaderWriterLock> lockFactory;
                        // Lookup a factory for the category
                        if (_categoryFactoryTable.TryGetValue(category, out lockFactory)) {
                            return WrapLock(lockFactory.Invoke(DefaultLockTimeout), category);
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

            return CreateDefaultLock(trueCategory);
        }

        /// <summary>
        /// Creates the default lock.
        /// </summary>
        /// <returns></returns>
        public IReaderWriterLock CreateDefaultLock()
        {
            return CreateDefaultLock(string.Empty);
        }

        /// <summary>
        /// Creates the default lock.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        public IReaderWriterLock CreateDefaultLock(string category)
        {
            var lockFactory = DefaultLockFactory;
            if (lockFactory == null) {
                throw new ApplicationException("default lock factory is not set");
            }

            return WrapLock(lockFactory.Invoke(DefaultLockTimeout), category);
        }
    }
}
