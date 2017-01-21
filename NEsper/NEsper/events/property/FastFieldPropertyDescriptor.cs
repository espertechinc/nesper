///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.ComponentModel;

using XLR8.CGLib;

namespace com.espertech.esper.events.property
{
    /// <summary>
    /// Provides a property descriptor that is obtained through a
    /// field.
    /// </summary>

	public class FastFieldPropertyDescriptor : PropertyDescriptor
	{
		private readonly FastField fieldInfo ;

		/// <summary>
		/// Indicates whether the value of this property should be
		/// persisted.
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		
		public override bool ShouldSerializeValue(object component)
		{
			return false ;
		}
		
		/// <summary>
		/// Indicates whether or not the descriptor is readonly
		/// </summary>

		public override bool IsReadOnly
		{
			get { return true ; }
		}
		
		/// <summary>
		/// Gets the type of component this property is bound to
		/// </summary>
		
		public override Type ComponentType
		{
			get { return fieldInfo.Target.DeclaringType ; }
		}
		
		/// <summary>
		/// Gets the return type of the property
		/// </summary>
		
		public override Type PropertyType 
		{
			get { return fieldInfo.FieldType ; }
		}
		
		/// <summary>
		/// Call the accessor method
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		
		public override Object GetValue(object component)
		{
		    return fieldInfo.Get(component);
		}
		
		/// <summary>
		/// Sets the value of the property
		/// </summary>
		/// <param name="component"></param>
		/// <param name="value"></param>
		
		public override void SetValue(object component, object value)
		{
			throw new NotSupportedException() ;
		}
		
		/// <summary>
		/// Can not override values with the simple accessor model
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		
		public override bool CanResetValue(object component)
		{
			return false ;
		}

		/// <summary>
		/// Resets the value of the property
		/// </summary>
		/// <param name="component"></param>
		
		public override void ResetValue(object component)
		{
			throw new NotSupportedException() ;
		}

        /// <summary>
        /// Returns true if the objects are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>

        public override bool Equals(object obj)
        {
            FastFieldPropertyDescriptor temp = obj as FastFieldPropertyDescriptor;
            if (temp != null)
            {
                return
                    Object.Equals( this.Name, temp.Name ) &&
                    Object.Equals( this.fieldInfo, temp.fieldInfo ) ;
            }

            return false;
        }

        /// <summary>
        /// Returns a hahscode for the object.
        /// </summary>
        /// <returns></returns>

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
		
		/// <summary>
		/// Constructor
		/// </summary>
		
		public FastFieldPropertyDescriptor( String name, FastField fieldInfo ) :
			base( name, null )
		{
			this.fieldInfo = fieldInfo;
		}

        /// <summary>
        /// Constructor
        /// </summary>

        public FastFieldPropertyDescriptor(FastField fieldInfo)
            : base(fieldInfo.Target.Name, null)
        {
            this.fieldInfo = fieldInfo;
        }
	}
}
