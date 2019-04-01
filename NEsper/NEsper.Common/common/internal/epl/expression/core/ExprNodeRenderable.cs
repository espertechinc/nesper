///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public interface ExprNodeRenderable
    {
        void ToEPL(StringWriter writer, ExprPrecedenceEnum parentPrecedence);
    }

    public class ProxyExprNodeRenderable : ExprNodeRenderable
    {
        public Action<StringWriter, ExprPrecedenceEnum> ProcToEPL;

        public ProxyExprNodeRenderable()
        {
        }

        public ProxyExprNodeRenderable(Action<StringWriter, ExprPrecedenceEnum> procToEpl)
        {
            ProcToEPL = procToEpl;
        }

        public void ToEPL(StringWriter writer, ExprPrecedenceEnum parentPrecedence)
        {
            ProcToEPL(writer, parentPrecedence);
        }
    }
} // end of namespace