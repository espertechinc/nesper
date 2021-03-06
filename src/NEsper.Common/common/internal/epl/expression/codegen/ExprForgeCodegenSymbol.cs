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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.codegen
{
    public class ExprForgeCodegenSymbol : CodegenSymbolProvider
    {
        private readonly bool? newDataValue;

        private int currentParamNum;
        private CodegenExpressionRef optionalEPSRef;
        private CodegenExpressionRef optionalExprEvalCtxRef;
        private CodegenExpressionRef optionalIsNewDataRef;

        private IDictionary<int, EventTypeWithOptionalFlag> underlyingStreamNums =
            Collections.GetEmptyMap<int, EventTypeWithOptionalFlag>();

        public ExprForgeCodegenSymbol(
            bool allowUnderlyingReferences,
            bool? newDataValue)
        {
            IsAllowUnderlyingReferences = allowUnderlyingReferences;
            this.newDataValue = newDataValue;
        }

        public bool IsAllowUnderlyingReferences { get; }

        public virtual void Provide(IDictionary<string, Type> symbols)
        {
            if (optionalEPSRef != null) {
                symbols.Put(optionalEPSRef.Ref, typeof(EventBean[]));
            }

            if (optionalExprEvalCtxRef != null) {
                symbols.Put(optionalExprEvalCtxRef.Ref, typeof(ExprEvaluatorContext));
            }

            if (optionalIsNewDataRef != null) {
                symbols.Put(optionalIsNewDataRef.Ref, typeof(bool));
            }

            if (IsAllowUnderlyingReferences) {
                foreach (var entry in underlyingStreamNums) {
                    symbols.Put(entry.Value.Ref.Ref, entry.Value.EventType.UnderlyingType);
                }
            }
        }

        // --------------------------------------------------------------------------------
        // Adding the following elements to scope is a fundamental wash because the
        // scope for a lambda is the same as it's parent (assuming we're dealing with
        // a lambda).
        // --------------------------------------------------------------------------------

        public CodegenExpressionRef GetAddEPS(CodegenMethodScope scope)
        {
            if (optionalEPSRef == null) {
                optionalEPSRef = ExprForgeCodegenNames.REF_EPS;
            }

            scope.AddSymbol(optionalEPSRef);
            return optionalEPSRef;
        }

        public CodegenExpression GetAddIsNewData(CodegenMethodScope scope)
        {
            if (newDataValue != null) { // new-data can be a const
                return Constant(newDataValue);
            }

            if (optionalIsNewDataRef == null) {
                optionalIsNewDataRef = ExprForgeCodegenNames.REF_ISNEWDATA;
            }

            scope.AddSymbol(optionalIsNewDataRef);
            return optionalIsNewDataRef;
        }

        public CodegenExpressionRef GetAddExprEvalCtx(CodegenMethodScope scope)
        {
            if (optionalExprEvalCtxRef == null) {
                optionalExprEvalCtxRef = ExprForgeCodegenNames.REF_EXPREVALCONTEXT;
            }

            scope.AddSymbol(optionalExprEvalCtxRef);
            return optionalExprEvalCtxRef;
        }

        // --------------------------------------------------------------------------------

        public CodegenExpressionRef GetAddRequiredUnderlying(
            CodegenMethodScope scope,
            int streamNum,
            EventType eventType,
            bool optionalEvent)
        {
            if (underlyingStreamNums.IsEmpty()) {
                underlyingStreamNums = new Dictionary<int, EventTypeWithOptionalFlag>();
            }

            var existing = underlyingStreamNums.Get(streamNum);
            if (existing != null) {
                scope.AddSymbol(existing.Ref);
                return existing.Ref;
            }

            var assigned = Ref("u" + currentParamNum);
            underlyingStreamNums.Put(streamNum, new EventTypeWithOptionalFlag(assigned, eventType, optionalEvent));
            currentParamNum++;
            scope.AddSymbol(assigned);
            return assigned;
        }
        
        public void DerivedSymbolsCodegen(
            CodegenMethod parent,
            CodegenBlock processBlock,
            CodegenClassScope codegenClassScope)
        {
            foreach (var underlying in underlyingStreamNums) {
                var underlyingValue = underlying.Value;
                var underlyingType = underlyingValue.EventType.UnderlyingType;
                var name = underlying.Value.Ref.Ref;
                var arrayAtIndex = ArrayAtIndex(Ref(ExprForgeCodegenNames.NAME_EPS), Constant(underlying.Key));

                if (!underlyingValue.IsOptionalEvent) {
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
                        .AddParam(typeof(EventBean[]), ExprForgeCodegenNames.NAME_EPS);
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