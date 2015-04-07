///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.util;

namespace com.espertech.esper.epl.db
{
	/// <summary>
    /// Descriptor for SQL output columns.
    /// </summary>
	
    public sealed class DBOutputTypeDesc
	{
	    /// <summary> Returns the SQL type of the output column.</summary>
	    /// <returns> sql type
	    /// </returns>
	    public string SqlType { get; private set; }

	    /// <summary> Returns the type that getObject on the output column produces.</summary>
	    /// <returns> type from statement metadata
	    /// </returns>
	    public Type DataType { get; private set; }

	    /// <summary>
	    /// Gets the optional mapping from output column type to built-in.
	    /// </summary>
	    public DatabaseTypeBinding OptionalBinding { get; private set; }

	    /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="sqlType">the type of the column</param>
        /// <param name="dataType">the type reflecting column type</param>
        /// <param name="optionalBinding">the optional mapping from output column type to built-in</param>
		
        public DBOutputTypeDesc(string sqlType, Type dataType, DatabaseTypeBinding optionalBinding)
		{
			SqlType = sqlType;
            DataType = dataType;
            OptionalBinding = optionalBinding;
		}

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
		public override String ToString()
		{
			return "sqlType=" + SqlType + " dataType=" + DataType;
		}
	}
}
