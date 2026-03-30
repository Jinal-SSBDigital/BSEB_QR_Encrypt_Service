using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSEB_QR_Encrypt_Service.Models
{
    public class QREncryptedData
    {
        public string RollCode { get; set; }
        public string RollNo { get; set; }
        public string EncryptedData { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
