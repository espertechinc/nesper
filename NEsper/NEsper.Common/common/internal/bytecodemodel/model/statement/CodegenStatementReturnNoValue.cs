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
    public class CodegenStatementReturnNoValue : CodegenStatementBase,
        CodegenStatement
    {
        public static readonly CodegenStatementReturnNoValue INSTANCE = new CodegenStatementReturnNoValue();

        private CodegenStatementReturnNoValue()
        {
        }

        public override void MergeClasses(ISet<Type> classes)
        {
        }

        public override void RenderStatement(
            StringBuilder builder, IDictionary<Type, string> imports, bool isInnerClass)
        {
            builder.Append("return");
        }
    }
} // end of namespace