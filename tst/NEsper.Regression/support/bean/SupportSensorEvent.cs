///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportSensorEvent
    {
        private double _confidence;
        private string _device;
        private int _id;
        private double _measurement;
        private string _type;

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
            _id = id;
            _type = type;
            _device = device;
            _measurement = measurement;
            _confidence = confidence;
        }

        public int Id {
            get => _id;
            set => _id = value;
        }

        public string Type {
            get => _type;
            set => _type = value;
        }

        public string Device {
            get => _device;
            set => _device = value;
        }

        public double Measurement {
            get => _measurement;
            set => _measurement = value;
        }

        public double Confidence {
            get => _confidence;
            set => _confidence = value;
        }
    }
} // end of namespace