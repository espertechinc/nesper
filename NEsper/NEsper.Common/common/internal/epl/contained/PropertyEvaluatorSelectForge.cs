///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.contained
{
    /// <summary>
    ///     Property evaluator that considers a select-clauses and relies
    ///     on an accumulative property evaluator that presents events for all columns and rows.
    /// </summary>
    public class PropertyEvaluatorSelectForge : PropertyEvaluatorForge
    {
        private readonly PropertyEvaluatorAccumulativeForge accumulative;
        private readonly SelectExprProcessorDescriptor selectExprProcessor;

        public PropertyEvaluatorSelectForge(
            SelectExprProcessorDescriptor selectExprProcessor,
            PropertyEvaluatorAccumulativeForge accumulative)
        {
            this.selectExprProcessor = selectExprProcessor;
            this.accumulative = accumulative;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(PropertyEvaluatorSelect), GetType(), classScope);
            var processor = SelectExprProcessorUtil.MakeAnonymous(
                selectExprProcessor.Forge,
                method,
                symbols.GetAddInitSvc(method),
                classScope);
            method.Block
                .DeclareVar<PropertyEvaluatorSelect>("pe", NewInstance(typeof(PropertyEvaluatorSelect)))
                .SetProperty(
                    Ref("pe"),
                    "ResultEventType",
                    EventTypeUtility.ResolveTypeCodegen(
                        selectExprProcessor.Forge.ResultEventType,
                        symbols.GetAddInitSvc(method)))
                .SetProperty(Ref("pe"), "Accumulative", accumulative.Make(method, symbols, classScope))
                .SetProperty(Ref("pe"), "SelectExprProcessor", processor)
                .MethodReturn(Ref("pe"));
            return LocalMethod(method);
        }

        public EventType FragmentEventType => selectExprProcessor.Forge.ResultEventType;

        public bool CompareTo(PropertyEvaluatorForge other)
        {
            return false;
        }
    }
} // end of namespace