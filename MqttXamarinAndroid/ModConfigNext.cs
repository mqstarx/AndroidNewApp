using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using Android.Net.Wifi;
using Newtonsoft.Json;
using Android.Graphics;
using System.Diagnostics;

namespace SmartLoadControl
{
    [Activity(Label = "@string/dev_config")]
    public class ModConfigNext : Activity
    {
       // TcpClient client; // Creates a TCP Client
      
        EditText ssidText;
        EditText ssid_pass;
        EditText devName;
      
        Button btn_ok;
        Button btn_read;
      
        Button back_btn;
        string wifi_old;
        string ip_mod;
        string ID;
        bool is_write =false;
        bool m_timer_enable = false;
        long m_timer_count;
        Stopwatch m_watch;
        private WifiManager wifiManager;
        Thread thdUDPServer;
        Thread timethread;
       // private System.Timers.Timer write_timer;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            
           
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.ModConfigNext);
           // client = new TcpClient(); //Trys to Connect
       
            ssidText = FindViewById<EditText>(Resource.Id.ssidText);
             ssid_pass = FindViewById<EditText>(Resource.Id.passText);
             devName = FindViewById<EditText>(Resource.Id.DevName);
            
            btn_ok = FindViewById<Button>(Resource.Id.ok_config_btn);
            
            btn_ok.Click += Btn_ok_Click;
            back_btn = FindViewById<Button>(Resource.Id.back_btn);
            back_btn.Click += Back_btn_Click;
            btn_read = FindViewById<Button>(Resource.Id.read_config_btn);
            btn_read.Click += Btn_read_Click;
            wifi_old = Intent.GetStringExtra("WIFI_OLD_NAME");
            ip_mod = Intent.GetStringExtra("IP_MOD").Replace("\"", ""); 
            ID = Intent.GetStringExtra("ID").Replace("\"", "");
            if (wifi_old!=null)
                ssidText.Text = wifi_old.Replace("\"","");
            m_watch = new Stopwatch();
            m_watch.Start();
            //long _count = m_watch.ElapsedMilliseconds;
           
            //wifiManager = (WifiManager)Application.Context.GetSystemService(WifiService);
        
           // int kkk = wifiManager.ConnectionInfo.Rssi;
           // while ((wifiManager.ConnectionInfo.SSID.Contains("SH_")==false && wifiManager.ConnectionInfo.Rssi <-100) ||  m_watch.ElapsedMilliseconds-_count<5000)
                 //   { }
           
                 thdUDPServer = new Thread(new
          ThreadStart(serverThread));
            thdUDPServer.Start();
           timethread = new Thread(new
          ThreadStart(timethread_function));
            timethread.Start();



        }

        private void timethread_function()
        {
            while (true)
            {
                if (m_watch.ElapsedMilliseconds - m_timer_count > 1000 && m_timer_enable == true)
                {

                    // Time  tick 1ms
                    RunOnUiThread(() =>
                    {
                        btn_ok.Enabled = true;
                        if (is_write)
                        {
                            btn_ok.SetBackgroundColor(Color.Green);
                        }
                        else
                        {
                            btn_ok.SetBackgroundColor(Color.Red);
                        }
                        is_write = false;
                    });

                    StopTimer1ms();


                }
            }
        }

        private void StartTimer1ms()
        {
            m_timer_count = m_watch.ElapsedMilliseconds;
            m_timer_enable = true;
        }
        private void StopTimer1ms()
        {
           
            m_timer_enable = false;
        }
       

        private void Btn_read_Click(object sender, EventArgs e)
        {
            SendUdp("ASKCONFIG");
        }

        private void Back_btn_Click(object sender, EventArgs e)
        {
           
                // Save modid;
                SendUdp("WIFICONNECT");
            List<DevInfo> dev_list_saved;
              var prefs = Application.Context.GetSharedPreferences("SmartHomeApp", FileCreationMode.Private);
              string dev_list_saved_json = prefs.GetString("DEVINFO", null);
            if (dev_list_saved_json == null)
            {
                 dev_list_saved = new List<DevInfo>();
            }
            else
            {
                dev_list_saved= JsonConvert.DeserializeObject<List<DevInfo>>(dev_list_saved_json);
            }

            foreach (DevInfo s in dev_list_saved.ToList())
            {
                if (s.ID ==ID)
                {
                    dev_list_saved.Remove(s);
                }
            }


            // dev_list_saved.Add("ID:" + ID + ";NAME:" + devName.Text);
            dev_list_saved.Add(new DevInfo(2, devName.Text, ID + "/control/out", ID));
            string stringjson = JsonConvert.SerializeObject(dev_list_saved);
            var prefEditor = prefs.Edit();
                
                prefEditor.PutString("DEVINFO", stringjson);
                prefEditor.Commit();
            
            wifiManager = (WifiManager)Application.Context.GetSystemService(WifiService);
            List<WifiConfiguration> list = new List<WifiConfiguration>(wifiManager.ConfiguredNetworks);
            for (int j = 0; j < list.Count; j++)
            {
                if (list[j].Ssid.Contains(wifi_old))
                {
                    wifiManager.Disconnect();
                    wifiManager.EnableNetwork(list[j].NetworkId, true);

                    wifiManager.Reconnect();
                }
            }

            thdUDPServer.Abort();
            timethread.Abort();
                    this.Finish();
          
        }
        private void SendUdp(string packet)
        {
            try
            {
                UdpClient client = new UdpClient();
                // Create broadcast endpoint
                IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 666);
                byte[] bytes = Encoding.UTF8.GetBytes(packet);
                client.Send(bytes, bytes.Length, ip);
                client.Close();
            }
            catch
            {

            }
        }
        private void serverThread()
        {
            
            UdpClient udpClient = new UdpClient(6666);
           
            while (true)
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.UTF8.GetString(receiveBytes);
                if (returnData.Contains("WRITE_OK"))
                {
                    // RunOnUiThread(() => { btn_ok.Enabled = true; });

                     is_write = true;
                }
                if (returnData.Contains("CONFIG:"))
                {
                    RunOnUiThread(() => { ssidText.Text = returnData.Split(':')[1].Trim();ssid_pass.Text = returnData.Split(':')[2].Trim(); devName.Text = returnData.Split(':')[3].Trim(); });

                }
              

            }
        }
        private void Btn_ok_Click(object sender, EventArgs e)
        {

                 btn_ok.Enabled = false;
           is_write = false;
            StartTimer1ms();
            SendUdp("CONFIG:" + ssidText.Text.Replace(" ", "")+":" + ssid_pass.Text+":"+ devName.Text);
                     
        }

        public override void OnBackPressed()
        {
            thdUDPServer.Abort();
            timethread.Abort();
            base.OnBackPressed();

        }
    }
}