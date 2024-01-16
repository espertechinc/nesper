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
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational; // LE

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.distinctof {
    public class EnumDistinctOfScalarNoParams : EnumForgeBasePlain,
        EnumForge,
        EnumEval {
        private readonly Type _fieldType;

        public EnumDistinctOfScalarNoParams(
            int streamCountIncoming,
            Type fieldType) : base(streamCountIncoming)
        {
            _fieldType = fieldType;
        }

        public override EnumEval EnumEvaluator => this;

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (enumcoll.Count <= 1) {
                return enumcoll;
            }

            if (enumcoll is ISet<object>) {
                return enumcoll;
            }

            return new HashSet<object>(enumcoll);
        }

        public override CodegenExpression Codegen(
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var componentType = args.EnumcollType.GetComponentType();
            var collectionType = typeof(ICollection<>).MakeGenericType(componentType);

            var method = codegenMethodScope
                .MakeChild(collectionType, typeof(EnumDistinctOfScalarNoParams), codegenClassScope)
                .AddParam(ExprForgeCodegenNames.FP_EPS)
                .AddParam(args.EnumcollType, EnumForgeCodegenNames.REF_ENUMCOLL.Ref)
                .AddParam(ExprForgeCodegenNames.FP_ISNEWDATA)
                .AddParam(ExprForgeCodegenNames.FP_EXPREVALCONTEXT);

            method.Block
                .IfCondition(Relational(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), LE, Constant(1)))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL);

            if (!(_fieldType is { IsArray: true })) {
                var setType = typeof(ISet<>).MakeGenericType(componentType);
                var hashSetType = typeof(LinkedHashSet<>).MakeGenericType(componentType);
                method.Block
                    .IfCondition(InstanceOf(Ref("enumcoll"), setType))
                    .BlockReturn(Cast(setType, EnumForgeCodegenNames.REF_ENUMCOLL))
                    .MethodReturn(NewInstance(hashSetType, EnumForgeCodegenNames.REF_ENUMCOLL));
            }
            else {
                var dictType = typeof(IDictionary<,>).MakeGenericType(typeof(object), componentType);
                var hashDictType = typeof(LinkedHashMap<,>).MakeGenericType(typeof(object), componentType);
                var arrayMK = MultiKeyPlanner.GetMKClassForComponentType(_fieldType.GetElementType());
                method.Block.DeclareVar(dictType, "distinct", NewInstance(hashDictType));

                var loop = method.Block.ForEachVar("next", EnumForgeCodegenNames.REF_ENUMCOLL);
                loop.DeclareVar(arrayMK, "comparable", NewInstance(arrayMK, Cast(_fieldType, Ref("next"))))
                    .Expression(ExprDotMethod(Ref("distinct"), "Put", Ref("comparable"), Ref("next")));

                method.Block.MethodReturn(ExprDotName(Ref("distinct"), "Values"));
            }

            return LocalMethod(method, args.Expressions);
        }
    }
} // end of namespace