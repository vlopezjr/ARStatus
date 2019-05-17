using System;

namespace ARStatus
{
    public class CustSummary
    {
        public string CustId { get; set; }
        public string CustName { get; set; }
        public int CustKey { get; set; }
        public string CustClassId { get; set; }
        public string Collector { get; set; }
        public AccountStatus? HoldStatusKey { get; set; }
        public string HoldStatusId { get; set; }
        public string ARStatus { get; set; }
        public DateTime? AgingDate { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal Balance45 { get; set; }
        public decimal Balance60 { get; set; }
        public decimal Balance90 { get; set; }
        public decimal TotalLate { get; set; }
        public decimal CreditLimit { get; set; }
        public string PmtTermsId { get; set; }
        public decimal? LastPmtAmt { get; set; }
        public DateTime? LastPmtDate { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string PhoneExt { get; set; }
        public short Status { get; set; }
        public string AccountType { get; set; }
    }
}
