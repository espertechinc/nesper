///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.statement;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionLambda : CodegenStatementWBlockBase,
        CodegenExpression
    {
        public CodegenExpressionLambda(CodegenBlock parent) : base(parent)
        {
            ParamNames = new List<CodegenNamedParam>();
            Block = new CodegenBlock(this);
        }

        public CodegenExpressionLambda(
            CodegenBlock parent,
            IList<CodegenNamedParam> paramNames) : base(parent)
        {
            ParamNames = new List<CodegenNamedParam>(paramNames);
            Block = new CodegenBlock();
        }

        public Type LambdaType { get; internal set; }

        public IList<CodegenNamedParam> ParamNames { get; }

        public CodegenBlock Block { get; }

        public CodegenExpressionLambda WithLambdaType(Type lambdaType)
        {
            LambdaType = lambdaType;
            return this;
        }

        public CodegenExpressionLambda WithParams(params CodegenNamedParam[] argNames)
        {
            ParamNames.AddAll(argNames);
            return this;
        }

        public CodegenExpressionLambda WithParams(IEnumerable<CodegenNamedParam> argNames)
        {
            ParamNames.AddAll(argNames);
            return this;
        }

        public CodegenExpressionLambda WithParam(CodegenNamedParam param)
        {
            ParamNames.Add(param);
            return this;
        }

        public CodegenExpressionLambda WithParam(
            Type paramType,
            string paramName)
        {
            ParamNames.Add(new CodegenNamedParam(paramType, paramName));
            return this;
        }

        public CodegenExpressionLambda WithParam<T>(string paramName)
        {
            ParamNames.Add(new CodegenNamedParam(typeof(T), paramName));
            return this;
        }

        public CodegenExpressionLambda WithBody(Consumer<CodegenBlock> blockHandler)
        {
            blockHandler.Invoke(Block);
            return this;
        }

        public override void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            if (LambdaType != null) {
                builder.Append("new ");
                CodeGenerationHelper.AppendClassName(builder, LambdaType);
                builder.Append("(");
            }

            builder.Append('(');

            var delimiter = "";
            foreach (var parameter in ParamNames) {
                builder.Append(delimiter);
                parameter.Render(builder);
                delimiter = ",";
            }

            builder.Append(") => {");

            Block.Render(builder, isInnerClass, level + 1, indent);

            builder.Append("}");

            if (LambdaType != null) {
                builder.Append(")");
            }
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            ParamNames.ForEach(paramName => paramName.MergeClasses(classes));
            Block.MergeClasses(classes);
        }

        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            // TODO
            Block.Statements.ForEach(statement => { statement.TraverseExpressions(consumer); });
        }
    }
} // end of namespace