///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Represents a wildcard in the select-clause.
    /// </summary>
	[Serializable]
    public class SelectClauseWildcard : SelectClauseElement
	{
	    /// <summary>Renders the element in textual representation.</summary>
	    /// <param name="writer">to output to</param>
        public void ToEPLElement(TextWriter writer)
	    {
	        writer.Write("*");
	    }
	}
} // End of namespace
