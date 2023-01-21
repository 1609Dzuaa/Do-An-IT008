﻿using MotoStore.Database;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Linq;
using Wpf.Ui.Common.Interfaces;
using Microsoft.Data.SqlClient;
using System.Windows.Input;
using System.Windows.Controls;
using MotoStore.Views.Pages.LoginPages;
using System.Collections.ObjectModel;

namespace MotoStore.Views.Pages.DataPagePages
{
    /// <summary>
    /// Interaction logic for DataView.xaml
    /// </summary>
    public partial class MotoListPage
    {

        internal ObservableCollection<MatHang> TableData;

        public MotoListPage()
        {
            InitializeComponent();
            RefreshDataGrid();
        }

        private void RefreshDataGrid()
        {
            MainDatabase con = new();
            TableData = new(con.MatHangs);
            foreach (var MatHang in TableData.ToList())
                if (MatHang.DaXoa)
                    TableData.Remove(MatHang);
            grdMoto.ItemsSource = TableData;
        }

        private void SaveToDatabase(object sender, RoutedEventArgs e)
        {
            if ((from c in from object i in grdMoto.ItemsSource select grdMoto.ItemContainerGenerator.ContainerFromItem(i) where c != null select Validation.GetHasError(c)).FirstOrDefault(x => x))
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
                    foreach (object obj in grdMoto.Items)
                    {
                        // Trường hợp gặp dòng trắng dưới cùng của bảng (để người dùng có thể thêm dòng)
                        if (obj.GetType().GetProperties().Where(pi => pi.PropertyType == typeof(string)).Select(pi => (string)pi.GetValue(obj)).All(value => string.IsNullOrEmpty(value)))
                            continue;
                        if (obj is not MatHang mh)
                            continue;
                        // Kiểm tra dữ liệu null & gán giá trị mặc định
                        if (string.IsNullOrWhiteSpace(mh.TenMh))
                            throw new("Tên mặt hàng không được để trống!");
                        if (string.IsNullOrEmpty(mh.MaNcc))
                            throw new("Mã nhà cung cấp không được để trống!");
                        string giaNhapMh = mh.GiaNhapMh.HasValue ? mh.GiaNhapMh.Value.ToString() : "null";
                        string giaBanMh = mh.GiaBanMh.HasValue ? mh.GiaBanMh.Value.ToString() : "null";
                        // Thêm mới
                        if (string.IsNullOrEmpty(mh.MaMh))
                            cmd.CommandText += $"\nInsert into MatHang values(N'{mh.TenMh}', {mh.SoPhanKhoi}, N'{mh.Mau}', {giaNhapMh}, {giaBanMh}, {mh.SoLuongTonKho}, '{mh.MaNcc}', N'{mh.HangSx}', N'{mh.XuatXu}', N'{mh.MoTa}', 0)";

                        // Cập nhật
                        else
                            cmd.CommandText += $"\nUpdate MatHang Set TenMh = N'{mh.TenMh}', SoPhanKhoi = {mh.SoPhanKhoi}, Mau = N'{mh.Mau}', GiaNhapMh = {giaNhapMh}, GiaBanMh = {giaNhapMh}, SoLuongTonKho = {mh.SoLuongTonKho}, MaNCC = '{mh.MaNcc}', HangSx = N'{mh.HangSx}', XuatXu = N'{mh.XuatXu}', MoTa = N'{mh.MoTa}' Where MaMh = '{mh.MaMh}';";
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
                            if (obj is not MatHang mh)
                                continue;
                            // Trường hợp chưa thêm mới nên chưa có mã mh
                            if (string.IsNullOrEmpty(mh.MaMh))
                                continue;
                            // Xóa hàng
                            else
                                cmd.CommandText += $"Update MatHang Set DaXoa = 1 Where MaMh = '{mh.MaMh}';\n";
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
        {
            RefreshDataGrid();
        }

        private void UiPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                bool isQuanLy = string.Equals(PageChinh.getChucVu, "Quản Lý", StringComparison.OrdinalIgnoreCase);

                grdMoto.IsReadOnly = !isQuanLy;

                if (sender is Button button)
                    button.Visibility = isQuanLy ? Visibility.Visible : Visibility.Collapsed;

                RefreshDataGrid();
            }
        }

        private void AddRow(object sender, RoutedEventArgs e)
            => TableData.Add(new());

        private void grdMoto_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
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
    }
}