///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Holds property information for joined properties in a lookup.
    /// </summary>
    [Serializable]
    public class SubordPropInKeywordSingleIndex
    {
        public SubordPropInKeywordSingleIndex(String indexedProp, Type coercionType, IList<ExprNode> expressions)
        {
            IndexedProp = indexedProp;
            CoercionType = coercionType;
            Expressions = expressions;
        }

        public string IndexedProp { get; private set; }

        public Type CoercionType { get; private set; }

        public IList<ExprNode> Expressions { get; private set; }
    }
}