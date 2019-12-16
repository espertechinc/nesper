///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.core
{
    public class ModuleScriptInitializeSymbol : CodegenSymbolProvider
    {
        public readonly static CodegenExpressionRef REF_INITSVC = Ref("moduleScriptInitSvc");

        private CodegenExpressionRef optionalInitServicesRef;

        public CodegenExpressionRef GetAddInitSvc(CodegenMethodScope scope)
        {
            if (optionalInitServicesRef == null) {
                optionalInitServicesRef = REF_INITSVC;
            }

            scope.AddSymbol(optionalInitServicesRef);
            return optionalInitServicesRef;
        }

        public void Provide(IDictionary<string, Type> symbols)
        {
            if (optionalInitServicesRef != null) {
                symbols.Put(optionalInitServicesRef.Ref, typeof(EPModuleScriptInitServices));
            }
        }
    }
} // end of namespace