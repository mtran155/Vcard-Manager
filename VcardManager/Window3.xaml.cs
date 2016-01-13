using System;
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
using System.Windows.Shapes;

using System.Data.SQLite;

namespace VcardManager
{
    /// <summary>
    /// Interaction logic for Window3.xaml
    /// </summary>
    public partial class Window3 : Window
    {
        SQLiteConnection db;
        SQLiteCommand command;
        string query;

        public Window3(SQLiteCommand command, SQLiteConnection db)
        {
            InitializeComponent();

            this.command = command;
            this.db = db;

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            string value = textBox.Text;
            SQLiteDataReader reader;
            SQLiteDataReader reader2;
            FoundCard card;
            List<FoundCard> results = new List<FoundCard>();           

            query = @"SELECT COUNT (*) FROM PROPERTY WHERE VALUE LIKE @value;";
            command = new SQLiteCommand(query, db);
            command.Parameters.AddWithValue("@value", "%" + value + "%");
            int count = Convert.ToInt32(command.ExecuteScalar());

            if (count > 0)
            {
                query = @"SELECT * FROM PROPERTY WHERE VALUE LIKE @value;";
                command = new SQLiteCommand(query, db);
                command.Parameters.AddWithValue("@value", "%" + value + "%");
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    card = new FoundCard();
                    card.Property = reader["pname"].ToString();
                    card.Value = reader["value"].ToString();

                    query = @"SELECT * FROM PROPERTY WHERE (PNAME = 'FN') AND (name_id = @nameID);";
                    command = new SQLiteCommand(query, db);
                    command.Parameters.AddWithValue("@nameID", reader["name_id"]);
                    reader2 = command.ExecuteReader();

                    while (reader2.Read())
                    {
                        card.Name = reader2["value"].ToString();
                    }

                    results.Add(card);

                }

                resultsTable.ItemsSource = results;
            }
            else
            {
                MessageBox.Show("No results were found", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public class FoundCard
        {
            public string Name { get; set; }

            public string Property { get; set; }

            public string Value { get; set; }

        }
    }
}
