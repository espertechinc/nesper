///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public class ExprDotNodeFilterAnalyzerInputProp : ExprDotNodeFilterAnalyzerInput
    {
        public ExprDotNodeFilterAnalyzerInputProp(
            int streamNum,
            string propertyName)
        {
            StreamNum = streamNum;
            PropertyName = propertyName;
        }

        public int StreamNum { get; private set; }

        public string PropertyName { get; private set; }
    }
}