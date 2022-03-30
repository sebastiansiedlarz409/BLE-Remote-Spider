using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BLESpiderApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualView : ContentPage
    {
        private BtConnector bt;

        private bool connected = false;

        private byte SPEED = 0;
        private sbyte DIR = 0;
        private byte FORWARD = 1;

        private int Y = 0;

        public ManualView()
        {
            InitializeComponent();

            ConnectionBtn.Clicked += ConnectBtn_Clicked;

            SpeedUpBtn.Clicked += SpeedUpBtn_Clicked;
            SpeedDwBtn.Clicked += SpeedDwBtn_Clicked;
            StopBtn.Clicked += StopBtn_Clicked;
            DirBtn.Clicked += DirBtn_Clicked;
            SwitchViewBtn.Clicked += SwitchViewBtn_Clicked;

            ParamsBtn.IsEnabled = false;

            if (!Accelerometer.IsMonitoring)
                Accelerometer.Start(SensorSpeed.Default);

            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
        }

        private void SwitchViewBtn_Clicked(object sender, EventArgs e)
        {
            if (Accelerometer.IsMonitoring)
                Accelerometer.Stop();

            this.Navigation.PushAsync(new ButtonsView());
            this.Navigation.RemovePage(this);

            SendCommands();

            if (connected)
            {
                if (bt != null)
                    Disconnect();
            }
        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            Y = (int)((e.Reading.Acceleration.Y * 100)) /2;

            if (SPEED >= 30)
                DIR = (sbyte)Y;

            ParamsBtn.Text = $"S: {SPEED} DIR: {Y} {(FORWARD == 1 ? "FORWARD" : "BACKWARD")}";

            SendCommands();
        }

        private void DirBtn_Clicked(object sender, EventArgs e)
        {
            SPEED = 0;
            DIR = 0;

            FORWARD = FORWARD == (byte)1 ? (byte)0 : (byte)1;

            SendCommands();
        }

        private void StopBtn_Clicked(object sender, EventArgs e)
        {
            SPEED = 0;
            DIR = 0;

            SendCommands();
        }

        private void SpeedDwBtn_Clicked(object sender, EventArgs e)
        {
            if (SPEED >= 10)
            {
                if (SPEED <= 40)
                    SPEED = 0;
                else
                    SPEED -= 10;
            }

            SendCommands();
        }

        private void SpeedUpBtn_Clicked(object sender, EventArgs e)
        {
            if (SPEED < 100)
            {
                if (SPEED < 40)
                    SPEED = 40;
                else
                    SPEED += 10;
            }

            SendCommands();
        }

        private void OnConnection()
        {
            ConnectionBtn.Text = "Disconnect!";
            ConnectionBtn.TextColor = Color.Green;
            connected = true;
        }

        private void Disconnect()
        {
            DIR = 0;
            SPEED = 0;
            FORWARD = 0;

            bt.Disconnect();
            ConnectionBtn.Text = "Connect!";
            ConnectionBtn.TextColor = Color.Red;
            connected = false;
        }

        private void ConnectBtn_Clicked(object sender, EventArgs e)
        {
            if (connected)
            {
                if (bt != null)
                    Disconnect();
            }
            else
            {
                if (bt == null)
                    bt = new BtConnector();

                bt.Connect(OnConnection);
            }
        }

        private void SendCommands()
        {
            if (bt != null)
                bt.Send(new byte[] { 0xAA, SPEED, (byte)DIR, FORWARD }, 4);
        }
    }
}