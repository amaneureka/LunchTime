/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2015, Aman Priyadarshi                                                    *
 * All rights reserved.                                                                    *
 *                                                                                         *
 * Redistribution and use in  source and  binary  forms,  with  or  without  modification  *
 * are permitted provided that the following conditions are met:                           *
 *                                                                                         *
 *        1. Redistributions of  source  code  must  retain the  above  copyright  notice  *
 *           this list of conditions and the following disclaimer.                         *
 *        2. Redistributions in  binary form  must  reproduce  the above copyright notice  *
 *           this list of conditions and the following  disclaimer  in  the documentation  *
 *           and/or other materials provided with the distribution.                        *
 *                                                                                         *
 * THIS SOFTWARE IS PROVIDED BY THE  COPYRIGHT HOLDERS  AND  CONTRIBUTORS "AS IS" AND ANY  *
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED  TO,  THE IMPLIED WARRANTIES  *
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE  DISCLAIMED. IN  NO  EVENT  *
 * SHALL THE   COPYRIGHT   HOLDER  OR  CONTRIBUTORS  BE  LIABLE  FOR ANY DIRECT, INDIRECT  *
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR  CONSEQUENTIAL DAMAGES  (INCLUDING, BUT NOT LIMITED  *
 * TO, PROCUREMENT OF SUBSTITUTE GOODS  OR  SERVICES; LOSS OF USE, DATA,  OR  PROFITS; OR  *
 * BUSINESS INTERRUPTION) HOWEVER CAUSED  AND  ON  ANY  THEORY OF  LIABILITY,  WHETHER IN  *
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY  *
 * WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.IO;
