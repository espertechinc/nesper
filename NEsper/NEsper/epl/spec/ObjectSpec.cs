///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Encapsulates the information required to specify an object identification and construction.
    /// <para>
    /// Abstract class for use with any object, such as views, pattern guards or pattern observers.
    /// </para>
    /// <para>
    /// A object construction specification can be equal to another specification. This information can be
    /// important to determine reuse of any object.
    /// </para>
    /// </summary>
    [Serializable]
    public abstract class ObjectSpec : MetaDefItem
    {
        /// <summary>Constructor.</summary>
        /// <param name="_namespace">if the namespace the object is in</param>
        /// <param name="objectName">is the name of the object</param>
        /// <param name="objectParameters">
        /// is a list of values representing the object parameters
        /// </param>
        public ObjectSpec(String _namespace, String objectName, IList<ExprNode> objectParameters)
        {
            this.ObjectNamespace = _namespace;
            this.ObjectName = objectName;
            this.ObjectParameters = objectParameters;
        }

        /// <summary>Returns namespace for view object.</summary>
        /// <returns>namespace</returns>
        public string ObjectNamespace { get; private set; }

        /// <summary>Returns the object name.</summary>
        /// <returns>object name</returns>
        public string ObjectName { get; private set; }

        /// <summary>Returns the list of object parameters.</summary>
        /// <returns>list of values representing object parameters</returns>
        public IList<ExprNode> ObjectParameters { get; private set; }

        public override bool Equals(Object otherObject)
        {
            if (otherObject == this)
            {
                return true;
            }

            if (otherObject == null)
            {
                return false;
            }

            if (GetType() != otherObject.GetType())
            {
                return false;
            }

            var other = (ObjectSpec)otherObject;
            if (ObjectName != other.ObjectName)
            {
                return false;
            }

            if (ObjectParameters.Count != other.ObjectParameters.Count)
            {
                return false;
            }

            // Compare object parameter by object parameter
            int index = 0;
            foreach (var thisParam in ObjectParameters)
            {
                var otherParam = other.ObjectParameters[index];
                index++;

                if (!ExprNodeUtility.DeepEquals(thisParam, otherParam, false))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override int GetHashCode()
        {
            return
                (ObjectName.GetHashCode() * 397) +
                (ObjectNamespace.GetHashCode());
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override String ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("objectName=");
            buffer.Append(ObjectName);
            buffer.Append("  objectParameters=(");
            char delimiter = ' ';

            if (ObjectParameters != null)
            {
                foreach (var param in ObjectParameters)
                {
                    buffer.Append(delimiter);
                    buffer.Append(param.ToExpressionStringMinPrecedenceSafe());
                    delimiter = ',';
                }
            }

            buffer.Append(')');

            return buffer.ToString();
        }
    }
} // End of namespace
