using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NaradaBatReader
{
    public class CountDownTimer : System.Windows.Forms.Timer
    {
        int countdown_interval; // the interval we are counting down from
        int countdown; // the variable holding the count down value
        Timer t;

        public delegate void CountdownExpiredEventHandler(Object sender, EventArgs e);
        public event CountdownExpiredEventHandler CountdownExpired;
        protected virtual void RaiseCountdownExpired(EventArgs e)
        {
            if (CountdownExpired != null) CountdownExpired(this, null);
        }

        public CountDownTimer()
        {
            countdown_interval = 1000;
            t = new Timer();
            t.Interval = 1000;
            t.Enabled = false;
            t.Tick += T_Tick;
        }

        private void T_Tick(object sender, EventArgs e)
        {
            countdown=countdown-t.Interval;
            if (countdown<=0)
            {
                countdown = countdown_interval;
                RaiseCountdownExpired(null);
            }
        }

        public int CountdownInterval
        {
            get {  return countdown_interval; }
            set {  countdown_interval = value; }
        }

        public int Countdown
            { 
            get { return countdown; } 
            set { countdown = value; }
            }

        public int TimerInterval
        {
            get { return t.Interval; }
            set { t.Interval = value; }
        }

        public void StartCountdownTimer()
        {
            t.Enabled = true; 
            countdown = countdown_interval; 
        }

        public void StopCountdownTimer()
        { 
            countdown = countdown_interval; 
            t.Enabled = false; 
        }

        public void RestartCountdownTimer()
        {
            countdown = countdown_interval;
        }


    }
}
