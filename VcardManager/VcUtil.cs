using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VcardManager
{
    class VcUtil
    {
        public enum VcError
        {
            OK = 0,
            IOERR,      // I/O error
            SYNTAX,     // property not in form 'name:value'
            PAROVER,    // parameter overflow
            BEGEND,     // BEGIN...END not found as expected
            BADVER,     // version not accepted
            NOPVER,     // no "VERSION" property
            NOPNFN,     // no "N" or "FN" property
        };

        public enum VcPname
        {
            VCP_BEGIN,
            VCP_END,
            VCP_VERSION,
            VCP_N,      // name
            VCP_ADR,    // address
            VCP_BDAY,
            VCP_EMAIL,
            VCP_FN,     // formatted name
            VCP_GEO,    // lat,long
            VCP_LABEL,  // address formatted for printing
            VCP_NICKNAME,
            VCP_NOTE,
            VCP_ORG,
            VCP_PHOTO,
            VCP_TEL,
            VCP_TITLE,
            VCP_UID,    // unique ID
            VCP_URL,
            VCP_OTHER,   // used for any other property name
            VCP_BAD
        };

        public class VcStatus
        {
            private VcError code;
            private int linefrom, lineto;

            public VcStatus()
            {
                linefrom = lineto = 0;
            }

            public void setCode(VcError value)
            {
                code = value;
            }

            public void setLines(int from, int to)
            {
                linefrom = from;
                lineto = to;
            }

            public int getLineFrom()
            {
                return linefrom;
            }

            public int getLineto()
            {
                return lineto;
            }

            public VcError getCode()
            {
                return code;
            }
        }

        public class VcFile : IComparer
        {
            private int nCards;
            private ArrayList cardp;

            public VcFile()
            {
                nCards = 0;
                cardp = new ArrayList();
            }

            public void setNcards(int num)
            {
                nCards += num;
            }

            public void setCardp(Vcard value)
            {
                cardp.Add(value);
            }

            public int getNcards()
            {
                return nCards;
            }

            public Vcard getCardp(int index)
            {
                Vcard card = (Vcard)cardp[index];
                return card;
            }

            public ArrayList getList()
            {
                return this.cardp;
            }

            public void removeCard(Vcard card)
            {
                cardp.Remove(card);
                this.setNcards(-1);
            }

            public override string ToString()
            {
                StringBuilder cards = new StringBuilder();
                Vcard card = new Vcard();

                for (int i = 0; i < this.nCards; i++)
                {
                    //Console.WriteLine(cardp.Capacity);
                    if (cardp[i] != null)
                    {
                        card = (Vcard)cardp[i];
                        cards.Append("BEGIN:VCARD\n").Append("VERSION:3.0\n");
                        for (int j = 0; j < card.getNprops(); j++)
                        {
                            cards.Append(card.getProp(j).getValue()).Append("\n");
                        }
                        cards.Append("END:VCARD\n\n");
                    }
                }

                return cards.ToString();
            }

            public int Compare(object x, object y)
            {
                string[] tokens;
                string c1First = null;
                string c1Last = null;
                string c2First = null;
                string c2Last = null;

                for (int i = 0; i < ((Vcard)x).getNprops(); i++)
                {
                    if (((Vcard)x).getProp(i).getName() == VcPname.VCP_N)
                    {
                        tokens = ((Vcard)x).getProp(i).getValue().Split(';');
                        c1First = tokens[1];
                        c1Last = tokens[0];

                        if (tokens[1].Contains("(none)"))
                        {
                            c1First = "";
                        }
                        break;
                    }
                }

                for (int i = 0; i < ((Vcard)y).getNprops(); i++)
                {
                    if (((Vcard)y).getProp(i).getName() == VcPname.VCP_N)
                    {
                        tokens = ((Vcard)y).getProp(i).getValue().Split(';');
                        c2First = tokens[1];
                        c2Last = tokens[0];

                        if (tokens[1].Contains("(none)"))
                        {
                            c2First = "";
                        }
                        break;
                    }
                }

                if (c1Last.CompareTo(c2Last) == 0)
                {
                    return c1First.CompareTo(c2First);
                }
                else
                    return c1Last.CompareTo(c2Last);

                throw new NotImplementedException();
            }
        }

        public class Vcard
        {
            private int nProps;
            private ArrayList propList;

            public Vcard()
            {
                nProps = 0;
                propList = new ArrayList();
            }

            public void setNprops(int value)
            {
                nProps += value;
            }

            public void setProplist(VcProp value)
            {
                if (getNprops() > 0)
                {
                    bool added = false;

                    for (int i = 0; i < getNprops(); i++)
                    {
                        VcProp prop = getProp(i);
                        //Console.WriteLine(prop.getName() + " " + prop.getValue());
                        if (value.getName() < prop.getName())
                        {
                            propList.Insert(i, value);
                            added = true;
                            break;
                        }

                        if (value.getName() == prop.getName())
                        {
                            propList.Insert(i + 1, value);
                            added = true;
                            break;
                        }
                    }
                    if (!added)
                    {
                        propList.Add(value);
                    }
                }
                else
                {
                    propList.Add(value);
                }
            }

            public int getNprops()
            {
                return nProps;
            }

            public VcProp getProp(int index)
            {
                VcProp prop = (VcProp)propList[index];
                return prop;
            }
        }

        public class VcProp
        {
            private VcPname name;
            private string partype;
            private string parval;
            private string value;

            public VcProp()
            {
                partype = null;
                parval = null;
                value = null;
            }

            public void setName(VcPname value)
            {
                name = value;
            }

            public void setPartype(string value)
            {
                partype = value;
            }

            public void setParval(string value)
            {
                parval = value;
            }

            public void setValue(string value)
            {
                this.value = value;
            }

            public string getPartype()
            {
                return partype;
            }

            public string getParVal()
            {
                return parval;
            }

            public string getValue()
            {
                return value;
            }

            public VcPname getName()
            {
                return name;
            }
        }

        //static private int currentLine = 1;
        //static private int numLinesRead = 1;
        static private string VCARD_VER = "3.0";

        public VcStatus readVcFile(StreamReader sr, VcFile filep)
        {
            VcStatus status = new VcStatus();
            Vcard newCard = new Vcard();

            status.setCode(VcError.OK);

            while (status.getCode() == VcError.OK)
            {
                status = readVcard(sr, newCard);
                if (newCard == null)
                {
                    break;
                }

                if (status.getCode() == VcError.OK && newCard.getNprops() > 0)
                {
                    //Console.WriteLine(newCard.ToString());
                    filep.setCardp(newCard);
                    filep.setNcards(1);
                    newCard = new Vcard();
                }

                if (sr.EndOfStream)
                {
                    //Console.WriteLine(filep.getNcards());
                    break;
                }
            }

            return status;
        }

        public VcStatus readVcard(StreamReader sr, Vcard card)
        {
            VcStatus status = new VcStatus();
            VcProp propp;
            string buffer = null;
            bool nameCheck = false;
            bool fnCheck = false;
            int counter = 0;
            bool beginFlag = false;
            bool endFlag = false;
            bool versionFlag = false;


            status.setCode(VcError.OK);

            while (status.getCode() == VcError.OK)
            {
                propp = new VcProp();
                status = getUnfolded(sr, ref buffer);
                status.setCode(parseVcProp(buffer, propp));

                if (sr.EndOfStream)
                {
                    if (propp.getValue() == null)
                    {
                        break;
                    }

                    if (buffer == null || status.getCode() == VcError.SYNTAX || status.getCode() == VcError.PAROVER)
                    {
                        return status;
                    }

                    if (propp.getName() != VcPname.VCP_END || string.Compare(propp.getValue(), "VCARD") != 0)
                    {
                        status.setCode(VcError.BEGEND);
                        return status;
                    }

                    if (nameCheck == false || fnCheck == false)
                    {
                        status.setCode(VcError.NOPNFN);
                        return status;
                    }

                    break;
                }
                else
                {
                    if (counter == 0 && propp.getName() == VcPname.VCP_BEGIN)
                    {
                        if (string.Compare(propp.getValue().ToUpper(), "VCARD") == 0)
                        {
                            beginFlag = true;
                        }
                        else
                        {
                            status.setCode(VcError.BEGEND);
                            return status;
                        }
                    }
                    else if (counter == 0 && propp.getName() != VcPname.VCP_BEGIN)
                    {
                        status.setCode(VcError.BEGEND);
                        return status;
                    }

                    if (counter == 1 && beginFlag == true && propp.getName() == VcPname.VCP_VERSION)
                    {
                        if (string.Compare(propp.getValue(), VCARD_VER) == 0)
                        {
                            versionFlag = true;
                        }
                        else
                        {
                            status.setCode(VcError.NOPVER);
                            return status;
                        }
                    }
                    else if (counter == 1 && beginFlag == true && propp.getName() != VcPname.VCP_VERSION)
                    {
                        status.setCode(VcError.NOPVER);
                        return status;
                    }

                    if (propp.getName() == VcPname.VCP_N)
                    {
                        nameCheck = true;
                    }

                    if (propp.getName() == VcPname.VCP_FN)
                    {
                        fnCheck = true;
                    }

                    if (counter == 2 && propp.getName() == VcPname.VCP_END)
                    {
                        if (string.Compare(propp.getValue().ToUpper(), "VCARD") == 0)
                        {
                            counter++;
                            endFlag = true;
                        }
                        else
                        {
                            status.setCode(VcError.BEGEND);
                            return status;
                        }

                        if (nameCheck == false && fnCheck == false)
                        {
                            status.setCode(VcError.NOPNFN);
                            return status;
                        }
                    }
                    else if (counter == 2 && propp.getName() == VcPname.VCP_BEGIN)
                    {
                        status.setCode(VcError.BEGEND);
                        return status;
                    }

                    if (counter == 2 && beginFlag == true && versionFlag == true)
                    {
                        if (status.getCode() == VcError.OK)
                        {
                            card.setProplist(propp);
                            card.setNprops(1);
                        }
                        else
                        {
                            return status;
                        }
                    }

                    if (counter == 3 && endFlag == true && beginFlag == true && versionFlag == true)
                    {
                        return status;
                    }

                    if (counter == 2)
                    {
                        continue;
                    }
                    else
                    {
                        counter++;
                    }
                }
                //Console.WriteLine("INSIDE READ VCARD " + buffer);

            }

            return status;
        }

        public VcStatus getUnfolded(StreamReader sr, ref string buff)
        {
            VcStatus status = new VcStatus();
            string buffer;
            char ch;
            StringBuilder line = new StringBuilder();

            status.setCode(VcError.OK);

            buffer = sr.ReadLine();

            if (buffer == null)
            {
                buff = null;
                return status;
            }

            if (char.IsWhiteSpace(buffer[0]))
            {
                buffer.Trim();
            }

            line.Append(buffer);

            while (true)
            {
                if (char.IsWhiteSpace(ch = (char)sr.Peek()))
                {
                    buffer = sr.ReadLine();
                    line.Append(buffer.Trim());
                }
                break;
            }

            //Console.WriteLine(line.ToString());
            buff = line.ToString();

            return status;
        }

        public VcError parseVcProp(string buffer, VcProp propp)
        {
            VcError status = VcError.OK;
            string line;
            string leftSide;
            string[] tokens;
            int numColon = 0;
            int numEqualSign = 0;
            int numSemiColon = 0;
            int numType = 0;
            int numVal = 0;
            VcPname name;

            if (buffer == null)
            {
                return status;
            }

            //Check for group and ignore it
            if (checkGroup(buffer))
            {
                line = buffer.Substring(buffer.IndexOf('.') + 1);
            }
            else
            {
                line = buffer;
            }

            //Check for syntax errors before parsing
            numColon = countPunctuation(':', line);

            if (numColon == 0 || line[0] == ';' || line[0] == ':')
            {
                propp.setName(VcPname.VCP_BAD);
                status = VcError.SYNTAX;
                return status;
            }

            leftSide = line.Substring(0, buffer.IndexOf(':'));
            numEqualSign = countPunctuation('=', leftSide);
            numSemiColon = countPunctuation(';', leftSide);



            if (numSemiColon != numEqualSign)
            {
                propp.setName(VcPname.VCP_BAD);
                status = VcError.SYNTAX;
                return status;
            }

            //Case 1 only property name and value
            if (numSemiColon == 0)
            {
                if (leftSide.Contains(" ") || leftSide.Contains("\t"))
                {
                    propp.setName(VcPname.VCP_BAD);
                    status = VcError.SYNTAX;
                    return status;
                }

                name = getVcPname(leftSide);

                if (name == VcPname.VCP_OTHER)
                {
                    propp.setName(name);
                    propp.setValue(line);
                }
                else
                {
                    propp.setName(name);
                    propp.setValue(line.Substring(line.IndexOf(':') + 1));
                }
            }

            //Case 2 optional parameters present
            else
            {
                tokens = leftSide.Split(';');
                name = getVcPname(tokens[0]);

                if (name == VcPname.VCP_OTHER)
                {
                    propp.setName(name);
                    propp.setValue(line);
                }
                else
                {
                    propp.setName(name);

                    for (int i = 1; i < tokens.Length; i++)
                    {
                        if (paramCheck(tokens[i]) == 1)
                        {
                            if (numType >= 1)
                            {
                                propp.setPartype(propp.getPartype() + "," + tokens[i].Substring(tokens[i].IndexOf("=") + 1));
                            }
                            else
                            {
                                propp.setPartype(tokens[i].Substring(tokens[i].IndexOf("=") + 1));
                                numType++;
                            }
                        }
                        else if (paramCheck(tokens[i]) == 2)
                        {
                            if (numVal >= 1)
                            {
                                propp.setParval(propp.getParVal() + "," + tokens[i].Substring(tokens[i].IndexOf("=") + 1));
                            }
                            else
                            {
                                propp.setParval(tokens[i].Substring(tokens[i].IndexOf("=") + 1));
                                numVal++;
                            }
                        }
                        else if (paramCheck(tokens[i]) == 3)
                        {
                            continue;
                        }
                        else if (paramCheck(tokens[i]) == 4)
                        {
                            propp.setName(VcPname.VCP_BAD);
                            status = VcError.PAROVER;
                            return status;
                        }
                        else
                        {
                            propp.setName(VcPname.VCP_BAD);
                            status = VcError.SYNTAX;
                            return status;
                        }
                    }
                }

                propp.setValue(line.Substring(line.IndexOf(':') + 1));
            }
            //Console.WriteLine(propp.getName() + " " + propp.getPartype() + " " + propp.getParVal() + " " + propp.getValue());
            return status;
        }

        public VcStatus writeVcFile(VcFile filep)
        {
            VcStatus status = new VcStatus();
            StringBuilder finalCard = new StringBuilder();
            StringBuilder cardBuilder = new StringBuilder();
            int length = 0;
            string propName = null;
            //string finalString = null;

            status.setCode(VcError.OK);

            using (StreamWriter wr = new StreamWriter("TextFile2.txt"))
            {
                for (int i = 0; i < filep.getNcards(); i++)
                {
                    if (filep.getCardp(i) != null)
                    {
                        finalCard.Append("BEGIN:VCARD\r\n").Append("VERSION:3.0\r\n");

                        for (int j = 0; j < filep.getCardp(i).getNprops(); j++)
                        {
                            propName = propertyName(filep.getCardp(i).getProp(j).getName());

                            if (string.Compare(propName, "") != 0)
                            {
                                cardBuilder.Append(propName);
                            }

                            length = cardBuilder.Length;


                            if (filep.getCardp(i).getProp(j).getPartype() != null)
                            {
                                cardBuilder.Append(";TYPE=");
                                length += 6;

                                for (int k = 0; k < filep.getCardp(i).getProp(j).getPartype().Length; k++)
                                {
                                    if (length == 75)
                                    {
                                        cardBuilder.Append("\r\n ");
                                        length = 1;
                                    }

                                    cardBuilder.Append(filep.getCardp(i).getProp(j).getPartype()[k]);
                                    length++;
                                }
                            }

                            if (filep.getCardp(i).getProp(j).getParVal() != null)
                            {
                                cardBuilder.Append(";VALUE=");
                                length += 7;

                                for (int k = 0; k < filep.getCardp(i).getProp(j).getParVal().Length; k++)
                                {
                                    if (length == 75)
                                    {
                                        cardBuilder.Append("\r\n ");
                                        length = 1;
                                    }

                                    cardBuilder.Append(filep.getCardp(i).getProp(j).getParVal()[k]);
                                    length++;
                                }
                            }

                            if (filep.getCardp(i).getProp(j).getValue() != null)
                            {
                                cardBuilder.Append(":");
                                length += 1;

                                for (int k = 0; k < filep.getCardp(i).getProp(j).getValue().Length; k++)
                                {
                                    if (length == 75)
                                    {
                                        cardBuilder.Append("\r\n ");
                                        length = 1;
                                    }

                                    cardBuilder.Append(filep.getCardp(i).getProp(j).getValue()[k]);
                                    length++;
                                }
                            }

                            cardBuilder.Append("\r\n");
                            finalCard.Append(cardBuilder.ToString());
                            cardBuilder.Clear();
                        }

                        finalCard.Append("END:VCARD\r\n\n");

                    }
                    //finalString = finalCard.ToString();
                    wr.Write(finalCard.ToString());
                    finalCard.Clear();

                }
            }

            return status;
        }

        static private bool checkGroup(string group)
        {
            for (int i = 0; i < group.Length; i++)
            {
                if (group[i] == '.')
                {
                    return true;
                }
                else if (group[i] == ';' || group[i] == ':')
                {
                    return false;
                }
            }

            return false;
        }
        static private int countPunctuation(char ch, string line)
        {
            int counter = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ch)
                {
                    counter++;
                }
            }

            return counter;
        }
        public VcPname getVcPname(string name)
        {
            VcPname propName;

            if (string.Compare(name, "BEGIN") == 0)
            {
                propName = VcPname.VCP_BEGIN;
                return propName;
            }
            else if (string.Compare(name, "END") == 0)
            {
                propName = VcPname.VCP_END;
                return propName;
            }
            else if (string.Compare(name, "VERSION") == 0)
            {
                propName = VcPname.VCP_VERSION;
                return propName;
            }
            else if (string.Compare(name, "N") == 0)
            {
                propName = VcPname.VCP_N;
                return propName;
            }
            else if (string.Compare(name, "FN") == 0)
            {
                propName = VcPname.VCP_FN;
                return propName;
            }
            else if (string.Compare(name, "NICKNAME") == 0)
            {
                propName = VcPname.VCP_NICKNAME;
                return propName;
            }
            else if (string.Compare(name, "PHOTO") == 0)
            {
                propName = VcPname.VCP_PHOTO;
                return propName;
            }
            else if (string.Compare(name, "BDAY") == 0)
            {
                propName = VcPname.VCP_BDAY;
                return propName;
            }
            else if (string.Compare(name, "ADR") == 0)
            {
                propName = VcPname.VCP_ADR;
                return propName;
            }
            else if (string.Compare(name, "LABEL") == 0)
            {
                propName = VcPname.VCP_LABEL;
                return propName;
            }
            else if (string.Compare(name, "TEL") == 0)
            {
                propName = VcPname.VCP_TEL;
                return propName;
            }
            else if (string.Compare(name, "EMAIL") == 0)
            {
                propName = VcPname.VCP_EMAIL;
                return propName;
            }
            else if (string.Compare(name, "GEO") == 0)
            {
                propName = VcPname.VCP_GEO;
                return propName;
            }
            else if (string.Compare(name, "TITLE") == 0)
            {
                propName = VcPname.VCP_TITLE;
                return propName;
            }
            else if (string.Compare(name, "ORG") == 0)
            {
                propName = VcPname.VCP_ORG;
                return propName;
            }
            else if (string.Compare(name, "NOTE") == 0)
            {
                propName = VcPname.VCP_NOTE;
                return propName;
            }
            else if (string.Compare(name, "UID") == 0)
            {
                propName = VcPname.VCP_UID;
                return propName;
            }
            else if (string.Compare(name, "URL") == 0)
            {
                propName = VcPname.VCP_URL;
                return propName;
            }
            else
            {
                propName = VcPname.VCP_OTHER;
                return propName;
            }
        }
        static private int paramCheck(string param)
        {
            if (param[0] == ' ' || param[0] == '=' || param.Contains(" "))
            {
                return 5;
            }

            if (param.Contains("TYPE"))
            {
                if (param[4] == '=')
                {
                    return 1;
                }
                else
                {
                    return 4;
                }
            }
            else if (param.Contains("VALUE"))
            {
                if (param[5] == '=')
                {
                    return 2;
                }
                else
                {
                    return 4;
                }
            }
            else if (param.Contains("ENCODING"))
            {
                if (param[8] == '=')
                {
                    return 3;
                }
                else
                {
                    return 4;
                }
            }
            else if (param.Contains("CHARSET"))
            {
                if (param[7] == '=')
                {
                    return 3;
                }
                else
                {
                    return 4;
                }
            }
            return 4;
        }
        public string propertyName(VcPname prop)
        {
            string name = null;

            switch (prop)
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
                    name = "";
                    break;
            }

            return name;
        }
    }
}
