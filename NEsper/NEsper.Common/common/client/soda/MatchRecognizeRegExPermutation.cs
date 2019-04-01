///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
	/// <summary>
	/// Interface representing a permutation expression for use in match-recognize.
	/// </summary>
	[Serializable]
    public class MatchRecognizeRegExPermutation : MatchRecognizeRegEx
	{
	    public override void WriteEPL(TextWriter writer)
        {
	        string delimiter = "";
	        writer.Write("match_recognize_permute(");
	        foreach (MatchRecognizeRegEx node in Children)
	        {
	            writer.Write(delimiter);
	            node.WriteEPL(writer);
	            delimiter = ",";
	        }
	        writer.Write(")");
	    }
	}
} // end of namespace
