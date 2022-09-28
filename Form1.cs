using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LS_MIP;

namespace zbuffer
{
    public partial class Form1 : Form
    {
        LSimage outputImage;

        public int z ; // Nummer der Bilder   
        public byte T ;  // Threshold
        public double phi; //Startwinkel in radian
        public int phidegree; //Startwinkel in grad
        public byte B, C;
        public int[,] array;
        public int[,] array2;
        public bool loaded;

        public Form1()
        {
            InitializeComponent();
            Text = "zBuffer"; 
            outputImage = new LSimage("G:\\MIP\\Black.bmp", "zBuffer");

            loaded = false;
        }

        private unsafe void button1_Click(object sender, EventArgs e)
        {
            z = (int) numericUpDown1.Value; // Nummer der Bilder   
            T = (byte) numericUpDown2.Value;  // Threshold
            phidegree = (int)numericUpDown3.Value; //Startwinkel
            progressBar1.Maximum = z;
            loaded = true;
            array = new int[512, z];
            array2 = new int[512, z];


            /* ----------code für mehrmalige bilder
            phidegree = 0;
            int phiend = 360;
            int phistep = 1;

            T = 20;
            int Tend = 220;

            //while (phidegree < phiend) //bilder nach winkel
            while (T < Tend) //bilder nach threshold
            {
            */ //-------------


                phi = (phidegree * Math.PI / 180.0);

                for (int zz = 1; zz < z + 1; zz++) //Bilder öffnen nacheinander
                {
                    Bitmap bmp = new Bitmap("G:\\MIP\\SlicesInBMP\\IM-0001-0" + Convert.ToString(zz) + ".bmp");
                    BitmapData bData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                    byte* scan0 = (byte*)bData.Scan0; //pointer, der auf die bits der bitmap zeigt

                    Bitmap bmpr = new Bitmap("G:\\MIP\\SlicesInBMPr\\IM-0001-0" + Convert.ToString(zz) + ".bmp");
                    BitmapData bDatar = bmpr.LockBits(new Rectangle(0, 0, bmpr.Width, bmpr.Height), ImageLockMode.ReadWrite, bmpr.PixelFormat);
                    byte* scan0r = (byte*)bDatar.Scan0;

                    if (phi != 0)
                    {
                        int cxi = (int)((bmp.Width - 1) / 2.0); //die Bildmitten ermitteln
                        int cyi = (int)((bmp.Height - 1) / 2.0);
                        int cxo = (int)((bmpr.Width - 1) / 2.0);
                        int cyo = (int)((bmpr.Height - 1) / 2.0);

                        for (int x = 0; x < bmp.Width; x++)
                        {
                            for (int y = 0; y < bmp.Height; y++)
                            {
                                scan0r[bmpr.Width * y + x] = 0;
                            }
                        }

                        for (int x = 0; x < bmp.Width; x++)
                        {
                            for (int y = 0; y < bmp.Height; y++)
                            {
                                int xr = (int)((x - cxo) * Math.Cos(phi) + (y - cyo) * Math.Sin(phi) + cxi); // Rotationsformeln für das Bild um den Mittelpunkt
                                int yr = (int)(-(x - cxo) * Math.Sin(phi) + (y - cyo) * Math.Cos(phi) + cyi);

                                if (xr >= 0 && yr >= 0 && xr < bmp.Width && yr < bmp.Height)
                                {
                                    scan0r[bmpr.Width * yr + xr] = scan0[bmp.Width * y + x];
                                }
                            }
                        }
                    }

                    for (int x = 0; x < bmpr.Width; x++)
                    {
                        int i = 0;
                        int i2 = 0;
                        int y = 0;

                        while (i2 == 0 & y < bmpr.Height) //überprüfe jeden nten y Wert bis der Körperbereich erreicht ist, zur Beschleunigung des Programmes
                        {
                            B = scan0r[bmpr.Width * y + x];

                            if (B == 0)
                            {
                                y += 10;
                            }

                            else
                            {
                                while (i2 == 0) //gehe rückwärts, bis erster körperpixel erreicht ist
                                {
                                    y--;
                                    C = scan0r[bmpr.Width * y + x];

                                    if (C == 0)
                                    {
                                        i2++;
                                    }
                                }
                            }
                        }

                        while (i == 0 & y < bmpr.Height) //führe Suche nach Pixel über Threshold im Körper fort
                        {
                            B = scan0r[bmpr.Width * y + x];

                            if (B > T)
                            {
                                byte zBuffer = (byte)((1 - ((double)y / bmpr.Height)) * 255.0); //errechnet zBuffer Wert, abhängig von der Höhe der Bilder
                                Color Tiefe = Color.FromArgb(zBuffer, zBuffer, zBuffer);
                                outputImage.bitmap.SetPixel(x, zz - 1, Tiefe);
                                array[x, zz - 1] = y; // speichert die Tiefen Information der Pixel über dem Threshold für spätere Bearbeitung in einem Array
                                array2[x, zz - 1] = zBuffer; //speichert zBuffer werte in array2                        
                                i++;
                            }

                            else
                            {
                                y++;

                            }
                        }
                    }
                    bmpr.UnlockBits(bData);
                    bmpr.Dispose();
                    bmp.UnlockBits(bData);
                    bmp.Dispose();
                    outputImage.UpdateScreen();
                    progressBar1.PerformStep();
                }

            /* --------code für mehrmalige bilder
            //outputImage.bitmap.Save("C:\\Users\\nikla\\Desktop\\zbuffer\\" + Convert.ToString(phidegree) + ".bmp"); //winkel
            outputImage.bitmap.Save("C:\\Users\\nikla\\Desktop\\zbufferT\\" + Convert.ToString(T) + ".bmp"); //threshold

            Array.Clear(array, 0, array.Length);
            Array.Clear(array2, 0, array2.Length);
            progressBar1.Value = 0;
            for (int x = 0; x < outputImage.bitmap.Width; x++)
            {
                for (int y = 0; y < outputImage.bitmap.Height; y++)
                {
                    Color black = Color.FromArgb(0, 0, 0);
                    outputImage.bitmap.SetPixel(x, y, black);
                }
            }
            outputImage.UpdateScreen();
            */ //------------

            outputImage.bitmap.Save("C:\\Users\\nikla\\Desktop\\nativ.bmp");


            if (checkBox1.Checked == true)
                {
                    Kontrast();
                }


            outputImage.bitmap.Save("C:\\Users\\nikla\\Desktop\\autokontrast.bmp");

            /*---------code für mehrmalige bilder
            //phidegree += phistep;
            T++;
        }
        */ //----------------
        }


