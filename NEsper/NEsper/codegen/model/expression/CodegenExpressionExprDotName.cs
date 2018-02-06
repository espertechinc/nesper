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

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionExprDotName : ICodegenExpressionExprDotName
    {
        private readonly ICodegenExpression _lhs;
        private readonly string _name;

        public CodegenExpressionExprDotName(ICodegenExpression lhs, string name)
        {
            this._lhs = lhs;
            this._name = name;
        }

        public void Render(TextWriter textWriter)
        {
            if (_lhs is CodegenExpressionRef)
            {
                _lhs.Render(textWriter);
            }
            else
            {
                textWriter.Write("(");
                _lhs.Render(textWriter);
                textWriter.Write(")");
            }
            textWriter.Write('.');
            textWriter.Write(_name);
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            _lhs.MergeClasses(classes);
        }
    }
} // end of namespace