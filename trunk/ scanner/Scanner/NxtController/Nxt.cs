using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using NKH.MindSqualls;

namespace NxtController
{
    public class Nxt
    {
        protected NxtBrick nxt;
        
        public Nxt()
        {
            nxt = new NxtBrick(3);
            nxt.MotorC = new NxtMotor();
            try
            {
                this.nxt.Connect();
            }
            catch (Exception) { }
        }

        public void RotateMotor(sbyte acc = 100, uint degree = 360)
        {            
            nxt.MotorC.Run(acc, degree);
        }

        public string ConnectToNXT()
        {
            string msg = "";
            if (this.nxt.IsConnected)
            {
                msg = "Connected";
            }
            else
            {
                msg = "Not connected";
                try
                {
                    this.nxt.Connect();
                }
                catch (Exception){
                    MessageBox.Show("Hiba az NXT csatlakozásnál!");
                }
                
            }
            return msg;
        }
    }
}
