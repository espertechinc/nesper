///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration.compiler;

namespace com.espertech.esper.common.client.hook.singlerowfunc
{
    /// <summary>
    ///     Annotation for use in EPL statements with inline classes for providing a plug-in single-row function.
    /// </summary>
    public class ExtensionSingleRowFunctionAttribute : Attribute
    {
        public ExtensionSingleRowFunctionAttribute()
        {
            ValueCache = ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED;
            FilterOptimizable = ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED;
            RethrowExceptions = false;
            EventTypeName = "";
        }

        public string Name { get; set; }
        public string MethodName { get; set; }
        public ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum ValueCache { get; set; }
        public ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum FilterOptimizable { get; set; }

        public bool RethrowExceptions { get; set; }
        public string EventTypeName { get; set; }
    }
} // end of namespace