using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using Emgu.CV.VideoSurveillance;
using ImageProcess;
using NxtController;
using System.ComponentModel;
using System.Drawing;

namespace _3dScanner
{
    class ScanEngine
    {
        protected int rawimgs = 0;                          /// count of raw images
        protected int textimgs = 0;                         /// count of texture images
        protected List<Image<Bgr, Byte>> rawImages;         /// List of raw images
        protected List<Image<Bgr, Byte>> textImages;        /// List of texture images
        protected iProcess iproc;                           /// iProcess object for imageprocessing
        protected Nxt nxt;                                  /// Nxt object to rotate the pad        
        BackgroundWorker bgWorker = new BackgroundWorker(); /// Background Thread Worker
        
        
        ImageBox img_box;
        int percent = 0;
        ProgressBar pb;
        Label lbl;
        string CurrentProcessName = "";

        

        #region Constructor
        /// <summary>
        /// Constructor of ScanEngine
        /// </summary>
        /// <param name="ip"></param>
        public ScanEngine(iProcess ip, ImageBox img_box)
        {
            this.img_box = img_box;
            rawImages = new List<Image<Bgr, byte>>();
            textImages = new List<Image<Bgr, byte>>();
            this.iproc = ip;
            this.nxt = new Nxt();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += new DoWorkEventHandler(bw_DoWork);
            bgWorker.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);            
        }
        #endregion

        #region Scan indítás
        public void StartScan(ProgressBar pb, Label lbl)
        {
            this.pb = pb;
            this.lbl = lbl;
            if (bgWorker.IsBusy != true)
            {                
                this.bgWorker.RunWorkerAsync();
            }     
        }
        #endregion

        #region DoWork
        /// <summary>
        /// A háttér "munkás" feladatai vannak itt beállítva.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            //////////////////////////////////////////////////
            // 360-szor fut végig a ciklus
            // 5 fokkal fog fordulni ciklusváltozonként
            Console.WriteLine("Nxt forgatás - Kontúr kinyerés");
            CurrentProcessName = "Nxt forgatás - Kontúr kinyerés";
            for (int i = 0; i <= 3600; i+=5)
            {
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    // Nxt forgatás - Kontúr kinyerés
                    this.sc_Nxt_Contour(i);
                    this.percent += i;
                    //////////////////////////////////////
                    //függvény helye
                    Thread.Sleep(10);
                    /////////////////////////////////////
                } 
            }
            //////////////////////////////////////////////////
            // LED LÁMPA BEKAPCSOLÁS !!!
            //////////////////////////////////////////////////
            //////////////////////////////////////////////////
            //////////////////////////////////////////////////
            // Textúra kiszedés
            this.percent = 0;
            Console.WriteLine("Nxt forgatás - Textúra kinyerés");
            CurrentProcessName = "Nxt forgatás - Textúra kinyerés";
            for (int i = 0; i <= 3600; i += 5)
            {
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    // Nxt forgatás - Textúra kinyerés
                    this.sc_Nxt_Contour(i);
                    this.percent += i;
                    Thread.Sleep(50);
                }
            }           
        }
        #endregion

        public int getProgress()
        {
            return this.percent;
        }

        #region Worker Completed
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                Console.WriteLine("canceled");
                CurrentProcessName = "Megszakítva !";
            }

            else if (!(e.Error == null))
            {
                //this.tbProgress.Text = ("Error: " + e.Error.Message);
                Console.WriteLine("Error: " + e.Error.Message);
                CurrentProcessName = "Hiba !"+ e.Error.Message;
            }
            else
            {
                //this.tbProgress.Text = "Done!";
                Console.WriteLine("Done");
                CurrentProcessName = "Kész !";
            }
            this.lbl.Text = this.CurrentProcessName.ToString();
        }
        #endregion

        #region Worker Progress Changed
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //this.tbProgress.Text = (e.ProgressPercentage.ToString() + "%");
            Console.WriteLine(e.ProgressPercentage.ToString() + "%");
            //this.pb.Value = e.ProgressPercentage;
            this.lbl.Text = this.CurrentProcessName.ToString();
        }
        #endregion

        #region Nxt és Kontúr kinyerés
        /// <summary>
        /// Nxt forgatás és Kontúr kinyerés
        /// </summary>
        /// <param name="i">aktuális fok</param>
        protected void sc_Nxt_Contour(int i)
        {
            try
            {
                this.percent = 0;
                bgWorker.ReportProgress(i);
                
                
                //kontúr koordináta számolás és tárolás
                iproc.ContourCoordinates();                
                //kontúr visszarajzolás
                this.img_box.Image = iproc.getShowImage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        #endregion

        #region Nxt és Textúra kinyerés
        /// <summary>
        /// Nxt forgatás és Textúra kinyerés
        /// </summary>
        /// <param name="i">aktuális fok</param>
        protected void sc_Nxt_Texture(int i)
        {
            try
            {
                this.percent = 0;
                bgWorker.ReportProgress(i);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        #endregion

        #region StopScanning
        /// <summary>
        /// Aktuális szkennelés leállítása
        /// </summary>
        public void StopScan()
        {     
            bgWorker.CancelAsync();            
            this.rawimgs = 0;
            this.textimgs = 0;
            this.rawImages.Clear();
            this.textImages.Clear();
            iproc.ClearCoordinates();
        }
        #endregion

        
    }
}
