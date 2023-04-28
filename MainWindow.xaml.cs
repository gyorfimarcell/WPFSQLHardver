using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace WPFSQLHardver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string kapcsolatLeiro = "datasource=127.0.0.1;port=3306;username=root;password=;database=hardver;charset=utf8;";
        MySqlConnection SQLKapcsolat;

        List<Termek> termekek = new List<Termek>();

        public MainWindow()
        {
            InitializeComponent();
            AdatbazisMegnyitas();

            GyartokBetoltese();
            KategoriakBetoltese();

            cbGyarto.SelectedIndex = 0;
            cbKategoria.SelectedIndex = 0;

            TermekekBetoltese();

            this.Closed += new EventHandler(AdatbazisLezaras);
        }

        private void AdatbazisMegnyitas()
        {
            try
            {
                SQLKapcsolat = new MySqlConnection(kapcsolatLeiro);
                SQLKapcsolat.Open();
            }
            catch (Exception)
            {
                MessageBox.Show("Nem lehet kapcsolódni az adatbázishoz!");
                this.Close();
            }
        }

        private void AdatbazisLezaras(object sender, EventArgs e)
        {
            SQLKapcsolat.Close();
            SQLKapcsolat.Dispose();
        }

        private void GyartokBetoltese()
        {
            string kategoria = cbKategoria.SelectedIndex <= 0 ? "%" : cbKategoria.SelectedItem.ToString();
            string SQLGyartok = $"SELECT DISTINCT Gyártó FROM termékek WHERE Kategória LIKE '{kategoria}' ORDER BY Gyártó";
            ComboboxFrissit(cbGyarto, SQLGyartok);
        }

        private void KategoriakBetoltese()
        {
            string gyarto = cbGyarto.SelectedIndex <= 0 ? "%" : cbGyarto.SelectedItem.ToString();
            string SQLKategoriak = $"SELECT DISTINCT Kategória FROM termékek WHERE Gyártó LIKE '{gyarto}' ORDER BY Kategória";
            ComboboxFrissit(cbKategoria, SQLKategoriak);
        }

        private void ComboboxFrissit(ComboBox cb, string sql)
        {
            string elozoItem = "";
            if (cb.SelectedIndex != -1)
            {
                elozoItem = cb.SelectedItem.ToString();
            }

            cb.Items.Clear();

            cb.Items.Add(" - Nincs kiválasztva - ");

            MySqlCommand parancs = new MySqlCommand(sql, SQLKapcsolat);
            MySqlDataReader olvaso = parancs.ExecuteReader();

            while (olvaso.Read())
            {
                cb.Items.Add(olvaso.GetString(0));
            }
            olvaso.Close();

            if (elozoItem != "")
            {
                cb.SelectedItem = elozoItem;
            }
        }

        private void TermekekBetoltese()
        {
            termekek.Clear();

            string kategoria = cbKategoria.SelectedIndex <= 0 ? "%" : cbKategoria.SelectedItem.ToString();
            string gyarto = cbGyarto.SelectedIndex <= 0 ? "%" : cbGyarto.SelectedItem.ToString();

            string SQLTermekek = $"SELECT * FROM termékek WHERE Kategória LIKE '{kategoria}' AND Gyártó LIKE '{gyarto}' AND Név LIKE '%{txtTermek.Text}%'";
            MySqlCommand parancs = new MySqlCommand(SQLTermekek, SQLKapcsolat);
            MySqlDataReader olvaso = parancs.ExecuteReader();

            while (olvaso.Read())
            {
                Termek uj = new Termek(
                    olvaso.GetString("Kategória"),
                    olvaso.GetString("Gyártó"),
                    olvaso.GetString("Név"),
                    olvaso.GetInt32("Ár"),
                    olvaso.GetInt32("Garidő")
                );

                termekek.Add(uj);
            }
            olvaso.Close();

            dgTermekek.ItemsSource = termekek;
            dgTermekek.Items.Refresh();
        }

        private void CSVMentes() {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV fájl|*.csv";
            if (sfd.ShowDialog() == true)
            {
                string fajlnev = sfd.FileName;

                StreamWriter sw = new StreamWriter(fajlnev);
                foreach (Termek termek in termekek)
                {
                    sw.WriteLine(termek.ToCSVString());
                }
                sw.Close();
                MessageBox.Show("Mentés sikeres!");
            }
        }

        private void HTMLMentes()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "HTML fájl|*.html";
            if (sfd.ShowDialog() == true)
            {
                string fajlnev = sfd.FileName;

                StreamWriter sw = new StreamWriter(fajlnev, false, Encoding.UTF8);

                string tablazatSorok = "";
                foreach (Termek termek in termekek)
                {
                    tablazatSorok += $"<tr>\n{termek.ToHTMLString()}</tr>\n";
                }

                string sablon = File.ReadAllText("sablon.html");

                sw.Write(String.Format(sablon, cbKategoria.SelectedItem, cbGyarto.SelectedItem, txtTermek.Text, tablazatSorok));

                sw.Close();
                MessageBox.Show("Mentés sikeres!");
            }
        }


        private void btnMentes_Click(object sender, RoutedEventArgs e)
        {
            CSVMentes();
        }

        private void btnMentesHTML_Click(object sender, RoutedEventArgs e)
        {
            HTMLMentes();
        }

        private void cbKategoria_DropDownClosed(object sender, EventArgs e)
        {
            GyartokBetoltese();
            TermekekBetoltese();
        }

        private void cbGyarto_DropDownClosed(object sender, EventArgs e)
        {
            KategoriakBetoltese();
            TermekekBetoltese();
        }

        private void txtTermek_TextChanged(object sender, TextChangedEventArgs e)
        {
            TermekekBetoltese();
        }
    }
}
