using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using Engine3D;
using UV_DLP_3D_Printer;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform.Windows;
using System.IO;
namespace Engine3D
{
    public class PolyLine3d
    {
        public ArrayList m_points; // world coordinate points
        public Color m_color;
        // precached
        double minx, maxx;
        double miny, maxy;
        double minz, maxz;
        Point3d pmin, pmax;
        bool cached = false;
        public int linewidth;
        public bool visible;

        public PolyLine3d(PolyLine3d src) 
        {
            m_color = src.m_color;
            minx = src.minx;
            miny = src.miny;
            minz = src.minz;
            maxx = src.maxx;
            maxy = src.maxy;
            maxz = src.maxz;
            linewidth = 1;
            visible = true;
            m_points = new ArrayList();
            foreach (Point3d pnt in src.m_points) 
            {
                Point3d p = new Point3d(pnt.x, pnt.y, pnt.z,0.0);
                m_points.Add(p);
            }
        }
        public void SetZ(double z) 
        {
            //sets the z val of all the points
            foreach (Point3d pnt in m_points)
            {
                pnt.z = z;
            }
        }
        public PolyLine3d(Point3d p1, Point3d p2, Color clr) 
        {
            m_points = new ArrayList();
            linewidth = 1;
            m_color = clr;
            visible = true;
            m_points.Add(p1);
            m_points.Add(p2);
        }
        public PolyLine3d() 
        {
            m_points = new ArrayList();
            m_color = Color.Green;
            linewidth = 1;
            visible = true;
        }
        /*
         This function assumes that the polyline consists of 2 points
         */
        public Point3d IntersectZ(double zcur)
        {
            try
            {                
                // now, using the 3d line equation, calculate the x/y intersection of the z plane
                // the line is in the 0 and 1 index (start/end)
                Point3d p1 = (Point3d)m_points[0];
                Point3d p2 = (Point3d)m_points[1];
                
                //if both points are above or below it, return nothing
                if ((p1.z > zcur && p2.z > zcur) || (p1.z < zcur && p2.z < zcur))
                {
                    return null;// no intersection
                }

                // if both points are on the line
                if (p2.z == zcur && p1.z == zcur)
                    return null;

                // if one points z cordinate equals the z level:
                if (p1.z == zcur || p2.z == zcur) 
                {
                    // if the other point is below, return nothing
                    if ((p1.z == zcur && p2.z < zcur) || (p2.z == zcur && p1.z < zcur))
                    {
                        return null;
                    }
                    // if the other point is above, return the first point
                    if (p1.z == zcur && p2.z > zcur) 
                        return p1;
                    if (p2.z == zcur && p1.z > zcur) 
                        return p2;
                }


                Point3d p3d = new Point3d();                                
                // if 1 is above and 1 is below, calculate it.
            //    if ((p1.z > zcur && p2.z < zcur) || (p1.z < zcur && p2.z > zcur)) // i think this check is unessariy at this point
                //{
                    //should pre-cache this too
                if (cached == false)
                {
                    minx = (double)Math.Min(p1.x, p2.x);
                    maxx = (double)Math.Max(p1.x, p2.x);
                    miny = (double)Math.Min(p1.y, p2.y);
                    maxy = (double)Math.Max(p1.y, p2.y);
                    minz = (double)Math.Min(p1.z, p2.z);
                    maxz = (double)Math.Max(p1.z, p2.z);
                    if (p1.z < p2.z) // find the point with the min z
                    {
                        pmin = p1;
                        pmax = p2;
                    }
                    else
                    {
                        pmin = p2;
                        pmax = p1;
                    }

                    cached = true;
                }
                double zrange = maxz - minz;// the range of the z coord
                double scale = (double)((zcur - minz) / zrange);
                p3d.z = zcur; // set to the current z
                //p3d.x = LERP(pmin.x, pmax.x, scale); // do the intersection
                //p3d.y = LERP(pmin.y, pmax.y, scale);
                p3d.x =  (pmax.x - pmin.x) * scale + pmin.x;
                p3d.y =  (pmax.y - pmin.y) * scale + pmin.y;

                return p3d;
            }
            catch (Exception) 
            {
                return null;
            }
        }
        private static double LERP(double a, double b, double c) { return (double)(((b) - (a)) * (c) + (a)); }
        public void AddPoint(Point3d pnt) 
        {
            m_points.Add(pnt);
        }

        public void RenderGL() 
        {
            if (!visible)
                return;
            GL.Begin(BeginMode.LineStrip);//.Lines);
            GL.Color3(m_color);
            GL.LineWidth(linewidth);
            foreach (Point3d p in this.m_points) 
            {
                GL.Vertex3(p.x, p.y, p.z);
            }
            GL.End();
        }
        public bool Load(StreamReader sr) 
        {
            try
            {
                //load color
                int a = byte.Parse(sr.ReadLine());
                int r = byte.Parse(sr.ReadLine()); //R
                int g = byte.Parse(sr.ReadLine()); //G
                int b = byte.Parse(sr.ReadLine()); //B
                m_color = Color.FromArgb(a, r, g, b);

                //load numer of points
                int npoint = int.Parse(sr.ReadLine());
                //load points
                for (int c = 0; c< npoint; c++)
                {
                    Point3d p = new Point3d();
                    p.Load(sr);
                    m_points.Add(p);
                }
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }        
        }
        public bool Save(StreamWriter sw) 
        {
            try
            {
                //save color
                sw.WriteLine(m_color.A);
                sw.WriteLine(m_color.R);
                sw.WriteLine(m_color.G);
                sw.WriteLine(m_color.B);
                //save numer of points
                sw.WriteLine(m_points.Count);
                foreach (Point3d p in m_points) 
                {
                    p.Save(sw);
                }
                //save points
                return true;
            }
            catch (Exception) 
            {
                return false;
            }
        }
        public void Render(Camera cam, PaintEventArgs ev,int wid, int hei) 
        {
            try
            {
                ArrayList m_campnts = new ArrayList();
                //transform this from world to camera coordinates
                //project
                // then draw
                foreach (Point3d pnt in m_points)
                {
                    Point3d p = cam.viewmat.Transform(pnt);
                    m_campnts.Add(p);
                }
                if (m_campnts.Count > 1)
                {
                    Point[] line = new Point[m_campnts.Count];
                    if (cam.m_protyp == eProjectType.eParallel)
                    {
                        int idx = 0;
                        foreach (Point3d pnt in m_campnts)
                        {
                            line[idx].X = (int)((cam.m_scalex * pnt.x) + (wid / 2)); // need to add x and y offsets
                            line[idx].Y = (int)((cam.m_scaley * pnt.y) + (hei / 2)); // need to add x and y offsets
                            idx++;
                        }
                    }
                    Pen pen = new Pen(m_color, 2);
                    ev.Graphics.DrawLines(pen, line);
                }
            }
            catch (Exception) { }
        }
    }
}
