///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.collection
{
    /// <summary>
    /// Provides an enumeration of each combination of numbers between zero and Count-1 with Count must 
    /// be at least 1, with each combination cannot have duplicates, with each combination at least 
    /// one element, with the longest combination gets returned first and the least long combination 
    /// of the highest Count-1 value last. <para/>For example, for Count=3:
    ///     {0, 1, 2} {0, 1} {0, 2} {1, 2} {0} {1} {2}
    /// </summary>
    public class NumberAscCombinationEnumeration : IEnumerator<int[]>
    {
        private readonly int _n;
        private int _level;
        private int[] _current;
        private bool _first;

        public NumberAscCombinationEnumeration(int n)
        {
            if (n < 1) {
                throw new ArgumentException();
            }

            _n = n;
            _level = n;
            _first = true;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            if (_first) {
                _current = LevelCurrent(_n);
                _first = false;
                return _current != null;
            }

            ComputeNext();

            return (_current != null);
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator.Current {
            get { return Current; }
        }

        public int[] Current {
            get {
                if (_current == null) {
                    throw new InvalidOperationException();
                }

                var copy = new int[_current.Length];
                Array.Copy(_current, 0, copy, 0, _current.Length);
                return copy;
            }
        }

        private void ComputeNext()
        {
            // determine whether there is a next for the outermost
            int last = _current.Length - 1;
            if (_current[last] + 1 < _n) {
                _current[last]++;
                return;
            }

            // overflow
            int currOverflowedLevel = last - 1;
            while (currOverflowedLevel >= 0) {
                int maxAtPosition = _n - _level + currOverflowedLevel;
                if (_current[currOverflowedLevel] < maxAtPosition) {
                    _current[currOverflowedLevel]++;
                    for (int i = currOverflowedLevel + 1; i < _current.Length; i++) {
                        _current[i] = _current[i - 1] + 1;
                    }

                    return;
                }

                currOverflowedLevel--;
            }

            // bump level down
            _level--;
            if (_level == 0) {
                _current = null;
            }
            else {
                _current = LevelCurrent(_level);
            }
        }

        private static int[] LevelCurrent(int level)
        {
            var current = new int[level];
            for (int i = 0; i < level; i++) {
                current[i] = i;
            }

            return current;
        }

        private int[] CopyCurrent(int[] current)
        {
            var updated = new int[current.Length];
            Array.Copy(current, 0, updated, 0, updated.Length);
            return updated;
        }
    }
}