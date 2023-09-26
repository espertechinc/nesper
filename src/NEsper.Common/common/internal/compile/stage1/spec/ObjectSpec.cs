///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Encapsulates the information required to specify an object identification and construction.
    ///     <para />
    ///     Abstract class for use with any object, such as views, pattern guards or pattern observers.
    ///     <para />
    ///     A object construction specification can be equal to another specification. This information can be
    ///     important to determine reuse of any object.
    /// </summary>
    [Serializable]
    public abstract class ObjectSpec
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="namespace">if the namespace the object is in</param>
        /// <param name="objectName">is the name of the object</param>
        /// <param name="objectParameters">is a list of values representing the object parameters</param>
        public ObjectSpec(
            string @namespace,
            string objectName,
            IList<ExprNode> objectParameters)
        {
            ObjectNamespace = @namespace;
            ObjectName = objectName;
            ObjectParameters = objectParameters;
        }

        /// <summary>
        ///     Returns namespace for view object.
        /// </summary>
        /// <returns>namespace</returns>
        public string ObjectNamespace { get; }

        /// <summary>
        ///     Returns the object name.
        /// </summary>
        /// <returns>object name</returns>
        public string ObjectName { get; }

        /// <summary>
        ///     Returns the list of object parameters.
        /// </summary>
        /// <returns>list of expressions representing object parameters</returns>
        public IList<ExprNode> ObjectParameters { get; }

        public override bool Equals(object otherObject)
        {
            if (otherObject == this) {
                return true;
            }

            if (otherObject == null) {
                return false;
            }

            if (GetType() != otherObject.GetType()) {
                return false;
            }

            var other = (ObjectSpec)otherObject;
            if (!ObjectName.Equals(other.ObjectName)) {
                return false;
            }

            if (ObjectParameters.Count != other.ObjectParameters.Count) {
                return false;
            }

            // Compare object parameter by object parameter
            var index = 0;
            foreach (var thisParam in ObjectParameters) {
                var otherParam = other.ObjectParameters[index];
                index++;

                if (!ExprNodeUtilityCompare.DeepEquals(thisParam, otherParam, false)) {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return CompatExtensions.HashAll(ObjectNamespace, ObjectName);
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("objectName=");
            buffer.Append(ObjectName);
            buffer.Append("  objectParameters=(");
            var delimiter = ' ';

            if (ObjectParameters != null) {
                foreach (var param in ObjectParameters) {
                    buffer.Append(delimiter);
                    buffer.Append(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(param));
                    delimiter = ',';
                }
            }

            buffer.Append(')');

            return buffer.ToString();
        }
    }
} // end of namespace