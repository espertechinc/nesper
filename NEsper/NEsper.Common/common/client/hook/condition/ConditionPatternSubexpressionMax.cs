///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.condition
{
    /// <summary>
    /// Indicates that the followed-by pattern operator, when parameterized with a 
    /// max number of sub-expressions, has reached that limit at runtime. 
    /// </summary>
    public class ConditionPatternSubexpressionMax : BaseCondition
    {
        /// <summary>Ctor. </summary>
        /// <param name="max">limit reached</param>
        public ConditionPatternSubexpressionMax(int max) {
            Max = max;
        }

        /// <summary>Returns the limit reached. </summary>
        /// <value>limit</value>
        public int Max { get; private set; }
    }
}
