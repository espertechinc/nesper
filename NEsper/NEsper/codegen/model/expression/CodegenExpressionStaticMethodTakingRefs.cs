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

//import static com.espertech.esper.codegen.core.CodeGenerationHelper.appendClassName;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionStaticMethodTakingRefs : ICodegenExpression
    {
        private readonly Type _target;
        private readonly string _methodName;
        private readonly string[] _refs;

        public CodegenExpressionStaticMethodTakingRefs(Type target, string methodName, string[] refs)
        {
            this._target = target;
            this._methodName = methodName;
            this._refs = refs;
        }

        public void Render(TextWriter textWriter)
        {
            CodeGenerationHelper.AppendClassName(textWriter, _target, null);
            textWriter.Write(".");
            textWriter.Write(_methodName);
            textWriter.Write("(");
            string delimiter = "";
            foreach (string parameter in _refs)
            {
                textWriter.Write(delimiter);
                textWriter.Write(parameter);
                delimiter = ",";
            }
            textWriter.Write(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(_target);
        }
    }
} // end of namespace