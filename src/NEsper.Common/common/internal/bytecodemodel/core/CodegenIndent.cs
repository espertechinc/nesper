///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenIndent
    {
        private readonly bool indent;

        public CodegenIndent(bool indent)
        {
            this.indent = indent;
        }

        public void Indent(
            StringBuilder builder,
            int level)
        {
            if (!indent) {
                return;
            }

            for (var i = 0; i < level; i++) {
                builder.Append("  ");
            }
        }
    }
} // end of namespace