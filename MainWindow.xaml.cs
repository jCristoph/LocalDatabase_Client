﻿using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Data.SqlClient;
using System.Data;


namespace PZ_Panel_Logowania
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        { 
            InitializeComponent();
            MainContainer.Children.Add(new Logowanie());
        }



        /*private void Des_Click(object sender, RoutedEventArgs e)
        {
            MainContainer.Children.Clear();
            MainContainer.Children.Add(new Rejestracja());

        }
        */
        int i = 0;

        private void log(object sender, RoutedEventArgs e)
        {
            MainContainer.Children.Clear();
            MainContainer.Children.Add(new Logowanie());
        }

        private void rej(object sender, RoutedEventArgs e)
        {
            MainContainer.Children.Clear();
            MainContainer.Children.Add(new Rejestracja());
        }


        //
        /*        private void Txt_nazwa_TextChanged(object sender, EventArgs e)
                {

                }

        private void btn_zaloguj_Click(object sender, RoutedEventArgs e)
                {
                    SqlConnection polaczenie = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\krzem\source\repos\PZ_Panel_Logowania\PZ_Panel_Logowania\Baza_Danych\PZ_BD.mdf;Integrated Security=True;Connect Timeout=30");
                    SqlCommand zapytanie = new SqlCommand();
                    zapytanie.Connection = polaczenie;
                    zapytanie.CommandText = "SELECT * FROM [User] WHERE Login = '" + Txt_nazwa.Text.Trim()+ "' and Password = '" + Txt_haslo.Password.Trim()+"'";
                    SqlDataAdapter adapter = new SqlDataAdapter(zapytanie);
                    DataTable tabela = new DataTable();

                    adapter.Fill(tabela);
                    if (tabela.Rows.Count == 1)
                    {
                        MessageBox.Show("zalogowano");
                    }
                    else
                    {
                        MessageBox.Show("błędny login lub hasło");
                    }
                }

                private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
                {
                }

                private void PasswordBox_TextChanged(object sender, TextChangedEventArgs e)
                {

                }*/
    }
}
