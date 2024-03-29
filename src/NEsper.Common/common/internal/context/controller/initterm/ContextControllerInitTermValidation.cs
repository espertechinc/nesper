///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.compat.function;

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

        public void VisitFilterAddendumEventTypes(Consumer<EventType> consumer)
        {
            // no filter addendums added by controller
        }
    }
} // end of namespace