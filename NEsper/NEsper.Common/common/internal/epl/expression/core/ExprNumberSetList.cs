///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     Expression for use within crontab to specify a list of values.
    /// </summary>
    [Serializable]
    public class ExprNumberSetList : ExprNodeBase,
        ExprForge,
        ExprEvaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string METHOD_HANDLEEXPRNUMBERSETLISTADD = "handleExprNumberSetListAdd";
        private const string METHOD_HANDLEEXPRNUMBERSETLISTEMPTY = "handleExprNumberSetListEmpty";

        [NonSerialized] private ExprEvaluator[] evaluators;

        public override ExprForge Forge => this;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            IList<NumberSetParameter> parameters = new List<NumberSetParameter>();
            foreach (var child in evaluators) {
                var value = child.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                HandleExprNumberSetListAdd(value, parameters);
            }

            HandleExprNumberSetListEmpty(parameters);
            return new ListParameter(parameters);
        }

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType => typeof(ListParameter);

        public ExprForgeConstantType ForgeConstantType {
            get {
                var max = -1;
                foreach (var child in ChildNodes) {
                    if (child.Forge.ForgeConstantType.Ordinal > max) {
                        max = child.Forge.ForgeConstantType.Ordinal;
                    }
                }

                if (max == -1) {
                    return ExprForgeConstantType.COMPILETIMECONST;
                }

                return ExprForgeConstantType.VALUES[max];
            }
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(ListParameter),
                typeof(ExprNumberSetList),
                codegenClassScope);
            var block = methodNode.Block
                .DeclareVar<IList<object>>("parameters", NewInstance(typeof(List<object>)));
            var count = -1;
            foreach (var node in ChildNodes) {
                count++;
                var forge = node.Forge;
                var evaluationType = forge.EvaluationType;
                var refname = "value" + count;
                block.DeclareVar(
                        evaluationType,
                        refname,
                        forge.EvaluateCodegen(requiredType, methodNode, exprSymbol, codegenClassScope))
                    .StaticMethod(
                        typeof(ExprNumberSetList),
                        METHOD_HANDLEEXPRNUMBERSETLISTADD,
                        Ref(refname),
                        Ref("parameters"));
            }

            block.StaticMethod(typeof(ExprNumberSetList), METHOD_HANDLEEXPRNUMBERSETLISTEMPTY, Ref("parameters"))
                .MethodReturn(NewInstance<ListParameter>(Ref("parameters")));
            return LocalMethod(methodNode);
        }

        public ExprNodeRenderable ExprForgeRenderable => this;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            var delimiter = "";

            writer.Write('[');

            foreach (var expr in ChildNodes) {
                writer.Write(delimiter);
                expr.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
                delimiter = ",";
            }

            writer.Write(']');
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return node is ExprNumberSetList;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // all nodes must either be int, frequency or range
            evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(ChildNodes);
            for (var i = 0; i < ChildNodes.Length; i++) {
                var type = ChildNodes[i].Forge.EvaluationType;
                if (type == typeof(FrequencyParameter) || type == typeof(RangeParameter)) {
                    continue;
                }

                if (!type.IsNumericNonFP()) {
                    throw new ExprValidationException("Frequency operator requires an integer-type parameter");
                }
            }

            return null;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="parameters">params</param>
        public static void HandleExprNumberSetListEmpty(IList<NumberSetParameter> parameters)
        {
            if (parameters.IsEmpty()) {
                Log.Warn("Empty list of values in list parameter, using upper bounds");
                parameters.Add(new IntParameter(int.MaxValue));
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="parameters">params</param>
        public static void HandleExprNumberSetListAdd(
            object value,
            IList<NumberSetParameter> parameters)
        {
            if (value == null) {
                Log.Info("Null value returned for lower bounds value in list parameter, skipping parameter");
                return;
            }

            if (value is FrequencyParameter || value is RangeParameter) {
                parameters.Add((NumberSetParameter) value);
                return;
            }

            int intValue = value.AsInt();
            parameters.Add(new IntParameter(intValue));
        }
    }
} // end of namespace