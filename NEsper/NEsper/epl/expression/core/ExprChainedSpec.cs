///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.epl.expression.core
{
    [Serializable]
    public class ExprChainedSpec 
    {
        public ExprChainedSpec(String name, IList<ExprNode> parameters, bool property)
        {
            Name = name;
            Parameters = parameters;
            IsProperty = property;
        }

        public string Name { get; set; }

        public IList<ExprNode> Parameters { get; set; }

        public bool IsProperty { get; private set; }

        public bool Equals(ExprChainedSpec other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name) && ExprNodeUtility.DeepEquals(other.Parameters, Parameters, false);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. 
        ///                 </param><exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.
        ///                 </exception><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ExprChainedSpec)) return false;
            return Equals((ExprChainedSpec) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0)*397) ^ (Parameters != null ? Parameters.GetHashCode() : 0);
            }
        }
        
        public override String ToString() {
            return "ExprChainedSpec{" +
                    "name='" + Name + '\'' +
                    ", parameters=" + Parameters +
                    '}';
        }
    }
}
