///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.epl.spec
{
	[Serializable]
    public class SubstitutionParameterExpressionIndexed : SubstitutionParameterExpressionBase
	{
	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="index">is the index of the substitution parameter</param>
	    public SubstitutionParameterExpressionIndexed(int index)
	    {
	        Index = index;
	    }

	    protected override void ToPrecedenceFreeEPLUnsatisfied(TextWriter writer)
        {
	        writer.Write("?");
	    }

	    public int Index { get; private set; }
	}
} // end of namespace
