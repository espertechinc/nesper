///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.datetime.interval
{
    public abstract class IntervalComputerConstantBase
    {
        protected readonly long Start;
        protected readonly long End;

        protected IntervalComputerConstantBase(IntervalStartEndParameterPair pair, bool allowSwitch)
        {
            var startVal = pair.Start.OptionalConstant.Value;
            var endVal = pair.End.OptionalConstant.Value;

            if (startVal > endVal && allowSwitch)
            {
                Start = endVal;
                End = startVal;
            }
            else
            {
                Start = startVal;
                End = endVal;
            }
        }
    }
}