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

using com.espertech.esper.codegen.model.expression;

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementExpression : CodegenStatementBase
    {
        private readonly ICodegenExpression expression;

        public CodegenStatementExpression(ICodegenExpression expression)
        {
            this.expression = expression;
        }

        public override void RenderStatement(StringBuilder builder, IDictionary<Type, string> imports)
        {
            expression.Render(builder, imports);
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            expression.MergeClasses(classes);
        }
    }
} // end of namespace