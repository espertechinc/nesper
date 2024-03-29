///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.codegen
{
    public class ExprForgeCodegenSymbol : CodegenSymbolProvider
    {
        private readonly bool _allowUnderlyingReferences;
        private readonly bool? _newDataValue;

        private int _currentParamNum;
        private IDictionary<int, EventTypeWithOptionalFlag> _underlyingStreamNums = EmptyDictionary<int, EventTypeWithOptionalFlag>.Instance;
        private CodegenExpressionRef _optionalEpsRef;
        private CodegenExpressionRef _optionalIsNewDataRef;
        private CodegenExpressionRef _optionalExprEvalCtxRef;

        public ExprForgeCodegenSymbol(
            bool allowUnderlyingReferences,
            bool? newDataValue)
        {
            _allowUnderlyingReferences = allowUnderlyingReferences;
            _newDataValue = newDataValue;
        }

        public bool IsAllowUnderlyingReferences => _allowUnderlyingReferences;

        public CodegenExpressionRef GetAddEps(CodegenMethodScope scope)
        {
            if (_optionalEpsRef == null) {
                _optionalEpsRef = ExprForgeCodegenNames.REF_EPS;
            }

            scope.AddSymbol(_optionalEpsRef);
            return _optionalEpsRef;
        }

        public CodegenExpression GetAddIsNewData(CodegenMethodScope scope)
        {
            if (_newDataValue != null) { // new-data can be a const
                return Constant(_newDataValue);
            }

            if (_optionalIsNewDataRef == null) {
                _optionalIsNewDataRef = ExprForgeCodegenNames.REF_ISNEWDATA;
            }

            scope.AddSymbol(_optionalIsNewDataRef);
            return _optionalIsNewDataRef;
        }

        public CodegenExpressionRef GetAddExprEvalCtx(CodegenMethodScope scope)
        {
            if (_optionalExprEvalCtxRef == null) {
                _optionalExprEvalCtxRef = ExprForgeCodegenNames.REF_EXPREVALCONTEXT;
            }

            scope.AddSymbol(_optionalExprEvalCtxRef);
            return _optionalExprEvalCtxRef;
        }

        public CodegenExpressionRef GetAddRequiredUnderlying(
            CodegenMethodScope scope,
            int streamNum,
            EventType eventType,
            bool optionalEvent)
        {
            if (_underlyingStreamNums.IsEmpty()) {
                _underlyingStreamNums = new Dictionary<int, EventTypeWithOptionalFlag>();
            }

            var existing = _underlyingStreamNums.Get(streamNum);
            if (existing != null) {
                scope.AddSymbol(existing.Ref);
                return existing.Ref;
            }

            var assigned = Ref("u" + _currentParamNum);
            _underlyingStreamNums.Put(streamNum, new EventTypeWithOptionalFlag(assigned, eventType, optionalEvent));
            _currentParamNum++;
            scope.AddSymbol(assigned);
            return assigned;
        }

        public virtual void Provide(IDictionary<string, Type> symbols)
        {
            if (_optionalEpsRef != null) {
                symbols.Put(_optionalEpsRef.Ref, typeof(EventBean[]));
            }

            if (_optionalExprEvalCtxRef != null) {
                symbols.Put(_optionalExprEvalCtxRef.Ref, typeof(ExprEvaluatorContext));
            }

            if (_optionalIsNewDataRef != null) {
                symbols.Put(_optionalIsNewDataRef.Ref, typeof(bool));
            }

            if (_allowUnderlyingReferences) {
                foreach (var entry in _underlyingStreamNums) {
                    symbols.Put(entry.Value.Ref.Ref, entry.Value.EventType.UnderlyingType);
                }
            }
        }

        public void DerivedSymbolsCodegen(
            CodegenMethod parent,
            CodegenBlock processBlock,
            CodegenClassScope codegenClassScope)
        {
            foreach (var underlying in _underlyingStreamNums) {
                var underlyingType = underlying.Value.EventType.UnderlyingType;
                var name = underlying.Value.Ref.Ref;
                var arrayAtIndex = ArrayAtIndex(Ref(ExprForgeCodegenNames.NAME_EPS), Constant(underlying.Key));

                if (!underlying.Value.IsOptionalEvent) {
                    // Unwrapping Method - non-optional event
                    // ----------------------------------------
                    // {underlyingType} {name} = ({underlyingType}) eps[someConstantIndex];
                    
                    processBlock.DeclareVar(
                        underlyingType,
                        name,
                        Cast(underlyingType, ExprDotUnderlying(arrayAtIndex)));
                }
                else {
                    // Unwrapping Method - optional event
                    // ----------------------------------------
                    // {underlyingType} M{N}(EventBean eps) {
                    //     EventBean @event = eps[someConstantIndex];
                    //     if (@event == null) {
                    //         return null;
                    //     }
                    //     return ({underlyingType}) @event.Underlying;
                    // }
                    
                    var methodNode = parent
                        .MakeChild(underlyingType, typeof(ExprForgeCodegenSymbol), codegenClassScope)
                        .AddParam<EventBean[]>(ExprForgeCodegenNames.NAME_EPS);
                    methodNode.Block
                        .DeclareVar<EventBean>("@event", arrayAtIndex)
                        .IfRefNullReturnNull("@event")
                        .MethodReturn(Cast(underlyingType, ExprDotUnderlying(Ref("@event"))));
                    processBlock.DeclareVar(
                        underlyingType,
                        name,
                        LocalMethod(methodNode, ExprForgeCodegenNames.REF_EPS));
                }
            }
        }
    }
} // end of namespace