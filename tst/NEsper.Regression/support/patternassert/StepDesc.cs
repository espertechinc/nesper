///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.patternassert
{
    public class StepDesc
    {
        public StepDesc(
            int step,
            object[][] newDataPerRow,
            object[][] oldDataPerRow)
        {
            Step = step;
            NewDataPerRow = newDataPerRow;
            OldDataPerRow = oldDataPerRow;
        }

        public int Step { get; }

        public object[][] NewDataPerRow { get; }

        public object[][] OldDataPerRow { get; }
    }
} // end of namespace