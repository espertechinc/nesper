///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubselectUtil
    {
        public static string GetStreamName(
            string optionalStreamName,
            int subselectNumber)
        {
            var subexpressionStreamName = optionalStreamName;
            if (subexpressionStreamName == null) {
                subexpressionStreamName = "$subselect_" + subselectNumber;
            }

            return subexpressionStreamName;
        }
    }
} // end of namespace