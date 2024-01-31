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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public class SelectExprProcessorCodegenSymbol
    {
        public const string NAME_ISSYNTHESIZE = "isSynthesize";
        public const string LAMBDA_NAME_ISSYNTHESIZE = "_isSynthesize";

        public static readonly CodegenExpressionRef REF_ISSYNTHESIZE =
            new CodegenExpressionRef(NAME_ISSYNTHESIZE);

        public static readonly CodegenExpressionRef LAMBDA_REF_ISSYNTHESIZE =
            new CodegenExpressionRef(LAMBDA_NAME_ISSYNTHESIZE);

        private CodegenExpressionRef optionalSynthesizeRef;

        public CodegenExpressionRef GetAddSynthesize(CodegenMethod processMethod)
        {
            if (optionalSynthesizeRef == null) {
                optionalSynthesizeRef = REF_ISSYNTHESIZE;
            }

            processMethod.AddSymbol(optionalSynthesizeRef);
            return optionalSynthesizeRef;
        }

        public void Provide(IDictionary<string, Type> symbols)
        {
            if (optionalSynthesizeRef != null) {
                symbols.Put(optionalSynthesizeRef.Ref, typeof(bool));
            }
        }
    }
} // end of namespace