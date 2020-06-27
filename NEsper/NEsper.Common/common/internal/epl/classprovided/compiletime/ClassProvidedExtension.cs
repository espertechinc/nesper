///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.epl.classprovided.compiletime
{
    public interface ClassProvidedExtension : ExtensionClass,
        ExtensionSingleRow,
        ExtensionAggregationFunction,
        ExtensionAggregationMultiFunction
    {
        public void Add(
            IList<Type> classes,
            IDictionary<string, byte[]> bytes);

        public IDictionary<string, byte[]> GetBytes();
        public bool IsLocalInlinedClass(Type declaringClass);
    }
} // end of namespace