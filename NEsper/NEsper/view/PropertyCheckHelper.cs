///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.util;

namespace com.espertech.esper.view
{	
	/// <summary>
    /// Utility class for checking in a schema if fields exist and/or have an
    /// expected type.
    /// </summary>
	public sealed class PropertyCheckHelper
	{
		/// <summary> Check if the field identified by the field name exists according to the schema.</summary>
		/// <param name="type">contains metadata about fields</param>
		/// <param name="fieldName">is the field's field name to test</param>
		/// <returns> a String error message if the field doesn't exist, or null to indicate success</returns>

        public static String Exists(EventType type, String fieldName)
		{
			Type clazz = GetClass(type, fieldName);
			
			if (clazz == null)
			{
				return "Parent view does not contain a field named '" + fieldName + "'";
			}
			
			return null;
		}
		
		/// <summary> Check if the fields identified by the field names both exists according to the schema.</summary>
		/// <param name="type">contains metadata about fields</param>
		/// <param name="fieldNameOne">is the first field's field name to test</param>
		/// <param name="fieldNameTwo">is the first field's field name to test</param>
		/// <returns> a String error message if either of the fields doesn't exist, or null to indicate success
		/// </returns>
		public static String Exists(EventType type, String fieldNameOne, String fieldNameTwo)
		{
			Type clazz = GetClass(type, fieldNameOne);
			
			if (clazz == null)
			{
				return "Parent view does not contain a field named '" + fieldNameOne + "'";
			}
			
			clazz = GetClass(type, fieldNameTwo);
			
			if (clazz == null)
			{
				return "Parent view does not contain a field named '" + fieldNameTwo + "'";
			}
			
			return null;
		}
		
		/// <summary> Check if the field identified by the field name is a valid numeric field according to the schema.</summary>
		/// <param name="type">contains metadata about fields</param>
		/// <param name="numericFieldName">is the field's field name to test</param>
		/// <returns> a String error message if the field doesn't exist or is not numeric, or null to indicate success</returns>
		
        public static String CheckNumeric(EventType type, String numericFieldName)
		{
			return CheckFieldNumeric(type, numericFieldName);
		}
		
		/// <summary> Check if the fields identified by their field names are valid numeric field according to the schema.</summary>
		/// <param name="type">contains metadata about fields</param>
		/// <param name="numericFieldNameX">is the first field's field name to test</param>
		/// <param name="numericFieldNameY">is the second field's field name to test</param>
		/// <returns> a String error message if the field doesn't exist or is not numeric, or null to indicate success
		/// </returns>
		public static String CheckNumeric(EventType type, String numericFieldNameX, String numericFieldNameY)
		{
			String error = CheckFieldNumeric(type, numericFieldNameX);
			if (error != null)
			{
				return error;
			}
			
			return CheckFieldNumeric(type, numericFieldNameY);
		}
		
		/// <summary> Check if the field identified by the field name is of type long according to the schema.</summary>
		/// <param name="type">contains metadata about fields</param>
		/// <param name="longFieldName">is the field's field name to test</param>
		/// <returns> a String error message if the field doesn't exist or is not a long, or null to indicate success</returns>
		public static String CheckLong(EventType type, String longFieldName)
		{
			Type clazz = GetClass(type, longFieldName);
			
			if (clazz == null)
			{
				return "Parent view does not contain a field named '" + longFieldName + "'";
			}
			
			if ((clazz != typeof(long)) && (clazz != typeof(long?)))
			{
				return "Parent view field named '" + longFieldName + "' is not of type long";
			}
			
			return CheckFieldNumeric(type, longFieldName);
		}
		
		/// <summary> Returns the class for the field as defined in the schema.</summary>
		/// <param name="type">contains metadata about fields
		/// </param>
		/// <param name="fieldName">is the field's name to return the type for
		/// </param>
		/// <returns> type of field.
		/// </returns>
		private static Type GetClass(EventType type, String fieldName)
		{
			return type.GetPropertyType(fieldName);
		}
		
		// Perform the schema checking for if a field exists and is numeric
		private static String CheckFieldNumeric(EventType type, String numericFieldName)
		{
			Type clazz = GetClass(type, numericFieldName);
			
			if (clazz == null)
			{
				return "Parent view does not contain a field named '" + numericFieldName + "'";
			}
			
			if (!TypeHelper.IsNumeric(clazz))
			{
				return "Parent view field named '" + numericFieldName + "' is not a number";
			}
			
			return null;
		}
	}
}
