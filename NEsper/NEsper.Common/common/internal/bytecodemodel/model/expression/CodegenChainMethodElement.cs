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

using com.espertech.esper.common.@internal.bytecodemodel.core;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenChainMethodElement : CodegenChainElement
    {
        private readonly string _method;
        private readonly Type[] _methodTypeParameters;
        private readonly CodegenExpression[] _optionalParams;

        public CodegenChainMethodElement(
            string method,
            Type[] methodTypeParameters,
            CodegenExpression[] optionalParams)
        {
            _method = method;
            _methodTypeParameters = methodTypeParameters;
            _optionalParams = optionalParams;
        }

        public override void Render(
            StringBuilder builder,
            bool isInnerClass)
        {
            var indent = new CodegenIndent(true);
            builder.Append(_method);

            if (_methodTypeParameters != null && _methodTypeParameters.Length > 0) {
                var delimiter = "";
                builder.Append('<');

                foreach (var typeParameter in _methodTypeParameters) {
                    builder.Append(delimiter);
                    CodeGenerationHelper.AppendClassName(builder, typeParameter);
                    delimiter = ",";
                }

                builder.Append('>');
            }

            builder.Append('(');

            if (_optionalParams != null) {
                var delimiter = "";
                foreach (var param in _optionalParams) {
                    builder.Append(delimiter);
                    param.Render(builder, isInnerClass, 1, indent);
                    delimiter = ",";
                }
            }

            builder.Append(')');
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            if (_optionalParams != null) {
                foreach (var param in _optionalParams) {
                    param.MergeClasses(classes);
                }
            }
        }
    }
}