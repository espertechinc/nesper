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

using com.espertech.esper.codegen.core;

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementIfRefNullReturnFalse : CodegenStatement
    {
        private readonly string var;

        public CodegenStatementIfRefNullReturnFalse(string var)
        {
            this.var = var;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            builder.Append("if (").Append(var).Append("== null) { return false;}\n");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
        }
    }
} // end of namespace