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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementCommentFullLine : CodegenStatementBase
    {
        private readonly string comment;

        public CodegenStatementCommentFullLine(string comment)
        {
            this.comment = comment;
        }

        public override void RenderStatement(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass)
        {
            builder.Append("// ").Append(comment);
        }

        public override void MergeClasses(ISet<Type> classes)
        {
        }
    }
} // end of namespace