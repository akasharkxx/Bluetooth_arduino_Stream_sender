using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Bluetooth;
using System;
using System.Linq;


namespace Software_Part_0._01
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        BluetoothConnection mybluetooth = new BluetoothConnection();

        private string[] Items;
        int count = 0;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Xamarin.Essentials.Platform.Init(this, bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            Items = new string[10];
                
            EditText edittext = FindViewById<EditText>(Resource.Id.editText1);

            Button button = FindViewById<Button>(Resource.Id.AddButton);
            Button button2 = FindViewById<Button>(Resource.Id.SendButton);
            Button button3 = FindViewById<Button>(Resource.Id.ConnectButton);
            Button button4 = FindViewById<Button>(Resource.Id.DisconnectButton);

            TextView textView = FindViewById<TextView>(Resource.Id.textView1);
            TextView textView2 = FindViewById<TextView>(Resource.Id.textView2);
            TextView textView3 = FindViewById<TextView>(Resource.Id.textView3);
            TextView textView4 = FindViewById<TextView>(Resource.Id.textView4);

            BluetoothSocket _socket = null;


            System.Threading.Thread listenThread = new System.Threading.Thread(listener);
            listenThread.Abort();

            button3.Click += delegate {

                listenThread.Start();

                mybluetooth = new BluetoothConnection();
                //mybluetooth.thisSocket = null;
                //_socket = null;
                mybluetooth.getAdapter();
                mybluetooth.thisAdapter.StartDiscovery();

                try
                { 
                    mybluetooth.getDevice();
                    mybluetooth.thisDevice.SetPairingConfirmation(false);
                    //mybluetooth.thisDevice.Dispose();
                    mybluetooth.thisDevice.SetPairingConfirmation(true);
                    mybluetooth.thisDevice.CreateBond();
                }
                catch (Exception deviceEX)
                {
                }

                mybluetooth.thisAdapter.CancelDiscovery();
                _socket = mybluetooth.thisDevice.CreateRfcommSocketToServiceRecord(Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"));
                mybluetooth.thisSocket = _socket;
                //   System.Threading.Thread.Sleep(500);
                try
                {
                    mybluetooth.thisSocket.Connect();

                    textView4.Text = "Connected!";
                    button4.Enabled = true;
                    button3.Enabled = false;

                    if (listenThread.IsAlive == false)
                    {
                        listenThread.Start();
                    }
                    //else
                    //{
                    //    listenThread.Abort();
                    //}
                }
                catch (Exception CloseEX)
                {
                }                
            };

            button4.Click += delegate {

                try
                {
                    //  buttonDisconnect.Enabled = false;
                    button3.Enabled = true;
                    listenThread.Abort();

                    mybluetooth.thisDevice.Dispose();

                    mybluetooth.thisSocket.OutputStream.WriteByte(187);
                    mybluetooth.thisSocket.OutputStream.Close();

                    mybluetooth.thisSocket.Close();

                    mybluetooth = new BluetoothConnection();
                    _socket = null;

                    textView4.Text = "Disconnected!";
                }
                catch { }
            };

            button.Click += delegate{
                if (edittext.Text == "") {
                    Toast.MakeText(this, "Please Enter any number then press Add", ToastLength.Short).Show();
                }
                else{
                    textView.Text = "Added : " + edittext.Text;
                    Items[count++] = edittext.Text;
                    textView3.Text = "Count : " + count;
                    button2.Enabled = true;
                    edittext.Text = "";
                }
            };
            button2.Click += delegate
            {
                if (count <= 0){
                    button2.Enabled = false; 
                }
                else { 
                    textView2.Text = Items[--count];
                    textView3.Text = "Count : " + count;
                }
                byte[] SendString = new byte[3];
                for(int i = 0; i < Items[count].Length; i++){
                    SendString[i] = Convert.ToByte(Items[count][i]);
                }
                try{
                    mybluetooth.thisSocket.OutputStream.WriteAsync(SendString, 0, SendString.Length);
                }
                catch (Exception outPutEX)
                {
                }
            };


        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        void listener()
        {
            byte[] read = new byte[1];

            TextView readTextView = FindViewById<TextView>(Resource.Id.textView5);
            //DateTime onTime = DateTime.Now;
            //DateTime offTime = DateTime.Now;
            //DateTime thisTime;
            //TimeSpan thisSpan;

            //int timeSetOn = 0;
            //int timeSetOff = 0;

            TextView timeTextView = FindViewById<TextView>(Resource.Id.textView6);
            while (true)
            {
            //thisTime = DateTime.Now;
            try
            {
                    mybluetooth.thisSocket.InputStream.Read(read, 0, 1);
                    mybluetooth.thisSocket.InputStream.Close();
                    RunOnUiThread(() =>
                    {
                        if (read[0] == 1)
                        {

                            readTextView.Text = "Relay ON";

                            //if (timeSetOn == 0)
                            //{
                            //    onTime = DateTime.Now;
                            //    timeSetOn = 1;
                            //}
                        }
                        else if (read[0] == 0)
                        {
                            readTextView.Text = "Relay OFF";
                            //timeSetOn = 0;

                            timeTextView.Text = "";
                        }
                        //if (timeSetOn == 1)
                        //{
                        //    thisSpan = thisTime-onTime;
                        //    timeTextView.Text = thisSpan.Minutes + ":" + thisSpan.Seconds;
                        //}
                    });
                }
                catch { }
            }
        }
    }

    public class BluetoothConnection{
        public void getAdapter() { this.thisAdapter = BluetoothAdapter.DefaultAdapter;  }
        public void getDevice() { this.thisDevice = (from bd in this.thisAdapter.BondedDevices where bd.Name == "HC-05" select bd).FirstOrDefault(); }
        public BluetoothAdapter thisAdapter { get; set; }
        public BluetoothDevice thisDevice { get; set; }
        public BluetoothSocket thisSocket { get; set; }
    }
}