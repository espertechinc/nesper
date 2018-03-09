///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

namespace com.espertech.esper.supportregression.rowrecog
{
    public class SupportRecogBean {
        private string theString;
        private int value;
        private string cat;
    
        public SupportRecogBean(string theString) {
            this.TheString = theString;
        }
    
        public SupportRecogBean(string theString, int value) {
            this.TheString = theString;
            this.value = value;
        }
    
        public SupportRecogBean(string theString, string cat, int value) {
            this.TheString = theString;
            this.cat = cat;
            this.value = value;
        }

        public string TheString
        {
            get => theString;
            set => theString = value;
        }

        public int Value
        {
            get => value;
            set => this.value = value;
        }

        public string Cat
        {
            get => cat;
            set => cat = value;
        }

        public string GetTheString() {
            return theString;
        }
    
        public string GetCat() {
            return cat;
        }
    
        public void SetCat(string cat) {
            this.cat = cat;
        }
    
        public void SetTheString(string theString) {
            this.TheString = theString;
        }
    
        public int GetValue() {
            return value;
        }
    
        public void SetValue(int value) {
            this.value = value;
        }
    
        public override string ToString() {
            return theString;
        }
    }
} // end of namespace
