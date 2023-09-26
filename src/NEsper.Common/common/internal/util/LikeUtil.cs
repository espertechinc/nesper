///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///  Utility for performing a SQL Like comparsion.
    /// </summary>
    [Serializable]
    public class LikeUtil
    {
        private const int UNDERSCORE_CHAR = 1;
        private const int PERCENT_CHAR = 2;

        private char[] _cLike;
        private int[] _wildCardType;
        private int _iLen;
        private readonly bool _isIgnoreCase;
        private int _iFirstWildCard;
        private bool _isNull;
        private char? _escapeChar;

        public bool EquivalentToFalsePredicate => _isNull;

        public bool EquivalentToEqualsPredicate => _iFirstWildCard == -1;

        public bool EquivalentToNotNullPredicate {
            get {
                if (_isNull || !HasWildcards) {
                    return false;
                }

                for (var i = 0; i < _wildCardType.Length; i++) {
                    if (_wildCardType[i] != PERCENT_CHAR) {
                        return false;
                    }
                }

                return true;
            }
        }

        public bool EquivalentToBetweenPredicate =>
            _iFirstWildCard > 0 &&
            _iFirstWildCard == _wildCardType.Length - 1 &&
            _cLike[_iFirstWildCard] == '%';

        public bool EquivalentToBetweenPredicateAugmentedWithLike =>
            _iFirstWildCard > 0 && _cLike[_iFirstWildCard] == '%';

        /// <summary> Ctor.</summary>
        /// <param name="pattern">is the SQL-like pattern to</param>
        /// <param name="escape">is the escape character</param>
        /// <param name="ignorecase">is true to ignore the case, or false if not</param>
        public LikeUtil(
            string pattern,
            char? escape,
            bool ignorecase)
        {
            _escapeChar = escape;
            _isIgnoreCase = ignorecase;
            Normalize(pattern);
        }

        /// <summary> Execute the string.</summary>
        /// <param name="compareString">is the string to compare
        /// </param>
        /// <returns> true if pattern matches, or false if not
        /// </returns>
        public virtual bool CompareTo(string compareString)
        {
            if (_isIgnoreCase) {
                compareString = compareString.ToUpper();
            }

            return CompareAt(compareString, 0, 0, compareString.Length) ? true : false;
        }

        /// <summary> Resets the search pattern.</summary>
        /// <param name="pattern">is the new pattern to match against
        /// </param>
        public virtual void ResetPattern(string pattern)
        {
            Normalize(pattern);
        }

        private bool CompareAt(
            string s,
            int i,
            int j,
            int jLen)
        {
            for (; i < _iLen; i++) {
                switch (_wildCardType[i]) {
                    case 0: // general character
                        if (j >= jLen || _cLike[i] != s[j++]) {
                            return false;
                        }

                        break;


                    case UNDERSCORE_CHAR: // underscore: do not test this character
                        if (j++ >= jLen) {
                            return false;
                        }

                        break;


                    case PERCENT_CHAR: // percent: none or any character(s)
                        if (++i >= _iLen) {
                            return true;
                        }

                        while (j < jLen) {
                            if (_cLike[i] == s[j] && CompareAt(s, i, j, jLen)) {
                                return true;
                            }

                            j++;
                        }

                        return false;
                }
            }

            if (j != jLen) {
                return false;
            }

            return true;
        }

        private void Normalize(string pattern)
        {
            _isNull = pattern == null;

            if (!_isNull && _isIgnoreCase) {
                pattern = pattern.ToUpper();
            }

            _iLen = 0;
            _iFirstWildCard = -1;

            var l = pattern?.Length ?? 0;

            _cLike = new char[l];
            _wildCardType = new int[l];

            bool bEscaping = false, bPercent = false;

            for (var i = 0; i < l; i++) {
                var c = pattern[i];

                if (!bEscaping) {
                    if (_escapeChar != null && _escapeChar.Value == c) {
                        bEscaping = true;

                        continue;
                    }
                    else if (c == '_') {
                        _wildCardType[_iLen] = UNDERSCORE_CHAR;

                        if (_iFirstWildCard == -1) {
                            _iFirstWildCard = _iLen;
                        }
                    }
                    else if (c == '%') {
                        if (bPercent) {
                            continue;
                        }

                        bPercent = true;
                        _wildCardType[_iLen] = PERCENT_CHAR;

                        if (_iFirstWildCard == -1) {
                            _iFirstWildCard = _iLen;
                        }
                    }
                    else {
                        bPercent = false;
                    }
                }
                else {
                    bPercent = false;
                    bEscaping = false;
                }

                _cLike[_iLen++] = c;
            }

            for (var i = 0; i < _iLen - 1; i++) {
                if (_wildCardType[i] == PERCENT_CHAR &&
                    _wildCardType[i + 1] == UNDERSCORE_CHAR) {
                    _wildCardType[i] = UNDERSCORE_CHAR;
                    _wildCardType[i + 1] = PERCENT_CHAR;
                }
            }
        }

        internal bool HasWildcards => _iFirstWildCard != -1;
    }
}