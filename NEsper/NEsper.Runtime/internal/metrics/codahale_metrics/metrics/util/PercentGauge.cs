///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.util
{
    /// <summary>
    ///     A <seealso cref="RatioGauge" /> extension which returns a percentage, not a ratio.
    /// </summary>
    public abstract class PercentGauge : RatioGauge
    {
        private const int ONE_HUNDRED = 100;

        public override double Value => base.Value * ONE_HUNDRED;
    }
} // end of namespace
