///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.rowrecog.nfa;

namespace com.espertech.esper.common.@internal.epl.rowrecog.state
{
    public interface RowRecogPartitionState
    {
        RowRecogStateRandomAccess RandomAccess { get; }

        IEnumerator<RowRecogNFAStateEntry> CurrentStatesEnumerator { get; }

        IList<RowRecogNFAStateEntry> CurrentStates { get; set; }

        object OptionalKeys { get; }

        int NumStates { get; }

        IList<RowRecogNFAStateEntry> CurrentStatesForPrint { get; }

        bool IsEmptyCurrentState { get; }
    }
} // end of namespace