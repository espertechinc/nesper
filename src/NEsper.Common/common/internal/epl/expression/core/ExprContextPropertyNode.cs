///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public interface ExprContextPropertyNode : ExprEnumerationForgeProvider
    {
        EventPropertyGetterSPI Getter { get; }

        string PropertyName { get; }

        Type ValueType { get; }
    }
} // end of namespace