        private void button2_Click(object sender, EventArgs e) //setzt alles zurück
        {
            Array.Clear(array, 0, array.Length);
            Array.Clear(array2, 0, array2.Length);
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            trackBar1.Maximum = 255;
            trackBar1.Value = 0;
            trackBar2.Minimum = 0;
            trackBar2.Value = 255;
            label6.Text = Convert.ToString(trackBar1.Value);
            label7.Text = Convert.ToString(trackBar2.Value);
            progressBar1.Value = 0;
            loaded = false;

            for (int x = 0; x < outputImage.bitmap.Width; x++)
            {
                for (int y = 0; y < outputImage.bitmap.Height; y++)
                {
                    Color black = Color.FromArgb( 0 , 0 , 0 );
                    outputImage.bitmap.SetPixel(x, y, black);
                }
            }
            outputImage.UpdateScreen();
        }


        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label6.Text = Convert.ToString(trackBar1.Value);
            trackBar2.Minimum = trackBar1.Value;

            if (loaded == true)
            {
                Kontrast();
            }
        }


        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            label7.Text = Convert.ToString(trackBar2.Value);
            trackBar1.Maximum = trackBar2.Value;

            if (loaded == true)
            {
                Kontrast();
            }
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == true)
            {
                Kontrast();
            }
        }


        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded == true)
            {
                Kontrast();
            }
        }


        private void Kontrast() //kontrastanpassung
        {
            if (checkBox1.Checked == true) 
            {
                int max = 0;
                int min = 512;

                if (checkBox2.Checked == true) //automatisch
                {

                    for (int x = 0; x < 512; x++) //findet minimalen und maximalen pixel wert, ignoriert den Background
                    {
                        for (int y = 0; y < z; y++)
                        {
                            int a = array[x, y];

                            if (a > max && a != 0)
                            {
                                max = a;
                            }

                            if (a < min && a != 0)
                            {
                                min = a;
                            }
                        }
                    }
                }

                if (checkBox2.Checked == false) //manuell
                {
                    min =(int) (trackBar1.Value / 255.0 * 512);
                    max =(int) (trackBar2.Value / 255.0 * 512);
                }

                for (int x = 0; x < 512; x++)
                {
                    for (int y = 0; y < z; y++)
                    {
                        int d = max - min; //bereich der validen Pixel/Körperpixel
                        int kontrast = array[x, y];

                        if (kontrast != 0)  //background ignorieren
                        {                            
                            byte zBuffer2 = (byte)((1 - ((double)(kontrast - min) / d)) * 255.0); //skaliert einen neuen zBuffer Wert abhängig von maximalen und minimalen zBuffer Werten

                            if (kontrast <= min)
                            {
                                zBuffer2 = 255;
                            }

                            if (kontrast >= max)
                            {
                                zBuffer2 = 0;
                            }

                            Color Kontrast = Color.FromArgb(zBuffer2, zBuffer2, zBuffer2);
                            outputImage.bitmap.SetPixel(x, y, Kontrast);
                        }
                    }
                }
                outputImage.UpdateScreen();
            }


            if (checkBox1.Checked == false) // altes bild laden (ohne kontrastanpassung)
            {
                for (int x = 0; x < outputImage.bitmap.Width; x++)
                {
                    for (int y = 0; y < outputImage.bitmap.Height; y++)
                    {
                        int altewerte = array2[x, y];

                        if (altewerte != 0)  //background ignorieren
                        {
                            Color Old = Color.FromArgb(altewerte, altewerte, altewerte);
                            outputImage.bitmap.SetPixel(x, y, Old);
                        }

                    }
                }
                outputImage.UpdateScreen();
            }
        }

    }
}