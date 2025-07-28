using System;

namespace WpfBankProject.Models
{
    public class Mt940RawLine
    {
        public string Tag { get; set; }  // Example : "20", "25", "61", etc.
        public string Content { get; set; }
        public Mt940Line61Details Details61 { get; set; } // for line :61
        public Mt940Line86Details Details86 { get; set; } // for line :86
        public bool HasFormatError { get; set; } // for thr format error

    }
}
