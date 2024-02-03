///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.declared.core;

namespace com.espertech.esper.common.@internal.context.module
{
    public interface EPModuleExprDeclaredInitServices
    {
        ExprDeclaredCollector ExprDeclaredCollector { get; }
    }

    public static class EPModuleExprDeclaredInitServicesConstants
    {
        public static readonly CodegenExpressionRef REF = ModuleContextInitializeSymbol.REF_INITSVC;
        public static readonly string GETEXPRDECLAREDCOLLECTOR = "ExprDeclaredCollector";
    }
} // end of namespace