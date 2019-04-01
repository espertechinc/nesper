///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.collection
{
    /// <summary>
    /// Based on the <see cref="PermutationEnumerator" /> this enumeration provides, among a set of supplied integer
    /// values, all permutations of order these values can come in, ie.
    /// Example: {0, 2, 3} results in 6 enumeration values ending in {3, 2, 0}.
    /// </summary>
	public class NumberSetPermutationEnumeration
	{
	    /// <summary>
	    /// Creates the specified number set.
	    /// </summary>
	    /// <param name="numberSet">The number set.</param>
	    /// <returns></returns>
	    public static IEnumerable<int[]> New(int[] numberSet)
	    {
	        var permutationEnumerator = PermutationEnumerator.Create(numberSet.Length).GetEnumerator();
	        while (permutationEnumerator.MoveNext())
	        {
	            var permutation = permutationEnumerator.Current;
	            var result = new int[numberSet.Length];
	            for (var i = 0; i < numberSet.Length; i++)
	            {
	                result[i] = numberSet[permutation[i]];
	            }

	            yield return result;
	        }
	    }
    }
}
