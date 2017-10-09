///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

namespace com.espertech.esper.supportregression.bean
{
    [Serializable]
    public class SupportBeanRange
    {
        private String _id;
        private String _key;
        private long? _keyLong;
        private int? _rangeEnd;
        private long? _rangeEndLong;
        private String _rangeEndStr;
        private int? _rangeStart;
        private long? _rangeStartLong;
        private String _rangeStartStr;

        public SupportBeanRange()
        {
        }

        public SupportBeanRange(long? keyLong)
        {
            _keyLong = keyLong;
        }
        
        public SupportBeanRange(String id,
                                int? rangeStart,
                                int? rangeEnd)
        {
            _id = id;
            _rangeStart = rangeStart;
            _rangeEnd = rangeEnd;
        }

        public SupportBeanRange(String id,
                                String key,
                                String rangeStartStr,
                                String rangeEndStr)
        {
            _id = id;
            _key = key;
            _rangeStartStr = rangeStartStr;
            _rangeEndStr = rangeEndStr;
        }

        public SupportBeanRange(String id,
                                String key,
                                int? rangeStart,
                                int? rangeEnd)
        {
            _id = id;
            _key = key;
            _rangeStart = rangeStart;
            _rangeEnd = rangeEnd;
        }

        public long? KeyLong
        {
            get { return _keyLong; }
            set { _keyLong = value; }
        }

        public long? RangeStartLong
        {
            get { return _rangeStartLong; }
            set { _rangeStartLong = value; }
        }

        public long? RangeEndLong
        {
            get { return _rangeEndLong; }
            set { _rangeEndLong = value; }
        }

        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public int? RangeStart
        {
            get { return _rangeStart; }
            set { _rangeStart = value; }
        }

        public int? RangeEnd
        {
            get { return _rangeEnd; }
            set { _rangeEnd = value; }
        }

        public string RangeStartStr
        {
            get { return _rangeStartStr; }
            set { _rangeStartStr = value; }
        }

        public string RangeEndStr
        {
            get { return _rangeEndStr; }
            set { _rangeEndStr = value; }
        }

        public static SupportBeanRange MakeKeyLong(String id,
                                                   long? keyLong,
                                                   int rangeStart,
                                                   int rangeEnd)
        {
            var sbr = new SupportBeanRange(id, rangeStart, rangeEnd);
            sbr.KeyLong = keyLong;
            return sbr;
        }

        public static SupportBeanRange MakeLong(String id,
                                                String key,
                                                long? rangeStartLong,
                                                long? rangeEndLong)
        {
            var bean = new SupportBeanRange();
            bean._id = id;
            bean._key = key;
            bean._rangeStartLong = rangeStartLong;
            bean._rangeEndLong = rangeEndLong;
            return bean;
        }

        public static SupportBeanRange MakeLong(String id,
                                                String key,
                                                long? keyLong,
                                                long? rangeStartLong,
                                                long? rangeEndLong)
        {
            var range = new SupportBeanRange();
            range.Id = id;
            range.Key = key;
            range.KeyLong = keyLong;
            range.RangeStartLong = rangeStartLong;
            range.RangeEndLong = rangeEndLong;
            return range;
        }
    }
}