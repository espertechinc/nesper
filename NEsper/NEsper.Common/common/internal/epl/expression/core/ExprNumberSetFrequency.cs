///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     Expression for use within crontab to specify a frequency.
    /// </summary>
    public class ExprNumberSetFrequency : ExprNodeBase,
        ExprForge,
        ExprEvaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [NonSerialized] private ExprEvaluator evaluator;

        public override ExprForge Forge => this;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.MINIMUM;

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var value = evaluator.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (value == null) {
                return HandleNumberSetFreqNullValue();
            }

            var intValue = value.AsInt();
            return new FrequencyParameter(intValue);
        }

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType => typeof(FrequencyParameter);

        public ExprNodeRenderable ForgeRenderable => this;

        public ExprForgeConstantType ForgeConstantType => ChildNodes[0].Forge.ForgeConstantType;

        public CodegenExpression EvaluateCodegen(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var forge = ChildNodes[0].Forge;
            var evaluationType = forge.EvaluationType;
            var methodNode = codegenMethodScope.MakeChild(
                typeof(FrequencyParameter), typeof(ExprNumberSetFrequency), codegenClassScope);
            var block = methodNode.Block
                .DeclareVar(
                    evaluationType, "value",
                    forge.EvaluateCodegen(requiredType, methodNode, exprSymbol, codegenClassScope));
            if (!evaluationType.IsPrimitive) {
                block.IfRefNull("value")
                    .BlockReturn(StaticMethod(typeof(ExprNumberSetFrequency), "handleNumberSetFreqNullValue"));
            }

            block.MethodReturn(
                NewInstance(
                    typeof(FrequencyParameter),
                    SimpleNumberCoercerFactory.CoercerInt.CodegenInt(Ref("value"), evaluationType)));
            return LocalMethod(methodNode);
        }

        public override void ToPrecedenceFreeEPL(StringWriter writer)
        {
            writer.Write("*/");
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            if (!(node is ExprNumberSetFrequency)) {
                return false;
            }

            return true;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            var forge = ChildNodes[0].Forge;
            if (!forge.EvaluationType.IsNumericNonFP()) {
                throw new ExprValidationException("Frequency operator requires an integer-type parameter");
            }

            evaluator = forge.ExprEvaluator;
            return null;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <returns>frequence params</returns>
        public static FrequencyParameter HandleNumberSetFreqNullValue()
        {
            Log.Warn("Null value returned for frequency parameter");
            return new FrequencyParameter(int.MaxValue);
        }
    }
} // end of namespace