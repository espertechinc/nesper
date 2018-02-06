///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using com.espertech.esper.codegen.core;

// import static com.espertech.esper.codegen.core.CodeGenerationHelper.appendClassName;
// import static com.espertech.esper.codegen.model.expression.CodegenExpressionUtil.renderConstant;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionBeanUndCastDotMethodConst : ICodegenExpression
    {
        private readonly Type _clazz;
        private readonly ICodegenExpression _expression;
        private readonly string _method;
        private readonly object _constant;

        public CodegenExpressionBeanUndCastDotMethodConst(Type clazz, ICodegenExpression expression, string method, object constant)
        {
            this._clazz = clazz;
            this._expression = expression;
            this._method = method;
            this._constant = constant;
        }

        public void Render(TextWriter textWriter)
        {
            textWriter.Write("((");
            CodeGenerationHelper.AppendClassName(textWriter, _clazz, null);
            textWriter.Write(")");
            _expression.Render(textWriter);
            textWriter.Write(".Underlying).");
            textWriter.Write(_method);
            textWriter.Write("(");
            CodegenExpressionUtil.RenderConstant(textWriter, _constant);
            textWriter.Write(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            _expression.MergeClasses(classes);
            classes.Add(_clazz);
        }
    }
} // end of namespace