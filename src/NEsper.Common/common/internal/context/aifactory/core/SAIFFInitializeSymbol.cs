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

namespace com.espertech.esper.common.@internal.context.aifactory.core
{
    public class SAIFFInitializeSymbol : CodegenSymbolProvider
    {
        public static readonly CodegenExpressionRef REF_STMTINITSVC = EPStatementInitServicesConstants.REF;

        private CodegenExpressionRef optionalInitServicesRef;

        public virtual void Provide(IDictionary<string, Type> symbols)
        {
            if (optionalInitServicesRef != null) {
                symbols.Put(optionalInitServicesRef.Ref, typeof(EPStatementInitServices));
            }
        }

        public CodegenExpressionRef GetAddInitSvc(CodegenMethodScope scope)
        {
            if (optionalInitServicesRef == null) {
                optionalInitServicesRef = REF_STMTINITSVC;
            }

            scope.AddSymbol(optionalInitServicesRef);
            return optionalInitServicesRef;
        }
    }
} // end of namespace