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
        string cardName;
        int nameID = 0;

        public Window2(SQLiteCommand command, SQLiteConnection db, object card, string name)
        {
            InitializeComponent();

            this.command = command;
            this.db = db;
            this.card = (VcUtil.Vcard) card;
            cardName = name;

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

            nameID = Convert.ToInt32(reader["name_id"]);

            while (reader.Read())
            {
                listBox1.Items.Add("" + reader["pname"] + ":  " + reader["value"]);
            }
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            int value = 0;
            string name = null;

            query = @"DELETE FROM PROPERTY WHERE ([name_id] = @nameID);";
            command = new SQLiteCommand(query, db);
            command.Parameters.AddWithValue("@nameID", nameID);
            command.ExecuteNonQuery();

            for (int i = 0; i < card.getNprops(); i++)
            {
                if (card.getProp(i).getName() != VcUtil.VcPname.VCP_N)
                {
                    value = numPinst(nameID, card.getProp(i).getName());
                    query = "INSERT INTO PROPERTY (name_id, pname, pinst, partype, parval, value) VALUES (@ID, @pname, @pinst, @partype, @parval, @value)";
                    command = new SQLiteCommand(query, db);
                    command.Parameters.AddWithValue("@ID", nameID);
                    command.Parameters.AddWithValue("@pname", util.propertyName(card.getProp(i).getName()));
                    command.Parameters.AddWithValue("@pinst", value);
                    command.Parameters.AddWithValue("@partype", card.getProp(i).getPartype());
                    command.Parameters.AddWithValue("@parval", card.getProp(i).getParVal());
                    command.Parameters.AddWithValue("@value", card.getProp(i).getValue());
                    command.ExecuteNonQuery();

                    if (card.getProp(i).getName() == VcUtil.VcPname.VCP_FN)
                    {
                        name = card.getProp(i).getValue();
                    }
                }
            }

            MessageBox.Show(name + " Vcard has been replaced", name, MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void Merge_Click(object sender, RoutedEventArgs e)
        {
            string name = null;
            bool match = false;

            query = @"SELECT name_id FROM NAME WHERE ([name] = @name);";
            command = new SQLiteCommand(query, db);
            command.Parameters.AddWithValue("@name", cardName);
            SQLiteDataReader reader = command.ExecuteReader();

            for (int i = 0; i < card.getNprops(); i++)
            {
                query = @"SELECT value FROM PROPERTY WHERE ([name_id] = @nameID) AND ([pname] = @pname);";
                command = new SQLiteCommand(query, db);
                command.Parameters.AddWithValue("@nameID", reader["name_id"]);
                command.Parameters.AddWithValue("@pname", util.propertyName(card.getProp(i).getName()));
                SQLiteDataReader reader2 = command.ExecuteReader();

                if (card.getProp(i).getName() == VcUtil.VcPname.VCP_FN)
                {
                    name = card.getProp(i).getValue();
                }

                while (reader2.Read())
                {
                    if (string.Compare(card.getProp(i).getValue(), reader2["value"].ToString()) == 0)
                    {
                        match = true;
                    }
                }

                if (!match)
                {
                    query = "INSERT INTO PROPERTY (name_id, pname, pinst, partype, parval, value) VALUES (@ID, @pname, @pinst, @partype, @parval, @value)";
                    command = new SQLiteCommand(query, db);
                    command.Parameters.AddWithValue("@ID", reader["name_id"]);
                    command.Parameters.AddWithValue("@pname", util.propertyName(card.getProp(i).getName()));
                    command.Parameters.AddWithValue("@pinst", numPinst(nameID, card.getProp(i).getName()));
                    command.Parameters.AddWithValue("@partype", card.getProp(i).getPartype());
                    command.Parameters.AddWithValue("@parval", card.getProp(i).getParVal());
                    command.Parameters.AddWithValue("@value", card.getProp(i).getValue());
                    command.ExecuteNonQuery();
                }
            }

            if (!match)
            {
                MessageBox.Show(name + " Vcard has been merged", "", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                MessageBox.Show("No new property was added to " + name + " Vcard.\nThe Vcard in the database already has all the properties your trying to merge",
                                name, MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
        }

        private int numPinst(int nameID, VcUtil.VcPname name)
        {
            string query = "SELECT COUNT(*) FROM PROPERTY WHERE ([name_id] = @nameID) AND ([pname] = @pname);";
            command = new SQLiteCommand(query, db);
            command.Parameters.AddWithValue("@nameID", nameID);
            //command.Parameters.AddWithValue("@pname", pNameConvertor(name));
            command.Parameters.AddWithValue("@pname", util.propertyName(name));
            int count = Convert.ToInt32(command.ExecuteScalar());

            //LogBox.Text += "" + pNameConvertor(name) + " what count equals = " + count + "\n";

            return (count + 1);
        }


    }
}
