///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementIfRefNullReturnFalse : CodegenStatement
    {
        private readonly string _var;

        public CodegenStatementIfRefNullReturnFalse(string var)
        {
            _var = var;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("if (");
            builder.Append(_var);
            builder.Append("== null) { return false;}\n");
        }

        public void MergeClasses(ISet<Type> classes)
        {
        }
        
        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
        }
    }
} // end of namespace