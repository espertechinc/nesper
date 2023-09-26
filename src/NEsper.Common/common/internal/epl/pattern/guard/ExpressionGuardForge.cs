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
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.compile.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
    public class ExpressionGuardForge : GuardForge
    {
        private MatchedEventConvertorForge convertor;

        private ExprNode expression;

        public void SetGuardParameters(
            IList<ExprNode> parameters,
            MatchedEventConvertorForge convertor,
            StatementCompileTimeServices services)
        {
            var errorMessage =
                "Expression pattern guard requires a single expression as a parameter returning a true or false (boolean) value";
            if (parameters.Count != 1) {
                throw new GuardParameterException(errorMessage);
            }

            expression = parameters[0];

            if (parameters[0].Forge.EvaluationType.GetBoxedType() != typeof(bool?)) {
                throw new GuardParameterException(errorMessage);
            }

            this.convertor = convertor;
        }

        public void CollectSchedule(
            short factoryNodeId,
            Func<short, CallbackAttribution> callbackAttribution,
            IList<ScheduleHandleTracked> schedules)
        {
            // nothing to collect
        }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ExpressionGuardFactory), GetType(), classScope);
            method.Block
                .DeclareVar<ExpressionGuardFactory>(
                    "factory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.PATTERNFACTORYSERVICE)
                        .Add("GuardWhile"))
                .SetProperty(Ref("factory"), "Convertor", convertor.MakeAnonymous(method, classScope))
                .SetProperty(
                    Ref("factory"),
                    "Expression",
                    ExprNodeUtilityCodegen.CodegenEvaluator(expression.Forge, method, GetType(), classScope))
                .MethodReturn(Ref("factory"));
            return LocalMethod(method);
        }
    }
} // end of namespace