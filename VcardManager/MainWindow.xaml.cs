using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using VcardManager.Model;

using System.Data.SQLite;



namespace VcardManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        VcUtil util = new VcUtil();
        VcUtil.VcFile filep;
        List<CardDetails> cardList = new List<CardDetails>();
        SQLiteConnection db = new SQLiteConnection("Data Source=../../Database/VcardDatabase.db;Version=3;");
        SQLiteCommand command;
        bool databaseOpen = false;

        public MainWindow()
        {
            InitializeComponent();
            if (!File.Exists("../../Database/VcardDatabase.db")) { SQLiteConnection.CreateFile("../../Database/VcardDatabase.db"); }            
            db.Open();

            string sql = @"CREATE TABLE IF NOT EXISTS NAME (name_id INTEGER PRIMARY KEY, 
                                                            name TEXT NOT NULL);";
            command = new SQLiteCommand(sql, db);
            command.ExecuteNonQuery();

            sql = @"CREATE TABLE IF NOT EXISTS PROPERTY (name_id INTEGER NOT NULL, 
                                                         pname TEXT NOT NULL, 
                                                         pinst INTEGER NOT NULL, 
                                                         partype TEXT, 
                                                         parval TEXT, 
                                                         value TEXT,
                                                         PRIMARY KEY (name_id, pname, pinst),
                                                         FOREIGN KEY (name_id) REFERENCES NAME (name_id) ON DELETE CASCADE);";

            command = new SQLiteCommand(sql, db);
            command.ExecuteNonQuery();
        }

        /*File Menu Buttons*/

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open VCF File";
            openFileDialog.Filter = "VCF files (*.vcf)|*.vcf|All files (*.*)|*.*";


            if (openFileDialog.ShowDialog() == true)
            {
                VcUtil.VcStatus status = new VcUtil.VcStatus();
                string fileName = openFileDialog.FileName;
                StreamReader sr = new StreamReader(fileName);
                filep = new VcUtil.VcFile();
                status = util.readVcFile(sr, filep);

                if (status.getCode() == VcUtil.VcError.OK)
                {
                    databaseOpen = false;
                    RefreshCardDisplay();
                    //LogBox.Text = filep.ToString();
                }
                else
                {
                    MessageBox.Show("Could not open file. Error code: " + status.getCode().ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //LogBox.Text = status.getCode().ToString();
                }
                //LogBox.Text = filep.ToString();

            }
        }

        private void AppendFile_Click(object sender, RoutedEventArgs e)
        {
            if (cardList.Count > 0)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Open VCF File";
                openFileDialog.Filter = "VCF files (*.vcf)|*.vcf|All files (*.*)|*.*";


                if (openFileDialog.ShowDialog() == true)
                {
                    VcUtil.VcStatus status = new VcUtil.VcStatus();
                    string fileName = openFileDialog.FileName;
                    StreamReader sr = new StreamReader(fileName);
                    status = util.readVcFile(sr, filep);

                    if (status.getCode() == VcUtil.VcError.OK)
                    {
                        databaseOpen = false;
                        RefreshCardDisplay();
                        //LogBox.Text = filep.ToString();
                    }
                    else
                    {
                        LogBox.Text = status.getCode().ToString();
                    }
                    //LogBox.Text = filep.ToString();

                }
            }
            else
            {
                MessageBox.Show("No cards have been loaded.\nCannot append from file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }            
        }

        private void CloseFile_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to close this applicaiton?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                db.Close();
                Application.Current.Shutdown();
            }
        }


        /*Database Menu Buttons*/

        private void OpenDatabase_Click(object sender, RoutedEventArgs e)
        {
            VcUtil.Vcard card;
            VcUtil.VcProp prop;
            string query;
            SQLiteDataReader reader1;
            SQLiteDataReader reader2;

            query = @"SELECT * FROM NAME";
            command = new SQLiteCommand(query, db);
            reader1 = command.ExecuteReader();
            int count = reader1["name"].ToString().Length;

            if (count == 0)
            {
                MessageBox.Show("No cards have been stored in database.\nCannot load from database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                filep = new VcUtil.VcFile();

                while (reader1.Read())
                {
                    //LogBox.Text += "Name: " + reader["name"].ToString() + "\n"; 

                    card = new VcUtil.Vcard();
                    prop = new VcUtil.VcProp();

                    prop.setName(VcUtil.VcPname.VCP_N);
                    prop.setPartype("");
                    prop.setParval("");
                    prop.setValue(reader1["name"].ToString());

                    card.setProplist(prop);
                    card.setNprops(1);

                    query = @"SELECT * FROM PROPERTY WHERE ([name_id] = @nameID);";
                    command = new SQLiteCommand(query, db);
                    command.Parameters.AddWithValue("@nameID", reader1["name_id"]);
                    reader2 = command.ExecuteReader();

                    while (reader2.Read())
                    {
                        prop = new VcUtil.VcProp();
                        //LogBox.Text += "PNAME: " + reader2["pname"].ToString() + " PARTYPE: " + reader2["partype"].ToString() + " PARVAL: " + reader2["parval"].ToString() + "  VALUE: " + reader2["value"].ToString() + "\n";
                        prop.setName(util.getVcPname(reader2["pname"].ToString()));
                        prop.setPartype(reader2["partype"].ToString());
                        prop.setParval(reader2["parval"].ToString());
                        prop.setValue(reader2["value"].ToString());

                        card.setProplist(prop);
                        card.setNprops(1);
                    }

                    filep.setCardp(card);
                    filep.setNcards(1);
                }

                databaseOpen = true;
                RefreshCardDisplay();
            }
        }

        private void AppendDatabase_Click(object sender, RoutedEventArgs e)
        {
            VcUtil.Vcard card;
            VcUtil.VcProp prop;
            string query;
            SQLiteDataReader reader1;
            SQLiteDataReader reader2;

            if (cardList.Count != 0)
            {
                query = @"SELECT * FROM NAME";
                command = new SQLiteCommand(query, db);
                reader1 = command.ExecuteReader();
                int count = reader1["name"].ToString().Length;

                if (count == 0)
                {
                    MessageBox.Show("No cards have been stored in database.\nCannot load from database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    while (reader1.Read())
                    {
                        //LogBox.Text += "Name: " + reader["name"].ToString() + "\n"; 

                        card = new VcUtil.Vcard();
                        prop = new VcUtil.VcProp();

                        prop.setName(VcUtil.VcPname.VCP_N);
                        prop.setPartype("");
                        prop.setParval("");
                        prop.setValue(reader1["name"].ToString());

                        card.setProplist(prop);
                        card.setNprops(1);

                        query = @"SELECT * FROM PROPERTY WHERE ([name_id] = @nameID);";
                        command = new SQLiteCommand(query, db);
                        command.Parameters.AddWithValue("@nameID", reader1["name_id"]);
                        reader2 = command.ExecuteReader();

                        while (reader2.Read())
                        {
                            prop = new VcUtil.VcProp();
                            //LogBox.Text += "PNAME: " + reader2["pname"].ToString() + " PARTYPE: " + reader2["partype"].ToString() + " PARVAL: " + reader2["parval"].ToString() + "  VALUE: " + reader2["value"].ToString() + "\n";
                            prop.setName(util.getVcPname(reader2["pname"].ToString()));
                            prop.setPartype(reader2["partype"].ToString());
                            prop.setParval(reader2["parval"].ToString());
                            prop.setValue(reader2["value"].ToString());

                            card.setProplist(prop);
                            card.setNprops(1);
                        }

                        filep.setCardp(card);
                        filep.setNcards(1);
                    }
                    //LogBox.Text = cardList.Count.ToString();
                    //cardList.Clear();
                    //LogBox.Text += " after: " + cardList.Count.ToString();
                    databaseOpen = true;
                    RefreshCardDisplay();
                }
            }
            else
            {
                MessageBox.Show("No cards have been loaded.\nCannot append from database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StoreAll_Click(object sender, RoutedEventArgs e)
        {
            string query = null;
            int value = 0;
            int nameID = 0;
            bool duplicate = false;
            StringBuilder nameList = new StringBuilder();
            int counter = 0;

            if (cardList.Count != 0)
            {
                for (int i = 0; i < filep.getNcards(); i++)
                {
                    for (int j = 0; j < filep.getCardp(i).getNprops(); j++)
                    {
                        if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_N)
                        {
                            duplicate = checkExistingCard(filep.getCardp(i).getProp(j).getValue());

                            if (!duplicate)
                            {
                                query = "INSERT INTO NAME VALUES (NULL, @name);";
                                command = new SQLiteCommand(query, db);
                                command.Parameters.AddWithValue("@name", filep.getCardp(i).getProp(j).getValue());
                                command.ExecuteNonQuery();

                                query = "SELECT last_insert_rowid();";
                                command = new SQLiteCommand(query, db);
                                nameID = Convert.ToInt32(command.ExecuteScalar());
                            }
                            else
                            {
                                Window2 window = new Window2(command, db, filep.getCardp(i), filep.getCardp(i).getProp(j).getValue());
                                window.ShowDialog();
                                //LogBox.Text += "checking dups " + i + "\n";
                                break;
                            }

                        }
                        else
                        {
                            value = numPinst(nameID, filep.getCardp(i).getProp(j).getName());
                            query = "INSERT INTO PROPERTY (name_id, pname, pinst, partype, parval, value) VALUES (@ID, @pname, @pinst, @partype, @parval, @value)";
                            command = new SQLiteCommand(query, db);
                            command.Parameters.AddWithValue("@ID", nameID);
                            command.Parameters.AddWithValue("@pname", util.propertyName(filep.getCardp(i).getProp(j).getName()));
                            command.Parameters.AddWithValue("@pinst", value);
                            command.Parameters.AddWithValue("@partype", filep.getCardp(i).getProp(j).getPartype());
                            command.Parameters.AddWithValue("@parval", filep.getCardp(i).getProp(j).getParVal());
                            command.Parameters.AddWithValue("@value", filep.getCardp(i).getProp(j).getValue());
                            command.ExecuteNonQuery();
                            counter++;

                            if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_FN)
                            {
                                nameList.Append(filep.getCardp(i).getProp(j).getValue()).Append(",\n");
                            }
                        }
                    }
                }

                if (counter > 0)
                {
                    MessageBox.Show(nameList.ToString() + "has been successfully added to the database", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            else
            {
                MessageBox.Show("No cards are loaded to store", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StoreSelected_Click(object sender, RoutedEventArgs e)
        {
            string query = null;
            int value = 0;
            int nameID = 0;
            CardDetails card;
            //StringBuilder name = new StringBuilder();
            StringBuilder nameList = new StringBuilder();
            bool duplicate = false;
            int index = 0;

            //LogBox.Text = dgUsers.SelectedItems.Count.ToString();

            if (cardList.Count != 0)
            {
                if (dgUsers.SelectedItem != null)
                {
                    for (int i = 0; i < dgUsers.SelectedItems.Count; i++)
                    {
                        card = new CardDetails();
                        card = (CardDetails)dgUsers.SelectedItems[i];
                        index = card.cardNumber - 1;

                        for (int j = 0; j < filep.getCardp(index).getNprops(); j++)
                        {
                            if (filep.getCardp(index).getProp(j).getName() == VcUtil.VcPname.VCP_N)
                            {
                                duplicate = checkExistingCard(filep.getCardp(index).getProp(j).getValue());

                                if (!duplicate)
                                {
                                    query = "INSERT INTO NAME VALUES (NULL, @name);";
                                    command = new SQLiteCommand(query, db);
                                    command.Parameters.AddWithValue("@name", filep.getCardp(index).getProp(j).getValue());
                                    command.ExecuteNonQuery();

                                    query = "SELECT last_insert_rowid();";
                                    command = new SQLiteCommand(query, db);
                                    nameID = Convert.ToInt32(command.ExecuteScalar());
                                }
                                else
                                {
                                    Window2 window = new Window2(command, db, filep.getCardp(index), filep.getCardp(index).getProp(j).getValue());
                                    window.ShowDialog();
                                    //LogBox.Text += "checking dups " + i + "\n";
                                    break;
                                }

                            }
                            else
                            {
                                value = numPinst(nameID, filep.getCardp(index).getProp(j).getName());
                                query = "INSERT INTO PROPERTY (name_id, pname, pinst, partype, parval, value) VALUES (@ID, @pname, @pinst, @partype, @parval, @value)";
                                command = new SQLiteCommand(query, db);
                                command.Parameters.AddWithValue("@ID", nameID);
                                command.Parameters.AddWithValue("@pname", util.propertyName(filep.getCardp(index).getProp(j).getName()));
                                command.Parameters.AddWithValue("@pinst", value);
                                command.Parameters.AddWithValue("@partype", filep.getCardp(index).getProp(j).getPartype());
                                command.Parameters.AddWithValue("@parval", filep.getCardp(index).getProp(j).getParVal());
                                command.Parameters.AddWithValue("@value", filep.getCardp(index).getProp(j).getValue());
                                command.ExecuteNonQuery();

                                if (filep.getCardp(index).getProp(j).getName() == VcUtil.VcPname.VCP_FN)
                                {
                                    nameList.Append(filep.getCardp(index).getProp(j).getValue()).Append(",\n");
                                }

                            }
                        }
                    }

                    if (!duplicate)
                    {
                        MessageBox.Show(nameList.ToString() + "has been successfully added to the database", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please select card to store to database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("No cards are loaded to store", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "VCF files (*.vcf)|*.vcf|All files (*.*)|*.*";

            if (cardList.Count == 0)
            {
                MessageBox.Show("Database is not open.\nPlease open database before exporting.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (saveFileDialog.ShowDialog() == true)
                {
                    string fileName = saveFileDialog.FileName;
                    util.writeVcFile(filep, fileName);
                }
            }
        }

        private void ExportSelected_Click(object sender, RoutedEventArgs e)
        {
            CardDetails card;
            VcUtil.VcFile list = new VcUtil.VcFile();

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "VCF files (*.vcf)|*.vcf|All files (*.*)|*.*";

            if (cardList.Count == 0)
            {
                MessageBox.Show("Database is not open.\nPlease open database before exporting.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (dgUsers.SelectedItem != null)
                {
                    for (int i = 0; i < dgUsers.SelectedItems.Count; i++)
                    {
                        card = new CardDetails();
                        card = (CardDetails)dgUsers.SelectedItems[i];

                        list.setCardp(filep.getCardp(card.cardNumber - 1));
                        list.setNcards(1);

                        //LogBox.Text += filep.getCardp(card.cardNumber - 1).getProp(1).getParVal().Length;
                    }

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string fileName = saveFileDialog.FileName;
                        util.writeVcFile(list, fileName);
                    }
                }
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            // Select * FROM Property where value like  '%woo%'
            Window3 searchWindow = new Window3(command, db);
            searchWindow.Show();
        }


        /*Database Helper Functions*/
        private bool checkExistingCard(string name)
        {
            string query = "SELECT COUNT(*) FROM NAME WHERE ([name] = @name);";
            command = new SQLiteCommand(query, db);
            command.Parameters.AddWithValue("@name", name);
            int UserExist = Convert.ToInt32(command.ExecuteScalar());

            if (UserExist > 0)
            {
                return true;
            }
            else
                return false;
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


        /*Help Menu Button*/

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is a Vcard Manager\n\n" +
                            "Created By: Michael Tran\n\n" +
                            "This application is only compatible with Vcard Version 3.0", "About Vcard Manager", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        /*Display Card Functions*/

        private void viewCard_Click(object sender, RoutedEventArgs e)
        {
            Window1 secondWindow = new Window1();
            CardDetails card = (CardDetails)dgUsers.SelectedItem;
            secondWindow.Title = card.Name;
            fillViewCardPanel(secondWindow, card);
            secondWindow.Show();                      
        }

        private void fillViewCardPanel(Window1 window, CardDetails card)
        {
            string[] tokens = null;

            if (card.Image != null)
            {
                var image = new BitmapImage();
                int BytesToRead = 100;

                WebRequest request = WebRequest.Create(new Uri(card.Image, UriKind.Absolute));
                request.Timeout = -1;

                try
                {
                    WebResponse response = request.GetResponse();
                    Stream responseStream = response.GetResponseStream();
                    BinaryReader reader = new BinaryReader(responseStream);
                    MemoryStream memoryStream = new MemoryStream();

                    byte[] bytebuffer = new byte[BytesToRead];
                    int bytesRead = reader.Read(bytebuffer, 0, BytesToRead);

                    while (bytesRead > 0)
                    {
                        memoryStream.Write(bytebuffer, 0, bytesRead);
                        bytesRead = reader.Read(bytebuffer, 0, BytesToRead);
                    }

                    image.BeginInit();
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    image.StreamSource = memoryStream;
                    image.EndInit();

                    window.ImageProfile.Source = image;
                }
                catch (Exception)
                {
                    Uri uri = new Uri("/Images/BlankProfile.png", UriKind.Relative);
                    ImageSource imgSource = new BitmapImage(uri);
                    window.ImageProfile.Source = imgSource;
                }
            }
            else
            {
                Uri uri = new Uri("/Images/BlankProfile.png", UriKind.Relative);
                ImageSource imgSource = new BitmapImage(uri);
                window.ImageProfile.Source = imgSource;
            }

            /*Summary Tab*/
            window.TextName.Text = card.Name;
            window.TextEmail.Text = card.mainEmail;

            if (card.Company != null)
            {
                window.TextCompany.Text = card.Company[0];
            }
            
            window.TextJobTitle.Text = card.Title;
            window.TextWorkPhone.Text = card.workPhone;
            window.TextWorkWebsite.Text = card.workWebsite;
            window.TextHomePhone.Text = card.homePhone;
            window.TextCellPhone.Text = card.cellPhone;
            window.TextPersonalWebsite.Text = card.Website;
            /*************************************/

            /*Name and Email Tab*/
            string[] token = card.fullName.Split(';');


            for (int i = 0; i < token.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        window.LastName.Text = token[i];
                        break;
                    case 1:
                        window.FirstName.Text = token[i];
                        break;
                    case 2:
                        window.MiddleName.Text = token[i];
                        break;
                    default:
                        break;
                }
            }
            tokens = card.Email.Split('\n');
            window.EmailList.Items.Add(card.mainEmail + "  (Preferred e-mail)");
            for (int i = 0; i < tokens.Length; i++)
            {
                if (!tokens[i].Contains(card.mainEmail))
                {
                    window.EmailList.Items.Add(tokens[i]);
                }
            }
            /*************************************/

            /*Home Tab*/
            if (card.homeAddress != null)
            {
                for (int i = 0; i < card.homeAddress.Length; i++)
                {
                    switch (i)
                    {
                        case 2:
                            window.HomeStreet.Text = card.homeAddress[i];
                            break;
                        case 3:
                            window.HomeCity.Text = card.homeAddress[i];
                            break;
                        case 4:
                            window.HomeProvince.Text = card.homeAddress[i];
                            break;
                        case 5:
                            window.HomePostal.Text = card.homeAddress[i];
                            break;
                        case 6:
                            window.HomeCountry.Text = card.homeAddress[i];
                            break;
                        default:
                            break;
                    }
                }
            }
            

            window.HomePhone.Text = card.homePhone;
            window.HomeCell.Text = card.cellPhone;
            window.HomeWebsite.Text = card.Website;
            /*************************************/

            /*Work Tab*/
            if (card.workAddress != null)
            {
                for (int i = 0; i < card.workAddress.Length; i++)
                {
                    switch (i)
                    {
                        case 1:
                            window.WorkOffice.Text = card.workAddress[i];
                            break;
                        case 2:
                            window.WorkStreet.Text = card.workAddress[i];
                            break;
                        case 3:
                            window.WorkCity.Text = card.workAddress[i];
                            break;
                        case 4:
                            window.WorkProvince.Text = card.workAddress[i];
                            break;
                        case 5:
                            window.WorkPostal.Text = card.workAddress[i];
                            break;
                        case 6:
                            window.WorkCountry.Text = card.workAddress[i];
                            break;
                        default:
                            break;
                    }
                }
            }

            if (card.Company != null)
            {
                for (int i = 0; i < card.Company.Length; i++)
                {
                    switch (i)
                    {
                        case 0:
                            window.WorkCompany.Text = card.Company[i];
                            break;
                        case 1:
                            window.WorkDepartment.Text = card.Company[i];
                            break;
                        default:
                            break;
                    }
                }                
            }

            
            window.WorkJobTitle.Text = card.Title;
            window.WorkPhone.Text = card.workPhone; 
            window.WorkWebsite.Text = card.workWebsite;
            /*************************************/

            window.NotesTextBox.Text = card.Note;
            
        }

        private void RefreshCardDisplay()
        {
            bool loaded = false;

            if (cardList.Count > 0)
            {
                //cardList.Clear();
                dgUsers.ItemsSource = null;
                loaded = true;
            }

            for (int i = 0; i < filep.getNcards(); i++)
            {
                if (loaded)
                {
                    if (i + 1 > cardList.Count)
                    {
                        buildCard(i);
                    }
                }
                else
                {
                    buildCard(i);
                }
                
            }

            dgUsers.ItemsSource = cardList;
        }

        private void PropertyValue(VcUtil.VcPname name, VcUtil.VcProp prop, CardDetails card)
        {
            string[] tokens = null;

            switch (name)
            {
                case VcUtil.VcPname.VCP_N:                    
                    //tokens = prop.getValue().Split(';');
                    card.fullName = prop.getValue();
                    break;

                case VcUtil.VcPname.VCP_FN:
                    card.Name = prop.getValue();
                    break;

                case VcUtil.VcPname.VCP_NICKNAME:
                    break;

                case VcUtil.VcPname.VCP_PHOTO:
                    card.Image = prop.getValue();
                    break;

                case VcUtil.VcPname.VCP_BDAY:
                    break;

                case VcUtil.VcPname.VCP_ADR:
                    card.numAddress += 1;

                    if (card.Address == null)
                    {
                        card.Address = addressValue(prop.getValue());

                        tokens = prop.getValue().Split(';');

                        if (tokens[4].Length > 0)
                        {
                            card.Region = tokens[4];
                        }

                        if (tokens[6].Length > 0)
                        {
                            card.Country = tokens[6];
                        }
                    }
                    else
                    {
                        card.Address += "\n" + addressValue(prop.getValue());
                    }

                    if (prop.getPartype().ToUpper().Contains("HOME"))
                    {
                        card.homeAddress = prop.getValue().Split(';');
                    }

                    if (prop.getPartype().ToUpper().Contains("WORK"))
                    {
                        card.workAddress = prop.getValue().Split(';');
                    }
                    break;

                case VcUtil.VcPname.VCP_LABEL:
                    break;

                case VcUtil.VcPname.VCP_TEL:
                    card.numTelephone += 1;
                    if (card.Telephone == null)
                    {
                        card.Telephone = prop.getValue();
                    }
                    else
                    {
                        card.Telephone += "\n" + prop.getValue();
                    }

                    if (prop.getPartype().ToUpper().Contains("HOME"))
                    {
                        card.homePhone = prop.getValue();
                    }

                    if (prop.getPartype().ToUpper().Contains("WORK"))
                    {
                        card.workPhone = prop.getValue();
                    }

                    if (prop.getPartype().ToUpper().Contains("CELL"))
                    {
                        card.cellPhone = prop.getValue();
                    }

                    break;

                case VcUtil.VcPname.VCP_EMAIL:
                    if (card.Email == null)
                    {
                        card.Email = prop.getValue();
                    }
                    else
                    {
                        card.Email += "\n" + prop.getValue();
                    }

                    if (prop.getPartype().ToUpper().Contains("PREF"))
                    {
                        card.mainEmail = prop.getValue();
                    }

                    break;

                case VcUtil.VcPname.VCP_GEO:
                    break;

                case VcUtil.VcPname.VCP_TITLE:
                    card.Title = prop.getValue();
                    break;

                case VcUtil.VcPname.VCP_ORG:
                    tokens = prop.getValue().Split(';');
                    card.Company = tokens;
                    break;

                case VcUtil.VcPname.VCP_NOTE:
                    card.Note = prop.getValue();
                    break;

                case VcUtil.VcPname.VCP_URL:
                    if (prop.getPartype().ToUpper().Contains("HOME"))
                    {
                        card.Website = prop.getValue();
                    }

                    if (prop.getPartype().ToUpper().Contains("WORK"))
                    {
                        card.workWebsite = prop.getValue();
                    }
                    break;

                case VcUtil.VcPname.VCP_OTHER:
                    break;

                default:
                    break;
            }
        }

        private string addressValue(string value)
        {
            string[] tokens = null;
            StringBuilder address = new StringBuilder(); 

            tokens = value.Split(';');

            for (int i = 2; i < tokens.Length; i++)
            {
                if (tokens[i].Length > 1)
                {
                    address.Append(tokens[i].Trim()).Append(" ");
                }
            }

            return address.ToString();
        }

        private void buildCard(int index)
        {
            CardDetails card = new CardDetails();
            card.cardNumber = index + 1;

            for (int j = 0; j < filep.getCardp(index).getNprops(); j++)
            {
                PropertyValue(filep.getCardp(index).getProp(j).getName(), filep.getCardp(index).getProp(j), card);
            }

            if (databaseOpen)
            {
                card.Stored = true;
            }
            else
            {
                card.Stored = false;
            }

            cardList.Add(card);
        }


        /*Window Functions*/

        protected override void OnClosed(EventArgs e)
        {
            db.Close();
            base.OnClosed(e);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            CardDetails card = (CardDetails)dgUsers.SelectedItem;
            string query;
            int nameID;


            if (card.Stored)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete " + card.Name + " from database", "", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    query = @"SELECT name_id FROM NAME WHERE ([name] = @name);";
                    command = new SQLiteCommand(query, db);
                    command.Parameters.AddWithValue("@name", card.fullName);
                    SQLiteDataReader reader = command.ExecuteReader();

                    nameID = Convert.ToInt32(reader["name_id"]);

                    query = @"DELETE FROM PROPERTY WHERE ([name_id] = @nameID);";
                    command = new SQLiteCommand(query, db);
                    command.Parameters.AddWithValue("@nameID", nameID);
                    command.ExecuteNonQuery();

                    query = @"DELETE FROM NAME WHERE ([name_id] = @nameID);";
                    command = new SQLiteCommand(query, db);
                    command.Parameters.AddWithValue("@nameID", nameID);
                    command.ExecuteNonQuery();

                    filep.removeCard(filep.getCardp(card.cardNumber - 1));
                    cardList.RemoveAt(card.cardNumber - 1);
                    MessageBox.Show(card.Name + " has been removed from the database", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshCardDisplay();
                }
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to remove " + card.Name + " from the list", "", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    filep.removeCard(filep.getCardp(card.cardNumber - 1));
                    cardList.RemoveAt(card.cardNumber - 1);
                    MessageBox.Show(card.Name + " has been removed from list", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshCardDisplay();
                }
            }            
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
