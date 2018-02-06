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
    public class CodegenExpressionCastUnderlying : ICodegenExpression
    {
        private readonly Type _clazz;
        private readonly ICodegenExpression _expression;

        public CodegenExpressionCastUnderlying(Type clazz, ICodegenExpression expression)
        {
            this._clazz = clazz;
            this._expression = expression;
        }

        public void Render(TextWriter textWriter)
        {
            textWriter.Write("((");
            CodeGenerationHelper.AppendClassName(textWriter, _clazz, null);
            textWriter.Write(")");
            _expression.Render(textWriter);
            textWriter.Write(".");
            textWriter.Write("GetUnderlying())");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(_clazz);
            _expression.MergeClasses(classes);
        }
    }
} // end of namespace