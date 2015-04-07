///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Provides information about a single-row function.
    /// </summary>
    [Serializable]
    public class EngineImportSingleRowDesc 
    {
        public EngineImportSingleRowDesc(String className, String methodName, ValueCache valueCache, FilterOptimizable filterOptimizable, bool rethrowExceptions)
        {
            ClassName = className;
            MethodName = methodName;
            ValueCache = valueCache;
            FilterOptimizable = filterOptimizable;
            RethrowExceptions = rethrowExceptions;
        }

        public string ClassName { get; private set; }

        public string MethodName { get; private set; }

        public ValueCache ValueCache { get; private set; }

        public FilterOptimizable FilterOptimizable { get; private set; }

        public Boolean RethrowExceptions { get; private set; }
    }
}
