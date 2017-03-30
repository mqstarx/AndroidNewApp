using Android.App;
using Android.Widget;
using Android.OS;
using uPLibrary.Networking.M2Mqtt;
using System;
using uPLibrary.Networking.M2Mqtt.Messages;
using Android.Net.Wifi;
using Android.Content;
using Android.Preferences;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Linq;
using Android.Graphics;
using System.Threading;
using System.Diagnostics;

namespace SmartLoadControl
{

    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/ic_launcher")]

    public class MainActivity : Activity
    {

        private Button m_BtnAdd;
        private MqttClient mqttClient;
        private LinearLayout m_frame_control;
        private List<string> topic_list;
        private FrameLayout m_frame_bottom;
        private byte m_conStateAlgoritm =0;
        Stopwatch m_watch;
        Thread timethread;
        bool m_timer_enable = false;
        long m_timer_count;
        bool m_has_fail_connect =false;
        //System.Timers.Timer m_CheckConnectionTimer;
        // Timer m_t;

        private void initWidget()
        {
            
            topic_list = new List<string>();
            m_frame_control = FindViewById<LinearLayout>(Resource.Id.linearLayout_mod);
            m_BtnAdd = FindViewById<Button>(Resource.Id.btn_AddMod);
            m_BtnAdd.Click += M_BtnAdd_Click;
            m_frame_bottom = FindViewById<FrameLayout>(Resource.Id.btn_add_layer);
            
            MqttConnect();
            AddModules();
            m_watch = new Stopwatch();
            m_watch.Start();
            timethread = new Thread(new
           ThreadStart(timethread_function));
            timethread.Start();
            // m_CheckConnectionTimer = new System.Timers.Timer(1000);
            //  m_CheckConnectionTimer.Elapsed += M_CheckConnectionTimer_Elapsed;
            // m_CheckConnectionTimer.Start();
            //   RunUpdateLoop();
            StartTimer1ms();


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
        private void timethread_function()
        {
            while (true)
            {
                if (m_watch.ElapsedMilliseconds - m_timer_count > 1000 && m_timer_enable == true)
                {
                    if (mqttClient == null || !mqttClient.IsConnected)
                    {

                        // Time  tick 1ms
                        RunOnUiThread(() =>
                         {
                             if (m_has_fail_connect)
                                 MqttConnect();
                             else
                             {
                                 m_frame_bottom.SetBackgroundColor(Color.Gray);
                                 m_has_fail_connect = true;
                             }
                            
                         });

                    }
                    else
                    {
                        if (m_has_fail_connect)
                        {
                            m_frame_bottom.SetBackgroundColor(Color.Green);
                            m_has_fail_connect = false;
                        }
                    }


                }
            }
        }
        public override void Finish()
        {
            timethread.Abort();
            base.Finish();

        }
        public override void OnBackPressed()
        {
            timethread.Abort();
            base.OnBackPressed();
        }
        /*private async void RunUpdateLoop()
        {
           
            while (true)
            {
                await Task.Delay(1000);
                if (mqttClient == null || !mqttClient.IsConnected)
                {
                    MqttConnect();
                    AddModules();
                }
            }
        }*/
        /* private void M_CheckConnectionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
         {
             RunOnUiThread(() =>
             {
                 MqttConnect();
                // AskAllDev();

             });

            // SubscrieAllDev();

         }*/

        private void MqttConnect()
        {
           
                if (mqttClient == null)
                {
                    try
                    {

                        mqttClient = new MqttClient("test.mosca.io");
                        
                        mqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;

                        mqttClient.Connect("SmartAPP" + new Random(10000).Next().ToString());
                        m_frame_bottom.SetBackgroundColor(Color.Green);
                        //AddModules();
                      

                    }
                    catch
                    {
                        m_frame_bottom.SetBackgroundColor(Color.Gray);
                       
                    }

                }
                else
                {
                    if (!mqttClient.IsConnected)
                    {
                        try
                        {

                            mqttClient.Connect("SmartAPP" + new Random(10000).Next().ToString());
                            m_frame_bottom.SetBackgroundColor(Color.Green);
                           
                           // AddModules();
                        }
                        catch
                        {
                            m_frame_bottom.SetBackgroundColor(Color.Gray);
                          
                        }
                    }
               
                }
            
           
          
        }
        private void MqttClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string result = System.Text.Encoding.UTF8.GetString(e.Message);
          
            RunOnUiThread(() => {


                int _btnCount = m_frame_control.ChildCount;

                List<Button> _listBtn = new List<Button>();
                for (int i = 0; i < _btnCount; i++)
                {
                    _listBtn.Add((Button)m_frame_control.GetChildAt(i));
                }

                foreach (Button b in _listBtn)
                {
                    if (((DevInfo)b.Tag).Topic == e.Topic)
                    {
                        if (result == "STATE:1")
                        {
                            b.SetTextColor(Android.Graphics.Color.Green);
                            ((DevInfo)b.Tag).State = 1;
                        }
                        if (result == "STATE:0")
                        {
                            b.SetTextColor(Android.Graphics.Color.Red);
                            ((DevInfo)b.Tag).State = 0;
                        }
                    }
                }


            });
        }

      
       

        private void M_BtnAdd_Click(object sender, EventArgs e)
        {


            var intent = new Intent(this, typeof(ModConfigAct));

            StartActivity(intent);

        }



