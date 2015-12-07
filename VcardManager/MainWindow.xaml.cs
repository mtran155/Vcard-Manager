using Microsoft.Win32;
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
        List<CardDetails> cards = new List<CardDetails>();

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

        public void RefreshCardDisplay()
        {
            int adrCount = 0;
            int telCount = 0;
            int nameCount = 0;
            int geoCount = 0;
            int photoCount = 0;
            int urlCount = 0;
            bool adrFlag = false;
            bool telFlag = false;
            bool emailFlag = false;
            string name = null;
            string region = null;
            string country = null;
            string[] tokens = null;
            string image = null;
            StringBuilder uidString = new StringBuilder();
            StringBuilder address = new StringBuilder();
            StringBuilder telephone = new StringBuilder();
            StringBuilder email = new StringBuilder();

            for (int i = 0; i < filep.getNcards(); i++)
            {
                adrCount = telCount = nameCount = geoCount = photoCount = urlCount = 0;
                adrFlag = telFlag = emailFlag = false;
                name = region = country = image = null;
                uidString.Clear();
                uidString.Append("?----");
                address.Clear();
                telephone.Clear();
                email.Clear();

                for (int j = 0; j < filep.getCardp(i).getNprops(); j++)
                {
                    if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_FN)
                    {
                        name = filep.getCardp(i).getProp(j).getValue();
                        nameCount++;
                    }
                    else if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_N)
                    {
                        nameCount++;
                    }
                    else if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_ADR)
                    {
                        if (adrFlag)
                        {
                            address.Append("\n");
                        }

                        adrCount++;
                        tokens = filep.getCardp(i).getProp(j).getValue().Split(';');
                        for (int k = 2; k < tokens.Length; k++)
                        {
                            if (tokens[k].Length > 1)
                            {
                                address.Append(tokens[k].Trim()).Append(" ");
                            }
                        }

                        if (!adrFlag)
                        {
                            region = tokens[4];
                            country = tokens[6];
                            adrFlag = true;
                        }
                    }
                    else if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_TEL)
                    {
                        if (telFlag)
                        {
                            telephone.Append("\n");
                        }

                        telephone.Append(filep.getCardp(i).getProp(j).getValue());
                        telCount++;
                        telFlag = true;
                    }
                    else if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_EMAIL)
                    {
                        if (emailFlag)
                        {
                            email.Append("\n");
                        }

                        email.Append(filep.getCardp(i).getProp(j).getValue());
                        emailFlag = true;
                    }
                    else if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_GEO)
                    {
                        geoCount++;
                    }
                    else if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_PHOTO)
                    {
                        image = filep.getCardp(i).getProp(j).getValue();
                        photoCount++;
                    }
                    else if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_URL)
                    {
                        urlCount++;
                    }
                    else if (filep.getCardp(i).getProp(j).getName() == VcUtil.VcPname.VCP_UID)
                    {
                        uidString.Append(filep.getCardp(i).getProp(j).getValue());
                        foreach (char c in filep.getCardp(i).getProp(j).getValue())
                        {
                            if (c == '*')
                            {
                                uidString[0] = '-';
                                break;
                            }
                            else
                            {
                                uidString[0] = 'C';
                            }
                        }
                    }
                }

                if (nameCount > 1)
                {
                    uidString[1] = 'M';
                }
                if (urlCount > 0)
                {
                    uidString[2] = 'U';
                }
                if (photoCount > 0)
                {
                    uidString[3] = 'P';
                }
                if (geoCount > 0)
                {
                    uidString[4] = 'G';
                }

                if (email.Length == 0)
                {
                    email.Append("Not Available");
                }

                cards.Add(new CardDetails()
                {
                    cardNumber = (i + 1),
                    Name = name,
                    Region = region,
                    Country = country,
                    Address = address.ToString().TrimEnd(),
                    numAddress = adrCount,
                    Telephone = telephone.ToString(),
                    numTelephone = telCount,
                    Email = email.ToString(),
                    UID = uidString.ToString(),
                    Image = image
                });
            }

            dgUsers.ItemsSource = cards;
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
    }
}
