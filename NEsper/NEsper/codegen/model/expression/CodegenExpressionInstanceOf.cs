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

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionInstanceOf : ICodegenExpression
    {
        private readonly ICodegenExpression _lhs;
        private readonly Type _clazz;
        private readonly bool _not;

        public CodegenExpressionInstanceOf(ICodegenExpression lhs, Type clazz, bool not)
        {
            this._lhs = lhs;
            this._clazz = clazz;
            this._not = not;
        }

        public void Render(TextWriter textWriter)
        {
            if (_not)
            {
                textWriter.Write("!(");
            }
            _lhs.Render(textWriter);
            textWriter.Write(" ");
            textWriter.Write("is ");
            CodeGenerationHelper.AppendClassName(textWriter, _clazz, null);
            if (_not)
            {
                textWriter.Write(")");
            }
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            _lhs.MergeClasses(classes);
            classes.Add(_clazz);
        }
    }
} // end of namespace