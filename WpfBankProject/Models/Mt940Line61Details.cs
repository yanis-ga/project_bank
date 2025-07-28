using System;

namespace WpfBankProject.Models
{
    public class Mt940Line61Details
    {
        public DateTime? ValueDate { get; set; }
        public DateTime? EntryDate { get; set; }
        public string DebitCredit { get; set; }
        public decimal? Amount { get; set; }
        public string TransactionType { get; set; }
        public string Reference { get; set; }
        public string Reference2 { get; set; } 
    }
}
