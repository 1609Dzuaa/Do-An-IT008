﻿using MotoStore.Database;
using System.Collections.ObjectModel;
using System;
using System.Windows;
using System.Linq;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Input;
using System.Windows.Controls;
using MotoStore.Views.Pages.LoginPages;
using OfficeOpenXml;
using System.IO;
using Microsoft.Win32;
using OfficeOpenXml.Style;

namespace MotoStore.Views.Pages.DataPagePages
{
    /// <summary>
    /// Interaction logic for DataView.xaml
    /// </summary>
    public partial class CustomerListPage
    {
        internal ObservableCollection<KhachHang> TableData;

        public CustomerListPage()
        {
            InitializeComponent();
            RefreshDataGrid();
        }

        private void RefreshDataGrid()
        {
            MainDatabase con = new();
            TableData = new(con.KhachHangs);
            foreach (var khachHang in TableData.Where(kh => kh.DaXoa).ToList())
                TableData.Remove(khachHang);
            grdCustomer.ItemsSource = TableData;
        }

        private void SaveToDatabase(object sender, RoutedEventArgs e)
        {
            if ((from c in from object i in grdCustomer.ItemsSource select grdCustomer.ItemContainerGenerator.ContainerFromItem(i) where c != null select Validation.GetHasError(c)).FirstOrDefault(x => x))
            {
                MessageBox.Show("Dữ liệu đang có lỗi, không thể lưu!");
                return;
            }
            SqlCommand cmd;
            using SqlConnection con = new(System.Configuration.ConfigurationManager.ConnectionStrings["Data"].ConnectionString);
            try
            {
                con.Open();
                using var trans = con.BeginTransaction();
                try
                {
                    cmd = new("set dateformat dmy", con, trans);

                    // Lý do cứ mỗi lần có cell sai là break:
                    // - Tránh trường hợp hiện MessageBox liên tục
                    // - Người dùng không thể nhớ hết các lỗi sai, mỗi lần chỉ hiện 1 lỗi sẽ dễ hơn với họ
                    foreach (var obj in grdCustomer.Items)
                    {
                        // Trường hợp gặp dòng trắng được người dùng thêm mà chưa chỉnh sửa
                        if (obj.GetType().GetProperties().Where(pi => pi.PropertyType == typeof(string)).Select(pi => (string)pi.GetValue(obj)).All(value => string.IsNullOrEmpty(value) || string.Equals(value, "Thường")))
                            continue;
                        if (obj is not KhachHang kh)
                            continue;
                        // Kiểm tra dữ liệu null & gán giá trị mặc định
                        if (string.IsNullOrWhiteSpace(kh.HoTenKh))
                            throw new("Họ tên không được để trống!");
                        if (string.IsNullOrEmpty(kh.GioiTinh))
                            throw new("Giới tính không được để trống!");
                        string ngSinh = kh.NgSinh.HasValue ? $"'{kh.NgSinh.Value:dd/MM/yyyy}'" : "null";

                        // Thêm mới
                        if (string.IsNullOrEmpty(kh.MaKh))
                            cmd.CommandText += $"\nInsert into KhachHang values(N'{kh.HoTenKh}', {ngSinh}, N'{kh.GioiTinh}', N'{kh.DiaChi}', '{kh.Sdt}', N'{kh.Email}', N'{kh.LoaiKh}', 0)";

                        // Cập nhật
                        else
                            cmd.CommandText += $"\nUpdate KhachHang Set HotenKh = N'{kh.HoTenKh}', NgSinh = {ngSinh}, GioiTinh = N'{kh.GioiTinh}', DiaChi = N'{kh.DiaChi}', Sdt = '{kh.Sdt}', Email = N'{kh.Email}', LoaiKh = N'{kh.LoaiKh}' Where MaKh = '{kh.MaKh}';";
                    }
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    // Làm mới nội dung hiển thị cho khớp với database
                    RefreshDataGrid();
                    MessageBox.Show("Lưu chỉnh sửa thành công!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    trans.Rollback();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Định nghĩa lại phím tắt Delete
        private new void PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not DataGrid dg)
                return;
            // Kiểm tra nếu không được phép chỉnh sửa thì không được xoá
            if (dg.IsReadOnly)
                return;
            // Kiểm tra xem key Delete có thực sự được bấm tại 1 hàng hoặc ô trong datagrid hay không
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            if (dep is not DataGridRow && dep is not DataGridCell)
                return;
            // Kiểm tra xem key Delete có được bấm trong khi đang chỉnh sửa ô hay không
            DataGridRow dgr = (DataGridRow)dg.ItemContainerGenerator.ContainerFromIndex(dg.SelectedIndex);
            if (e.Key == Key.Delete && !dgr.IsEditing)
            {
                // Nếu đáp ứng đủ điều kiện sẽ bắt đầu vòng lặp để xóa
                SqlCommand cmd;
                using SqlConnection con = new(System.Configuration.ConfigurationManager.ConnectionStrings["Data"].ConnectionString);
                try
                {
                    con.Open();
                    using var trans = con.BeginTransaction();
                    try
                    {
                        cmd = new(" ", con, trans);

                        foreach (var obj in dg.SelectedItems)
                        {
                            if (obj is not KhachHang kh)
                                continue;
                            // Trường hợp chưa thêm mới nên chưa có mã KH
                            if (string.IsNullOrEmpty(kh.MaKh))
                                continue;
                            // Xóa hàng
                            else
                                cmd.CommandText += $"Update KhachHang Set DaXoa = 1 Where MaKh = '{kh.MaKh}';\n";
                        }
                        cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show(ex.Message);
                        // Báo đã thực hiện xong event để ngăn handler mặc định cho phím này hoạt động
                        e.Handled = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    // Báo đã thực hiện xong event để ngăn handler mặc định cho phím này hoạt động
                    e.Handled = true;
                }
            }
        }

        private void RefreshView(object sender, RoutedEventArgs e)
            => RefreshDataGrid();

        // Tắt các hoạt động chỉnh sửa data nếu không phải quản lý
        private void UiPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                bool isQuanLy = string.Equals(PageChinh.getChucVu, "Quản Lý", StringComparison.OrdinalIgnoreCase);

                grdCustomer.IsReadOnly = !isQuanLy;

                if (sender is Button button)
                    button.Visibility = isQuanLy ? Visibility.Visible : Visibility.Collapsed;

                RefreshDataGrid();
            }
        }

        private void AddRow(object sender, RoutedEventArgs e)
            => TableData.Add(new() { LoaiKh = "Thường" });

        // Đẩy event mousewheel cho scrollviewer xử lý
        private void grdCustomer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            var parent = ((Control)sender).Parent as UIElement;
            parent?.RaiseEvent(eventArg);
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string filePath = string.Empty;
            // tạo SaveFileDialog để lưu file excel
            SaveFileDialog dialog = new()
            {
                // chỉ lọc ra các file có định dạng Excel
                Filter = "Excel | *.xlsx | Excel 2003 | *.xls"
            };

            // Nếu mở file và chọn nơi lưu file thành công sẽ lưu đường dẫn lại dùng
            if (dialog.ShowDialog() == true)
                filePath = dialog.FileName;

            // nếu đường dẫn null hoặc rỗng thì return hàm
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                using (ExcelPackage p = new())
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "MotoStore App";

                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "Danh sách khách hàng";

                    //Tạo một sheet để làm việc trên đó
                    p.Workbook.Worksheets.Add("Customer");

                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = p.Workbook.Worksheets["Customer"];

                    // merge các column lại từ column 1 đến số column header
                    // gán giá trị cho cell vừa merge là Danh sách khách hàng từ MotoStore
                    ws.Cells[1, 1].Value = "Danh sách khách hàng từ MotoStore";
                    ws.Cells[1, 1, 1, grdCustomer.Columns.Count].Merge = true;
                    // in đậm
                    ws.Cells[1, 1, 1, grdCustomer.Columns.Count].Style.Font.Bold = true;
                    // căn giữa
                    ws.Cells[1, 1, 1, grdCustomer.Columns.Count].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    int colIndex = 1;
                    int rowIndex = 2;

                    //tạo các header từ column header đã tạo từ bên trên
                    foreach (var item in grdCustomer.Columns)
                    {
                        var cell = ws.Cells[rowIndex, colIndex];

                        //set màu thành light blue
                        var fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                        //căn chỉnh các border
                        var border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                        //gán giá trị
                        cell.Value = item.Header;

                        if (item.Header.ToString().Contains("Ngày", StringComparison.OrdinalIgnoreCase))
                            // Format cho ngày
                            ws.Column(colIndex).Style.Numberformat.Format = "dd/MM/yyyy";

                        colIndex++;
                    }

                    // lấy ra danh sách KH từ TableData

                    ObservableCollection<KhachHang> customerList = new(TableData);

                    // với mỗi kh trong danh sách sẽ ghi trên 1 dòng
                    foreach (var kh in customerList.Where(kh => grdCustomer.Items.PassesFilter(kh)))
                    {
                        // bắt đầu ghi từ cột 1. Excel bắt đầu từ 1 không phải từ 0
                        colIndex = 1;

                        // rowIndex tương ứng từng dòng dữ liệu
                        rowIndex++;

                        //gán giá trị cho từng cell                      
                        ws.Cells[rowIndex, colIndex++].Value = kh.MaKh;
                        ws.Cells[rowIndex, colIndex++].Value = kh.HoTenKh;
                        ws.Cells[rowIndex, colIndex++].Value = kh.NgSinh;
                        ws.Cells[rowIndex, colIndex++].Value = kh.GioiTinh;
                        ws.Cells[rowIndex, colIndex++].Value = kh.DiaChi;
                        ws.Cells[rowIndex, colIndex++].Value = kh.Sdt;
                        ws.Cells[rowIndex, colIndex++].Value = kh.Email;
                        ws.Cells[rowIndex, colIndex++].Value = kh.LoaiKh;
                    }
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    //Lưu file lại
                    byte[] bin = p.GetAsByteArray();
                    File.WriteAllBytes(filePath, bin);
                }
                MessageBox.Show("Xuất excel thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}