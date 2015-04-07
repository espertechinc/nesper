///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client
{
    /// <summary>Configuration information for plugging in a custom aggregation function. </summary>
    [Serializable]
    public class ConfigurationPlugInAggregationFunction 
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public ConfigurationPlugInAggregationFunction()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationPlugInAggregationFunction"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="factoryClassName">Name of the factory class.</param>
        public ConfigurationPlugInAggregationFunction(String name, String factoryClassName)
        {
            Name = name;
            FactoryClassName = factoryClassName;
        }

        /// <summary>Returns the aggregation function name. </summary>
        /// <value>aggregation function name</value>
        public string Name { get; set; }

        /// <summary>Returns the class name of the aggregation function factory class. </summary>
        /// <value>class name</value>
        public string FactoryClassName { get; set; }

        public bool Equals(ConfigurationPlugInAggregationFunction other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name) && Equals(other.FactoryClassName, FactoryClassName);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ConfigurationPlugInAggregationFunction)) return false;
            return Equals((ConfigurationPlugInAggregationFunction) obj);
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
                int result = (Name != null ? Name.GetHashCode() : 0);
                result = (result*397) ^ (FactoryClassName != null ? FactoryClassName.GetHashCode() : 0);
                return result;
            }
        }
    }
}
