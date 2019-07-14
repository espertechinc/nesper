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
    public class SupportBeanRange
    {
        private string _id;
        private string _key;
        private long? _keyLong;
        private int? _rangeEnd;
        private long? _rangeEndLong;
        private string _rangeEndStr;
        private int? _rangeStart;
        private long? _rangeStartLong;
        private string _rangeStartStr;

        public SupportBeanRange()
        {
        }

        public SupportBeanRange(long? keyLong)
        {
            _keyLong = keyLong;
        }

        public SupportBeanRange(
            string id,
            int? rangeStart,
            int? rangeEnd)
        {
            _id = id;
            _rangeStart = rangeStart;
            _rangeEnd = rangeEnd;
        }

        public SupportBeanRange(
            string id,
            string key,
            string rangeStartStr,
            string rangeEndStr)
        {
            _id = id;
            _key = key;
            _rangeStartStr = rangeStartStr;
            _rangeEndStr = rangeEndStr;
        }

        public SupportBeanRange(
            string id,
            string key,
            int? rangeStart,
            int? rangeEnd)
        {
            _id = id;
            Key = key;
            _rangeStart = rangeStart;
            _rangeEnd = rangeEnd;
        }

        public long? KeyLong {
            get => _keyLong;
            set => _keyLong = value;
        }

        public long? RangeStartLong {
            get => _rangeStartLong;
            set => _rangeStartLong = value;
        }

        public long? RangeEndLong {
            get => _rangeEndLong;
            set => _rangeEndLong = value;
        }

        public string Key {
            get => _key;
            set => _key = value;
        }

        public string Id {
            get => _id;
            set => _id = value;
        }

        public int? RangeStart {
            get => _rangeStart;
            set => _rangeStart = value;
        }

        public int? RangeEnd {
            get => _rangeEnd;
            set => _rangeEnd = value;
        }

        public string RangeStartStr {
            get => _rangeStartStr;
            set => _rangeStartStr = value;
        }

        public string RangeEndStr {
            get => _rangeEndStr;
            set => _rangeEndStr = value;
        }

        public static SupportBeanRange MakeKeyLong(
            string id,
            long? keyLong,
            int rangeStart,
            int rangeEnd)
        {
            var sbr = new SupportBeanRange(id, rangeStart, rangeEnd);
            sbr.KeyLong = keyLong;
            return sbr;
        }

        public static SupportBeanRange MakeLong(
            string id,
            string key,
            long? rangeStartLong,
            long? rangeEndLong)
        {
            var bean = new SupportBeanRange();
            bean._id = id;
            bean.Key = key;
            bean._rangeStartLong = rangeStartLong;
            bean._rangeEndLong = rangeEndLong;
            return bean;
        }

        public static SupportBeanRange MakeLong(
            string id,
            string key,
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
} // end of namespace