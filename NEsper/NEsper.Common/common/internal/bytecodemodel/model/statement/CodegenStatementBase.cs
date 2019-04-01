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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public abstract class CodegenStatementBase : CodegenStatement
    {
        public virtual void Render(
            StringBuilder builder, IDictionary<Type, string> imports, bool isInnerClass, int level,
            CodegenIndent indent)
        {
            RenderStatement(builder, imports, isInnerClass);
            builder.Append(";\n");
        }

        public abstract void RenderStatement(
            StringBuilder builder, IDictionary<Type, string> imports, bool isInnerClass);

        public abstract void MergeClasses(ISet<Type> classes);
    }
} // end of namespace