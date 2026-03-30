using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSEB_QR_Encrypt_Service.Models
{
    public class SubjectResult
    {
        public string? Sub { get; set; }
        public int? MaxMark { get; set; }
        public int? PassMark { get; set; }
        public string? Theory { get; set; }
        public string? OB_PR { get; set; }
        public string? GRC_THO { get; set; }
        public string? GRC_PR { get; set; }
        public string? TotSub { get; set; }
        public string? SubjectGroupName { get; set; }
        public string? CCEMarks { get; set; }
    }
}
