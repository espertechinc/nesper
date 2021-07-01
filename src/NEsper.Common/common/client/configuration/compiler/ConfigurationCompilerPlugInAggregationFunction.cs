///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.configuration.compiler
{
    /// <summary>
    ///     Configuration information for plugging in a custom aggregation function.
    /// </summary>
    [Serializable]
    public class ConfigurationCompilerPlugInAggregationFunction
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConfigurationCompilerPlugInAggregationFunction()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="name">of the aggregation function</param>
        /// <param name="forgeClassName">the name of the aggregation function factory class</param>
        public ConfigurationCompilerPlugInAggregationFunction(
            string name,
            string forgeClassName)
        {
            Name = name;
            ForgeClassName = forgeClassName;
        }

        /// <summary>
        ///     Returns the aggregation function name.
        /// </summary>
        /// <value>aggregation function name</value>
        public string Name { get; set; }

        /// <summary>
        ///     Returns the class name of the aggregation function factory class.
        /// </summary>
        /// <value>class name</value>
        public string ForgeClassName { get; set; }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (ConfigurationCompilerPlugInAggregationFunction) o;

            if (ForgeClassName != null ? !ForgeClassName.Equals(that.ForgeClassName) : that.ForgeClassName != null) {
                return false;
            }

            if (!Name.Equals(that.Name)) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^
                       (ForgeClassName != null ? ForgeClassName.GetHashCode() : 0);
            }
        }
    }
} // end of namespace