        protected override void OnResume()
        {
            base.OnResume();

            if (mqttClient == null || !mqttClient.IsConnected)
            {
                MqttConnect();
              
            }
            AddModules();
        }

        private void AddModules()
        {
            List<DevInfo> dev_list_saved;
            var prefs = Application.Context.GetSharedPreferences("SmartHomeApp", FileCreationMode.Private);
            string dev_list_saved_json = prefs.GetString("DEVINFO", null);
            if (dev_list_saved_json != null)
            {
                dev_list_saved = JsonConvert.DeserializeObject<List<DevInfo>>(dev_list_saved_json);
                m_frame_control.RemoveAllViews();

              
               
                foreach (DevInfo s in dev_list_saved)
                {
                    LinearLayout.LayoutParams leftMarginParams = new LinearLayout.LayoutParams(-1, -2);
                    Button btn_dev = new Button(this);
                    btn_dev.Tag = s;
                    btn_dev.Text = s.Name;
                    btn_dev.SetBackgroundResource(Resource.Drawable.btn_AddDev_selector);
                    btn_dev.SetTextColor(Android.Graphics.Color.Gray);

             
                    btn_dev.Click += Btn_dev_Click;
                    btn_dev.LongClick += Btn_dev_LongClick;
                    m_frame_control.AddView(btn_dev, leftMarginParams);
                    if (mqttClient != null && mqttClient.IsConnected)
                    {
                        try
                        {
                            mqttClient.Subscribe(new string[] { s.Topic }, new byte[] { 0 });
                            mqttClient.Publish(s.ID + "/control/", System.Text.Encoding.UTF8.GetBytes("ASKSTATE"));
                        }
                        catch
                        { }
                    }
                }
            
            }
        }
        private void AskAllDev()
        {
            if (mqttClient != null && mqttClient.IsConnected)
            {
                for(int i =0; i< m_frame_control.ChildCount; i++)
                {
                    DevInfo _devInfo = (DevInfo)(((Button)m_frame_control.GetChildAt(i)).Tag);
                    if(_devInfo.State == 2)
                        mqttClient.Publish(_devInfo.ID + "/control/", System.Text.Encoding.UTF8.GetBytes("ASKSTATE"));
                }
            }
        }
        private void SubscrieAllDev()
        {
            if (m_conStateAlgoritm == 1 || m_conStateAlgoritm == 3)
            {
                if (mqttClient != null && mqttClient.IsConnected)
                {
                    for (int i = 0; i < m_frame_control.ChildCount; i++)
                    {
                        DevInfo _devInfo = (DevInfo)(((Button)m_frame_control.GetChildAt(i)).Tag);

                        mqttClient.Subscribe(new string[] { _devInfo.Topic }, new byte[] { 0 });
                    }
                }
            }
        }
        private void Btn_dev_LongClick(object sender, Android.Views.View.LongClickEventArgs e)
        {
          
            if (mqttClient != null && mqttClient.IsConnected)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle(Resource.String.question);
                
                alert.SetItems(new string[] {GetString(Resource.String.qyes), GetString(Resource.String.qno), GetString(Resource.String.qdelete), GetString(Resource.String.qcancel) }, (senderAlert, args) => {
                  if(args.Which ==0)
                  {
                        mqttClient.Publish(((DevInfo)(((Button)sender).Tag)).ID + "/control/", System.Text.Encoding.UTF8.GetBytes("RESET"));
                  }
                    if (args.Which == 1)
                    {
                        mqttClient.Publish(((DevInfo)(((Button)sender).Tag)).ID + "/control/", System.Text.Encoding.UTF8.GetBytes("UPDATE"));
                    }
                    if (args.Which == 2)
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
                            if (s.ID == ((DevInfo)((Button)sender).Tag).ID)
                            {
                                dev_list_saved.Remove(s);
                            }
                        }


                    
                        string stringjson = JsonConvert.SerializeObject(dev_list_saved);
                        var prefEditor = prefs.Edit();

                        prefEditor.PutString("DEVINFO", stringjson);
                        prefEditor.Commit();
                        AddModules();
                    }
                    if (args.Which == 3)
                    {
                        Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();
                    }


                });
                Dialog dialog = alert.Create();
                dialog.Show();

            }
        }
        private void Btn_dev_Click(object sender, EventArgs e)
        {
            if (mqttClient != null && mqttClient.IsConnected)
            {
                DevInfo _devInfo = (DevInfo)((Button)sender).Tag;
                if (_devInfo.State==2)
                    mqttClient.Publish(_devInfo.ID+ "/control/", System.Text.Encoding.UTF8.GetBytes("ASKSTATE"));
                if (_devInfo.State == 1)
                    mqttClient.Publish(_devInfo.ID + "/control/", System.Text.Encoding.UTF8.GetBytes("OFF"));
                if (_devInfo.State  == 0)
                    mqttClient.Publish(_devInfo.ID + "/control/", System.Text.Encoding.UTF8.GetBytes("ON"));
            }
        }

        protected override void OnCreate (Bundle savedInstanceState)
		{
			Xamarin.Insights.Initialize (XamarinInsights.ApiKey, this);
            
			base.OnCreate (savedInstanceState);
			
			SetContentView (Resource.Layout.Main);
            
			initWidget ();
         
         



        }
       
	}
}
