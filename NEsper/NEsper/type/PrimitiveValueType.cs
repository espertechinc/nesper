///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.type
{
	/// <summary>
	/// Enumeration of types of primitive values.
	/// </summary>

    [Serializable]
    public class PrimitiveValueType
	{
		/// <summary> Byte.</summary>
		public readonly static PrimitiveValueType BYTE = new PrimitiveValueType( "Byte" ) ;

		/// <summary> Short.</summary>
		public readonly static PrimitiveValueType SHORT = new PrimitiveValueType( "Short" );

		/// <summary> Integer.</summary>
		public readonly static PrimitiveValueType INTEGER = new PrimitiveValueType( "Int" ) ;

		/// <summary> Long.</summary>
		public readonly static PrimitiveValueType LONG = new PrimitiveValueType( "Long" ) ;

		/// <summary> Float.</summary>
		public readonly static PrimitiveValueType FLOAT = new PrimitiveValueType( "Single" );

		/// <summary> Double.</summary>
		public readonly static PrimitiveValueType DOUBLE = new PrimitiveValueType( "Double" ) ;

        /// <summary> Double.</summary>
        public readonly static PrimitiveValueType DECIMAL = new PrimitiveValueType("Decimal");

        /// <summary> Boolean.</summary>
		public readonly static PrimitiveValueType BOOL = new PrimitiveValueType( "Boolean" ) ;

		/// <summary> String.</summary>
		public readonly static PrimitiveValueType STRING = new PrimitiveValueType( "String" );

		private readonly String typeName;

		private PrimitiveValueType( String typeName )
		{
			this.typeName = typeName;
		}

		/// <summary> Returns the name of the type.</summary>
		/// <returns> type name
		/// </returns>

		public String TypeName
		{
			get { return typeName; }
		}

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
		public override String ToString()
		{
			return typeName;
		}
	}
}
