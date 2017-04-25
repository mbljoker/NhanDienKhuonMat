using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using Luxand;

namespace NhanDienAnh

{

    public partial class Form1 : Form
    {
        public static List<TFaceRecord> FaceListID=new List<TFaceRecord>();
        public List<Anh> AnhList=new List<Anh>();
        public static float FaceDetectionThreshold = 3;
        public static float FARValue = 60;
        static ImageList imageList2;
        /// <summary>
        /// 
        /// </summary>
        Hashtable XacThucAnh;
        List<Anh> DanhSachAnh;
        ImageList imageListthumbnail = new ImageList();
        ListViewItem items;
        int dem=0;
        public Form1()
        {
            InitializeComponent();
            XacThucAnh = new Hashtable();
            DanhSachAnh = new List<Anh>();
            
        }

        private void thêmẢnhToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "All Files|*.*|Windows Bitmap|*.bmp|JPEG Image|*.jpg"; ;
            openFileDialog.Title = "Thêm ảnh ";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string []tamChuoi=openFileDialog.FileNames;
                foreach (string i in tamChuoi)
                    if(XacThucAnh.ContainsKey(i)==false)
                    {
                        Anh tamAnh = new Anh();
                        tamAnh.URL = i;
                        tamAnh.TrangThai = false;
                        XacThucAnh.Add(i,1);
                        loadAnh(i);
                        AnhList.Add(tamAnh);
                    }
                    else
                    {
                        MessageBox.Show("Ảnh "+i+" đã có trong ablum", "Thông Báo");
                    }
            }
        }

        bool loadAnh(string path)
        {
            try
            {
                imageListthumbnail.ImageSize = new Size(50, 50);
                Image image = Image.FromFile(path);
                imageListthumbnail.Images.Add(image);
                listView1.LargeImageList = imageListthumbnail;
                items = new ListViewItem();
                items.ImageIndex = dem++;
                items.Text = path;
                items.ToolTipText = path;
                listView1.Items.Add(items);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            listView1.ContextMenuStrip = contextMenuStrip1;
            if (FSDK.FSDKE_OK != FSDK.ActivateLibrary("iYL71M6OblPn4TOl8nYojjcGvZZaKo4seThAr+xuvRxW4gWSyK6glbCyrkFW9rzP1c/rLZbKCYeO15pjCoWGS9YAmb7i0U0RztaWBCPCdEqxy+YO1p0efMsRgocnVb1RM+Z2IRCMbvHoOQbg8fCZgKJ4wl+/1MfGHJKocXboYJU="))
            {
                MessageBox.Show("Lỗi dùng chùa !", "Lỗi ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            FSDK.InitializeLibrary();
            this.Height = this.Height - groupBox3.Height;
            groupBox3.Visible = false;

            imageList2 = new ImageList();
            Size size100x100 = new Size(100,100);
            imageList2.ImageSize = size100x100;
            imageList2.ColorDepth = ColorDepth.Depth24Bit;

            listView2.OwnerDraw = false;
            listView2.View = View.LargeIcon;
           
            listView2.LargeImageList = imageList2;
            
            
            
        }

        public void loadAnhDon(string url)
        {
            //MessageBox.Show(url);
            Image anh = Image.FromFile(url, true);
            pictureBox1.Image = anh;
            pictureBox1.Tag = url;

        }
      

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            
        }

        private void đàoTạoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DaoTao daoTao = new DaoTao();
            daoTao.ShowDialog();
            
            
        }

       
        private void dĐaoạoToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ảnhĐàoTạoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.ShowDialog();
        }

        private void nhậnDiệnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            if (AnhList.Count == 0|| Form2.FaceList==null?true:Form2.FaceList.Count==0 )
        
                MessageBox.Show("Chưa có dữ liệu ảnh đào tạo hoặc chưa có ảnh nhận diện ", "Lỗi ");
            else
            {
                demlv2 = 0;
                listView2.Items.Clear();
                imageList2.Images.Clear();
                FaceListID.Clear();
                listView2.LargeImageList = imageList2;
               
                for (int i3 = 0; i3 < AnhList.Count;i3++ )
                {
                    try
                    {
                        string fn = AnhList[i3].URL;
                        TFaceRecord fr = new TFaceRecord();
                        fr.ImageFileName = fn;
                        //fr.FacePosition = new FSDK.TFacePosition();
                        fr.FacialFeatures = new FSDK.TPoint[FSDK.FSDK_FACIAL_FEATURE_COUNT];
                        //fr.Template = new byte[FSDK.TemplateSize];

                        try
                        {
                            fr.image = new FSDK.CImage(fn);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Lỗi tải file ");
                        }

                        FSDK.DetectMultipleFaces(fr.image.ImageHandle, ref fr.CountFace, out fr.FacePosition, sizeof(long) * 256);
                        Array.Resize(ref fr.FacePosition, fr.CountFace);
                        if (fr.CountFace != 0)
                            for (int i = 0; i < fr.FacePosition.Length; i++)
                            {
                                //fr.faceImage = fr.image.CopyRect((int)(fr.FacePosition.xc - Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.yc - Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.xc + Math.Round(fr.FacePosition.w * 0.5)), (int)(fr.FacePosition.yc + Math.Round(fr.FacePosition.w * 0.5)));

                                bool eyesDetected = false;
                                try
                                {
                                    fr.FacialFeatures = fr.image.DetectEyesInRegion(ref fr.FacePosition[i]);
                                    eyesDetected = true;
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message, "Lỗi nhận diện mắt");
                                    byte[] by = new byte[FSDK.TemplateSize];
                                    by = null;
                                    fr.Template.Add(by);
                                }

                                if (eyesDetected)
                                {
                                    byte[] by = new byte[FSDK.TemplateSize];
                                    by = fr.image.GetFaceTemplateInRegion(ref fr.FacePosition[i]); // get template with higher precision
                                    fr.Template.Add(by);
                                }
                            }
                        FaceListID.Add(fr);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("không mở được ảnh : " + ex.Message.ToString(), "Lỗi ");
                    }


                }
                ShowXuLi showXuLi = new ShowXuLi();
                showXuLi.max = 100;
                showXuLi.min = 1;
                showXuLi.step = 100 / FaceListID.Count;
                showXuLi.Show();

                for (int i1 = 0; i1 < FaceListID.Count; i1++)
                {
                    xuLy(FaceListID[i1],i1);
                    if (demlv2 >0) 
                    {
                        listView2.SelectedIndices.Clear();
                        listView2.SelectedIndices.Add(0);
                        listView2.CheckBoxes = true;
                    }
                    showXuLi.getIndex(((i1+1)*showXuLi.step).ToString(),0);
                }
                showXuLi.getIndex("100", 100);
                MessageBox.Show("Nhận diện được "+listView2.Items.Count+" ảnh có mặt bạn !","Thông báo ");
                showXuLi.Close();
                if (groupBox3.Visible == false)
                {
                    this.Height = this.Height + groupBox3.Height;
                    groupBox3.Visible = true;
                }
                 

            }
        }
        int demlv2 = 0;
        /// <summary>
        /// Xử Lý
        /// </summary>
        /// <param name="SearchFace"></param>
        public void xuLy(TFaceRecord SearchFace, int index)
        {
           
            float Threshold = 0.0f;
            FSDK.GetMatchingThresholdAtFAR(Form1.FARValue / 100, ref Threshold);

            int MatchedCount = 0;
            int FaceCount = Form2.FaceList.Count;
            float[] Similarities = new float[FaceCount];
            int[] Numbers = new int[FaceCount];
            for (int j = 0; j < SearchFace.Template.Count;j++ )
                for (int i = 0; i < Form2.FaceList.Count; i++)
                {
                    float Similarity = 0.0f;
                    TFaceRecord CurrentFace = Form2.FaceList[i];
                    byte[] anh1 = new byte[FSDK.TemplateSize];
                    byte[] anh2 = new byte[FSDK.TemplateSize];
                    anh1 = SearchFace.Template[j];
                    anh2 = Form2.FaceList[i].Template[0]; 
                    FSDK.MatchFaces(ref anh1, ref anh2, ref Similarity);
                    if (Similarity >= Threshold)
                    {
                        SearchFace.Template[0] = SearchFace.Template[j];
                        Form2.FaceList.Add(SearchFace);
                        imageList2.Images.Add(SearchFace.image.ToCLRImage());
                        items = new ListViewItem();
                        items.ImageIndex = demlv2++;
                        items.Text =SearchFace.ImageFileName ;
                        items.ToolTipText = SearchFace.ImageFileName;
                        items.Tag = SearchFace;
                        listView2.Items.Add(items);
                       // MessageBox.Show(index.ToString()+" "+AnhList.Count+" "+FaceListID.Count);
                        AnhList[index].TrangThai = true;///sfsdfds
                        return;
                        ////Similarities[MatchedCount] = Similarity;
                        ////Numbers[MatchedCount] = i;
                        ////++MatchedCount;
                    }
                }
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void listView2_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            pictureBox1.Image = ((TFaceRecord)e.Item.Tag).image.ToCLRImage();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void listView1_ItemSelectionChanged_1(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            loadAnhDon(e.Item.Text);
        }

        private void lưuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.Items != null && listView2.Items.Count > 0 && listView2.CheckedItems.Count>0)
            {
                FolderBrowserDialog fBD = new FolderBrowserDialog();
                if (fBD.ShowDialog() == DialogResult.OK)
                {
                    foreach (ListViewItem item in listView2.CheckedItems)
                    {
                        string NameImg = "ADMIN" + "_" + DateTime.Now.Year.ToString()
                                + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString()
                                + "_" + DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString()
                                + "_" + DateTime.Now.Second.ToString() + "_" + DateTime.Now.Millisecond.ToString()
                                + ".jpg";
                        ((TFaceRecord)item.Tag).image.Save(fBD.SelectedPath+"\\"+NameImg);
                        
                    }
                    MessageBox.Show("Lưu Thành Công !");
                    System.Diagnostics.Process.Start(fBD.SelectedPath);
                }
                
            }
            else
            {
                    MessageBox.Show("Không được chức năng này vì chưa chọn ảnh xuất hoặc chưa nhận diện !");
            }
        }

        private void xóaTấtCảToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XacThucAnh.Clear();
            
            AnhList.Clear();
            DanhSachAnh.Clear();
            listView1.Items.Clear();
            imageListthumbnail.Images.Clear();
            dem = 0;
            pictureBox1.Image = null;
        }

    }
}
