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
    public class CodegenExpressionConstantFalse : ICodegenExpression
    {
        internal static readonly CodegenExpressionConstantFalse INSTANCE = new CodegenExpressionConstantFalse();

        private CodegenExpressionConstantFalse()
        {
        }

        public void Render(TextWriter textWriter)
        {
            textWriter.Write("false");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
        }
    }
} // end of namespace