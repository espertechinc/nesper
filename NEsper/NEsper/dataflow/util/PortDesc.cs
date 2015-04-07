///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

namespace com.espertech.esper.dataflow.util
{
    public class PortDesc
    {
        public PortDesc(int @operator, MethodInfo optionalMethod)
        {
            Operator = @operator;
            OptionalMethod = optionalMethod;
        }

        public int Operator { get; private set; }

        public MethodInfo OptionalMethod { get; private set; }

        public override String ToString()
        {
            return "{" +
                   "operator=" + Operator +
                   ", method=" + OptionalMethod +
                   '}';
        }
    }
}