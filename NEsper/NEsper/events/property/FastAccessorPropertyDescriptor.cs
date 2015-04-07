///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.ComponentModel;
using System.Reflection;

using XLR8.CGLib;

namespace com.espertech.esper.events.property
{
    /// <summary>
    /// Provides a simple property descriptor that is obtained through a
    /// method.  The method should be a read method that has no parameters
    /// and returns an object.
    /// </summary>

	public class FastAccessorPropertyDescriptor : PropertyDescriptor
	{
		private readonly FastMethod accessorMethod ;

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
			get { return accessorMethod.Target.DeclaringType ; }
		}
		
		/// <summary>
		/// Gets the return type of the property
		/// </summary>
		
		public override Type PropertyType 
		{
			get { return accessorMethod.ReturnType ; }
		}
		
		/// <summary>
		/// Call the accessor method
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		
		public override Object GetValue(object component)
		{
            try
            {
                return accessorMethod.Invoke(component, null);
            }
            catch (TargetException)
            {
                throw new ArgumentException("component");
            }
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
            FastAccessorPropertyDescriptor temp = obj as FastAccessorPropertyDescriptor;
            if (temp != null)
            {
                return
                    //Object.Equals(this.Name, temp.Name) &&
                    Object.Equals(this.accessorMethod, temp.accessorMethod);
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

        public FastAccessorPropertyDescriptor(String name, FastMethod accessorMethod)
            : base( name, null )
		{
			this.accessorMethod = accessorMethod;
		}

        /// <summary>
        /// Constructor
        /// </summary>

        public FastAccessorPropertyDescriptor(String name, MethodInfo accessorMethod)
            : this(name, FastClass.CreateMethod(accessorMethod))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FastAccessorPropertyDescriptor"/> class.
        /// </summary>
        /// <param name="accessorMethod">The accessor method.</param>
        public FastAccessorPropertyDescriptor(FastMethod accessorMethod)
            : base(accessorMethod.Target.Name, null)
        {
            this.accessorMethod = accessorMethod;
        }
	}
}
