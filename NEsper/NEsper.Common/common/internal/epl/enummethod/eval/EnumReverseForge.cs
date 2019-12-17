///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumReverseForge : EnumEval,
        EnumForge
    {
        public EnumReverseForge(
            int numStreams,
            bool isScalar)
        {
            StreamNumSize = numStreams;
            IsScalar = isScalar;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (enumcoll.IsEmpty()) {
                return enumcoll;
            }

            var result = new List<object>(enumcoll);
            result.Reverse();
            return result;
        }
        
        public virtual EnumEval EnumEvaluator => this;

        public int StreamNumSize { get; }

        public bool IsScalar { get; }

        public CodegenExpression Codegen(
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var returnType = typeof(FlexCollection);
            var namedParams = EnumForgeCodegenNames.PARAMS;
            var collectionType = IsScalar
                ? typeof(List<object>)
                : typeof(List<EventBean>);
            
            var method = codegenMethodScope
                .MakeChild(returnType, typeof(EnumReverseForge), codegenClassScope)
                .AddParam(namedParams);
                
            var block = method.Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL);

            if (IsScalar) {
                var listType = typeof(List<object>);
                block.DeclareVar(
                    listType, 
                    "result",
                    NewInstance(
                        listType,
                        ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "ObjectCollection")));
            }
            else {
                var listType = typeof(List<EventBean>);
                block.DeclareVar(
                    listType, 
                    "result",
                    NewInstance(
                        listType,
                        ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "EventBeanCollection")));
            }

            block
                .ExprDotMethod(Ref("result"), "Reverse")
                .MethodReturn(FlexWrap(Ref("result")));
            
            return LocalMethod(method, args.Expressions);
        }
    }
} // end of namespace