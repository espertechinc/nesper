///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextStatePathKey : IComparable
    {
        private readonly int _level;
        private readonly int _parentPath;
        private readonly int _subPath;
    
        public ContextStatePathKey(int level, int parentPath, int subPath)
        {
            _level = level;
            _parentPath = parentPath;
            _subPath = subPath;
        }

        public int Level
        {
            get { return _level; }
        }

        public int ParentPath
        {
            get { return _parentPath; }
        }

        public int SubPath
        {
            get { return _subPath; }
        }

        public int CompareTo(Object o)
        {
            if (o.GetType() != typeof(ContextStatePathKey)) {
                throw new ArgumentException("Cannot compare " + typeof(ContextStatePathKey).FullName + " to " + o.GetType().FullName);
            }
            var other = (ContextStatePathKey) o;
            if (Level != other.Level) {
                return Level < other.Level ? -1 : 1;
            }
            if (ParentPath != other.ParentPath) {
                return ParentPath < other.ParentPath ? -1 : 1;
            }
            if (SubPath != other.SubPath) {
                return SubPath < other.SubPath ? -1 : 1;
            }
            return 0;
        }
    
        public override bool Equals(Object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
    
            var that = (ContextStatePathKey) o;
    
            if (_level != that._level) return false;
            if (_parentPath != that._parentPath) return false;
            if (_subPath != that._subPath) return false;
    
            return true;
        }
    
        public override int GetHashCode()
        {
            int result = _level;
            result = 31 * result + _parentPath;
            result = 31 * result + _subPath;
            return result;
        }
    
        public override String ToString()
        {
            return "ContextStatePathKey{" +
                    "level=" + _level +
                    ", parentPath=" + _parentPath +
                    ", subPath=" + _subPath +
                    '}';
        }
    }
}
