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

namespace com.espertech.esper.codegen.core
{
    public class CodegenMethod : ICodegenMethod
    {
        private readonly Type _returnType;
        private readonly string _methodName;
        private readonly IList<CodegenNamedParam> _parameters;
        private readonly string _optionalComment;
        private CodegenBlock _block;

        internal CodegenMethod(Type returnType, string methodName, IList<CodegenNamedParam> parameters, string optionalComment)
        {
            _returnType = returnType;
            _methodName = methodName;
            _parameters = parameters;
            _optionalComment = optionalComment;
        }

        public Type ReturnType => _returnType;

        public string MethodName => _methodName;

        public IList<CodegenNamedParam> Parameters => _parameters;

        public ICodegenBlock Statements
        {
            get
            {
                AllocateBlock();
                return _block;
            }
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            AllocateBlock();
            classes.Add(_returnType);
            _block.MergeClasses(classes);
            foreach (CodegenNamedParam param in _parameters)
            {
                param.MergeClasses(classes);
            }
        }

        public void Render(TextWriter textWriter, bool isPublic)
        {
            AllocateBlock();
            if (_optionalComment != null)
            {
                textWriter.Write("// ");
                textWriter.Write(_optionalComment);
                textWriter.Write("\n");
            }
            if (isPublic)
            {
                textWriter.Write("public ");
            }
            CodeGenerationHelper.AppendClassName(textWriter, _returnType, null);
            textWriter.Write(" ");
            textWriter.Write(_methodName);
            textWriter.Write("(");
            CodegenNamedParam.Render(textWriter, _parameters);
            textWriter.Write("){{\n");
            _block.Render(textWriter);
            textWriter.Write("}}\n");
        }

        private void AllocateBlock()
        {
            if (_block == null)
            {
                _block = new CodegenBlock(this);
            }
        }
    }
} // end of namespace