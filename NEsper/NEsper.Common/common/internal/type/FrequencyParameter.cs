///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    /// Encapsulates a parameter specifying a frequency, i.e. '* / 5'.
    /// </summary>
    [Serializable]
    public class FrequencyParameter : NumberSetParameter
    {
        private readonly int _frequency;

        /// <summary> Returns frequency.</summary>
        /// <returns> frequency divisor
        /// </returns>
        public int Frequency {
            get { return _frequency; }
        }

        /// <summary> Ctor.</summary>
        /// <param name="frequency">divisor specifying frequency
        /// </param>
        public FrequencyParameter(int frequency)
        {
            _frequency = frequency;
            if (frequency <= 0) {
                throw new ArgumentException("Zero or negative value supplied as freqeuncy");
            }
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
            if (_frequency == 1) {
                return true;
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
            ICollection<int> values = new HashSet<int>();
            int start = min - min % _frequency;

            do {
                if (start >= min) {
                    values.Add(start);
                }

                start += _frequency;
            } while (start <= max);

            return values;
        }


        public bool ContainsPoint(int point)
        {
            return point % _frequency == 0;
        }

        public string Formatted()
        {
            return "*/" + _frequency;
        }
    }
}