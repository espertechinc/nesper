///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;

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
    /// Expression for use within crontab to specify a range.
    /// <para/>Differs from the between-expression since the value returned by evaluating is a cron-value object.
    /// </summary>
    public class ExprNumberSetRange : ExprNodeBase,
        ExprForge,
        ExprEvaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        public const string METHOD_HANDLENUMBERSETRANGELOWERNULL = "HandleNumberSetRangeLowerNull";
        public const string METHOD_HANDLENUMBERSETRANGEUPPERNULL = "HandleNumberSetRangeUpperNull";
        
        [JsonIgnore]
        [NonSerialized]
        private ExprEvaluator[] evaluators;

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
            writer.Write(":");
            ChildNodes[1].ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;
        public ExprEvaluator ExprEvaluator => this;
        public Type EvaluationType => typeof(RangeParameter);
        public override ExprForge Forge => this;
        public ExprNodeRenderable ExprForgeRenderable => this;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return node is ExprNumberSetRange;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(ChildNodes);
            var typeOne = ChildNodes[0].Forge.EvaluationType;
            var typeTwo = ChildNodes[1].Forge.EvaluationType;
            if (!typeOne.IsTypeNumericNonFP() || !typeTwo.IsTypeNumericNonFP()) {
                throw new ExprValidationException("Range operator requires integer-type parameters");
            }

            return null;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var valueLower = evaluators[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            var valueUpper = evaluators[1].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (valueLower == null) {
                HandleNumberSetRangeLowerNull();
                valueLower = 0;
            }

            if (valueUpper == null) {
                HandleNumberSetRangeUpperNull();
                valueUpper = int.MaxValue;
            }

            var intValueLower = valueLower.AsInt32();
            var intValueUpper = valueUpper.AsInt32();
            return new RangeParameter(intValueLower, intValueUpper);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        public static void HandleNumberSetRangeLowerNull()
        {
            Log.Warn("Null value returned for lower bounds value in range parameter, using zero as lower bounds");
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        public static void HandleNumberSetRangeUpperNull()
        {
            Log.Warn("Null value returned for upper bounds value in range parameter, using max as upper bounds");
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var lowerValue = ChildNodes[0].Forge;
            var upperValue = ChildNodes[1].Forge;
            var lowerType = lowerValue.EvaluationType;
            var upperType = upperValue.EvaluationType;
            var methodNode = codegenMethodScope.MakeChild(
                typeof(RangeParameter),
                typeof(ExprNumberSetRange),
                codegenClassScope);
            var block = methodNode.Block
                .DeclareVar(
                    lowerType,
                    "valueLower",
                    lowerValue.EvaluateCodegen(requiredType, methodNode, exprSymbol, codegenClassScope))
                .DeclareVar(
                    upperType,
                    "valueUpper",
                    upperValue.EvaluateCodegen(requiredType, methodNode, exprSymbol, codegenClassScope));
            if (!lowerType.IsPrimitive) {
                block.IfRefNull("valueLower")
                    .StaticMethod(typeof(ExprNumberSetRange), METHOD_HANDLENUMBERSETRANGELOWERNULL)
                    .AssignRef("valueLower", Constant(0))
                    .BlockEnd();
            }

            if (!upperType.IsPrimitive) {
                block.IfRefNull("valueUpper")
                    .StaticMethod(typeof(ExprNumberSetRange), METHOD_HANDLENUMBERSETRANGEUPPERNULL)
                    .AssignRef("valueUpper", EnumValue(typeof(int), "MaxValue"))
                    .BlockEnd();
            }

            block.MethodReturn(
                NewInstance(
                    typeof(RangeParameter),
                    SimpleNumberCoercerFactory.CoercerInt.CodegenInt(Ref("valueLower"), lowerType),
                    SimpleNumberCoercerFactory.CoercerInt.CodegenInt(Ref("valueUpper"), upperType)));
            return LocalMethod(methodNode);
        }

        public ExprForgeConstantType ForgeConstantType {
            get {
                var ordinal = Math.Max(
                    ChildNodes[0].Forge.ForgeConstantType.Ordinal,
                    ChildNodes[1].Forge.ForgeConstantType.Ordinal);
                return EnumHelper.GetValue<ExprForgeConstantType>(ordinal);
            }
        }
    }
} // end of namespace