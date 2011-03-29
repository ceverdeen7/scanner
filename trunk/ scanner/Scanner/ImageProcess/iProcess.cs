using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using Emgu.CV.VideoSurveillance;
using System.Drawing;
using System.Windows.Forms;

namespace ImageProcess
{
    public class iProcess
    {
        private Capture capture;
        private ImageViewer viewer;
        List<List<Point>> matrix = new List<List<Point>>();
        int w;
        int h;
        Image<Gray, Byte> showimg;

        #region Get Set methods
        public Image<Gray, Byte> getShowImage()
        {
            return this.showimg;
        }
        public List<List<Point>> getMatrix()
        {
            return this.matrix;
        }
        public void ClearCoordinates()
        {
            this.matrix.Clear();
        }
        public int getW()
        {
            return this.w;
        }
        public int getH()
        {
            return this.h;
        }
        #endregion

        #region Constructor of iProcess
        /// <summary>
        /// Constructor of iProcess. Its creates a viewer and a capture object.
        /// </summary>
        public iProcess()
        {            
            viewer = new ImageViewer(); //create an image viewer
            capture = new Capture(); //create a camera captue            
        }
        #endregion

        public void StartCameraCapture()
        {
            capture.FlipHorizontal = true;
            Application.Idle += new EventHandler(delegate(object sender, EventArgs e)
            {  //run this until application closed (close button click on image viewer)
                Image<Bgr, Byte> image = capture.QueryFrame().Convert<Bgr, Byte>(); //draw the image obtained from camera
                viewer.Image = image;
            });

            viewer.Text = "Webcamera";
            viewer.TopMost = false;
            viewer.ShowDialog(); //show the image viewer 
        }

        #region ShowCamera current frame
        /// <summary>
        /// Gets the frame from the webcam.
        /// </summary>
        /// <returns>Current frame.</returns>
        public Image<Bgr, Byte> ShowCamera()
        {
            return capture.QueryFrame().Convert<Bgr,Byte>();
        }
        #endregion

        public Image<Bgr, Byte> getCameraImage()
        {
            return (Image<Bgr,Byte>)this.viewer.Image;
        }

        public void ContourCoordinates()
        {
            Image<Bgr, Byte> img = this.ShowCamera();
            Image<Gray, Byte> g_img = this.FilterImage(img);
            Image<Gray, Byte> r_img = new Image<Gray, Byte>(new Size(g_img.Width, g_img.Height));
            this.h = g_img.Width;
            this.w = g_img.Height;

            using (MemStorage storage = new MemStorage()) //allocate storage for contour approximation
            {
                for (Contour<Point> contours = g_img.FindContours(
                    Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE,
                    Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_CCOMP,
                    storage);
                    contours != null;
                    contours = contours.HNext)
                {
                    Contour<Point> contour = contours.ApproxPoly(contours.Perimeter * 0.0005, storage);
                    
                    Point[] pts = contour.ToArray();
                    LineSegment2D[] edges = PointCollection.PolyLine(pts, false);

                    CvInvoke.cvDrawContours(r_img, contour, new MCvScalar(200), new MCvScalar(0, 200,0), 5, -1, LINE_TYPE.FOUR_CONNECTED, new Point(0, 0));
                    for (int k = 0; k < pts.Length; k++)
                    {
                        //r_img.Draw(new CircleF(pts[k], 2), new Gray(255), 1);
                        this.showimg = r_img;
                        //this.Coord2d.Add(pts[k]);
                        List<Point> p = new List<Point>();
                        p.Add(pts[k]);
                        matrix.Add(p);
                    }
                }
            }
        }

        #region FilterImage
        /// <summary>
        /// Filtering the image.
        /// </summary>
        /// <returns>Gray image.</returns>
        public Image<Gray, Byte> FilterImage(Image<Bgr,Byte> img)
        {
            Image<Bgr, Byte> temp = (Image<Bgr, Byte>)img;
            temp._ThresholdBinary(new Bgr(255,255,240), new Bgr(255,255,255));
            Image<Gray,Byte> gray_temp = temp.Convert<Gray, Byte>();
            gray_temp._EqualizeHist();

            return gray_temp;
        }
        #endregion
    }
}
