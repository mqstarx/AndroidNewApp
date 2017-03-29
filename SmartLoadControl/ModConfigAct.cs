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
using Android.Net.Wifi;
using System.Timers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.NetworkInformation;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace SmartLoadControl
{
    [Activity(Label = "@string/select_dev")]
    public class ModConfigAct : Activity
    {
        

        private Button btn_Find;
        private Button m_BtnBack;
        private LinearLayout m_frame_control;
        private WifiManager wifiManager;
        Thread thdUDPServer;
        ProgressBar progr_bar;
      
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ModConfig);
            thdUDPServer = new Thread(new
            ThreadStart(serverThread));
            thdUDPServer.Start();
            // Create your application here
            this.m_BtnBack = FindViewById<Button>(Resource.Id.btn_back);
            this.m_BtnBack.Click += M_BtnBack_Click;
            this.btn_Find = FindViewById<Button>(Resource.Id.btn_find);
            btn_Find.Click += Btn_Find_Click;
            m_frame_control = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
            progr_bar = FindViewById<ProgressBar>(Resource.Id.progressBar2);
          //  timer_find = new System.Timers.Timer();
          //  timer_find.Enabled = false;
          //  timer_find.Interval = 500;
          //  timer_find.Elapsed += Timer_find_Elapsed;
         
        }



        private void Timer_find_Elapsed(object sender, ElapsedEventArgs e)
        {
          //  RunOnUiThread(() => { progr_bar.Visibility = ViewStates.Invisible; });
           // RunOnUiThread(() => { btn_Find.Enabled = true; });
          //  RunOnUiThread(() => { progr_bar.Invalidate(); });
         //   timer_find.Stop();
        }

        private void Btn_Find_Click(object sender, EventArgs e)
        {
            FindDev();
        }
        
        
        private void M_BtnBack_Click(object sender, EventArgs e)
        {
            this.Finish();
         
        }

        private void FindDev()
        {
            try
            {
                btn_Find.Enabled = false;
                
                progr_bar.Visibility = ViewStates.Visible;
                wifiManager = (WifiManager)Application.Context.GetSystemService(WifiService);
                if (!wifiManager.IsWifiEnabled)
                {
                    wifiManager.SetWifiEnabled(true);
                }

                wifiManager.StartScan();

                for (int i = 0; i < wifiManager.ScanResults.Count; i++)
                {

                    if (wifiManager.ScanResults[i].Ssid.ToString().Contains("SH_"))
                    {
                        Button exist = (Button)m_frame_control.FindViewWithTag(wifiManager.ScanResults[i].Ssid.ToString());
                        if (exist == null)
                        {
                            LinearLayout.LayoutParams leftMarginParams = new LinearLayout.LayoutParams(-1, -2);
                            Button btn1 = new Button(this);
                            btn1.Text = wifiManager.ScanResults[i].Ssid.ToString();
                            btn1.Tag = wifiManager.ScanResults[i].Ssid.ToString();
                            btn1.SetBackgroundResource(Resource.Drawable.btn_AddDev_selector);
                            btn1.SetTextColor(new Android.Graphics.Color(0xED, 0xED, 0xED));

                            btn1.Tag = wifiManager.ScanResults[i].Ssid.ToString();
                            btn1.Click += Btn1_Click;

                            m_frame_control.AddView(btn1, leftMarginParams);
                        }
                    }
                }
                progr_bar.Visibility = ViewStates.Invisible;
                btn_Find.Enabled = true;
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Short);
            }
        }

        private void Btn1_Click(object sender, EventArgs e)
        {
            
            WifiConfiguration wconf = new WifiConfiguration();
            wconf.Ssid = "\"" + ((Button)sender).Text + "\"";
            wconf.PreSharedKey = "\"" + "12345678" + "\"";
            int hhhh = wifiManager.AddNetwork(wconf);
            List<WifiConfiguration> list = new List<WifiConfiguration>(wifiManager.ConfiguredNetworks);
            if (list == null)
                return;
            for (int j = 0; j < list.Count; j++)
            {
                if (list[j].Ssid.Contains( ((Button)sender).Text))
                {
                    var intent = new Intent(this, typeof(ModConfigNext));
                    intent.PutExtra("WIFI_OLD_NAME", wifiManager.ConnectionInfo.SSID);
                    intent.PutExtra("IP_MOD", "192.168.4.1");
                    intent.PutExtra("ID", ((Button)sender).Tag.ToString());
                    wifiManager.Disconnect();
                    wifiManager.EnableNetwork(list[j].NetworkId, true);

                    wifiManager.Reconnect();

                    //  while(wifiManager.ConnectionInfo.SSID!=wconf.Ssid )
                    //    { }
                    progr_bar.Visibility = ViewStates.Invisible;
                   
                    intent.PutExtra("DEV_WIFI_NAME", list[j].Ssid);
                    

                        StartActivity(intent);
                        break;
               
                }
            }
        }
        private void SendUdp(string packet)
        {
            try
            {
                UdpClient client = new UdpClient();
                // Create broadcast endpoint
                IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 666);
                byte[] bytes = Encoding.ASCII.GetBytes(packet);
                client.Send(bytes, bytes.Length, ip);
                client.Close();
            }
            catch
            {

            }
        }
        private void serverThread()
        {
            // button.Text = "thread started13";
            UdpClient udpClient = new UdpClient(6666);

            while (true)
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.UTF8.GetString(receiveBytes);
                if (returnData.Contains("ID:"))
                {
                    Button exist = (Button)m_frame_control.FindViewWithTag(returnData.Split(';')[0].Split(':')[1]);

                    if (exist == null)
                    {
                        LinearLayout.LayoutParams leftMarginParams = new LinearLayout.LayoutParams(-1, -2);
                        Button btn2 = new Button(this);
                        btn2.Text = returnData.Split(';')[1].Split(':')[1];
                        btn2.SetBackgroundResource(Resource.Drawable.btn_AddDev_selector);
                        btn2.SetTextColor(Android.Graphics.Color.Green);
                        btn2.Tag = returnData.Split(';')[0].Split(':')[1];
                        btn2.Click += Btn2_Click;
                        RunOnUiThread(() => { m_frame_control.AddView(btn2, leftMarginParams); });
                    }

                }
            }
        }

        private void Btn2_Click(object sender, EventArgs e)
        {           
            List<DevInfo> dev_list_saved;
            var prefs = Application.Context.GetSharedPreferences("SmartHomeApp", FileCreationMode.Private);
            string dev_list_saved_json = prefs.GetString("DEVINFO", null);
            if (dev_list_saved_json == null)
            {
                dev_list_saved = new List<DevInfo>();
            }
            else
            {
                dev_list_saved = JsonConvert.DeserializeObject<List<DevInfo>>(dev_list_saved_json);
            }

            foreach (DevInfo s in dev_list_saved.ToList())
            {
                if (s.ID == ((Button)sender).Tag.ToString())
                {
                    dev_list_saved.Remove(s);
                }
            }

           dev_list_saved.Add(new DevInfo(2, ((Button)sender).Text.ToString(), ((Button)sender).Tag.ToString()+"/control/out", ((Button)sender).Tag.ToString()));
            string stringjson = JsonConvert.SerializeObject(dev_list_saved);
            var prefEditor = prefs.Edit();

            prefEditor.PutString("DEVINFO", stringjson);
            prefEditor.Commit();


           
            this.Finish();
        }
    }
}