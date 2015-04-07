///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Helper class that assists with operations on arrays.
    /// </summary>

	public static class ArrayHelper
	{
		/// <summary>
		/// Compares two arrays for equality
		/// </summary>
		/// <param name="array1"></param>
		/// <param name="array2"></param>
		/// <returns></returns>
		
		public static bool AreEqual( Array array1, Array array2 )
		{
			if ( array1 == null ) {
				throw new ArgumentNullException( "array1" ) ;
			}
			if ( array2 == null ) {
				throw new ArgumentNullException( "array2" ) ;
			}
			if ( array1.Length != array2.Length ) {
				return false ;
			}

			for( int ii = array1.Length - 1 ; ii >= 0 ; ii-- ) {
				if ( ! Object.Equals( array1.GetValue(ii), array2.GetValue(ii) ) ) {
					return false ;
				}
			}
			
			return true;
		}
	}
}
