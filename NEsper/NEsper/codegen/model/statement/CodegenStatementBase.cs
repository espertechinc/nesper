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

namespace com.espertech.esper.codegen.model.statement
{
    public abstract class CodegenStatementBase : CodegenStatement
    {
        public abstract void RenderStatement(StringBuilder builder, IDictionary<Type, string> imports);
        public abstract void MergeClasses(ICollection<Type> classes);

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            RenderStatement(builder, imports);
            builder.Append(";\n");
        }
    }
} // end of namespace