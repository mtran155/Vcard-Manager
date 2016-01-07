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
        VcUtil.VcStatus status = new VcUtil.VcStatus();
        VcUtil util = new VcUtil();
        VcUtil.VcFile filep = new VcUtil.VcFile();
        List<CardDetails> cardList = new List<CardDetails>();
        SQLiteConnection db = new SQLiteConnection("Data Source=../../Database/VcardDatabase.db;Version=3;");
        SQLiteCommand command;
        SQLiteTransaction trans;


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

            //sql = @"INSERT INTO NAME VALUES (null, 'DOUG');";
            //command = new SQLiteCommand(sql, db);
            //command.ExecuteNonQuery();

            //sql = @"INSERT INTO NAME (name) VALUES ('JOE');";
            //command = new SQLiteCommand(sql, db);
            //command.ExecuteNonQuery();

            //sql = @"SELECT * FROM NAME";
            //command = new SQLiteCommand(sql, db);
            //SQLiteDataReader reader = command.ExecuteReader();
            //while (reader.Read()) { LogBox.Text += "Name: " + reader["name"]; }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open VCF File";
            openFileDialog.Filter = "VCF files (*.vcf)|*.vcf|All files (*.*)|*.*";


            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                StreamReader sr = new StreamReader(fileName);
                status = util.readVcFile(sr, filep);

                if (status.getCode() == VcUtil.VcError.OK)
                {
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

        private void CloseFile_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to close this applicaiton?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                db.Close();
                Application.Current.Shutdown();
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is a Vcard Manager\n\n" +
                            "Created By: Michael Tran\n\n" +
                            "This application is only compatible with Vcard Version 3.0", "About Vcard Editor", MessageBoxButton.OK, MessageBoxImage.Information);
        }

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
            for (int i = 0; i < card.fullName.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        window.LastName.Text = card.fullName[i];
                        break;
                    case 1:
                        window.FirstName.Text = card.fullName[i];
                        break;
                    case 2:
                        window.MiddleName.Text = card.fullName[i];
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

        public void RefreshCardDisplay()
        {
            for (int i = 0; i < filep.getNcards(); i++)
            {
                CardDetails card = new CardDetails();
                card.cardNumber = i + 1;

                for (int j = 0; j < filep.getCardp(i).getNprops(); j++)
                {
                    PropertyValue(filep.getCardp(i).getProp(j).getName(), filep.getCardp(i).getProp(j), card);
                }
                
                cardList.Add(card);
            }

            dgUsers.ItemsSource = cardList;
        }

        private void PropertyValue(VcUtil.VcPname name, VcUtil.VcProp prop, CardDetails card)
        {
            string[] tokens = null;

            switch (name)
            {
                case VcUtil.VcPname.VCP_N:                    
                    tokens = prop.getValue().Split(';');
                    card.fullName = tokens;
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

        protected override void OnClosed(EventArgs e)
        {
            db.Close();
            base.OnClosed(e);
        }

        private void StoreAll_Click(object sender, RoutedEventArgs e)
        {
            string query= null;
            int value = 0;

            if(filep.getNcards() != 0)
            {
                for (int i = 0; i < filep.getNcards(); i++)
                {
                    int numFN = 0;
                    int numNick = 0;
                    int numPhoto = 0;
                    int numBday = 0;
                    int numAdr = 0;
                    int numLabel = 0;
                    int numTel = 0;
                    int numEmail = 0;
                    int numGeo = 0;
                    int numTitle = 0;
                    int numOrg = 0;
                    int numNote = 0;
                    int numUid = 0;
                    int numUrl = 0;
                    int numOther = 0;

                    int nameID = 0;

                    for (int j = 0; j < filep.getCardp(i).getNprops(); j++)
                    {

                        if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_N)
                        {
                            bool duplicate = checkExistingCard(filep.getCardp(i).getProp(j).getValue());

                            if (!duplicate)
                            {
                                query = "INSERT INTO NAME VALUES (NULL, @name);";
                                command = new SQLiteCommand(query, db);
                                command.Parameters.AddWithValue("@name", filep.getCardp(i).getProp(j).getValue());

                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception)
                                {
                                    LogBox.Text = "Vcard already exists";
                                }

                                query = "SELECT last_insert_rowid();";
                                command = new SQLiteCommand(query, db);
                                nameID = Convert.ToInt32(command.ExecuteScalar());
                            }
                            else
                            {
                                LogBox.Text += "checking dups " + i + "\n";
                                break;
                            }
                            
                        }
                        else
                        {
                            if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_FN)
                            {
                                numFN += 1;
                                value = numFN;
                            }
                            if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_NICKNAME)
                            {
                                numNick += 1;
                                value = numNick;
                            }
                            if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_PHOTO)
                            {
                                numPhoto += 1;
                                value = numPhoto;
                            }
                            if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_BDAY)
                            {
                                numBday += 1;
                                value = numBday;
                            }
                            if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_ADR)
                            {
                                numAdr += 1;
                                value = numAdr;
                            }
                            if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_LABEL)
                            {
                                numLabel += 1;
                                value = numLabel;
                            }
                            if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_TEL)
                            {
                                numTel += 1;
                                value = numTel;
                            }
                            if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_EMAIL)
                            {
                                numEmail += 1;
                                value = numEmail;
                            }
                            if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_GEO)
                            {
                                numGeo += 1;
                                value = numGeo;
                            }
                            if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_TITLE)
                            {
                                numTitle += 1;
                                value = numTitle;
                            }
                            if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_ORG)
                            {
                                numOrg += 1;
                                value = numOrg;
                            }
                            if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_NOTE)
                            {
                                numNote += 1;
                                value = numNote;
                            }
                            if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_UID)
                            {
                                numUid += 1;
                                value = numUid;
                            }
                            if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_URL)
                            {
                                numUrl += 1;
                                value = numUrl;
                            }
                            if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_OTHER)
                            {
                                numOther += 1;
                                value = numOther;
                            }

                            query = "INSERT INTO PROPERTY (name_id, pname, pinst, partype, parval, value) VALUES (@ID, @pname, @pinst, @partype, @parval, @value)";
                            command = new SQLiteCommand(query, db);
                            command.Parameters.AddWithValue("@ID", nameID);
                            command.Parameters.AddWithValue("@pname", pNameConvertor(filep.getCardp(i).getProp(j).getName()));
                            command.Parameters.AddWithValue("@pinst", value);
                            command.Parameters.AddWithValue("@partype", filep.getCardp(i).getProp(j).getPartype());
                            command.Parameters.AddWithValue("@parval", filep.getCardp(i).getProp(j).getParVal());
                            command.Parameters.AddWithValue("@value", filep.getCardp(i).getProp(j).getValue());
                            command.ExecuteNonQuery();
                        }                  
                    }
                }
            }
            else
            {
                MessageBox.Show("No cards are loaded to store", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string pNameConvertor(VcUtil.VcPname value)
        {
            string name = null;

            switch (value)
            {
                case VcUtil.VcPname.VCP_FN:
                    name = "FN";
                    break;
                case VcUtil.VcPname.VCP_NICKNAME:
                    name = "NICKNAME";
                    break;
                case VcUtil.VcPname.VCP_PHOTO:
                    name = "PHOTO";
                    break;
                case VcUtil.VcPname.VCP_BDAY:
                    name = "BDAY";
                    break;
                case VcUtil.VcPname.VCP_ADR:
                    name = "ADR";
                    break;
                case VcUtil.VcPname.VCP_LABEL:
                    name = "LABEL";
                    break;
                case VcUtil.VcPname.VCP_TEL:
                    name = "TEL";
                    break;
                case VcUtil.VcPname.VCP_EMAIL:
                    name = "EMAIL";
                    break;
                case VcUtil.VcPname.VCP_GEO:
                    name = "GEO";
                    break;
                case VcUtil.VcPname.VCP_TITLE:
                    name = "TITLE";
                    break;
                case VcUtil.VcPname.VCP_ORG:
                    name = "ORG";
                    break;
                case VcUtil.VcPname.VCP_NOTE:
                    name = "NOTE";
                    break;
                case VcUtil.VcPname.VCP_UID:
                    name = "UID";
                    break;
                case VcUtil.VcPname.VCP_URL:
                    name = "URL";
                    break;
                case VcUtil.VcPname.VCP_OTHER:
                    name = "OTHER";
                    break;
                default:
                    break;
            }

            return name;
        }

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
    }
}
