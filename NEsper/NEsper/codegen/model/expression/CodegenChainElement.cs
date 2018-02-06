///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenChainElement
    {
        private readonly string _method;
        private readonly Object[] _consts;

        public CodegenChainElement(string method, Object[] consts)
        {
            this._method = method;
            this._consts = consts;
        }

        public void Render(StringBuilder builder)
        {
            builder.Append(_method).Append("(");
            if (_consts != null)
            {
                string delimiter = "";
                foreach (Object constant in _consts)
                {
                    builder.Append(delimiter);
                    if ((constant is char[]) || (constant is string))
                    {
                        builder.Append("\"");
                        builder.Append(constant);
                        builder.Append("\"");
                    }
                    else
                    {
                        builder.Append(constant);
                    }
                    delimiter = ",";
                }
            }
            builder.Append(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
        }
    }
} // end of namespace