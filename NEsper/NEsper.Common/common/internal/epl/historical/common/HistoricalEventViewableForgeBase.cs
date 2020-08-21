///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.common
{
    public abstract class HistoricalEventViewableForgeBase : HistoricalEventViewableForge
    {
        private readonly EventType _eventType;
        private readonly int _streamNum;
        private readonly SortedSet<int> _subordinateStreams = new SortedSet<int>();
        private ExprForge[] _inputParamEvaluators;
        private int _scheduleCallbackId = -1;
        private MultiKeyClassRef _multiKeyClassRef;

        public HistoricalEventViewableForgeBase(
            int streamNum,
            EventType eventType)
        {
            this._streamNum = streamNum;
            this._eventType = eventType;
        }

        public EventType EventType => _eventType;

        public SortedSet<int> RequiredStreams => _subordinateStreams;

        public ExprForge[] InputParamEvaluators {
            get => _inputParamEvaluators;
            set => _inputParamEvaluators = value;
        }

        public MultiKeyClassRef MultiKeyClassRef {
            get => _multiKeyClassRef;
            set => _multiKeyClassRef = value;
        }

        public int StreamNum => _streamNum;

        public SortedSet<int> SubordinateStreams => _subordinateStreams;

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(TypeOfImplementation(), GetType(), classScope);
            var @ref = Ref("hist");
            var evaluator = MultiKeyCodegen.CodegenEvaluatorReturnObjectOrArray(InputParamEvaluators, method, GetType(), classScope);
            var transform = GetHistoricalLookupValueToMultiKey(method, classScope);

            var eventTypeExpr = EventTypeUtility.ResolveTypeCodegen(_eventType, symbols.GetAddInitSvc(method));
            method.Block
                .DeclareVar(TypeOfImplementation(), @ref.Ref, NewInstance(TypeOfImplementation()))
                .SetProperty(@ref, "StreamNumber", Constant(_streamNum))
                .SetProperty(@ref, "EventType", eventTypeExpr)
                .SetProperty(@ref, "HasRequiredStreams", Constant(!_subordinateStreams.IsEmpty()))
                .SetProperty(@ref, "ScheduleCallbackId", Constant(_scheduleCallbackId))
                .SetProperty(@ref, "Evaluator", evaluator)
                .SetProperty(@ref, "LookupValueToMultiKey", transform);
            
            CodegenSetter(@ref, method, symbols, classScope);
            
            method.Block
                .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add("AddReadyCallback", @ref))
                .MethodReturn(@ref);
            return LocalMethod(method);
        }


        private CodegenExpression GetHistoricalLookupValueToMultiKey(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            // CodegenExpressionNewAnonymousClass transformer = NewAnonymousClass(method.Block, typeof(HistoricalEventViewableLookupValueToMultiKey));
            // CodegenMethod transform = CodegenMethod
            //     .MakeParentNode(typeof(object), this.GetType(), classScope)
            //     .AddParam(typeof(object), "lv");
            // transformer.AddMethod("transform", transform);

            var transformer = new CodegenExpressionLambda(method.Block)
                .WithParam<object>("lv")
                .WithBody(
                    block => {

                        if (InputParamEvaluators.Length == 0) {
                            block.BlockReturn(ConstantNull());
                        }
                        else if (InputParamEvaluators.Length == 1) {
                            var paramType = InputParamEvaluators[0].EvaluationType;
                            if (paramType == null || !paramType.IsArray) {
                                block.BlockReturn(Ref("lv"));
                            }
                            else {
                                var mktype = MultiKeyPlanner.GetMKClassForComponentType(paramType.GetElementType());
                                block.BlockReturn(NewInstance(mktype, Cast(paramType, Ref("lv"))));
                            }
                        }
                        else {
                            block.DeclareVar<object[]>("values", Cast(typeof(object[]), Ref("lv")));
                            CodegenExpression[] expressions = new CodegenExpression[MultiKeyClassRef.MKTypes.Length];
                            for (int i = 0; i < expressions.Length; i++) {
                                expressions[i] = Cast(MultiKeyClassRef.MKTypes[i], ArrayAtIndex(Ref("values"), Constant(i)));
                            }

                            block.BlockReturn(NewInstanceInner(MultiKeyClassRef.ClassNameMK, expressions));
                        }
                    });
            
            return transformer;
        }

        public int ScheduleCallbackId {
            set => _scheduleCallbackId = value;
        }

        public abstract IList<StmtClassForgeableFactory> Validate(
            StreamTypeService typeService,
            StatementBaseInfo @base,
            StatementCompileTimeServices services);

        public abstract Type TypeOfImplementation();

        public abstract void CodegenSetter(
            CodegenExpressionRef @ref,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);
    }
} // end of namespace