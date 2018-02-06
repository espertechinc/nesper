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
    /// Abstract class for literal values supplied in an event expression string and prepared expression values supplied
	/// by set methods.
	/// </summary>
    [Serializable]
    public abstract class PrimitiveValueBase : PrimitiveValue
    {
        /// <summary>
        /// Set a bool value.
        /// </summary>
        /// <value></value>
        public virtual bool _Boolean
        {
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Set a byte value.
        /// </summary>
        /// <value></value>
        public virtual byte _Byte
        {
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Sets the sbyte value.
        /// </summary>
	    public virtual sbyte _SByte
	    {
            set { throw new NotSupportedException(); }
	    }

        /// <summary>
        /// Set a float value.
        /// </summary>
        /// <value></value>
        public virtual float _Float
        {
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Set an int value.
        /// </summary>
        /// <value></value>
        public virtual int _Int
        {
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Set a short value.
        /// </summary>
        /// <value></value>
        public virtual short _Short
        {
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Set a string value.
        /// </summary>
        /// <value></value>
        public virtual String _String
        {
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Set a double value.
        /// </summary>
        /// <value></value>
        public virtual Double _Double
        {
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Set a long value.
        /// </summary>
        /// <value></value>
        public virtual long _Long
        {
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Returns the type of primitive value this instance represents.
        /// </summary>
        /// <value></value>
        /// <returns> enum type of primitive
        /// </returns>
        
        public abstract PrimitiveValueType Type { get;}

        /// <summary>
        /// Returns a value object.
        /// </summary>
        /// <value></value>
        /// <returns> value object
        /// </returns>
        
        public abstract Object ValueObject { get;}
        
        /// <summary>
        /// Parse the string literal values supplied in the array into the 
        /// specific data type.
        /// </summary>
        /// <param name="values">are the textual values to parse</param>
        
        public virtual void Parse(String[] values)
        {
            Parse(values[0]);
        }

        /// <summary>
        /// Parse the string literal value supplied into the specific
        /// the specific data type.
        /// </summary>
        /// <param name="param1"></param>

        public abstract void Parse(String param1);
    }
}
