///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashedGetterCRC32SerializedForge : EventPropertyValueGetterForge
    {
        private static readonly ILog Log =
            LogManager.GetLogger(typeof(ContextControllerHashedGetterCRC32SerializedForge));

        private readonly ExprNode[] nodes;
        private readonly int granularity;

        public ContextControllerHashedGetterCRC32SerializedForge(
            IList<ExprNode> nodes,
            int granularity)
        {
            this.nodes = nodes.ToArray();
            this.granularity = granularity;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="objectMayArray">value</param>
        /// <param name="granularity">granularity</param>
        /// <param name="serializers">serializers</param>
        /// <returns>hash</returns>
        public static int SerializeAndCRC32Hash(
            object objectMayArray,
            int granularity,
            Serializer[] serializers)
        {
            byte[] bytes;
            try {
                if (objectMayArray is object[]) {
                    bytes = SerializerFactory.Serialize(serializers, (object[]) objectMayArray);
                }
                else {
                    bytes = SerializerFactory.Serialize(serializers[0], objectMayArray);
                }
            }
            catch (IOException e) {
                Log.Error("Exception serializing parameters for computing consistent hash: " + e.Message, e);
                bytes = new byte[0];
            }

            long value = ByteExtensions.GetCrc32(bytes);
            int result = (int) value % granularity;
            if (result >= 0) {
                return result;
            }

            return -result;
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var serializers = classScope.AddDefaultFieldUnshared(
                true,
                typeof(Serializer[]),
                StaticMethod(
                    typeof(SerializerFactory),
                    "GetSerializers",
                    Constant(ExprNodeUtilityQuery.GetExprResultTypes(nodes))));

            CodegenMethod method = parent.MakeChild(typeof(object), this.GetType(), classScope)
                .AddParam(typeof(EventBean), "eventBean");
            method.Block
                .DeclareVar<EventBean[]>("events", NewArrayWithInit(typeof(EventBean), @Ref("eventBean")));

            // method to return object-array from expressions
            ExprForgeCodegenSymbol exprSymbol = new ExprForgeCodegenSymbol(true, null);
            CodegenMethod exprMethod = method
                .MakeChildWithScope(typeof(object), typeof(CodegenLegoMethodExpression), exprSymbol, classScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);
            CodegenExpression[] expressions = new CodegenExpression[nodes.Length];
            for (int i = 0; i < nodes.Length; i++) {
                expressions[i] = nodes[i]
                    .Forge.EvaluateCodegen(
                        nodes[i].Forge.EvaluationType,
                        exprMethod,
                        exprSymbol,
                        classScope);
            }

            exprSymbol.DerivedSymbolsCodegen(method, exprMethod.Block, classScope);

            if (nodes.Length == 1) {
                exprMethod.Block.MethodReturn(expressions[0]);
            }
            else {
                exprMethod.Block.DeclareVar<object[]>(
                    "values",
                    NewArrayByLength(typeof(object), Constant(nodes.Length)));
                for (int i = 0; i < nodes.Length; i++) {
                    CodegenExpression result = expressions[i];
                    exprMethod.Block.AssignArrayElement("values", Constant(i), result);
                }

                exprMethod.Block.MethodReturn(@Ref("values"));
            }

            method.Block
                .DeclareVar<object>(
                    "values",
                    LocalMethod(exprMethod, @Ref("events"), ConstantTrue(), ConstantNull()))
                .MethodReturn(
                    StaticMethod(
                        typeof(ContextControllerHashedGetterCRC32SerializedForge),
                        "SerializeAndCRC32Hash",
                        @Ref("values"),
                        Constant(granularity),
                        serializers));

            return LocalMethod(method, beanExpression);
        }
    }
} // end of namespace