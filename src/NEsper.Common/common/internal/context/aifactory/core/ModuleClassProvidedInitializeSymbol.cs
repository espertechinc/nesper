///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.core
{
    public class ModuleClassProvidedInitializeSymbol : CodegenSymbolProvider
    {
        public static readonly CodegenExpressionRef REF_INITSVC = Ref("moduleClassProvidedInitSvc");

        private CodegenExpressionRef _optionalInitServicesRef;

        public void Provide(IDictionary<string, Type> symbols)
        {
            if (_optionalInitServicesRef != null) {
                symbols.Put(_optionalInitServicesRef.Ref, typeof(EPModuleClassProvidedInitServices));
            }
        }

        public CodegenExpressionRef GetAddInitSvc(CodegenMethodScope scope)
        {
            if (_optionalInitServicesRef == null) {
                _optionalInitServicesRef = REF_INITSVC;
            }

            scope.AddSymbol(_optionalInitServicesRef);
            return _optionalInitServicesRef;
        }
    }
} // end of namespace