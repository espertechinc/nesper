///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


package net.esper.example.rfid;

public class LocationReport
{
    private String assetId;
    private int locX;
    private int locY;
    private int zone;
    private int[] categories;

    public LocationReport(String assetId, int zone)
    {
        this.assetId = assetId;
        this.zone = zone;
    }

    public String getAssetId()
    {
        return assetId;
    }

    public int getLocX()
    {
        return locX;
    }

    public int getLocY()
    {
        return locY;
    }

    public int getZone()
    {
        return zone;
    }

    public int[] getCategories()
    {
        return categories;
    }
}
