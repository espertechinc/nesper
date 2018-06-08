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
using com.espertech.esper.codegen.model.expression;

// import static com.espertech.esper.codegen.core.CodeGenerationHelper.appendClassName;

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementDeclareVar : CodegenStatementBase
    {
        private readonly Type _clazz;
        private readonly string _var;
        private readonly ICodegenExpression _initializer;

        public CodegenStatementDeclareVar(Type clazz, string var, ICodegenExpression initializer)
        {
            this._clazz = clazz;
            this._var = var;
            this._initializer = initializer;
        }

        public override void RenderStatement(TextWriter textWriter)
        {
            CodeGenerationHelper.AppendClassName(textWriter, _clazz, null);
            textWriter.Write(" ");
            textWriter.Write(_var);
            textWriter.Write("=");
            _initializer.Render(textWriter);
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(_clazz);
            _initializer.MergeClasses(classes);
        }
    }
} // end of namespace