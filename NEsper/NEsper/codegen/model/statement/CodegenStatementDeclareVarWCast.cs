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
    public class CodegenStatementDeclareVarWCast : CodegenStatementBase
    {
        private readonly string _var;
        private readonly Type _clazz;
        private readonly string _rhsName;

        public CodegenStatementDeclareVarWCast(Type clazz, string var, string rhsName)
        {
            this._var = var;
            this._clazz = clazz;
            this._rhsName = rhsName;
        }

        public override void RenderStatement(TextWriter textWriter)
        {
            CodeGenerationHelper.AppendClassName(textWriter, _clazz, null);
            textWriter.Write(" ");
            textWriter.Write(_var);
            textWriter.Write("=");
            textWriter.Write("(");
            CodeGenerationHelper.AppendClassName(textWriter, _clazz, null);
            textWriter.Write(")");
            textWriter.Write(_rhsName);
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(_clazz);
        }
    }
} // end of namespace