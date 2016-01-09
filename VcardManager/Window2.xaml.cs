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
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        VcUtil util = new VcUtil();
        VcUtil.Vcard card;
        SQLiteConnection db;
        SQLiteCommand command;
        string query;

        public Window2(SQLiteCommand command, SQLiteConnection db, object card, string name)
        {
            InitializeComponent();

            this.command = command;
            this.db = db;
            this.card = (VcUtil.Vcard) card;

            for (int i = 0; i < this.card.getNprops(); i++)
            {
                if (this.card.getProp(i).getName() != VcUtil.VcPname.VCP_N)
                {
                    listBox.Items.Add("" + util.propertyName(this.card.getProp(i).getName()) + ":  " + this.card.getProp(i).getValue());
                }

                if (this.card.getProp(i).getName() == VcUtil.VcPname.VCP_FN)
                {
                    Title = "Possible Duplicate Card " + this.card.getProp(i).getValue() + " found.";
                }
                            
            }

            query = @"SELECT name_id FROM NAME WHERE ([name] = @name);";
            command = new SQLiteCommand(query, db);
            command.Parameters.AddWithValue("@name", name);
            SQLiteDataReader reader = command.ExecuteReader();

            query = @"SELECT * FROM PROPERTY WHERE ([name_id] = @nameID);";
            command = new SQLiteCommand(query, db);
            command.Parameters.AddWithValue("@nameID", reader["name_id"]);
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                listBox1.Items.Add("" + reader["pname"] + ":  " + reader["value"]);
            }
        }
    }
}
