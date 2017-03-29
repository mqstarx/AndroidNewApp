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
using System.Xml.Serialization;
using Java.Lang;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace SmartLoadControl
{
    [DataContract]
    public class DevInfo:Java.Lang.Object
    {
        private byte m_State; // 2-неизвестно ; 1- включено; 0 - выключено
        private string m_Name;
        private string m_Topic;
        private string m_ID;
        public DevInfo(byte state,string name,string topic,string id)
        {
            m_State = state;
            m_Name = name;
            m_Topic = topic;
            m_ID = id;
        }
        public DevInfo()
        {
            m_State = 2;
            m_Name = "";
            m_Topic = "";
            m_ID = "";
        }
        [DataMember]
        public byte State
        {
            get
            {
                return m_State;
            }

            set
            {
                m_State = value;
            }
        }
        [DataMember]
        public string Name
        {
            get
            {
                return m_Name;
            }

            set
            {
                m_Name = value;
            }
        }
        [DataMember]
        public string Topic
        {
            get
            {
                return m_Topic;
            }

            set
            {
                m_Topic = value;
            }
        }
        [DataMember]
        public string ID
        {
            get
            {
                return m_ID;
            }

            set
            {
                m_ID = value;
            }
        }
        public override string ToString()
        {

            return "";
        }

       
    }
}