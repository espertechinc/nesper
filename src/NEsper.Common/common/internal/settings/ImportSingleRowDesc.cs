///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.hook.singlerowfunc;

namespace com.espertech.esper.common.@internal.settings
{
    /// <summary>
    ///     Provides information about a single-row function.
    /// </summary>
    [Serializable]
    public class ImportSingleRowDesc
    {
        public ImportSingleRowDesc(
            Type clazz,
            ExtensionSingleRowFunctionAttribute anno) : this(
            clazz.FullName,
            anno.MethodName,
            anno.ValueCache,
            anno.FilterOptimizable,
            anno.RethrowExceptions,
            anno.EventTypeName)
        {
        }

        public ImportSingleRowDesc(
            string className,
            string methodName,
            ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum valueCache,
            ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum filterOptimizable,
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

        public string ClassName { get; }

        public string MethodName { get; }

        public string OptionalEventTypeName { get; }

        public bool IsRethrowExceptions { get; }

        public ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum ValueCache { get; }

        public ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum FilterOptimizable { get; }
    }
} // end of namespace