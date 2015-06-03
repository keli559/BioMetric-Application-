using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// this application is a Sender
// made to work with a Receiver that is already made in Pascal by Ken Thompsen 
// The Receiver is located at 
// "K:\Curly\copydata\receiver\"
// 
// This application is meant to send string "Hello World" to the Receiver
// if successful, the Receiver would appear on screen:
//
//                 WM_CopyData from: 3819
//                 Received string "Hello World!" at 6/3/2015
//
// Application:
// 1. Put "Hello World!" message into a data structure called "COPYDATASTUCT"
// 2. Find receiver by its app title "ReceiverMainForm" and its Class "TReceiverMainForm" (debug window: a pop-up window should show up with non-zero integers)
// 3. Send message in COPYDATASTRUCT through WM_COPYDATA code to receiver

namespace WebKeyStreamerDualDemo
{
    class SenderIPC
    {
        public int senderID;
        //include SendMessage
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, ref COPYDATASTRUCT lParam);

        //include SendMessage
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpszClass, string lpszWindow);

        //this is a constant indicating the window that we want to send a text message
        const int WM_COPYDATA = 0x004A;
        //static void Main(string[] args)
        public void SendFingerPrintResult(string msg)
        {

            // ---------------- 1. set up message to be sent -----------------------
            //Put "Hello World!" message into a data structure called "COPYDATASTUCT"
            //string msg = "Hello World!";
            var cds = new COPYDATASTRUCT
            {
                dwData = new IntPtr(0),
                cbData = msg.Length + 1,
                lpData = msg
            };
            // ---------------- 2. Find receiver application window -----------------------
            IntPtr receiverHandle = FindWindow("TReceiverMainForm", "ReceiverMainForm");

            // debug window pop-up
            //MessageBox.Show(receiverHandle.ToString());
            if (receiverHandle == IntPtr.Zero)
            {
                MessageBox.Show("Receiver Not Found!");
            }
            else
            {
                // ---------------- 3. send message to receiver -----------------------
                try
                {
                    SendMessage(receiverHandle, WM_COPYDATA, senderID, ref cds);
                    // receiverHandle: handle for the receiver window
                    // WM_COPYDATA: an application that sends WM_COPYDATA message to pass data to another application
                    // 3819: handle of sender application, any preset integer can work. This is to distinguish which sender sends message on the receiver
                    // ref cds: data structure that contains the message to be sent.
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        //Used for WM_COPYDATA for string messages
        public struct COPYDATASTRUCT
        {
            //dwData
            //The data to be passed to the receiving application.
            public IntPtr dwData;
            // cbData
            // The size, in bytes, of the data pointed to by the lpData member.
            public int cbData;
            // lpDATA
            // The data to be passed to the receiving application. This member can be NULL.
            public string lpData;
        }
    }
}
