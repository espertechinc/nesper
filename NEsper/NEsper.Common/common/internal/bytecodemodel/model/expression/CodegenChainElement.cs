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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenChainElement
    {
        private readonly string _method;
        private readonly CodegenExpression[] _optionalParams;

        public CodegenChainElement(
            string method,
            CodegenExpression[] optionalParams)
        {
            this._method = method;
            this._optionalParams = optionalParams;
        }

        public void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass)
        {
            builder.Append(_method).Append("(");
            if (_optionalParams != null) {
                var delimiter = "";
                foreach (var param in _optionalParams) {
                    builder.Append(delimiter);
                    param.Render(builder, imports, isInnerClass);
                    delimiter = ",";
                }
            }

            builder.Append(")");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            if (_optionalParams != null) {
                foreach (var param in _optionalParams) {
                    param.MergeClasses(classes);
                }
            }
        }
    }
} // end of namespace