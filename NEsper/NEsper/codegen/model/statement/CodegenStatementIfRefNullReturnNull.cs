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

using com.espertech.esper.codegen.core;

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementIfRefNullReturnNull : ICodegenStatement
    {
        private readonly string _var;

        public CodegenStatementIfRefNullReturnNull(string var)
        {
            this._var = var;
        }

        public void Render(TextWriter textWriter)
        {
            textWriter.Write("if (");
            textWriter.Write(_var);
            textWriter.WriteLine("== null) {{ return null; }}");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
        }
    }
} // end of namespace