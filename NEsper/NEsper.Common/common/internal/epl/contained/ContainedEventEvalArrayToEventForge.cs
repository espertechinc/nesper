///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.contained
{
    public class ContainedEventEvalArrayToEventForge : ContainedEventEvalForge
    {
        private readonly ExprForge evaluator;
        private readonly EventBeanManufacturerForge manufacturer;

        public ContainedEventEvalArrayToEventForge(
            ExprForge evaluator,
            EventBeanManufacturerForge manufacturer)
        {
            this.evaluator = evaluator;
            this.manufacturer = manufacturer;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ContainedEventEvalArrayToEvent), GetType(), classScope);
            CodegenExpression eval = ExprNodeUtilityCodegen.CodegenEvaluator(evaluator, method, GetType(), classScope);
            method.Block
                .DeclareVar<EventBeanManufacturer>("manu", manufacturer.Make(method, classScope))
                .MethodReturn(NewInstance<ContainedEventEvalArrayToEvent>(eval, Ref("manu")));
            return LocalMethod(method);
        }
    }
} // end of namespace