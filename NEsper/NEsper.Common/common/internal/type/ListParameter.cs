///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.IO;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    /// Represents a list of values in a set of numeric parameters.
    /// </summary>
    [Serializable]
    public class ListParameter : NumberSetParameter
    {
        private readonly IList<NumberSetParameter> _parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListParameter"/> class.
        /// </summary>
        public ListParameter()
        {
            _parameters = new List<NumberSetParameter>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListParameter"/> class.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        public ListParameter(IList<NumberSetParameter> parameters)
        {
            _parameters = parameters;
        }

        /// <summary> Add to the list a further parameter.</summary>
        /// <param name="numberSetParameter">is the parameter to add
        /// </param>
        public virtual void Add(NumberSetParameter numberSetParameter)
        {
            _parameters.Add(numberSetParameter);
        }

        /// <summary> Returns list of parameters.</summary>
        /// <returns> list of parameters
        /// </returns>
        public IList<NumberSetParameter> Parameters {
            get { return _parameters; }
        }

        /// <summary>
        /// Returns true if all values between and including min and max are supplied by the parameter.
        /// </summary>
        /// <param name="min">lower end of range</param>
        /// <param name="max">upper end of range</param>
        /// <returns>
        /// true if parameter specifies all int values between min and max, false if not
        /// </returns>
        public virtual bool IsWildcard(
            int min,
            int max)
        {
            foreach (NumberSetParameter param in _parameters) {
                if (param.IsWildcard(min, max)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Return a set of int values representing the value of the parameter for the given range.
        /// </summary>
        /// <param name="min">lower end of range</param>
        /// <param name="max">upper end of range</param>
        /// <returns>set of integer</returns>
        public ICollection<int> GetValuesInRange(
            int min,
            int max)
        {
            ICollection<int> result = new HashSet<int>();

            foreach (NumberSetParameter param in _parameters) {
                result.AddAll(param.GetValuesInRange(min, max));
            }

            return result;
        }

        public bool ContainsPoint(int point)
        {
            return ContainsPoint(_parameters, point);
        }

        public String Formatted()
        {
            var writer = new StringWriter();
            var delimiter = "";
            foreach (var param in _parameters) {
                writer.Write(delimiter);
                writer.Write(param.Formatted());
                delimiter = ", ";
            }

            return writer.ToString();
        }

        public static Boolean ContainsPoint(
            IList<NumberSetParameter> parameters,
            int point)
        {
            foreach (var param in parameters) {
                if (param.ContainsPoint(point)) {
                    return true;
                }
            }

            return false;
        }
    }
}