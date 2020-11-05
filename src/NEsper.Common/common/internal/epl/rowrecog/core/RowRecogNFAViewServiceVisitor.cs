///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.rowrecog.state;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    public interface RowRecogNFAViewServiceVisitor
    {
        void VisitUnpartitioned(RowRecogPartitionState state);
        void VisitPartitioned(IDictionary<object, RowRecogPartitionState> states);
    }
} // end of namespace