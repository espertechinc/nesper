///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.lookupplan
{
    /// <summary>
    ///     Holds property information for joined properties in a lookup.
    /// </summary>
    public class SubordPropInKeywordMultiIndex
    {
        public SubordPropInKeywordMultiIndex(
            string[] indexedProp,
            Type coercionType,
            ExprNode expression)
        {
            IndexedProp = indexedProp;
            CoercionType = coercionType;
            Expression = expression;
        }

        public string[] IndexedProp { get; private set; }

        public Type CoercionType { get; private set; }

        public ExprNode Expression { get; private set; }
    }
}