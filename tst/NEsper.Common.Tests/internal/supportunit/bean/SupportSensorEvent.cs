///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    public class SupportSensorEvent
    {
        private SupportSensorEvent()
        {
        }

        public SupportSensorEvent(
            int id,
            string type,
            string device,
            double measurement,
            double confidence)
        {
            Id = id;
            Type = type;
            Device = device;
            Measurement = measurement;
            Confidence = confidence;
        }

        public int Id { get; set; }

        public string Type { get; set; }

        public string Device { get; set; }

        public double Measurement { get; set; }

        public double Confidence { get; set; }
    }
} // end of namespace
