///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.pattern
{
    public sealed class MatchedEventMapMeta
    {
        private readonly static int MIN_MAP_LOOKUP = 3;

        private readonly String[] _tagsPerIndex;
        private readonly bool _hasArrayProperties;
        private readonly IDictionary<String, int> _tagsPerIndexMap;

        public MatchedEventMapMeta(String[] tagsPerIndex, bool hasArrayProperties)
        {
            _tagsPerIndex = tagsPerIndex;
            _hasArrayProperties = hasArrayProperties;
            _tagsPerIndexMap = GetMap(tagsPerIndex);
        }

        public MatchedEventMapMeta(ICollection<String> allTags, bool hasArrayProperties)
        {
            _tagsPerIndex = allTags.ToArray();
            _hasArrayProperties = hasArrayProperties;
            _tagsPerIndexMap = GetMap(_tagsPerIndex);
        }

        public string[] TagsPerIndex
        {
            get { return _tagsPerIndex; }
        }

        public int GetTagFor(String key)
        {
            if (_tagsPerIndexMap != null)
            {
                return _tagsPerIndexMap.Get(key, -1);
            }
            for (int i = 0; i < _tagsPerIndex.Length; i++)
            {
                if (_tagsPerIndex[i].Equals(key))
                {
                    return i;
                }
            }
            return -1;
        }

        private IDictionary<String, int> GetMap(String[] tagsPerIndex)
        {
            if (tagsPerIndex.Length < MIN_MAP_LOOKUP)
            {
                return null;
            }

            var map = new Dictionary<String, int>();
            for (int i = 0; i < tagsPerIndex.Length; i++)
            {
                map.Put(tagsPerIndex[i], i);
            }
            return map;
        }

        public bool IsHasArrayProperties()
        {
            return _hasArrayProperties;
        }
    }
}
