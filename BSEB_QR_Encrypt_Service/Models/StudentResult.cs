using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSEB_QR_Encrypt_Service.Models
{
    public class StudentResult
    {
        public int? Status { get; set; }
        public string? RollCode { get; set; }
        public string? RollNo { get; set; }
        public string? BsebUniqueID { get; set; }
        public string? msg { get; set; }
        public string? RegistrationNo { get; set; }
        public DateTime? dob { get; set; }
        public int? IsCCEMarks { get; set; }
        public string? NameoftheCandidate { get; set; }
        public string? FathersName { get; set; }
        public string? CollegeName { get; set; }
        public string? Faculty { get; set; }
        public string? TotalAggregateMarkinNumber { get; set; }
        public string? TotalAggregateMarkinWords { get; set; }
        public string? Division { get; set; }
        //public bool? IsCCEMarks { get; set; }

        public List<SubjectResult> SubjectResults { get; set; } = new();
    }
}
