﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Definitions.Charts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using Microsoft.Data.SqlClient;
using MotoStore.Database;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MotoStore.Views.Pages
{
    /// <summary>
    /// Interaction logic for PageBieuDo.xaml
    /// </summary>
    public partial class PageBieuDo : Page
    {
        private MainDatabase mdb = new MainDatabase();
        private SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Data"].ConnectionString);
        public PageBieuDo()
        {
            InitializeComponent();
            SrC = new SeriesCollection();
            dothi.ChartLegend.Visibility = Visibility.Collapsed;
            gridChonNgay.Visibility = Visibility.Collapsed;
            MenuTuChon.Visibility = Visibility.Collapsed;

            decimal[] arrDoanhThu = new decimal[1000];
            ChartValues<decimal> ListDoanhThu = new();
            Labels = new();
            con.Open();
            //Các dòng dưới Select Doanh Thu của tháng này và hiển thị lên đầu trang Biểu Đồ
            string dauthangnay ="1/"+DateTime.Now.Month+"/"+DateTime.Now.Year;

            for (DateTime date = DateTime.Parse(dauthangnay); date <= DateTime.Today; date = date.AddDays(1.0))
            {
                SqlCommand cmd = new SqlCommand("Select Sum(ThanhTien) from HoaDon where NgayLapHD = @Today", con);
                cmd.Parameters.Add("@Today", System.Data.SqlDbType.SmallDateTime);
                cmd.Parameters["@Today"].Value = date;
                SqlDataReader sda = cmd.ExecuteReader();
                if (sda.Read())
                {
                    if (sda[0] != DBNull.Value)
                        ListDoanhThu.Add((decimal)sda[0]);
                    else
                        ListDoanhThu.Add(0);
                }
                Labels.Add(date.ToString("dd"));
            }
                    
            SrC.Add(new LineSeries
            {
                Title= "VNĐ",
                Values = ListDoanhThu,
                Stroke = Brushes.White,
                StrokeThickness=2,
                Fill = null
            }); 
            con.Close();  
            Values = value => value.ToString("N");
            DataContext = this;            
        }
        
        public SeriesCollection SrC { get; set; }   
        public List<string> Labels { get; set; }

        public Func<decimal,string>Values { get; set; }
        
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ReportPage());
        }

        static int solanbam = 0; //biến dùng để ẩn và hiện menu Tự Chọn
        private void btnTuChon_Click(object sender, RoutedEventArgs e)
        {
            if (solanbam % 2 == 0)
                MenuTuChon.Visibility = Visibility.Visible;
            else
                MenuTuChon.Visibility = Visibility.Collapsed;
            solanbam++;
            gridChonNgay.Visibility = Visibility.Collapsed;
        }

        private void subitemNamTrc_Click(object sender, RoutedEventArgs e)
        {
            /*Mỗi lần nhấn menu Năm Trước này, ta sẽ clear hết các trường
              dữ liệu cũ, clear luôn thanh chọn ngày xem(Nếu có)
              sau đó đổ lại dữ liệu vào.
             */
            gridChonNgay.Visibility = Visibility.Collapsed;
            lblSeries.Content = "Tháng";
            lblDTThgNay.Content = "So Sánh Doanh Thu(Đơn Vị: VNĐ)";
            dothi.ChartLegend.Visibility = Visibility.Visible;

            while (dothi.Series.Count > 0) 
                dothi.Series.RemoveAt(0);  //Clear dữ liệu cũ
            Labels.Clear();  //Clear Nhãn cũ

            for (int i = 1; i <= 12; i++)
                Labels.Add(i.ToString()); //Đổ 12 tháng vào Nhãn
            dothi.AxisX[0].MinValue = 0;
            dothi.AxisX[0].MaxValue = 12;

            dothi.FontSize = 20;
            TrucHoanhX.FontSize = 20;
            dothi.Zoom = ZoomingOptions.None; //Không cho Zoom
            dothi.Pan = PanningOptions.None;  //Không cho Pan(Lia đồ thị)
            TrucHoanhX.Separator.Step = 1; //Set step Trục hoành = 1 để nhìn rõ 12 Tháng

            decimal[] arrVal2022 = new decimal[12];
            decimal[] arrVal2021 = new decimal[12];
            string StartDate2022;
            string EndDate2022;
            string StartDate2021;
            string EndDate2021;
            /*Mảng doanh thu từng tháng của 2 năm 22 và 21,
              cùng với đó là các tham số để truyền vào câu 
              lệnh Query trên C#
              */

            con.Open(); //<Mở kết nối để đọc dữ liệu vào 2 mảng
            for (int i = 1; i <= 12; i++)
            {
                StartDate2022 = "1/" + i + "/2022";
                int thangsau2022 = i + 1;
                if (i == 12)
                    EndDate2022 = "1/1/2023";
                else
                    EndDate2022 = "1/" + thangsau2022 + "/2022";

                SqlCommand cmd = new SqlCommand("SET Dateformat dmy\nSelect Sum(ThanhTien) from HoaDon where NgayLapHD >= @StartDate2022 and NgayLapHD < @EndDate2022", con);
                cmd.Parameters.Add("@StartDate2022", System.Data.SqlDbType.SmallDateTime);
                cmd.Parameters["@StartDate2022"].Value = StartDate2022;
                cmd.Parameters.Add("@EndDate2022", System.Data.SqlDbType.SmallDateTime);
                cmd.Parameters["@EndDate2022"].Value = EndDate2022;

                SqlDataReader sda = cmd.ExecuteReader();
                if (sda.Read())
                {
                    if (sda[0] != DBNull.Value)
                        arrVal2022[i - 1] = (decimal)sda[0];
                    else
                        arrVal2022[i - 1] = 0;
                }

                StartDate2021 = "1/" + i + "/2021";
                int thangsau2021 = i + 1;
                if (i == 12)
                    EndDate2021 = "1/1/2022";
                else
                    EndDate2021 = "1/" + thangsau2021 + "/2021";
                SqlCommand cmd2 = new SqlCommand("SET Dateformat dmy\nSelect Sum(ThanhTien) from HoaDon where NgayLapHD >= @StartDate2021 and NgayLapHD < @EndDate2021", con);
                cmd2.Parameters.Add("@StartDate2021", System.Data.SqlDbType.SmallDateTime);
                cmd2.Parameters["@StartDate2021"].Value = StartDate2021;
                cmd2.Parameters.Add("@EndDate2021", System.Data.SqlDbType.SmallDateTime);
                cmd2.Parameters["@EndDate2021"].Value = EndDate2021;
                SqlDataReader sda2 = cmd2.ExecuteReader();
                if (sda2.Read())
                {
                    if (sda2[0] != DBNull.Value)
                        arrVal2021[i - 1] = (decimal)sda2[0];
                    else
                        arrVal2021[i - 1] = 0;
                }
            }
            con.Close();

            //Hàng dưới thêm dữ liệu vào đồ thị
            SrC.Add(new ColumnSeries
            {
                Title = "2021",
                Values = new ChartValues<decimal> { arrVal2021[0], arrVal2021[1], arrVal2021[2], arrVal2021[3], arrVal2021[4], arrVal2021[5], arrVal2021[6], arrVal2021[7], arrVal2021[8], arrVal2021[9], arrVal2021[10], arrVal2021[11] },
                Fill = Brushes.DeepSkyBlue
            });
            SrC.Add(new ColumnSeries
            {
                Title = "2022",
                Values = new ChartValues<decimal> { arrVal2022[0], arrVal2022[1], arrVal2022[2], arrVal2022[3], arrVal2022[4], arrVal2022[5], arrVal2022[6], arrVal2022[7], arrVal2022[8], arrVal2022[9], arrVal2022[10], arrVal2022[11] },
                Fill = Brushes.Red
            });

            Values = value => value.ToString("N");
        }

        private void subitem2NamTrc_Click(object sender, RoutedEventArgs e)
        {
            gridChonNgay.Visibility = Visibility.Collapsed;
            lblSeries.Content = "Tháng";
            lblDTThgNay.Content = "So Sánh Doanh Thu(Đơn Vị: VNĐ)";
            dothi.ChartLegend.Visibility = Visibility.Visible;
            while (dothi.Series.Count > 0)
                dothi.Series.RemoveAt(0);
            Labels.Clear();
            for (int i = 1; i <= 12; i++)
                Labels.Add(i.ToString());
            dothi.AxisX[0].MinValue = 0;
            dothi.AxisX[0].MaxValue = 12;

            dothi.FontSize = 20;
            TrucHoanhX.FontSize = 20;
            dothi.Zoom = ZoomingOptions.None;
            dothi.Pan = PanningOptions.None;
            TrucHoanhX.Separator.Step = 1;

            decimal[] arrVal2022 = new decimal[12];
            decimal[] arrVal2021 = new decimal[12];
            string StartDate2022;
            string EndDate2022;
            string StartDate2021;
            string EndDate2021;

            con.Open();
            for (int i = 1; i <= 12; i++)
            {
                StartDate2022 = "1/" + i + "/2022";
                int thangsau2022 = i + 1;
                if (i == 12)
                    EndDate2022 = "1/1/2023";
                else
                    EndDate2022 = "1/" + thangsau2022 + "/2022";

                SqlCommand cmd = new SqlCommand("SET Dateformat dmy\nSelect Sum(ThanhTien) from HoaDon where NgayLapHD >= @StartDate2022 and NgayLapHD <= @EndDate2022", con);
                cmd.Parameters.Add("@StartDate2022", System.Data.SqlDbType.SmallDateTime);
                cmd.Parameters["@StartDate2022"].Value = StartDate2022;
                cmd.Parameters.Add("@EndDate2022", System.Data.SqlDbType.SmallDateTime);
                cmd.Parameters["@EndDate2022"].Value = EndDate2022;

                SqlDataReader sda = cmd.ExecuteReader();
                if (sda.Read())
                {
                    if (sda[0] != DBNull.Value)
                        arrVal2022[i - 1] = (decimal)sda[0];
                    else
                        arrVal2022[i - 1] = 0;
                }

                StartDate2021 = "1/" + i + "/2021";
                int thangsau2021 = i + 1;
                if (i == 12)
                    EndDate2021 = "1/1/2022";
                else
                    EndDate2021 = "1/" + thangsau2021 + "/2021";
                SqlCommand cmd2 = new SqlCommand("SET Dateformat dmy\nSelect Sum(ThanhTien) from HoaDon where NgayLapHD >= @StartDate2021 and NgayLapHD < @EndDate2021", con);
                cmd2.Parameters.Add("@StartDate2021", System.Data.SqlDbType.SmallDateTime);
                cmd2.Parameters["@StartDate2021"].Value = StartDate2021;
                cmd2.Parameters.Add("@EndDate2021", System.Data.SqlDbType.SmallDateTime);
                cmd2.Parameters["@EndDate2021"].Value = EndDate2021;
                SqlDataReader sda2 = cmd2.ExecuteReader();
                if (sda2.Read())
                {
                    if (sda2[0] != DBNull.Value)
                        arrVal2021[i - 1] = (decimal)sda2[0];
                    else
                        arrVal2021[i - 1] = 0;
                }
            }
            con.Close();

            SrC.Add(new ColumnSeries
            {
                Title = "2021",
                Values = new ChartValues<decimal> { arrVal2021[0], arrVal2021[1], arrVal2021[2], arrVal2021[3], arrVal2021[4], arrVal2021[5], arrVal2021[6], arrVal2021[7], arrVal2021[8], arrVal2021[9], arrVal2021[10], arrVal2021[11] },
                Fill = Brushes.DeepSkyBlue
            });
            SrC.Add(new ColumnSeries
            {
                Title = "2022",
                Values = new ChartValues<decimal> { arrVal2022[0], arrVal2022[1], arrVal2022[2], arrVal2022[3], arrVal2022[4], arrVal2022[5], arrVal2022[6], arrVal2022[7], arrVal2022[8], arrVal2022[9], arrVal2022[10], arrVal2022[11] },
                Fill = Brushes.Red
            });

            //Add dữ liệu 2020 ở phía dưới
            /*SrC.Add(new ColumnSeries
            {
                Title = "2020",
                Values = new ChartValues<decimal> { arrVal2022[0], arrVal2022[1], arrVal2022[2], arrVal2022[3], arrVal2022[4], arrVal2022[5], arrVal2022[6], arrVal2022[7], arrVal2022[8], arrVal2022[9], arrVal2022[10], arrVal2022[11] },
                Fill = Brushes.Red
            });*/
        }

        private void subitemChonNgayXem_Click(object sender, RoutedEventArgs e)
        {
            gridChonNgay.Visibility = Visibility.Visible;
            //Hiện mục chọn ngày mỗi khi click vào menu Chọn Ngày Xem
        }

        private void txtTuNgay_TextChanged(object sender, TextChangedEventArgs e)
        {
            for(int i=0;i<txtTuNgay.Text.Length;i++)
            {
                if (!((int)txtTuNgay.Text[i] >= 47 && (int)txtTuNgay.Text[i] <= 57))
                {
                    MessageBox.Show("Ô Từ Ngày chứa kí tự không hợp lệ!");
                    txtTuNgay.Text = txtTuNgay.Text.Substring(0, txtTuNgay.Text.Length - 1);
                    txtTuNgay.SelectionStart = txtTuNgay.Text.Length;
                    break;
                }
            }
            //Hàm này kiểm tra ô Textbox Từ Ngày có phải ngày hợp lệ
        }
        public bool IsValidDateTimeTest(string dateTime)
        {
            string[] formats = { "d/MM/yyyy" };
            DateTime parsedDateTime;
            return DateTime.TryParseExact(dateTime, formats, new CultureInfo("vi-VN"),
                                           DateTimeStyles.None, out parsedDateTime);
            //Hàm kiểm tra ngày có hợp lệ hay không          
        }

        private void txtTuNgay_LostFocus(object sender, RoutedEventArgs e)
        {
            if(!(IsValidDateTimeTest(txtTuNgay.Text)))
            {
                MessageBox.Show("Ô Từ Ngày Chứa Ngày Không Hợp Lệ!");
                txtTuNgay.Clear();
            }
            //Nếu ô Từ Ngày bị LostFocus mà trong ô đó chứa ngày kh hợp lệ thì xoá text ô đó
        }

        private void txtDenNgay_TextChanged(object sender, TextChangedEventArgs e)
        {
            for (int i = 0; i < txtDenNgay.Text.Length; i++)
                if (!((int)txtDenNgay.Text[i] >= 47 && (int)txtDenNgay.Text[i] <= 57))
                {
                    MessageBox.Show("Ô Đến Ngày chứa kí tự không hợp lệ!");
                    txtDenNgay.Text = txtDenNgay.Text.Substring(0, txtDenNgay.Text.Length - 1);
                    txtDenNgay.SelectionStart = txtDenNgay.Text.Length;
                    break;
                }
            /*Hàm này sẽ kiểm tra ô Textbox Đến Ngày có phải ngày hợp lệ
              trước khi bấm Xem. Kí tự '-' kh đc coi là 1 phần của ngày hợp lệ*/
        }

        private void txtDenNgay_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!(IsValidDateTimeTest(txtTuNgay.Text)))
            {
                MessageBox.Show("Ô Đến Ngày Chứa Ngày Không Hợp Lệ!");
                txtDenNgay.Clear();
            }
            else if (DateTime.Parse(txtTuNgay.Text) >= DateTime.Parse(txtDenNgay.Text))
            {
                MessageBox.Show("Từ Ngày không được phép lớn hơn hoặc bằng Đến Ngày, Hãy Nhập Lại!");
                txtDenNgay.Clear();
            }
            /*Khi ô Đến Ngày LostFocus, ta sẽ check nó có phải ngày hợp lệ hay kh,
              và check xem nó có bé hơn hoặc = Từ Ngày hay kh
             */
        }

        private void btnXem_Click(object sender, RoutedEventArgs e)
        {
            ChartValues<decimal> ChartVal = new();
            if (string.IsNullOrEmpty(txtTuNgay.Text) || string.IsNullOrEmpty(txtDenNgay.Text))
                MessageBox.Show("Vui Lòng Nhập Đầy Đủ 2 Ngày");
            else //Thoả mãn hết các điều kiện => được phép xem 
            {
                while (dothi.Series.Count > 0)
                    dothi.Series.RemoveAt(0);
                Labels.Clear();
                /*3 dòng trên để Clear hết các dữ liệu cũ,
                  Dọn chỗ cho dữ liệu mới*/

                lblSeries.Content = "         Ngày";
                dothi.Zoom = ZoomingOptions.X; //Cho phép Zoom trục hoành
                dothi.Pan = PanningOptions.X;  //Cho phép Lia trục hoành
                dothi.FontSize = 15;
                TrucHoanhX.FontSize = 12;
                TrucHoanhX.Separator.Step = 3; //Đặt mỗi giá trị trục hoành cách nhau 3 đơn vị
                con.Open();

                for (DateTime date = DateTime.Parse(txtTuNgay.Text); date <= DateTime.Parse(txtDenNgay.Text); date = date.AddDays(1.0))
                {
                    SqlCommand cmd = new SqlCommand("Select Sum(ThanhTien) from HoaDon where NgayLapHD = @Today", con);
                    cmd.Parameters.Add("@Today", System.Data.SqlDbType.SmallDateTime);
                    cmd.Parameters["@Today"].Value = date;
                    SqlDataReader sda = cmd.ExecuteReader();
                    if (sda.Read())
                    {
                        if (sda[0] != DBNull.Value)
                            ChartVal.Add((decimal)sda[0]);
                        else
                            ChartVal.Add(0);
                    }
                    if (date.Day == 1)
                        Labels.Add(date.ToString("dd-MM"));
                    else if (int.Parse(date.ToString("dd")) == DateTime.DaysInMonth(date.Year, date.Month))
                        Labels.Add(date.ToString("dd-MM"));
                    else if (int.Parse(date.ToString("dd")) == int.Parse(DateTime.Parse(txtDenNgay.Text).ToString("dd")))
                        Labels.Add(date.ToString("dd-MM"));
                    else
                        Labels.Add(date.ToString("dd"));
                }

                SrC.Add(new LineSeries
                {
                    Title = "VNĐ",
                    Values = ChartVal,
                    Stroke = Brushes.White,
                    StrokeThickness = 2,
                    Fill = null
                });
                con.Close();
                Values = value => value.ToString("N");
            }
        }

        private void TrucHoanhX_PreviewRangeChanged(LiveCharts.Events.PreviewRangeChangedEventArgs eventArgs)
        {
            //if less than -0.5, cancel
            if (eventArgs.PreviewMinValue < -0.5) eventArgs.Cancel = true;

            //if greater than the number of items on our array plus a 0.5 offset, stay on max limit
            //if (eventArgs.PreviewMaxValue > dothi.Series.Count - 0.5) eventArgs.Cancel = true;

            //finally if the axis range is less than 1, cancel the event
            if (eventArgs.PreviewMaxValue - eventArgs.PreviewMinValue < 1) eventArgs.Cancel = true;

            /*Hàm Sự Kiện này để hạn chế ng dùng 
              Zoom vào khoảng thời gian nằm ngoài
              2 cái Textbox Từ ngày và Đến ngày,
              Tuy nhiên vẫn còn chút lỗi hiển thị
             */
        }
    }
}
