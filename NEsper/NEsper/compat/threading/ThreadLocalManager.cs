///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.threading
{
    public class ThreadLocalManager
    {
        internal static ThreadLocalFactory DefaultThreadLocalFactory { get; set; }

        /// <summary>
        /// Initializes the <see cref="ThreadLocalManager"/> class.
        /// </summary>
        static ThreadLocalManager()
        {
            // Establishes the default thread local style
            var defaultThreadLocalType = CompatSettings.Default.DefaultThreadLocalType;
            if (String.IsNullOrEmpty(defaultThreadLocalType)) {
                DefaultThreadLocalFactory = new FastThreadLocalFactory();
                return;
            }

            switch( defaultThreadLocalType.ToUpper() ) {
                case "FAST":
                    DefaultThreadLocalFactory = new FastThreadLocalFactory();
                    break;
                case "SLIM":
                    DefaultThreadLocalFactory = new SlimThreadLocalFactory();
                    break;
                case "XPER":
                    DefaultThreadLocalFactory = new XperThreadLocalFactory();
                    break;
                case "SYSTEM":
                    DefaultThreadLocalFactory = new SystemThreadLocalFactory();
                    break;
                default:
                    throw new ArgumentException("unknown thread local type '" + defaultThreadLocalType + "'");
            }
        }

        /// <summary>
        /// Creates a thread local instance.
        /// </summary>
        /// <returns></returns>
        public static IThreadLocal<T> Create<T>(FactoryDelegate<T> factoryDelegate)
            where T : class
        {
            return CreateDefaultThreadLocal(factoryDelegate);
        }


        /// <summary>
        /// Creates the default thread local.
        /// </summary>
        /// <returns></returns>
        public static IThreadLocal<T> CreateDefaultThreadLocal<T>(FactoryDelegate<T> factoryDelegate)
            where T : class
        {
            var localFactory = DefaultThreadLocalFactory;
            if (localFactory == null) {
                throw new ApplicationException("default thread local factory is not set");
            }

            return localFactory.CreateThreadLocal(factoryDelegate);
        }
    }
}
