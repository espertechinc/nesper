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
    ///     Expression for a parameter within a crontab.
    ///     <para />
    ///     May have one subnode depending on the cron parameter type.
    /// </summary>
    public class ExprNumberSetCronParam : ExprNodeBase,
        ExprForge,
        ExprEvaluator
    {
        public const string METHOD_HANDLENUMBERSETCRONPARAMNULLVALUE = "handleNumberSetCronParamNullValue";
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [NonSerialized] private ExprEvaluator evaluator;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="cronOperator">type of cron parameter</param>
        public ExprNumberSetCronParam(CronOperatorEnum cronOperator)
        {
            CronOperator = cronOperator;
        }

        /// <summary>
        ///     Returns the cron parameter type.
        /// </summary>
        /// <returns>type of cron parameter</returns>
        public CronOperatorEnum CronOperator { get; }

        public override ExprForge Forge => this;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (ChildNodes.Length == 0) {
                return new CronParameter(CronOperator, null);
            }

            var value = evaluator.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (value == null) {
                HandleNumberSetCronParamNullValue();
                return new CronParameter(CronOperator, null);
            }

            int intValue = value.AsInt();
            return new CronParameter(CronOperator, intValue);
        }

        public ExprEvaluator ExprEvaluator => this;

        public ExprNodeRenderable ForgeRenderable => this;

        public Type EvaluationType => typeof(CronParameter);

        public CodegenExpression EvaluateCodegen(
            Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var enumValue = EnumValue(typeof(CronOperatorEnum), CronOperator.GetName());
            var defaultValue = NewInstance(typeof(CronParameter), enumValue, ConstantNull());
            if (ChildNodes.Length == 0) {
                return defaultValue;
            }

            var forge = ChildNodes[0].Forge;
            var evaluationType = forge.EvaluationType;
            var methodNode = codegenMethodScope.MakeChild(
                typeof(CronParameter), typeof(ExprNumberSetCronParam), codegenClassScope);

            var block = methodNode.Block
                .DeclareVar(
                    evaluationType, "value",
                    forge.EvaluateCodegen(requiredType, methodNode, exprSymbol, codegenClassScope));
            if (!evaluationType.IsPrimitive) {
                block.IfRefNull("value")
                    .StaticMethod(typeof(ExprNumberSetCronParam), METHOD_HANDLENUMBERSETCRONPARAMNULLVALUE)
                    .BlockReturn(defaultValue);
            }

            block.MethodReturn(
                NewInstance(
                    typeof(CronParameter), enumValue,
                    SimpleNumberCoercerFactory.SimpleNumberCoercerInt.CodegenInt(Ref("value"), evaluationType)));
            return LocalMethod(methodNode);
        }

        public override void ToPrecedenceFreeEPL(StringWriter writer)
        {
            if (ChildNodes.Length != 0) {
                ChildNodes[0].ToEPL(writer, Precedence);
                writer.Write(" ");
            }

            writer.Write(CronOperator.GetSyntax());
        }

        public ExprForgeConstantType ForgeConstantType {
            get {
                if (ChildNodes.Length == 0) {
                    return ExprForgeConstantType.COMPILETIMECONST;
                }

                return ChildNodes[0].Forge.ForgeConstantType;
            }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            if (!(node is ExprNumberSetCronParam)) {
                return false;
            }

            var other = (ExprNumberSetCronParam) node;
            return other.CronOperator.Equals(CronOperator);
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length == 0) {
                return null;
            }

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
        public static void HandleNumberSetCronParamNullValue()
        {
            Log.Warn("Null value returned for cron parameter");
        }
    }
} // end of namespace