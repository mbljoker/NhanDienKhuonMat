using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Luxand;

namespace NhanDienAnh
{
    
    public partial class Form2 : Form
    {


        public static List<TFaceRecord> FaceList;

        static ImageList imageList1;
          
        
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            FaceList = new List<TFaceRecord>();
            imageList1 = new ImageList();
            Size size100x100 = new Size(100,100);
            imageList1.ImageSize = size100x100;
            imageList1.ColorDepth = ColorDepth.Depth24Bit;
            //textBox1.Dock = DockStyle.Bottom;
            listView1.OwnerDraw = false;
            listView1.View = View.LargeIcon;
           // listView1.Dock = DockStyle.Right;
            listView1.LargeImageList = imageList1;
            listView1.Visible = false;
            this.Width = this.Width - listView1.Width;

            if (FSDK.FSDKE_OK != FSDK.ActivateLibrary("iYL71M6OblPn4TOl8nYojjcGvZZaKo4seThAr+xuvRxW4gWSyK6glbCyrkFW9rzP1c/rLZbKCYeO15pjCoWGS9YAmb7i0U0RztaWBCPCdEqxy+YO1p0efMsRgocnVb1RM+Z2IRCMbvHoOQbg8fCZgKJ4wl+/1MfGHJKocXboYJU="))
            {
                MessageBox.Show("Lỗi dùng chùa !", "Error ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            if (FSDK.InitializeLibrary() != FSDK.FSDKE_OK)
                MessageBox.Show("Chưa cài đặt được!", "Error");
            if (FaceList.Count != 0)
            {
                listView1.Visible = true;
                for (int i = 0; i < FaceList.Count; i++)
                {
                    imageList1.Images.Add(FaceList[i].faceImage.ToCLRImage());
                    string fn = FaceList[i].ImageFileName;
                    listView1.Items.Add((imageList1.Images.Count - 1).ToString(), fn.Split('\\')[fn.Split('\\').Length - 1],i);
                    listView1.SelectedIndices.Clear();
                    listView1.SelectedIndices.Add(listView1.Items.Count - 1);

                }
            }


            
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "JPEG (*.jpg)|*.jpg|Windows bitmap (*.bmp)|*.bmp|All files|*.*";
            dlg.Multiselect = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                listView1.Visible = true;
                this.Width = this.Width + listView1.Width;
                try
                {
                    //Set các thông số độ nghiêng
                    FSDK.SetFaceDetectionParameters(false, true, 384);
                    ///Set thông số phát hiện khung mặt
                    FSDK.SetFaceDetectionThreshold((int)Form1.FaceDetectionThreshold);


                    foreach (string fn in dlg.FileNames)
                    {
                        TFaceRecord fr = new TFaceRecord();
                        fr.ImageFileName = fn;
                        //fr.FacePosition = new FSDK.TFacePosition();
                        fr.FacialFeatures = new FSDK.TPoint[2];
                        //fr.Template = new byte[FSDK.TemplateSize];
                        fr.image = new FSDK.CImage(fn);

                        //textBox1.Text += "Enrolling '" + fn + "'\r\n";
                        //textBox1.Refresh();
                        ///Lấy vị trí mặt trong ảnh 
                        //fr.FacePosition = fr.image.DetectFace();
                        FSDK.DetectMultipleFaces(fr.image.ImageHandle, ref fr.CountFace, out fr.FacePosition, sizeof(long) * 256);
                        Array.Resize(ref fr.FacePosition, fr.CountFace);
                        if (fr.FacePosition.Length == 0)
                            if (dlg.FileNames.Length <= 1)
                                MessageBox.Show("Không có khuôn mặt nào !", "Lỗi ");
                            else
                                MessageBox.Show (fn + ": Không tìm thấy khuôn mặt nào !");
                        else
                        {
                            //Sao chép mặt
                            fr.faceImage = fr.image.CopyRect((int)(fr.FacePosition[0].xc - Math.Round(fr.FacePosition[0].w * 0.5)),
                                                                    (int)(fr.FacePosition[0].yc - Math.Round(fr.FacePosition[0].w * 0.5)),
                                                                    (int)(fr.FacePosition[0].xc + Math.Round(fr.FacePosition[0].w * 0.5)),
                                                                    (int)(fr.FacePosition[0].yc + Math.Round(fr.FacePosition[0].w * 0.5)));

                            try
                            {
                                ///Lấy đặc điểm của mắt 
                                fr.FacialFeatures = fr.image.DetectEyesInRegion(ref fr.FacePosition[0]);
                            }
                            catch (Exception ex2)
                            {
                                MessageBox.Show(ex2.Message, "Lỗi không nhận diện được mắt !");
                                byte[] by = new byte[FSDK.TemplateSize];
                                by = null;
                                fr.Template.Add(by);
                            }

                            try
                            {
                               ///Lấy giá trị nhận diện khung mặt
                                byte[] by = new byte[FSDK.TemplateSize];
                                by = fr.image.GetFaceTemplateInRegion(ref fr.FacePosition[0]); // get template with higher precision
                                fr.Template.Add(by);
                            }
                            catch (Exception ex2)
                            {
                                MessageBox.Show(ex2.Message, "Không phát hiện khuôn mặt");
                            }
                            //Them vào danh sách 
                            FaceList.Add(fr);
                            
                            imageList1.Images.Add(fr.faceImage.ToCLRImage());
                            listView1.Items.Add((imageList1.Images.Count - 1).ToString(), fn.Split('\\')[fn.Split('\\').Length - 1], imageList1.Images.Count - 1);

                            //textBox1.Text += "File '" + fn + "' enrolled\r\n";
                            //textBox1.Refresh();

                            listView1.SelectedIndices.Clear();
                            listView1.SelectedIndices.Add(listView1.Items.Count - 1);
                           
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "Exception");
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView1.SelectedIndices.Count > 0)
            {
                Image img = FaceList[listView1.SelectedIndices[0]].image.ToCLRImage();
                //pictureBox1.Height = img.Height;
                //pictureBox1.Width = img.Width;
                //pictureBox1.Image = img;
                //pictureBox1.Refresh();
                Graphics gr = Graphics.FromImage(img);
                gr.DrawRectangle(Pens.LightGreen, FaceList[listView1.SelectedIndices[0]].FacePosition[0].xc - FaceList[listView1.SelectedIndices[0]].FacePosition[0].w / 2,
                    FaceList[listView1.SelectedIndices[0]].FacePosition[0].yc - FaceList[listView1.SelectedIndices[0]].FacePosition[0].w / 2,
                    FaceList[listView1.SelectedIndices[0]].FacePosition[0].w, FaceList[listView1.SelectedIndices[0]].FacePosition[0].w);
                ///Vẽ Mẳt
                //for (int i = 0; i < 2; ++i)
                //{
                //    FSDK.TPoint tp = FaceList[listView1.SelectedIndices[0]].FacialFeatures[i];
                //    gr.DrawEllipse(Pens.Blue, tp.x, tp.y, 3, 3);
                //}
                pictureBox1.Image = img;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FaceList = new List<TFaceRecord>();
            imageList1 = new ImageList();
            listView1.LargeImageList = imageList1;
            pictureBox1.Image = null;
            listView1.Items.Clear();
            listView1.Visible = false;
            this.Width = this.Width - listView1.Width;

        }
        
    }
}
