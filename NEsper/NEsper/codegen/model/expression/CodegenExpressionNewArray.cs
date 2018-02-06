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
using com.espertech.esper.util;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionNewArray : ICodegenExpression
    {
        private readonly Type _component;
        private readonly ICodegenExpression _expression;

        public CodegenExpressionNewArray(Type component, ICodegenExpression expression)
        {
            this._component = component;
            this._expression = expression;
        }

        public void Render(TextWriter textWriter)
        {
            int numDimensions = TypeHelper.GetNumberOfDimensions(_component);
            Type outermostType = TypeHelper.GetComponentTypeOutermost(_component);
            textWriter.Write("new ");
            CodeGenerationHelper.AppendClassName(textWriter, outermostType, null);
            textWriter.Write("[");
            _expression.Render(textWriter);
            textWriter.Write("]");
            for (int i = 0; i < numDimensions; i++)
            {
                textWriter.Write("[]");
            }
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(_component);
            _expression.MergeClasses(classes);
        }
    }
} // end of namespace