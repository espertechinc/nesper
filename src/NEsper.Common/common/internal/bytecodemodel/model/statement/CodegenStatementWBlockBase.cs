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

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public abstract class CodegenStatementWBlockBase : CodegenStatement
    {
        public CodegenStatementWBlockBase(CodegenBlock parent)
        {
            Parent = parent;
        }

        public CodegenBlock Parent { get; }

        public abstract void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent);

        public abstract void MergeClasses(ISet<Type> classes);

        public abstract void TraverseExpressions(Consumer<CodegenExpression> consumer);
    }
} // end of namespace