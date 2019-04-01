///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.core
{
    /// <summary>Provides information about a single-row function.</summary>
    [Serializable]
    public class EngineImportSingleRowDesc
    {
        public EngineImportSingleRowDesc(
            string className,
            string methodName,
            ValueCacheEnum valueCache,
            FilterOptimizableEnum filterOptimizable,
            bool rethrowExceptions,
            string optionalEventTypeName)
        {
            ClassName = className;
            MethodName = methodName;
            ValueCache = valueCache;
            FilterOptimizable = filterOptimizable;
            IsRethrowExceptions = rethrowExceptions;
            OptionalEventTypeName = optionalEventTypeName;
        }

        public string ClassName { get; private set; }

        public string MethodName { get; private set; }

        public ValueCacheEnum ValueCache { get; private set; }

        public FilterOptimizableEnum FilterOptimizable { get; private set; }

        public bool IsRethrowExceptions { get; private set; }

        public string OptionalEventTypeName { get; private set; }
    }
} // end of namespace
