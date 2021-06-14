///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage1;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CompilableEPL : Compilable
    {
        public CompilableEPL(string epl)
        {
            Epl = epl;
        }

        public string Epl { get; }

        public string ToEPL()
        {
            return Epl;
        }

        public override string ToString()
        {
            return "CompilableEPL{" +
                   "epl='" +
                   Epl +
                   '\'' +
                   '}';
        }
    }
} // end of namespace