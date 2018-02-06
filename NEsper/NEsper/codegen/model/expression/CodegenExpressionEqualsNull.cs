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
    public class CodegenExpressionEqualsNull : ICodegenExpression
    {
        private readonly ICodegenExpression _lhs;
        private readonly bool _not;

        public CodegenExpressionEqualsNull(ICodegenExpression lhs, bool not)
        {
            this._lhs = lhs;
            this._not = not;
        }

        public void Render(TextWriter textWriter)
        {
            _lhs.Render(textWriter);
            textWriter.Write(" ");
            textWriter.Write(_not ? "!=" : "==");
            textWriter.Write(" null");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            _lhs.MergeClasses(classes);
        }
    }
} // end of namespace