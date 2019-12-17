///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.annotation;

namespace com.espertech.esper.regressionlib.support.epl
{
    public enum SupportOutputLimitOpt
    {
        DEFAULT,
        ENABLED,
        DISABLED
    }

    public static class SupportOutputLimitOptExtensions
    {
        public static string GetHint(this SupportOutputLimitOpt value)
        {
            switch (value) {
                case SupportOutputLimitOpt.DEFAULT:
                    return "";

                case SupportOutputLimitOpt.ENABLED:
                    return "@Hint('" + HintEnum.ENABLE_OUTPUTLIMIT_OPT.GetValue() + "')";

                case SupportOutputLimitOpt.DISABLED:
                    return "@Hint('" + HintEnum.DISABLE_OUTPUTLIMIT_OPT.GetValue() + "')";

                default:
                    throw new ArgumentException(nameof(value));
            }
        }
    }
} // end of namespace