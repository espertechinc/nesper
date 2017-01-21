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
    public class ReaderWriterLockManager
    {
        private static readonly Object CategoryFactoryTableLock = new object();

        private static readonly IDictionary<string, Func<IReaderWriterLock>> CategoryFactoryTable =
            new Dictionary<string, Func<IReaderWriterLock>>();

        /// <summary>
        /// Engine that captures telemetry data.
        /// </summary>
        public static readonly TelemetryEngine TelemetryEngine =
            new TelemetryEngine();

        /// <summary>
        /// Gets or sets a value indicating whether lock telemetry is enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is telemetry enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsTelemetryEnabled { get; set; }

        /// <summary>
        /// Gets or sets the default lock factory.
        /// </summary>
        /// <value>The default lock factory.</value>
        public static Func<IReaderWriterLock> DefaultLockFactory { get; set; }

        /// <summary>
        /// Initializes the <see cref="LockManager"/> class.
        /// </summary>
        static ReaderWriterLockManager()
        {
            DefaultLockFactory = StandardLock; // NEW DEFAULT READER-WRITER LOCK
            IsTelemetryEnabled = false;

            // Establishes the default locking style
            var defaultLockTypeName = CompatSettings.Default.DefaultReaderWriterLockType;
            if (String.IsNullOrEmpty(defaultLockTypeName)) {
                return;
            }

            switch( defaultLockTypeName.ToUpper() ) {
                case "STD":
                case "STANDARD":
                    DefaultLockFactory = StandardLock;
                    break;
                case "SLIM":
                    DefaultLockFactory = SlimLock;
                    break;
                case "FAIR":
                    DefaultLockFactory = FairLock;
                    break;
                case "VOID":
                    DefaultLockFactory = VoidLock;
                    break;
                default:
                    throw new ArgumentException("unknown lock type '" + defaultLockTypeName + "'");
            }
        }
        
        /// <summary>
        /// Registers the category lock.
        /// </summary>
        /// <param name="lockFactory">The lock factory.</param>
        public static void RegisterCategoryLock<T>(Func<IReaderWriterLock> lockFactory)
        {
        	RegisterCategoryLock(typeof(T).FullName, lockFactory);
        }

        /// <summary>
        /// Registers the category lock.
        /// </summary>
        /// <param name="typeCategory">The type category.</param>
        /// <param name="lockFactory">The lock factory.</param>
        public static void RegisterCategoryLock(Type typeCategory, Func<IReaderWriterLock> lockFactory)
        {
            RegisterCategoryLock(typeCategory.FullName, lockFactory);
        }

        /// <summary>
        /// Registers the category lock.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="lockFactory">The lock factory.</param>
        public static void RegisterCategoryLock(string category, Func<IReaderWriterLock> lockFactory)
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
        public static IReaderWriterLock CreateLock(Type typeCategory)
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
        /// Wraps the lock.
        /// </summary>
        /// <param name="readerWriterLock">The reader writer lock.</param>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        private static IReaderWriterLock WrapLock(IReaderWriterLock readerWriterLock, string category)
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
        public static IReaderWriterLock CreateLock(string category)
        {
            var trueCategory = category;

            if (category != null) {
                lock (CategoryFactoryTableLock) {
                    trueCategory = category = category.TrimEnd('.');

                    while( category != String.Empty ) {
                        Func<IReaderWriterLock> lockFactory;
                        // Lookup a factory for the category
                        if (CategoryFactoryTable.TryGetValue(category, out lockFactory)) {
                            return WrapLock(lockFactory.Invoke(), category);
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
        public static IReaderWriterLock CreateDefaultLock()
        {
            return CreateDefaultLock(string.Empty);
        }

        /// <summary>
        /// Creates the default lock.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        private static IReaderWriterLock CreateDefaultLock(string category)
        {
            var lockFactory = DefaultLockFactory;
            if (lockFactory == null) {
                throw new ApplicationException("default lock factory is not set");
            }

            return WrapLock(lockFactory.Invoke(), category);
        }

        /// <summary>
        /// Creates a singularity lock.
        /// </summary>
        /// <returns></returns>
        public static IReaderWriterLock SingularityLock()
        {
            return new DummyReaderWriterLock();
        }

        /// <summary>
        /// Creates the standard reader writer lock.
        /// </summary>
        /// <returns></returns>
        public static IReaderWriterLock StandardLock()
        {
            return new StandardReaderWriterLock();
        }

        /// <summary>
        /// Creates the slim reader writer lock.
        /// </summary>
        /// <returns></returns>
        public static IReaderWriterLock SlimLock()
        {
            return new SlimReaderWriterLock();
        }

        /// <summary>
        /// Creates the void reader writer lock.
        /// </summary>
        /// <returns></returns>
        public static IReaderWriterLock VoidLock()
        {
            return new VoidReaderWriterLock();
        }

        /// <summary>
        /// Creates the fair reader writer lock.
        /// </summary>
        /// <returns></returns>
        public static IReaderWriterLock FairLock()
        {
            return new FairReaderWriterLock();
        }
        
        /// <summary>
        /// Creates the fifo reader writer lock.
        /// </summary>
        /// <returns></returns>
        public static IReaderWriterLock FifoLock()
        {
            return new FifoReaderWriterLock();
        }
    }
}
