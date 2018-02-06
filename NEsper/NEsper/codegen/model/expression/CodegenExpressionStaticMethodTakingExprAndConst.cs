///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.codegen.core;

//import static com.espertech.esper.codegen.core.CodeGenerationHelper.appendClassName;
//import static com.espertech.esper.codegen.model.expression.CodegenExpressionUtil.renderConstant;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionStaticMethodTakingExprAndConst : ICodegenExpression
    {
        private readonly Type _target;
        private readonly string _methodName;
        private readonly ICodegenExpression _expression;
        private readonly Object[] _consts;

        public CodegenExpressionStaticMethodTakingExprAndConst(Type target, string methodName, ICodegenExpression expression, Object[] consts)
        {
            this._target = target;
            this._methodName = methodName;
            this._expression = expression;
            this._consts = consts;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            CodeGenerationHelper.AppendClassName(builder, _target, null, imports);
            builder.Append(".");
            builder.Append(_methodName);
            builder.Append("(");
            _expression.Render(builder, imports);
            foreach (Object constant in _consts)
            {
                builder.Append(",");
                CodegenExpressionUtil.RenderConstant(builder, constant);
            }
            builder.Append(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            _expression.MergeClasses(classes);
            classes.Add(_target);
        }
    }
} // end of namespace