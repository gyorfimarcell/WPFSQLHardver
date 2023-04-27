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
            string SQLGyartok = "SELECT DISTINCT Gyártó FROM termékek ORDER BY Gyártó";
            MySqlCommand parancs = new MySqlCommand(SQLGyartok, SQLKapcsolat);
            MySqlDataReader olvaso = parancs.ExecuteReader();

            cbGyarto.Items.Add(" - Nincs kiválasztva - ");

            while (olvaso.Read())
            {
                cbGyarto.Items.Add(olvaso.GetString("Gyártó"));
            }
            olvaso.Close();
            cbGyarto.SelectedIndex = 0;
        }

        private void KategoriakBetoltese()
        {
            string SQLKategoriak = "SELECT DISTINCT Kategória FROM termékek ORDER BY Kategória";
            MySqlCommand parancs = new MySqlCommand(SQLKategoriak, SQLKapcsolat);
            MySqlDataReader olvaso = parancs.ExecuteReader();

            cbKategoria.Items.Add(" - Nincs kiválasztva - ");

            while (olvaso.Read())
            {
                cbKategoria.Items.Add(olvaso.GetString("Kategória"));
            }
            olvaso.Close();
            cbKategoria.SelectedIndex = 0;
        }

        private void TermekekBetoltese()
        {
            termekek.Clear();

            string kategoria = cbKategoria.SelectedIndex == 0 ? "%" : cbKategoria.SelectedItem.ToString();
            string gyarto = cbGyarto.SelectedIndex == 0 ? "%" : cbGyarto.SelectedItem.ToString();

            string SQLTermekek = "SELECT * FROM termékek WHERE Kategória LIKE '{0}' AND Gyártó LIKE '{1}' AND Név LIKE '%{2}%'";
            string SQLSzurt = string.Format(SQLTermekek, kategoria, gyarto, txtTermek.Text);
            MySqlCommand parancs = new MySqlCommand(SQLSzurt, SQLKapcsolat);
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
            string sablon = @"
                <!DOCTYPE HTML>
                <html lang='hu'>
                    <head>
                       <meta charset='utf-8'>
                       <title>Táblázat</title>
                    </head>
                    <body>
                        <table>
                            <thead>
                                <tr>
                                    <th>Kategória</th>
                                    <th>Gyártó</th>
                                    <th>Név</th>
                                    <th>Ár</th>
                                    <th>Garancia</th>
                                </tr>
                            </thead>
                            <tbody>
                                {0}
                            </tbody>
                        </table>
                    </body>
                </html>
            ";

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "HTML fájl|*.html";
            if (sfd.ShowDialog() == true)
            {
                string fajlnev = sfd.FileName;

                StreamWriter sw = new StreamWriter(fajlnev);

                string tablazatSorok = "";
                foreach (Termek termek in termekek)
                {
                    tablazatSorok += $"<tr>\n{termek.ToHTMLString()}</tr>\n";
                }

                sw.Write(String.Format(sablon, tablazatSorok));

                sw.Close();
                MessageBox.Show("Mentés sikeres!");
            }
        }

        private void btnSzukit_Click(object sender, RoutedEventArgs e)
        {
            TermekekBetoltese();
        }

        private void btnMentes_Click(object sender, RoutedEventArgs e)
        {
            CSVMentes();
        }

        private void btnMentesHTML_Click(object sender, RoutedEventArgs e)
        {
            HTMLMentes();
        }
    }
}
