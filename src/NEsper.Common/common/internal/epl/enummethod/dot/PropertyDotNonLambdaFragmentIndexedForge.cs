///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class PropertyDotNonLambdaFragmentIndexedForge : ExprForge,
        ExprEvaluator,
        ExprNodeRenderable
    {
        private readonly int _streamId;
        private readonly EventPropertyGetterSPI _getter;
        private readonly ExprNode _indexExpr;
        private readonly string _propertyName;

        public PropertyDotNonLambdaFragmentIndexedForge(
            int streamId,
            EventPropertyGetterSPI getter,
            ExprNode indexExpr,
            string propertyName)
        {
            _streamId = streamId;
            _getter = getter;
            _indexExpr = indexExpr;
            _propertyName = propertyName;
        }

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType => typeof(EventBean);

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var @event = eventsPerStream[_streamId];
            if (@event == null) {
                return null;
            }

            var result = _getter.GetFragment(@event);
            if (result == null || !result.GetType().IsArray) {
                return null;
            }

            var events = (EventBean[])result;
            var index = _indexExpr.Forge.ExprEvaluator.Evaluate(eventsPerStream, isNewData, context).AsBoxedInt32();
            if (index == null) {
                return null;
            }

            return events[index.Value];
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(EventBean),
                typeof(PropertyDotNonLambdaFragmentIndexedForge),
                classScope);
            var refEps = symbols.GetAddEPS(method);
            method.Block
                .DeclareVar<EventBean>("@event", ArrayAtIndex(refEps, Constant(_streamId)))
                .IfRefNullReturnNull("@event")
                .DeclareVar<EventBean[]>(
                    "array",
                    Cast(typeof(EventBean[]), _getter.EventBeanFragmentCodegen(Ref("@event"), method, classScope)))
                .DeclareVar(
                    typeof(int?),
                    "index",
                    _indexExpr.Forge.EvaluateCodegen(typeof(int?), method, symbols, classScope))
                .IfRefNullReturnNull("index")
                .IfCondition(
                    Relational(
                        Ref("index"),
                        CodegenExpressionRelational.CodegenRelational.GE,
                        ArrayLength(Ref("array"))))
                .BlockThrow(
                    NewInstance(
                        typeof(EPException),
                        Concat(
                            Constant("Array length "),
                            ArrayLength(Ref("array")),
                            Constant(" less than index "),
                            Ref("index"),
                            Constant(" for property '" + _propertyName + "'"))))
                .MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        typeof(EventBean),
                        ArrayAtIndex(
                            Ref("array"),
                            Unbox(Ref("index")))));
            return LocalMethod(method);
        }

        public ExprNodeRenderable ExprForgeRenderable => this;

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(GetType().Name);
        }
    }
} // end of namespace