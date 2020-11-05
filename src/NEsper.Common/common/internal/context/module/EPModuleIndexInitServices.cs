///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.index.compile;

namespace com.espertech.esper.common.@internal.context.module
{
    public interface EPModuleIndexInitServices
    {
        IndexCollector IndexCollector { get; }
    }

    public static class EPModuleIndexInitServicesConstants
    {
        public static readonly CodegenExpressionRef REF = ModuleIndexesInitializeSymbol.REF_INITSVC;
        public static readonly string INDEXCOLLECTOR = "IndexCollector";
    }
} // end of namespace