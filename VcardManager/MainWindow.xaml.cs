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

        public MainWindow()
        {
            InitializeComponent();
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
                Application.Current.Shutdown();
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is a Vcard Editor\n\n" +
                            "Created By: Michael Tran\n\n" +
                            "This application is only compatible with Vcard Version 3.0", "About Vcard Editor", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Flag_Click(object sender, RoutedEventArgs e)
        {
            string flagMeanging = "Flag Meanings:\n" +
                                  "C = card as a whole is in canonical form\n" +
                                  "M = card has multiple same mandatory propeties: FN or N\n" +
                                  "U, P, G = card has at least one URL, PHOTO or GEO property\n";

            string colourMeaning = "Colour Meanings:\nGreen = card is in canonical form (the C flag is on)\n" +
                                   "Red = card needs fixing because (a) it has multiple mandatory properties (the M flag is on) or (b) its FN property has the same value as the preceding card\'s FN.\n" +
                                   "Yellow  = default state if neither green nor red applies.\n";

            MessageBox.Show(flagMeanging + "\n" + colourMeaning, "About Flags and Colour", MessageBoxButton.OK, MessageBoxImage.Information);
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
            window.TextCompany.Text = card.Company;
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
            /*************************************/

            /*Work Tab*/
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
                    card.Company = prop.getValue();
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
    }
}
