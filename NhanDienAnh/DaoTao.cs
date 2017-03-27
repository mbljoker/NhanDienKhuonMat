using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Luxand;
using System.Threading;

namespace NhanDienAnh
{
    public partial class DaoTao : Form
    {
        // program states: whether we recognize faces, or user has clicked a face
        enum ProgramState { psRemember, psRecognize }
        ProgramState programState = ProgramState.psRecognize;

        String TenCamera;
        bool needClose = false;
        string Ten;
        protected String TrackerMemoryFile = "tracker.dat";
        int mouseX = 0;
        int mouseY = 0;

        // WinAPI procedure to release HBITMAP handles returned by FSDKCam.GrabFrame
        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(IntPtr hObject);
        
        public DaoTao()
        {
            InitializeComponent();
        
        }

        private void DaoTao_Load(object sender, EventArgs e)
        {
            //Load thư viện lên kiểm tra mã acti
            if (FSDK.FSDKE_OK != FSDK.ActivateLibrary("iYL71M6OblPn4TOl8nYojjcGvZZaKo4seThAr+xuvRxW4gWSyK6glbCyrkFW9rzP1c/rLZbKCYeO15pjCoWGS9YAmb7i0U0RztaWBCPCdEqxy+YO1p0efMsRgocnVb1RM+Z2IRCMbvHoOQbg8fCZgKJ4wl+/1MfGHJKocXboYJU="))
            {
                MessageBox.Show("Please run the License Key Wizard (Start - Luxand - FaceSDK - License Key Wizard)", "Error activating FaceSDK", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            //Khởi tạo thư viện
            FSDK.InitializeLibrary();
            //khởi tạo cammera
            FSDKCam.InitializeCapturing();

            string[] danhSachCamera;
            int soLuongCMR;
            FSDKCam.GetCameraList(out danhSachCamera, out soLuongCMR);

            if (0 == soLuongCMR)
            {
                MessageBox.Show("Please attach a camera", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            TenCamera = danhSachCamera[0];
            FSDKCam.VideoFormatInfo[] formatList;
            FSDKCam.GetVideoFormatList(ref TenCamera, out formatList, out soLuongCMR);

            int VideoFormat = 0; // choose a video format
            pictureBox1.Width = formatList[VideoFormat].Width;
            pictureBox1.Height = formatList[VideoFormat].Height;
            this.Width = formatList[VideoFormat].Width + 48;
            this.Height = formatList[VideoFormat].Height + 96;
            this.MaximumSize = new Size(this.Width,this.Height);
            this.MinimumSize = new Size(this.Width, this.Height);
            Thread td = new Thread(nhanDien);
            td.IsBackground = true;
            td.Priority=ThreadPriority.Highest;
            td.Start();
        }
        public void nhanDien()
        {
            int CamXuLy = 0;
            int r = FSDKCam.OpenVideoCamera(ref TenCamera, ref CamXuLy);
            if (r != FSDK.FSDKE_OK)
            {
                MessageBox.Show("Error opening the first camera", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            int tracker = 0; //tạo tracker
            if (FSDK.FSDKE_OK != FSDK.LoadTrackerMemoryFromFile(ref tracker, TrackerMemoryFile)) // try to load saved tracker state
                FSDK.CreateTracker(ref tracker); // if could not be loaded, create a new tracker

            int err = 0; // set realtime face detection parameters
            FSDK.SetTrackerMultipleParameters(tracker,
                   "HandleArbitraryRotations=false; DetermineFaceRotationAngle=false; InternalResizeWidth=100; FaceDetectionThreshold=3;",
                   ref err);

            while (!needClose)
            {
                Int32 xuLyAnh = 0;
                if (FSDK.FSDKE_OK != FSDKCam.GrabFrame(CamXuLy, ref xuLyAnh)) // grab the current frame from the camera
                {
                    Application.DoEvents();
                    continue;
                }

                FSDK.CImage image = new FSDK.CImage(xuLyAnh); //new FSDK.CImage(imageHandle);
                //Image img = Image.FromFile("E:\\10257275_557036397739997_2637418459313670467_o.jpg");
                //Bitmap b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                //Graphics g = Graphics.FromImage((Image)b);

                //g.DrawImage(img, 0, 0, pictureBox1.Width, pictureBox1.Height);
                //g.Dispose();

                ///FSDK.CImage image = new FSDK.CImage(b);
                //FSDK.CImage image = new FSDK.CImage("E:\\10257275_557036397739997_2637418459313670467_o.jpg");

                long[] IDs;
                long faceCount = 0;
                FSDK.FeedFrame(tracker, 0, image.ImageHandle, ref faceCount, out IDs, sizeof(long) * 256); // maximum of 256 faces detected
                Console.Write(IDs[0]);
                Array.Resize(ref IDs, (int)faceCount);

                // make UI controls accessible (to find if the user clicked on a face)
                Application.DoEvents();
                //Image.
                Image frameImage = image.ToCLRImage();
                Graphics gr = Graphics.FromImage(frameImage);

                for (int i = 0; i < IDs.Length; ++i)
                {
                    FSDK.TFacePosition facePosition = new FSDK.TFacePosition();
                    FSDK.GetTrackerFacePosition(tracker, 0, IDs[i], ref facePosition);
                    Console.Write(IDs.Length.ToString());
                    int left = facePosition.xc - (int)(facePosition.w * 0.6);
                    int top = facePosition.yc - (int)(facePosition.w * 0.5);
                    int w = (int)(facePosition.w * 1.2);

                    String name;
                    int res = FSDK.GetAllNames(tracker, IDs[i], out name, 65536); // maximum of 65536 characters

                    if (FSDK.FSDKE_OK == res && name.Length > 0)
                    { // draw name
                        StringFormat format = new StringFormat();
                        format.Alignment = StringAlignment.Center;

                        gr.DrawString(name, new System.Drawing.Font("Arial", 16),
                            new System.Drawing.SolidBrush(System.Drawing.Color.LightGreen),
                            facePosition.xc, top + w + 5, format);
                    }

                    Pen pen = Pens.LightGreen;
                    if (mouseX >= left && mouseX <= left + w && mouseY >= top && mouseY <= top + w)
                    {
                        pen = Pens.Blue;
                        if (ProgramState.psRemember == programState)
                        {
                            if (FSDK.FSDKE_OK == FSDK.LockID(tracker, IDs[i]))
                            {
                                // get the user name
                                NhapTen inputName = new NhapTen();
                                if (DialogResult.OK == inputName.ShowDialog())
                                {
                                    Ten = inputName.Ten;
                                    //if (Ten == null || Ten.Length == 0)
                                    //{
                                    //    String s = "";
                                    //    FSDK.SetName(tracker, IDs[i], "");
                                    //    FSDK.PurgeID(tracker, IDs[i]);
                                    //}
                                   
                                    FSDK.SetName(tracker, IDs[i], Ten);
                                    FSDK.UnlockID(tracker, IDs[i]);
                                }
                            }
                        }
                    }
                    gr.DrawRectangle(pen, left, top, w, w);

                }
                programState = ProgramState.psRecognize;

                // display current frame
                pictureBox1.Image = frameImage;
                GC.Collect(); // collect the garbage after the deletion
            }
            FSDK.SaveTrackerMemoryToFile(tracker, TrackerMemoryFile);
            FSDK.FreeTracker(tracker);
            FSDKCam.CloseVideoCamera(CamXuLy);
            FSDKCam.FinalizeCapturing();

        }

        private void DaoTao_FormClosing(object sender, FormClosingEventArgs e)
        {
            needClose = true;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            programState = ProgramState.psRemember;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            mouseX = 0;
            mouseY = 0;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;
        }
    }
}
