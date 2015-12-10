using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VcardManager.Model
{
    class CardDetails
    {
        public int cardNumber { get; set; }

        public string Name { get; set; }

        public string[] fullName { get; set; }

        public string Address { get; set; }

        public string[] homeAddress { get; set; }

        public string[] workAddress { get; set; }

        public string Region { get; set; }

        public string Country { get; set; }

        public int numAddress { get; set; }

        public string Telephone { get; set; }

        public string homePhone { get; set; }

        public string cellPhone { get; set; }

        public string workPhone { get; set; }

        public int numTelephone { get; set; }

        public string Email { get; set; }

        public string mainEmail { get; set; }

        public string Company { get; set; }

        public string Title { get; set; }

        public string Website { get; set; }

        public string workWebsite { get; set; }

        public string Note { get; set; }

        public string Image { get; set; }
    }
}
