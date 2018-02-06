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
// import static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder.renderExpressions;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionStaticMethodTakingAny : ICodegenExpression
    {
        private readonly Type _target;
        private readonly string _methodName;
        private readonly ICodegenExpression[] _parameters;

        public CodegenExpressionStaticMethodTakingAny(Type target, string methodName, ICodegenExpression[] parameters)
        {
            this._target = target;
            this._methodName = methodName;
            this._parameters = parameters;
        }

        public void Render(TextWriter textWriter)
        {
            CodeGenerationHelper.AppendClassName(textWriter, _target, null);
            textWriter.Write(".");
            textWriter.Write(_methodName);
            textWriter.Write("(");
            CodegenExpressionBuilder.RenderExpressions(textWriter, _parameters);
            textWriter.Write(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(_target);
            CodegenExpressionBuilder.MergeClassesExpressions(classes, _parameters);
        }
    }
} // end of namespace