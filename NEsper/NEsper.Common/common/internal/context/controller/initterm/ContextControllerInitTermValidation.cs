///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public class ContextControllerInitTermValidation : ContextControllerPortableInfo
    {
        public static readonly ContextControllerPortableInfo INSTANCE = new ContextControllerInitTermValidation();

        private ContextControllerInitTermValidation()
        {
        }

        public CodegenExpression Make(CodegenExpressionRef addInitSvc)
        {
            return PublicConstValue(typeof(ContextControllerInitTermValidation), "INSTANCE");
        }

        public void ValidateStatement(
            string contextName,
            StatementSpecCompiled spec,
            StatementCompileTimeServices compileTimeServices)
        {
        }
    }
} // end of namespace