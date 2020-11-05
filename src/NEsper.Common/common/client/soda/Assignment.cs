///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     An assignment is an expression specifically for the purpose of usage in updates.
    ///     Usually an assignment is an equal-expression with the lhs being an event property or variable
    ///     and the rhs being the new value expression.
    /// </summary>
    [Serializable]
    public class Assignment
    {
        private Expression value;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public Assignment()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="value">value to assign</param>
        public Assignment(Expression value)
        {
            this.value = value;
        }

        /// <summary>
        ///     Returns expression to eval.
        /// </summary>
        /// <returns>eval expression</returns>
        public Expression Value
        {
            get => value;
            set => this.value = value;
        }
    }
} // end of namespace