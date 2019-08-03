///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;

namespace com.espertech.esper.compiler.@internal.util
{
    public class EPCompilerSPIExpressionImpl : EPCompilerSPIExpression
    {
        private readonly ModuleCompileTimeServices moduleServices;

        public EPCompilerSPIExpressionImpl(ModuleCompileTimeServices moduleServices)
        {
            this.moduleServices = moduleServices;
        }

        public ExprNode CompileValidate(string expression)
        {
            var services = new StatementCompileTimeServices(0, moduleServices);

            ExprNode node;
            try {
                node = services.CompilerServices.CompileExpression(expression, services);
            }
            catch (ExprValidationException e) {
                throw new EPCompileException(
                    "Failed to compile expression '" + expression + "': " + e.Message,
                    e,
                    new EmptyList<EPCompileExceptionItem>());
            }

            try {
                ExprNodeUtilityValidate.ValidatePlainExpression(ExprNodeOrigin.API, node);

                StreamTypeService streamTypeService = new StreamTypeServiceImpl(true);
                var statementRawInfo = new StatementRawInfo(
                    0,
                    "API-provided",
                    null,
                    StatementType.INTERNAL_USE_API_COMPILE_EXPR,
                    null,
                    null,
                    new CompilableEPL(expression),
                    "API-provided");
                var validationContext =
                    new ExprValidationContextBuilder(streamTypeService, statementRawInfo, services).Build();
                node = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.API, node, validationContext);
            }
            catch (ExprValidationException e) {
                throw new EPCompileException(
                    "Failed to validate expression '" + expression + "': " + e.Message,
                    e,
                    new EmptyList<EPCompileExceptionItem>());
            }

            return node;
        }
    }
} // end of namespace