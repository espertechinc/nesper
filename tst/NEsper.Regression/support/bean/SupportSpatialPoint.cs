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
    public class SupportSpatialPoint
    {
        private string _category;
        private string _id;
        private double? _px;
        private double? _py;

        public SupportSpatialPoint(
            string id,
            double? px,
            double? py)
        {
            _id = id;
            _px = px;
            _py = py;
        }

        public SupportSpatialPoint(
            string id,
            double? px,
            double? py,
            string category)
        {
            _id = id;
            _px = px;
            _py = py;
            _category = category;
        }

        public string Id {
            get => _id;
            set => _id = value;
        }

        public double? Px {
            get => _px;
            set => _px = value;
        }

        public double? Py {
            get => _py;
            set => _py = value;
        }

        public string Category {
            get => _category;
            set => _category = value;
        }
    }
} // end of namespace