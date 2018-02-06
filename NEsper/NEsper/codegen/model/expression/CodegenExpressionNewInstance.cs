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

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionNewInstance : ICodegenExpression
    {
        private readonly Type _clazz;
        private readonly ICodegenExpression[] _parameters;

        public CodegenExpressionNewInstance(Type clazz, ICodegenExpression[] parameters)
        {
            this._clazz = clazz;
            this._parameters = parameters;
        }

        public void Render(TextWriter textWriter)
        {
            textWriter.Write("new ");
            CodeGenerationHelper.AppendClassName(textWriter, _clazz, null);
            textWriter.Write("(");
            CodegenExpressionBuilder.RenderExpressions(textWriter, _parameters);
            textWriter.Write(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(_clazz);
            CodegenExpressionBuilder.MergeClassesExpressions(classes, _parameters);
        }
    }
} // end of namespace