using System.Web;
using System.Net;
using System.Text;
using System.Drawing;
using System.Xml.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace LazyUrls
{
    public class Program
    {
        #region Fields
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private const string IMGUR_IMAGE_LINK = "<original_image>";
        private const string IMGUR_IMAGE_LINK2 = "</original_image>";

        public const string IMGUR_IMAGE = "http://i.imgur.com/";
        public const string IMGUR_DELETE_IMAGE = "http://imgur.com/delete/";
        #endregion
        
        #region Imports
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion

        private delegate IntPtr LowLevelKeyboardProc (int nCode, IntPtr wParam, IntPtr lParam);

        private static NotifyIcon TrayIcon;
        public static NotifyIcon ProgTray
        {
            get { return TrayIcon; }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            var ThisWindow = GetConsoleWindow();
            ShowWindow(ThisWindow, 0);
            
            _hookID = SetHook(_proc);

            RegisterIcon();

            Application.Run();
            UnhookWindowsHookEx(_hookID);
            TrayIcon.Visible = false;
        }

        private static void RegisterIcon()
        {
            TrayIcon = new NotifyIcon();
            TrayIcon.Text = Application.ProductName;
            TrayIcon.Icon = new Icon("app.ico");            
            TrayIcon.Visible = true;

            var Context = new ContextMenu();
            Context.MenuItems.Add(0, new MenuItem("Close", new EventHandler(delegate(object sender, EventArgs e)
            {
                Application.Exit();
            })));            
            TrayIcon.ContextMenu = Context;
        }
                
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static bool CTRL, SHIFT;
        public static bool Hook = false;//Just to make it *public* i had to write on more line :/
        private static IntPtr HookCallback (int nCode, IntPtr wParam, IntPtr lParam)
        {            
            if (nCode >= 0 && !Hook)
            {
                var key = (Keys)Marshal.ReadInt32(lParam);
                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    if (key == Keys.LControlKey)
                        CTRL = true;
                    else if (key == Keys.LShiftKey)
                        SHIFT = true;
                    else if (CTRL && SHIFT)
                    {
                        if (key == Keys.D4)
                            PerformShort();
                        else if (key == Keys.D5)
                        {
                            new ScreenForm().Show();
                        }
                    }
                }
                else
                {
                    if (key == Keys.LControlKey)
                        CTRL = false;
                    else if (key == Keys.LShiftKey)
                        SHIFT = false;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public static void UploadCapture(ScreenForm scrfrm, string file)
        {
            scrfrm.Close();
            try
            {
                using (var w = new WebClient())
                {
                    var values = new NameValueCollection
                    {
                        { "key", "433a1bf4743dd8d7845629b95b5ca1b4" },
                        { "image", Convert.ToBase64String(File.ReadAllBytes(file)) }
                    };
                    byte[] response = w.UploadValues("http://imgur.com/api/upload.xml", values);

                    string output = XDocument.Load(new MemoryStream(response)).ToString();
                    

                    //Parse Image Link from XML output
                    int i1 = output.IndexOf(IMGUR_IMAGE_LINK);
                    int i2 = output.IndexOf(IMGUR_IMAGE_LINK2);
                    if (i1 == -1
                        || i2 == -1)
                        throw new Exception("Bad Output!");

                    i1 += IMGUR_IMAGE_LINK.Length;

                    var url = output.Substring(i1, i2 - i1);
                    Clipboard.SetText(url);

                    LogDetails(ExtractUsefulImgur(output));
                    TrayIcon.ShowBalloonTip(1000, "Lazy Urls", "Screenshot Uploaded!", ToolTipIcon.Info);
                }
            }
            catch(Exception e)
            {
                TrayIcon.ShowBalloonTip(1000, "Lazy Urls", "Screenshot Upload Failed!", ToolTipIcon.Error);
            }
            File.Delete(file);
            Hook = false;
        }

        private static string ExtractUsefulImgur(string data)
        {
            const string org_hash1 = "<image_hash>";
            const string org_hash2 = "</image_hash>";
            const string del_hash1 = "<delete_hash>";
            const string del_hash2 = "</delete_hash>";

            var sb = new StringBuilder();
            sb.Append("[Capture] ");
            int i1 = data.IndexOf(org_hash1) + org_hash1.Length;
            int i2 = data.IndexOf(org_hash2, i1);

            sb.Append(IMGUR_IMAGE);
            sb.Append(data.Substring(i1, i2 - i1));
            sb.Append(',');

            i1 = data.IndexOf(del_hash1) + del_hash1.Length;
            i2 = data.IndexOf(del_hash2, i1);

            sb.Append(IMGUR_DELETE_IMAGE);
            sb.Append(data.Substring(i1, i2 - i1));
            return sb.ToString();
        }

        private static void PerformShort()
        {
            Hook = true;
            
            IDataObject ClipData = Clipboard.GetDataObject();
            if (ClipData.GetDataPresent(DataFormats.Text))
            {
                var urls = Clipboard.GetData(DataFormats.Text).ToString().Split(',');
                try
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < urls.Length; i++ )
                    {
                        sb.Append(GoogleShorten(urls[i]));
                        if (i != urls.Length - 1)
                            sb.Append(',');
                    }
                    Clipboard.SetText(sb.ToString());
                    LogDetails("[UrlShortner] " + sb.ToString());
                    TrayIcon.ShowBalloonTip(1000, "Lazy Urls", "Url Shorten Successful!", ToolTipIcon.Info);
                }
                catch(Exception ex)
                {                    
                    //For now ignore this
                    TrayIcon.ShowBalloonTip(1000, "Lazy Urls", "Url Shorten Failed!", ToolTipIcon.Error);
                }
            }

            Hook = false;
        }

        private const string QUERY_URL = "https://www.googleapis.com/urlshortener/v1/url?key=AIzaSyA1wk5SCltkXd8Lcn3qhH97RfQuNFJtFBo";
        public static string GoogleShorten(string url)
        {
            string post = "{\"longUrl\": \"" + url + "\"}";
            string shortUrl = url;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(QUERY_URL);

            try
            {
                request.ServicePoint.Expect100Continue = false;
                request.Method = "POST";
                request.ContentLength = post.Length;
                request.ContentType = "application/json";
                request.Headers.Add("Cache-Control", "no-cache");

                using (Stream requestStream = request.GetRequestStream())
                {
                    byte[] postBuffer = Encoding.ASCII.GetBytes(post);
                    requestStream.Write(postBuffer, 0, postBuffer.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader responseReader = new StreamReader(responseStream))
                        {
                            string json = responseReader.ReadToEnd();
                            shortUrl = Regex.Match(json, @"""id"": ?""(?<id>.+)""").Groups["id"].Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // if Google's URL Shortner is down...
                throw ex;
            }
            return shortUrl;
        }

        private static void LogDetails(string Output)
        {
            using(var SW = new StreamWriter("file.db", true))
            {
                SW.Write(DateTime.Now.ToString());
                SW.Write(' ');
                SW.WriteLine(Output);
                SW.Flush();
            }            
        }
    }
}
