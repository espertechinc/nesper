///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.util.serde;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashedGetterCRC32SerializedForge : EventPropertyValueGetterForge
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            IContainer container,
            object objectMayArray,
            int granularity,
            Serializer[] serializers)
        {
            return SerializeAndCRC32Hash(
                container.SerializerFactory(),
                objectMayArray,
                granularity,
                serializers);
        }
        
        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="objectMayArray">value</param>
        /// <param name="granularity">granularity</param>
        /// <param name="serializers">serializers</param>
        /// <returns>hash</returns>
        public static int SerializeAndCRC32Hash(
            SerializerFactory serializerFactory,
            object objectMayArray,
            int granularity,
            Serializer[] serializers)
        {
            byte[] bytes;
            try {
                if (objectMayArray is object[] array) {
                    bytes = serializerFactory.SerializeAndFlatten(serializers, array);
                }
                else {
                    bytes = serializerFactory.Serialize(serializers[0], objectMayArray);
                }
            }
            catch (IOException e) {
                Log.Error("Exception serializing parameters for computing consistent hash: " + e.Message, e);
                bytes = Array.Empty<byte>();
            }

            long value = bytes.GetCrc32();
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
                ExprDotMethod(
                    EnumValue(typeof(SerializerFactory), "Instance"),
                    "GetSerializers",
                    Constant(ExprNodeUtilityQuery.GetExprResultTypes(nodes))));

            var method = parent
                .MakeChild(typeof(object), GetType(), classScope)
                .AddParam<EventBean>("eventBean");
            method.Block
                .DeclareVar<EventBean[]>("events", NewArrayWithInit(typeof(EventBean), Ref("eventBean")));

            // method to return object-array from expressions
            var exprSymbol = new ExprForgeCodegenSymbol(true, null);
            var exprMethod = method
                .MakeChildWithScope(typeof(object), typeof(CodegenLegoMethodExpression), exprSymbol, classScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);
            var expressions = new CodegenExpression[nodes.Length];
            for (var i = 0; i < nodes.Length; i++) {
                expressions[i] = nodes[i]
                    .Forge.EvaluateCodegen(nodes[i].Forge.EvaluationType, exprMethod, exprSymbol, classScope);
            }

            exprSymbol.DerivedSymbolsCodegen(method, exprMethod.Block, classScope);

            if (nodes.Length == 1) {
                exprMethod.Block.MethodReturn(expressions[0]);
            }
            else {
                exprMethod.Block.DeclareVar<object[]>(
                    "values",
                    NewArrayByLength(typeof(object), Constant(nodes.Length)));
                for (var i = 0; i < nodes.Length; i++) {
                    var result = expressions[i];
                    exprMethod.Block.AssignArrayElement("values", Constant(i), result);
                }

                exprMethod.Block.MethodReturn(Ref("values"));
            }

            method.Block
                .DeclareVar<object>(
                    "values",
                    LocalMethod(exprMethod, Ref("events"), ConstantTrue(), ConstantNull()))
                .MethodReturn(
                    StaticMethod(
                        typeof(ContextControllerHashedGetterCRC32SerializedForge),
                        "SerializeAndCRC32Hash",
                        EnumValue(typeof(SerializerFactory), "Instance"),
                        Ref("values"),
                        Constant(granularity),
                        serializers));

            return LocalMethod(method, beanExpression);
        }
    }
} // end of namespace