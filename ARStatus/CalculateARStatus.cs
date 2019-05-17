using System;
using System.Collections.Generic;

namespace ARStatus
{
    public enum AccountStatus
    {
        VIP = 1,
        Good = 2,
        ManualHold = 3,
        AutoHold = 4,
        InCollection = 5,
        WriteOff = 6
    }

    public class CalculateARStatus
    {
        private const decimal MinAllowedOverdue = 1M;

        private decimal _totalOwed;
        private decimal _totalOver45;
        private decimal _totalOver60;
        private decimal _totalOver90;
        private int _totalOnHold;
        private int _totalNewHold;
        private int _totalOffHold;

        private AccountStatus _prevStatus;
        private AccountStatus _newStatus;

        public decimal CreditLimit { get; set; }
        public DateTime? AgingDate { get; set; }
        public string PmtTermsId { get; set; }
        public string Collector { get; set; }
        public AccountStatus? HoldStatusKey { get; set; }
        public string HoldStatusId { get; set; }
        public string ARStatus { get; set; }

        public void UpdateAll()
        {
            List<CustSummary> summaries = DataAccess.GetCustStatusAll();

            foreach (CustSummary summary in summaries)
            {
                _newStatus = AssessStatus(summary);
                UpdateStatus(summary);
            }
            DataAccess.LogSummary(_totalOwed, _totalOver45, _totalOver60, _totalOver90, _totalOnHold, _totalNewHold, _totalOffHold);
        }

        public void Load(int custKey)
        {
            CustSummary summary = DataAccess.GetCustStatus(custKey);
            CreditLimit = summary.CreditLimit;
            AgingDate = summary.AgingDate;
            PmtTermsId = summary.PmtTermsId;
            Collector = summary.Collector;

            HoldStatusKey = summary.HoldStatusKey;
            HoldStatusId = (HoldStatusKey == null) ? "Good" : summary.HoldStatusId;

            if (OverLimit(summary))
                ARStatus = "Over";
            if (OverDue(summary))
                ARStatus += "Late";
            if (ARStatus == null)
                ARStatus = "Good";
    }


        /*
            Called by SA Manage Customer to update a single customer's AR Status.

            newStatus can have one of these values:
               VIP - put this guy on permanent Good
               MANUAL_HOLD - put this guy on permanent Bad
               AUTO_HOLD - let ARStatus decide each night (different meaning than above)
        */
        public void Update(int custKey, AccountStatus newStatus)
        {
            CustSummary summary = DataAccess.GetCustStatus(custKey);

            _prevStatus = summary.HoldStatusKey == null ? AccountStatus.Good : (AccountStatus)summary.HoldStatusKey;
            _newStatus = newStatus;

            switch (_newStatus)
            {
                // AR rep forces a manual status
                case AccountStatus.VIP:
                case AccountStatus.ManualHold:

                    if (_newStatus == AccountStatus.VIP)
                        InjectRemark(summary.CustId, "Placed on VIP");
                    else if (_newStatus == AccountStatus.ManualHold)
                        InjectRemark(summary.CustId, "Placed on Manual Hold");

                    // if not in table then add, else update
                    if (_prevStatus == AccountStatus.Good)
                    {
                        DataAccess.InsertCustomerOnHold(summary, _newStatus);
                        DataAccess.LogStatusChange(summary.CustKey, _prevStatus, _newStatus);
                    }
                    else
                        DataAccess.UpdateCustomerOnHold(summary, _newStatus);
                    break;

                // if the customer was on manual hold, convert to Auto in good standing or bad
                case AccountStatus.AutoHold:
                    if (_prevStatus != AccountStatus.Good)
                        if (OverLimit(summary) || OverDue(summary))
                            DataAccess.UpdateCustomerOnHold(summary, _newStatus);
                        else
                        {
                            InjectRemark(summary.CustId, "Removed from Hold");
                            DataAccess.DeleteCustomerOnHold(summary);
                            DataAccess.LogStatusChange(summary.CustKey, _prevStatus, _newStatus);
                        }
                    break;
            }
        }

