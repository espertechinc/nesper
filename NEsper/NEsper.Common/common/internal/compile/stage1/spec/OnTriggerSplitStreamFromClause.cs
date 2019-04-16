///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    [Serializable]
    public class OnTriggerSplitStreamFromClause
    {
        public OnTriggerSplitStreamFromClause(
            PropertyEvalSpec propertyEvalSpec,
            string optionalStreamName)
        {
            PropertyEvalSpec = propertyEvalSpec;
            OptionalStreamName = optionalStreamName;
        }

        public PropertyEvalSpec PropertyEvalSpec { get; set; }

        public string OptionalStreamName { get; set; }
    }
} // end of namespace