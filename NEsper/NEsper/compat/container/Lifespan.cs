///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.container
{
    public class Lifespan
    {
        public static readonly Lifespan Singleton = new Lifespan();
        public static readonly Lifespan Transient = new Lifespan();
        public static Lifespan TypeBound<T>()
        {
            return new LifespanTypeBound(typeof(T));
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="Lifespan"/> class from being created.
        /// </summary>
        private Lifespan()
        {
        }

        internal class LifespanTypeBound : Lifespan
        {
            internal Type BoundType { get; private set; }
            internal LifespanTypeBound(Type boundType)
            {
                BoundType = boundType;
            }
        }
    }
}
