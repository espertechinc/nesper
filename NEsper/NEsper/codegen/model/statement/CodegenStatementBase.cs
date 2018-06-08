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

namespace com.espertech.esper.codegen.model.statement
{
    public abstract class CodegenStatementBase : ICodegenStatement
    {
        public abstract void RenderStatement(TextWriter textWriter);
        public abstract void MergeClasses(ICollection<Type> classes);

        public void Render(TextWriter textWriter)
        {
            RenderStatement(textWriter);
            textWriter.Write(";\n");
        }
    }
} // end of namespace