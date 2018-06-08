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

using com.espertech.esper.codegen.model.expression;

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementIfConditionReturnConst : CodegenStatementBase
    {
        private readonly ICodegenExpression _condition;
        private readonly Object _constant;

        public CodegenStatementIfConditionReturnConst(ICodegenExpression condition, Object constant)
        {
            this._condition = condition;
            this._constant = constant;
        }

        public override void RenderStatement(TextWriter textWriter)
        {
            textWriter.Write("if (");
            _condition.Render(textWriter);
            textWriter.Write(") return ");
            CodegenExpressionUtil.RenderConstant(textWriter, _constant);
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            _condition.MergeClasses(classes);
        }
    }
} // end of namespace