using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;
using System.Data.SqlClient;


namespace WebKeyStreamerDualDemo
{
    public partial class WebkeyWindow : Form
    {
        // --------------------------- Web-Key Settings --------------------------
        private String _baseURL = "http://octethp:8097/WEB-key/Main";

        // Check with Webkey Admin Site:
        // http://octethp:8097/WEB-keyAdmin/SiteManagementHome.jsp
        int _siteID = 20008;


        // ReaderCodes can be found at Registry
        // to check Registry, go to: "Registry Editor"
        // ReaderCodes can be found at: Computer\HKEY_CUREENT_USER\Software\BIO-key\Common\Settings
        // Upek: 7
        // Futronic: 31
        int[] _ReaderCodeList = new int[] { 7, 31 };
        RegistryKey _reg = Registry.CurrentUser.CreateSubKey
                                ("Software\\BIO-key\\Common\\Settings");
        int _ReaderValue1;
        int _ReaderValue2;
        bool _scanKnob;
        private System.Windows.Forms.Timer timer1;
        int appID;

        // --------------------------- Database Settings --------------------------
        string connectionString;
        SqlConnection cnn;
        string _serverName = "acer2";
        string _databaseName = "Odin";
        string _passWord = "nidoict9999";

        public WebkeyWindow(string[] args)
        {
            InitializeComponent();
            CenterToScreen();
            if (args.Contains("/1"))
            {
                _ReaderValue1 = _ReaderCodeList[0];
                _ReaderValue2 = _ReaderCodeList[1];
                appID = 1000001;
            }
            else if (args.Contains("/2"))
            {
                _ReaderValue1 = _ReaderCodeList[1];
                _ReaderValue2 = _ReaderCodeList[0];
                appID = 2000002;
            }
            else
            {
                MessageBox.Show("Error: Option needs to be 1 or 2. Using default Reader 7.");
                // set 7 to be the first reader by default
                _ReaderValue1 = _ReaderCodeList[0];
                _ReaderValue2 = _ReaderCodeList[1];
                appID = 1000001;
            }

        }

        private void WebkeyWindow_Load(object sender, EventArgs e)
        {
            // --------------------------- Connecting to MSSQL Database --------------------------
            connectionString = @"Server=" + _serverName +
                    ";Database=" + _databaseName + ";User ID=odin;Password=" + _passWord;
            //connectionString = "Server= ACER2;Database=Odin1;Trusted_Connection=True" ';User ID=UserName;Password=Password
            Debug.WriteLine(connectionString);
            cnn = new SqlConnection(connectionString);

            try
            {
                cnn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No Connection: " + ex.Message);
            }

            if (_ReaderValue1 == (int)_reg.GetValue("ReaderCode1", ""))
            {
                // the primary Reader is not changed in the registry
            }
            else
            {   // If the primary Reader is changed in the registry
                // swap ReaderCode1 and ReaderCode2
                _reg.SetValue("ReaderCode1", _ReaderValue1);
                _reg.SetValue("ReaderCode2", _ReaderValue2);
                _ReaderValue1 = (int)_reg.GetValue("ReaderCode1", "");
                _ReaderValue2 = (int)_reg.GetValue("ReaderCode2", "");
            }
            toolStripStatusLabel1.Text = "Reader: " + _ReaderValue1.ToString();

            _scanKnob = false;
            Identification();

            // time1 is set to check every 2 seconds.
            // more details, please go to 
            // private void timer1_Tick(object sender, EventArgs e)
            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 2000; // in miliseconds
            timer1.Start();


        }

        private void Identification()
        {

            //Debug.WriteLine(_ReaderValue1);
            //Debug.WriteLine((int)_reg.GetValue("ReaderCode1", ""));
            //Debug.WriteLine(_ReaderValue2);
            //Debug.WriteLine((int)_reg.GetValue("ReaderCode2", ""));
            if (_ReaderValue1 == (int)_reg.GetValue("ReaderCode1", ""))
            {
                // the primary Reader is not changed in the registry
            }
            else
            {   // If the primary Reader is changed in the registry
                // swap ReaderCode1 and ReaderCode2
                _reg.SetValue("ReaderCode1", _ReaderValue1);
                _reg.SetValue("ReaderCode2", _ReaderValue2);
                _ReaderValue1 = (int)_reg.GetValue("ReaderCode1", "");
                _ReaderValue2 = (int)_reg.GetValue("ReaderCode2", "");
            }
            int retVal = webkey.Init(_baseURL, 0, 0, "");
            if (retVal != 0)
            {
                MessageBox.Show(webkey.LastErrorMessage);
                return;
            }
            webkey.Visible = true;
            webkey.SiteID = _siteID;

            webkey.PersonIDFlag = BioKey.WebKey.AuthenticationMethod.IdentifiyOnServer;
            retVal = webkey.IdentificationStart();

        }
        private void webkey_Notify(object sender, AxWebKeyClientLib._IWebKeyAppCtrlEvents_NotifyEvent e)
        {
            // two options to send feedbacks:
            // 1. to the status label on the app
            // 2. to another application 
            _scanKnob = true;
            SenderIPC Sender = new SenderIPC();
            Sender.senderID = appID;

            if (e.errorCode == 0)
            {
                string IdMessage = "$B " + e.personID;
                Sender.SendFingerPrintResult(IdMessage);
                toolStripStatusLabel1.Text = IdMessage.ToString();
            }
            else
            {
                string ErrorMessage = "Operation Failed: " + webkey.LastErrorMessage;
                Debug.WriteLine(ErrorMessage);
                toolStripStatusLabel1.Text = ErrorMessage.ToString();
                Sender.SendFingerPrintResult(ErrorMessage);
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            // if the finger print is not scanned, then scanKnob is set false
            // that means the scanning UI hasn't received finger prints yet
            // If the finger print is scanned, then scanKnob is set to be true
            // that means the scanning UI has already received finger prints,
            // in 2 seconds, the timer will triger the new finger scanning UI for next user.
            if (_scanKnob == true)
            {
                Identification();
                _scanKnob = false;
            }
            else
            {
                //MessageBox.Show("Hello");
            }

        }
        
    }
}
