///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Base junction for conjunction (and) and disjunction (or).
    /// </summary>
    [Serializable]
    public abstract class Junction : ExpressionBase
    {
        /// <summary>Expression to add to the conjunction (AND) or disjunction (OR). </summary>
        /// <param name="expression">to add</param>
        /// <returns>expression</returns>
        public Junction Add(Expression expression)
        {
            Children.Add(expression);
            return this;
        }

        /// <summary>Property to add to the conjunction (AND) or disjunction (OR). </summary>
        /// <param name="propertyName">is the name of the property</param>
        /// <returns>expression</returns>
        public Junction Add(String propertyName)
        {
            Children.Add(new PropertyValueExpression(propertyName));
            return this;
        }
    }
}
