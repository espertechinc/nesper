///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


package net.esper.example.rfid;

import net.esper.client.EPServiceProvider;

import java.awt.*;
import java.awt.event.MouseEvent;
import java.awt.event.MouseListener;
import java.awt.event.MouseMotionAdapter;
import java.awt.image.BufferedImage;

import javax.swing.*;

public class DisplayCanvas extends JPanel 
{
    private static final int NUM_IMAGES = 3;

    private final EPServiceProvider engine;
    private final String[] assetIds;
    private final int[] x;
    private final int[] y;
    private final BufferedImage bi;
    private final int imageWidth;
    private final int imageHeight;
    
    private int currentImage;

    protected DisplayCanvas(EPServiceProvider engine, int width, int height) {
        this.engine = engine;
        
        setBackground(Color.white);
        setSize(width, height);
        addMouseMotionListener(new MouseMotionHandler());
        this.addMouseListener(new MouseListenerHandler(this));

        Image image = getToolkit().getImage("etc/rfid_tag_image.gif");

        // Load image
        MediaTracker mt = new MediaTracker(this);
        mt.addImage(image, 1);
        try {
            mt.waitForAll();
        } 
        catch (Exception e) {
            System.out.println("Exception while loading image.");
        }
        if (image.getWidth(this) == -1) {
            System.out.println("no gif file");
            System.exit(0);
        }

        // setup
        imageWidth = image.getWidth(this);
        imageHeight = image.getHeight(this);
        x = new int[NUM_IMAGES];
        y = new int[NUM_IMAGES];
        assetIds = new String[NUM_IMAGES];
        for (int i = 0; i < NUM_IMAGES; i++)
        {
            x[i] = i * (imageWidth + 10);
            assetIds[i] = "A" + Integer.toString(i + 1);
        }

        bi = new BufferedImage(image.getWidth(this), image.getHeight(this), BufferedImage.TYPE_INT_ARGB);
        Graphics2D big = bi.createGraphics();
        for (int i = 0; i < NUM_IMAGES; i++)
        {
            big.drawImage(image, x[i], y[i], this);
        }
    }

    public void paintComponent(Graphics g) {
        super.paintComponent(g);
        Graphics2D g2D = (Graphics2D) g;

        for (int i = 0; i < NUM_IMAGES; i++)
        {
            g2D.drawImage(bi, x[i], y[i], this);
        }
    }

    class MouseMotionHandler extends MouseMotionAdapter {
        public void mouseDragged(MouseEvent e) {
            if (currentImage != -1)
            {
                x[currentImage] = e.getX();
                y[currentImage] = e.getY();
            }
            repaint();
        }
    }
    
    class MouseListenerHandler implements MouseListener
    {
        private JPanel panel;

        public MouseListenerHandler(JPanel panel)
        {
            this.panel = panel;
        }

        /* (non-Javadoc)
        * @see java.awt.event.MouseListener#mouseClicked(java.awt.event.MouseEvent)
        */
        public void mouseClicked(MouseEvent arg0) {
        }

        /* (non-Javadoc)
         * @see java.awt.event.MouseListener#mouseEntered(java.awt.event.MouseEvent)
         */
        public void mouseEntered(MouseEvent arg0) {
        }

        /* (non-Javadoc)
         * @see java.awt.event.MouseListener#mouseExited(java.awt.event.MouseEvent)
         */
        public void mouseExited(MouseEvent arg0) {
        }

        /* (non-Javadoc)
         * @see java.awt.event.MouseListener#mousePressed(java.awt.event.MouseEvent)
         */
        public void mousePressed(MouseEvent e) {
            
            int imageNum = -1;
            for (int i = 0; i < NUM_IMAGES; i++)
            {
                if ( (e.getX() >= x[i]) && (e.getX() <= (x[i] + imageWidth)))
                {
                    if ((e.getY() >= y[i]) && (e.getY() <= (y[i] + imageHeight)))
                    {
                        imageNum = i;
                        break;                
                    }                            
                }
            }
            currentImage = imageNum;
        }

        /* (non-Javadoc)
         * @see java.awt.event.MouseListener#mouseReleased(java.awt.event.MouseEvent)
         */
        public void mouseReleased(MouseEvent arg0) {
            if (currentImage != -1)
            {
                int xpos = x[currentImage];
                int ypos = y[currentImage];

                // Determine zone
                int xzone = 0;
                int yzone = 0;
                if (xpos > panel.getSize().getWidth() / 2)
                {
                    xzone = 1;
                }
                if (ypos > panel.getSize().getHeight() / 2)
                {
                    yzone = 1;
                }
                int zone = xzone + 2 * yzone;

                System.out.println("Moved asset " + assetIds[currentImage] + " to " + xpos + " " + ypos + " in zone " + zone);

                // Send event
                LocationReport report = new LocationReport(assetIds[currentImage], zone);
                engine.getEPRuntime().sendEvent(report);
            }
        }
        
    }
}
