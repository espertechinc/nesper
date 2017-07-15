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

namespace com.espertech.esper.client.util
{
    /// <summary>Default provider for classname lookups.</summary>
    public class ClassForNameProviderDefault : ClassForNameProvider {
        public static readonly ClassForNameProviderDefault INSTANCE = new ClassForNameProviderDefault();
    
        private ClassForNameProviderDefault() {
        }
    
        public Type ClassForName(string className) {
            ClassLoader cl = Thread.CurrentThread().ContextClassLoader;
            return Type.ForName(className, true, cl);
        }
    }
} // end of namespace
