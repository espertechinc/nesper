///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodeRenderableFlags
    {
        public static readonly ExprNodeRenderableFlags DEFAULTFLAGS = new ExprNodeRenderableFlags(true);

        private bool _withStreamPrefix;

        public ExprNodeRenderableFlags(bool withStreamPrefix)
        {
            _withStreamPrefix = withStreamPrefix;
        }

        public bool WithStreamPrefix {
            get => _withStreamPrefix;
            set => _withStreamPrefix = value;
        }

        public bool IsWithStreamPrefix => _withStreamPrefix;
    }
}