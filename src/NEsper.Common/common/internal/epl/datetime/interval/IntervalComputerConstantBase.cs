///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public abstract class IntervalComputerConstantBase
    {
        internal readonly long end;
        internal readonly long start;

        protected IntervalComputerConstantBase(
            IntervalStartEndParameterPairForge pair,
            bool allowSwitch)
        {
            var startVal = pair.Start.OptionalConstant.GetValueOrDefault();
            var endVal = pair.End.OptionalConstant.GetValueOrDefault();

            if (startVal > endVal && allowSwitch) {
                start = endVal;
                end = startVal;
            }
            else {
                start = startVal;
                end = endVal;
            }
        }
    }
} // end of namespace