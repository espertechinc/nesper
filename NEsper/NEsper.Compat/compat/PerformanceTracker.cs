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

namespace com.espertech.esper.compat
{
    public class PerformanceTracker : IDisposable
    {
        private string _label;
        private readonly long _baseTime;
        private readonly LinkedList<long> _timeLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceTracker"/> class.
        /// </summary>
        public PerformanceTracker(string label)
        {
            _label = label;
            _baseTime = PerformanceObserver.MicroTime;
            _timeLine = new LinkedList<long>();
        }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>The label.</value>
        public string Label
        {
            get { return _label; }
            set { _label = value; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Format("{0,-10} ", _label));
            foreach (var measurement in _timeLine) {
                stringBuilder.AppendFormat("|{0,8}", measurement - _baseTime);
            }

            Console.Out.WriteLine(stringBuilder.ToString());
        }

        /// <summary>
        /// Adds the measurment.
        /// </summary>
        public void AddMeasurement()
        {
            _timeLine.AddLast(PerformanceObserver.MicroTime);
        }
    }
}