        private AccountStatus AssessStatus(CustSummary summary)
        {
            _prevStatus = summary.HoldStatusKey == null ? AccountStatus.Good : (AccountStatus)summary.HoldStatusKey;

            switch (_prevStatus)
            {
                case AccountStatus.VIP:
                    return AccountStatus.VIP;

                case AccountStatus.ManualHold:
                    return AccountStatus.ManualHold;

                case AccountStatus.AutoHold:
                case AccountStatus.Good:
                    if (OverLimit(summary) || OverDue(summary))
                        return AccountStatus.AutoHold;
                    else
                        return AccountStatus.Good;
                default:  //for completeness; this should never be the case
                    return AccountStatus.Good;
            }
        }


        /*
           Update tcpCustHold as appropriate
           if New & Prev Status = Good then do nothing
           if NewStatus <> Good and PrevStatus = Good then INSERT
           if NewStatus = Good and PrevStatus <> Good then DELETE
           else UPDATE Status field and other fields
        */
        private void UpdateStatus(CustSummary summary)
        {
            //accumulate values across all customers
            _totalOwed += summary.TotalBalance;
            _totalOver45 += summary.Balance45;
            _totalOver60 += summary.Balance60;
            _totalOver90 += summary.Balance90;

            if (_newStatus == AccountStatus.ManualHold || _newStatus == AccountStatus.AutoHold)
                _totalOnHold++;

            // state machine trnasition logic

            if (_prevStatus == AccountStatus.VIP || _prevStatus == AccountStatus.ManualHold)
                DataAccess.UpdateCustomerOnHold(summary, _newStatus);

            else if (_prevStatus == AccountStatus.Good && _newStatus == AccountStatus.AutoHold)
            {
                InjectRemark(summary.CustId, "Placed on Hold");
                DataAccess.InsertCustomerOnHold(summary, _newStatus);
                DataAccess.LogStatusChange(summary.CustKey, _prevStatus, _newStatus);
                _totalNewHold++;
            }

            else if (_prevStatus == AccountStatus.AutoHold && _newStatus == AccountStatus.Good)
            {
                InjectRemark(summary.CustId, "Removed from Hold");
                DataAccess.DeleteCustomerOnHold(summary);
                DataAccess.LogStatusChange(summary.CustKey, _prevStatus, _newStatus);
                _totalOffHold++;
            }

            else if (_prevStatus == AccountStatus.AutoHold && _newStatus == AccountStatus.AutoHold)
            {
                DataAccess.UpdateCustomerOnHold(summary, _newStatus);
                if (DataAccess.HasLettersSent(summary))
                {
                    if (!OverDue(summary) && OverLimit(summary))
                    {
                        DataAccess.ClearLetterFlags(summary);
                        InjectRemark(summary.CustId, "Now current, but still over creditlimit");
                    }
                }
            }
        }

        private void InjectRemark(string custId, string remark)
        {
            Console.WriteLine(custId + ": " + remark);

            //RemarkContext context = new RemarkContext();
            //context.Load("ARCustLoad", custId);
            //context.AddRemark("Cust.AR.Coll", remark);
            //context.Save(true);
        }

        public string GetCallingData(string start, string end)
        {
            string html = "";
            string lastCollector = "";
            int count = 0;
            string background;

            List<CallingData> calls = DataAccess.GetCallingData(start, end);

            if (calls.Count > 0)
            {
                html = "<table border=0>";

                foreach (CallingData call in calls)
                {
                    //collector transition
                    if (call.Collector != lastCollector)
                    {
                        if (count > 0)
                        {
                            html += "<tr><td colspan=4>Total Calls:" + count + "</td></tr><tr><td colspan=4>&nbsp;</td></tr>";
                            count = 0;
                        }
                        lastCollector = call.Collector;
                        html += "<tr><td colspan=4><b>" + lastCollector + "</b></td></tr>";
                    }

                    background = (count % 2 > 0) ? "" : "cornflower";

                    html += String.Format(@"<tr bgcolor='{0}'><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>",
                        background, call.Time.ToShortTimeString(), call.CustId, call.CustName, call.RemarkText);

                    count++;
                }

                if (count > 0)
                {
                    html += "<tr><td colspan=4>Total Calls: " + count + "</td></tr>";
                    html += "</table>";
                }
            }
            return html;
        }

        public bool OverLimit(CustSummary summary)
        {
            return summary.TotalBalance - summary.CreditLimit > 0;
        }

        public bool OverDue(CustSummary summary)
        {
            return summary.TotalLate > MinAllowedOverdue;
        }

    }
}
