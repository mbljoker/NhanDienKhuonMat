using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Luxand;

namespace NhanDienAnh
{
    public class TFaceRecord
    {
        public List<byte[]> Template = new List<byte[]>();///giá trị nhận dạng khung mặt
        public FSDK.TFacePosition[] FacePosition;//Lưu vị trí khung mặt
        public FSDK.TPoint[] FacialFeatures;///Lấy đặt điểm khung mặt mắt 
        public string ImageFileName; //Lưu đường dẫn file
        public FSDK.CImage image;//Lưu ảnh 
        public FSDK.CImage faceImage;//Lưu ảnh khuôn mặt
        public int CountFace;
    }
}
