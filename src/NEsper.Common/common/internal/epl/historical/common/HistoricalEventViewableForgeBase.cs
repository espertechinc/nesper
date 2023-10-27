///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.compile.multikey.MultiKeyCodegen;

namespace com.espertech.esper.common.@internal.epl.historical.common
{
    public abstract class HistoricalEventViewableForgeBase : HistoricalEventViewableForge
    {
        private readonly EventType _eventType;
        private readonly int _streamNum;
        private readonly ISet<int> _subordinateStreams = new SortedSet<int>();
        private ExprForge[] _inputParamEvaluators;
        private int _scheduleCallbackId = -1;
        private MultiKeyClassRef _multiKeyClassRef;

        protected MultiKeyClassRef MultiKeyClassRef {
            get => _multiKeyClassRef;
            set => _multiKeyClassRef = value;
        }

        protected ISet<int> SubordinateStreams => _subordinateStreams;

        protected ExprForge[] InputParamEvaluators {
            get => _inputParamEvaluators;
            set => _inputParamEvaluators = value;
        }

        protected int StreamNum => _streamNum;

        public abstract Type TypeOfImplementation();

        public abstract void CodegenSetter(
            CodegenExpressionRef @ref,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        public HistoricalEventViewableForgeBase(
            int streamNum,
            EventType eventType)
        {
            this._streamNum = streamNum;
            this._eventType = eventType;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(TypeOfImplementation(), GetType(), classScope);
            var @ref = Ref("hist");
            var evaluator = CodegenEvaluatorReturnObjectOrArray(
                _inputParamEvaluators,
                method,
                GetType(),
                classScope);
            var transform = GetHistoricalLookupValueToMultiKey(method, classScope);
            method.Block.DeclareVarNewInstance(TypeOfImplementation(), @ref.Ref)
                .SetProperty(@ref, "StreamNumber", Constant(_streamNum))
                .SetProperty(@ref, "EventType", EventTypeUtility.ResolveTypeCodegen(_eventType, symbols.GetAddInitSvc(method)))
                .SetProperty(@ref, "HasRequiredStreams", Constant(!_subordinateStreams.IsEmpty()))
                .SetProperty(@ref, "ScheduleCallbackId", Constant(_scheduleCallbackId))
                .SetProperty(@ref, "Evaluator", evaluator)
                .SetProperty(@ref, "LookupValueToMultiKey", transform);
            CodegenSetter(@ref, method, symbols, classScope);
            method.Block.Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add("AddReadyCallback", @ref))
                .MethodReturn(@ref);
            return LocalMethod(method);
        }

        private CodegenExpression GetHistoricalLookupValueToMultiKey(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            // CodegenExpressionNewAnonymousClass transformer = NewAnonymousClass(
            //     method.Block,
            //     typeof(HistoricalEventViewableLookupValueToMultiKey));
            // CodegenMethod transform = CodegenMethod.MakeParentNode(typeof(object), GetType(), classScope)
            //     .AddParam<object>("lv");
            // transformer.AddMethod("transform", transform);

            var transformer = new CodegenExpressionLambda(method.Block)
                .WithParam<object>("lv")
                .WithBody(
                    block => {
                        if (_inputParamEvaluators.Length == 0) {
                            block.BlockReturn(ConstantNull());
                        }
                        else if (_inputParamEvaluators.Length == 1) {
                            var paramType = _inputParamEvaluators[0].EvaluationType;
                            if (paramType == null || !paramType.IsArray) {
                                block.BlockReturn(Ref("lv"));
                            }
                            else {
                                var paramClass = paramType;
                                var componentType = paramClass.GetComponentType();
                                var mktype = MultiKeyPlanner.GetMKClassForComponentType(componentType);
                                block.BlockReturn(NewInstance(mktype, Cast(paramClass, Ref("lv"))));
                            }
                        }
                        else {
                            block.DeclareVar<object[]>("values", Cast(typeof(object[]), Ref("lv")));
                            var expressions = new CodegenExpression[_multiKeyClassRef.MKTypes.Length];
                            for (var i = 0; i < expressions.Length; i++) {
                                var type = _multiKeyClassRef.MKTypes[i];
                                expressions[i] = type == null
                                    ? ConstantNull()
                                    : Cast(type, ArrayAtIndex(Ref("values"), Constant(i)));
                            }

                            var instance = MultiKeyClassRef.ClassNameMK.Type != null
                                ? NewInstance(MultiKeyClassRef.ClassNameMK.Type, expressions)
                                : NewInstanceInner(MultiKeyClassRef.ClassNameMK.Name, expressions);
                            
                            block.BlockReturn(instance);
                        }
                    });

            return transformer;
        }

        public EventType EventType => _eventType;

        public ISet<int> RequiredStreams => _subordinateStreams;

        public int ScheduleCallbackId {
            get => _scheduleCallbackId;
            set => _scheduleCallbackId = value;
        }

        public abstract IList<StmtClassForgeableFactory> Validate(
            StreamTypeService typeService,
            IDictionary<int, IList<ExprNode>> sqlParameters,
            StatementRawInfo rawInfo,
            StatementCompileTimeServices services);
    }
} // end of namespace