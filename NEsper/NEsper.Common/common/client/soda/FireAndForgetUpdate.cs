///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>Fire-and-forget (on-demand) Update DML. </summary>
    [Serializable]
    public class FireAndForgetUpdate : FireAndForgetClause
    {
        /// <summary>Returns the set-assignments. </summary>
        /// <value>assignments</value>
        public IList<Assignment> Assignments { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FireAndForgetUpdate"/> class.
        /// </summary>
        public FireAndForgetUpdate()
        {
            Assignments = new List<Assignment>();
        }


        /// <summary>Add an assignment </summary>
        /// <param name="assignment">to add</param>
        /// <returns>assignment</returns>
        public IList<Assignment> AddAssignment(Assignment assignment)
        {
            Assignments.Add(assignment);
            return Assignments;
        }
    }
}