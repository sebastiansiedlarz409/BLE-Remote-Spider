using Plugin.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BLESpiderApp
{
    public class BtConnector
    {
        private readonly IAdapter adapter;
        private IDevice car = null;
        private IGattCharacteristic cmdCharacteristic;
        private IGattCharacteristic statusCharacteristic;
        private readonly string spiderName = "BTLE-REMOTE-SPIDER";

        public BtConnector()
        {
            adapter = CrossBleAdapter.Current;
        }

        public void Connect(Action action)
        {
            Debug.WriteLine("Scanning...");
            try
            {
                adapter.ScanForUniqueDevices().Subscribe(result =>
                {
                    if (result.Name == null)
                        return;

                    if (result.Name.Equals(spiderName))
                    {
                        Debug.WriteLine($"Spider found {result.Name}");
                        car = result;
                        action();

                        car.Connect();
                        action();

                        car.WhenAnyCharacteristicDiscovered().Subscribe(chr =>
                        {
                            if (chr.CanWrite())
                            {
                                cmdCharacteristic = chr;
                                Debug.WriteLine("Cmd characteristic found");
                            }
                            else if (chr.CanRead())
                            {
                                statusCharacteristic = chr;
                                Debug.WriteLine("Status characteristic found");
                            }
                        });
                    }
                    else
                    {
                        Debug.WriteLine($"Device found {result.Name}");
                    }
                });
            }
            catch (ArgumentException)
            {

            }
        }

        public void Disconnect()
        {
            if (car != null)
            {
                if (car.IsConnected())
                {
                    adapter.StopScan();
                    car.CancelConnection();
                }
            }
        }

        public byte[] CalculateChecksum(byte[] data)
        {
            byte a = 0;
            byte b = 0;

            foreach (byte item in data)
            {
                a = (byte)((a + item) % 255);
                b = (byte)((a + b) % 255);
            }

            return new byte[] { b, a };
        }

        public void Send(byte[] data, int size)
        {
            byte[] crc = CalculateChecksum(data);

            data = data.ToList().Concat(crc.ToList()).ToArray();

            if (cmdCharacteristic != null && car.IsConnected())
            {
                cmdCharacteristic.Write(data).Subscribe(result =>
                {
                    Debug.WriteLine($"Send {size} bytes");
                });
            }
        }
    }
}
