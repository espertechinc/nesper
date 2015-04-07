///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// An assignment to a variable or property name of an expression value.
    /// </summary>
    [Serializable]
    public class AssignmentPair 
    {
        /// <summary>Ctor. </summary>
        public AssignmentPair() {
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="name">property or variable</param>
        /// <param name="value">value to assign</param>
        public AssignmentPair(String name, Expression value) {
            Name = name;
            Value = value;
        }

        /// <summary>Returns property or variable name. </summary>
        /// <returns>name</returns>
        public string Name { get; set; }

        /// <summary>Returns expression to eval. </summary>
        /// <returns>eval expression</returns>
        public Expression Value { get; set; }
    }
}
