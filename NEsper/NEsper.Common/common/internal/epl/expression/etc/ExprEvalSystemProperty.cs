///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalSystemProperty : ExprNodeBase,
        ExprForge,
        ExprEvaluator
    {
        public const string SYSTEM_PROPETIES_NAME = "systemproperties";

        private readonly string systemPropertyName;

        public ExprEvalSystemProperty(string systemPropertyName)
        {
            this.systemPropertyName = systemPropertyName;
        }

        public override ExprForge Forge => this;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return Environment.GetEnvironmentVariable(systemPropertyName);
        }

        public ExprEvaluator ExprEvaluator => this;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(typeof(Environment), "GetEnvironmentVariable", Constant(systemPropertyName));
        }

        public Type EvaluationType => typeof(string);

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            return null;
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get {
                return new ProxyExprNodeRenderable {
                    ProcToEPL = (
                        writer,
                        parentPrecedence) => {
                        ToPrecedenceFreeEPL(writer);
                    }
                };
            }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(SYSTEM_PROPETIES_NAME);
            writer.Write("'");
            writer.Write(systemPropertyName);
            writer.Write("'");
        }

        public override bool EqualsNode(
            ExprNode other,
            bool ignoreStreamPrefix)
        {
            if (this == other) {
                return true;
            }

            if (other == null || GetType() != other.GetType()) {
                return false;
            }

            var that = (ExprEvalSystemProperty) other;

            return systemPropertyName.Equals(that.systemPropertyName);
        }
    }
} // end of namespace