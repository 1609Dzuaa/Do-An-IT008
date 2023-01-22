﻿using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using MotoStore.Database;
using MotoStore.Views.Pages.LoginPages;
/*Trong Quá Trình Chạy Login Form, Nếu Gặp Lỗi 
 Unable to copy file "obj\Debug\Demo.exe" to "bin\Debug\Demo.exe".
 The process cannot access the file 'bin\Debug\Demo.exe' because it is being used by another process.Demo:
1.Tắt VSCode
2.Bật TaskManager
3.Tìm Demo.exe và EndTask nó
4.Bật VSCode và chạy lại
 */

namespace MotoStore.Views.Windows
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
            
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
           /* string maNV = PageChinh.getMa;
            MainDatabase mdb = new();
            SqlConnection con = new(System.Configuration.ConfigurationManager.ConnectionStrings["Data"].ConnectionString);
            SqlCommand cmd;
            con.Open();
            DateTime DT = DateTime.Now;
            cmd = new("Set Dateformat dmy\nInsert into LichSuHoatDong values(newid(), '" + PageChinh.getMa+"', '" + DT.ToString("dd-MM-yyyy HH:mm:ss")+"', N'đăng xuất')", con);
            cmd.ExecuteNonQuery();
            con.Close();
            Thread.Sleep(2000); */
            Window.GetWindow(this).Close();
            Application.Current.Shutdown();
        }
    }
}
