///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    public class CoercionDesc
    {
        public CoercionDesc(bool coerce, Type[] coercionTypes)
        {
            IsCoerce = coerce;
            CoercionTypes = coercionTypes;
        }

        public bool IsCoerce { get; private set; }

        public Type[] CoercionTypes { get; private set; }
    }
}