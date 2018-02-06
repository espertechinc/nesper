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

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenChainElement
    {
        private readonly string _method;
        private readonly object[] _consts;

        public CodegenChainElement(string method, object[] consts)
        {
            this._method = method;
            this._consts = consts;
        }

        public void Render(TextWriter textWriter)
        {
            textWriter.Write(_method);
            textWriter.Write("(");
            if (_consts != null)
            {
                string delimiter = "";
                foreach (object constant in _consts)
                {
                    textWriter.Write(delimiter);
                    if ((constant is char[]) || (constant is string))
                    {
                        textWriter.Write("\"");
                        textWriter.Write(constant);
                        textWriter.Write("\"");
                    }
                    else
                    {
                        textWriter.Write(constant);
                    }
                    delimiter = ",";
                }
            }
            textWriter.Write(")");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
        }
    }
} // end of namespace