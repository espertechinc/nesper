///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// A for-clause is a means to specify listener and observer delivery.
    /// </summary>
    [Serializable]
    public class ForClause
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForClause"/> class.
        /// </summary>
        public ForClause()
        {
            Items = new List<ForClauseItem>();
        }

        /// <summary>Creates an empty group-by clause, to add to via add methods. </summary>
        /// <returns>group-by clause</returns>
        public static ForClause Create()
        {
            return new ForClause();
        }

        /// <summary>Returns for-clause items. </summary>
        /// <value>items</value>
        public IList<ForClauseItem> Items { get; set; }

        /// <summary>Renders the clause in textual representation. </summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            String delimiter = "";
            foreach (ForClauseItem child in Items) {
                writer.Write(delimiter);
                child.ToEPL(writer);
                delimiter = " ";
            }
        }
    }
}