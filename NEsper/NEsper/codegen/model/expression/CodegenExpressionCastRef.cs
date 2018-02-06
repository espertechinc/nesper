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

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionCastRef : ICodegenExpression
    {
        private readonly Type _clazz;
        private readonly string _ref;

        public CodegenExpressionCastRef(Type clazz, string @ref)
        {
            this._clazz = clazz;
            this._ref = @ref;
        }

        public void Render(TextWriter textWriter)
        {
            textWriter.Write("((");
            CodeGenerationHelper.AppendClassName(textWriter, _clazz, null);
            textWriter.Write(")");
            textWriter.Write(_ref);
            textWriter.Write(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(_clazz);
        }
    }
} // end of namespace