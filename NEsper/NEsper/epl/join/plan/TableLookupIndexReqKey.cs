///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.join.plan
{
    public class TableLookupIndexReqKey
    {
        private readonly string _name;
        private readonly string _tableName;
    
        public TableLookupIndexReqKey(string name)
            : this(name, null)
        {
        }
    
        public TableLookupIndexReqKey(string name, string tableName)
        {
            _name = name;
            _tableName = tableName;
        }

        public string Name
        {
            get { return _name; }
        }

        public string TableName
        {
            get { return _tableName; }
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
    
            var that = (TableLookupIndexReqKey) o;
    
            if (_tableName != null ? !_tableName.Equals(that._tableName) : that._tableName != null)
                return false;
            if (!_name.Equals(that._name)) return false;
    
            return true;
        }
    
        public override int GetHashCode()
        {
            int result = _name.GetHashCode();
            result = 31 * result + (_tableName != null ? _tableName.GetHashCode() : 0);
            return result;
        }
    
        public override string ToString()
        {
            if (_tableName == null) {
                return _name;
            }
            else {
                return "table '" + _tableName + "' index '" + _name + "'";
            }
        }
    }
}
