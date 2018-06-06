///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace NEsper.Examples.MatchMaker.eventbean
{
    public class MobileUserBean
    {
        private readonly Gender _myGender;
        private readonly HairColor _myHairColor;
        private readonly AgeRange _myAgeRange;
        private readonly Gender _preferredGender;
        private readonly HairColor _preferredHairColor;
        private readonly AgeRange _preferredAgeRange;

        public MobileUserBean(int userId, double locationX, double locationY, Gender myGender, HairColor myHairColor, AgeRange myAgeRange, Gender preferredGender, HairColor preferredHairColor, AgeRange preferredAgeRange)
        {
            UserId = userId;
            LocationX = locationX;
            LocationY = locationY;
            _myGender = myGender;
            _myHairColor = myHairColor;
            _myAgeRange = myAgeRange;
            _preferredGender = preferredGender;
            _preferredHairColor = preferredHairColor;
            _preferredAgeRange = preferredAgeRange;
        }

        public int UserId { get; private set; }

        public double LocationX { get; private set; }

        public double LocationY { get; set; }

        public void SetLocation(double locationX, double locationY)
        {
            LocationX = locationX;
            LocationY = locationY;
        }

        public String MyGender
        {
            get { return _myGender.ToString(); }
        }

        public String MyHairColor
        {
            get { return _myHairColor.ToString(); }
        }

        public String MyAgeRange
        {
            get { return _myAgeRange.ToString(); }
        }

        public String PreferredGender
        {
            get { return _preferredGender.ToString(); }
        }

        public String PreferredHairColor
        {
            get { return _preferredHairColor.ToString(); }
        }

        public String PreferredAgeRange
        {
            get { return _preferredAgeRange.ToString(); }
        }
    }
}
