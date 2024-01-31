///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence,
            ExprNodeRenderableFlags flags);
    }

    public class ProxyExprNodeRenderable : ExprNodeRenderable
    {
        public Action<TextWriter, ExprPrecedenceEnum, ExprNodeRenderableFlags> ProcToEPL;

        public ProxyExprNodeRenderable()
        {
        }

        public ProxyExprNodeRenderable(Action<TextWriter, ExprPrecedenceEnum, ExprNodeRenderableFlags> procToEpl)
        {
            ProcToEPL = procToEpl;
        }

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence,
            ExprNodeRenderableFlags flags)
        {
            ProcToEPL(writer, parentPrecedence, flags);
        }
    }
} // end of namespace