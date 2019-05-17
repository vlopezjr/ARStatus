using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ARStatus
{
    internal static class DataAccess
    {
        internal static List<CustSummary> GetCustStatusAll()
        {
            List<CustSummary> summaries = new List<CustSummary>();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["sage"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "spOParAllCustStatusFull";
                cmd.CommandType = CommandType.StoredProcedure;
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    CustSummary summary = new CustSummary();

                    summary.CustKey = (int)dr["CustKey"];
                    summary.CustId = dr["CustId"].ToString().TrimEnd();
                    summary.CustName = dr["CustName"].ToString().TrimEnd();
                    summary.CustClassId = dr["CustClassId"].ToString().TrimEnd();
                    summary.Collector = dr["Collector"].ToString();
                    summary.HoldStatusKey = dr["HoldStatusKey"] == DBNull.Value ? (AccountStatus?)null : (AccountStatus)dr["HoldStatusKey"];
                    summary.TotalBalance = Convert.ToDecimal(dr["TotalBalance"]);
                    summary.Balance45 = Convert.ToDecimal(dr["Balance45"]);
                    summary.Balance60 = Convert.ToDecimal(dr["Balance60"]);
                    summary.Balance90 = Convert.ToDecimal(dr["Balance90"]);
                    summary.TotalLate = Convert.ToDecimal(dr["TotalLate"]);
                    summary.CreditLimit = Convert.ToDecimal(dr["CreditLimit"]);
                    summary.PmtTermsId = dr["PmtTermsId"].ToString().TrimEnd();
                    summary.LastPmtAmt = dr["LastPmtAmt"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["LastPmtAmt"]);
                    summary.LastPmtDate = dr["LastPmtDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["LastPmtDate"]);
                    summary.Name = dr["Name"].ToString();
                    summary.Phone = dr["Phone"].ToString();
                    summary.PhoneExt = dr["PhoneExt"] == DBNull.Value ? null : dr["PhoneExt"].ToString();
                    summary.Status = (short)dr["Status"];
                    summary.AccountType = dr["AccountType"].ToString();

                    summaries.Add(summary);
                }
                return summaries;
            }
        }


        internal static CustSummary GetCustStatus(int custKey)
        {
            CustSummary summary = new CustSummary();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["sage"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "spOParCustStatusFull";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@CustKey", custKey));

                var dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    summary.CustKey = (int)dr["CustKey"];
                    summary.CustId = dr["CustId"].ToString().TrimEnd();
                    summary.CustName = dr["CustName"].ToString().TrimEnd();
                    summary.CustClassId = dr["CustClassId"].ToString().TrimEnd();
                    summary.Collector = dr["Collector"].ToString();
                    summary.HoldStatusKey = dr["HoldStatusKey"] == DBNull.Value ? (AccountStatus?)null : (AccountStatus)dr["HoldStatusKey"];
                    summary.HoldStatusId = dr["HoldStatusId"] == DBNull.Value ? "Good" : dr["HoldStatusId"].ToString();
                    summary.AgingDate = dr["AgingDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["AgingDate"]);
                    summary.TotalBalance = Convert.ToDecimal(dr["TotalBalance"]);
                    summary.Balance45 = Convert.ToDecimal(dr["Balance45"]);
                    summary.Balance60 = Convert.ToDecimal(dr["Balance60"]);
                    summary.Balance90 = Convert.ToDecimal(dr["Balance90"]);
                    summary.TotalLate = Convert.ToDecimal(dr["TotalLate"]);
                    summary.CreditLimit = Convert.ToDecimal(dr["CreditLimit"]);
                    summary.PmtTermsId = dr["PmtTermsId"].ToString().TrimEnd();
                    summary.LastPmtAmt = dr["LastPmtAmt"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["LastPmtAmt"]);
                    summary.LastPmtDate = dr["LastPmtDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["LastPmtDate"]);
                    summary.Name = dr["Name"].ToString();
                    summary.Phone = dr["Phone"].ToString();
                    summary.PhoneExt = dr["PhoneExt"] == DBNull.Value ? null : dr["PhoneExt"].ToString();
                    summary.Status = (short)dr["Status"];
                    summary.AccountType = dr["AccountType"].ToString();
                }
                return summary;
            }
        }


        internal static void InsertCustomerOnHold(CustSummary summary, AccountStatus newStatus)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["sage"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("spOPInsertARStatus", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CustKey", summary.CustKey);
                cmd.Parameters.AddWithValue("@HoldStatusKey", newStatus);
                cmd.Parameters.AddWithValue("@OnHoldDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                cmd.Parameters.AddWithValue("@Balance45", summary.Balance45);
                cmd.Parameters.AddWithValue("@Balance60", summary.Balance60);
                cmd.Parameters.AddWithValue("@Balance90", summary.Balance90);
                cmd.Parameters.AddWithValue("@TotalBalance", summary.TotalBalance);
                if (summary.LastPmtDate == null)
                    cmd.Parameters.AddWithValue("@LastPmtDate", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@LastPmtDate", summary.LastPmtDate);
                cmd.Parameters.AddWithValue("@CreditLimit", summary.CreditLimit);
                cmd.Parameters.AddWithValue("@Collector", summary.Collector);
                cmd.Parameters.AddWithValue("@CustID", summary.CustId);
                cmd.Parameters.AddWithValue("@CustName", summary.CustName);
                cmd.Parameters.AddWithValue("@PmtTerms", summary.PmtTermsId);
                if (summary.LastPmtAmt == null)
                    cmd.Parameters.AddWithValue("@LastPmtAmt", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@LastPmtAmt", summary.LastPmtAmt);
                cmd.Parameters.AddWithValue("@ContactName", summary.Name);
                cmd.Parameters.AddWithValue("@ContactPhone", summary.Phone);
                if (summary.PhoneExt == null)
                    cmd.Parameters.AddWithValue("@ContactPhoneExt", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@ContactPhoneExt", summary.PhoneExt);
                cmd.Parameters.AddWithValue("@CustType", summary.CustClassId);
                cmd.ExecuteNonQuery();
            }
        }


        internal static void UpdateCustomerOnHold(CustSummary summary, AccountStatus newStatus)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["sage"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("spOPUpdateARStatus", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CustKey", summary.CustKey);
                cmd.Parameters.AddWithValue("@HoldStatusKey", newStatus);
                cmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
                cmd.Parameters.AddWithValue("@Balance45", summary.Balance45);
                cmd.Parameters.AddWithValue("@Balance60", summary.Balance60);
                cmd.Parameters.AddWithValue("@Balance90", summary.Balance90);
                cmd.Parameters.AddWithValue("@TotalBalance", summary.TotalBalance);

                if (summary.LastPmtDate == null)
                    cmd.Parameters.AddWithValue("@LastPmtDate", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@LastPmtDate", summary.LastPmtDate);

                cmd.Parameters.AddWithValue("@CreditLimit", summary.CreditLimit);
                cmd.Parameters.AddWithValue("@Collector", summary.Collector);
                cmd.Parameters.AddWithValue("@CustID", summary.CustId);
                cmd.Parameters.AddWithValue("@CustName", summary.CustName);
                cmd.Parameters.AddWithValue("@PmtTerms", summary.PmtTermsId);

                if (summary.LastPmtAmt == null)
                    cmd.Parameters.AddWithValue("@LastPmtAmt", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@LastPmtAmt", summary.LastPmtAmt);

                cmd.Parameters.AddWithValue("@ContactName", summary.Name);
                cmd.Parameters.AddWithValue("@ContactPhone", summary.Phone);

                if (summary.PhoneExt == null)
                    cmd.Parameters.AddWithValue("@ContactPhoneExt", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@ContactPhoneExt", summary.PhoneExt);

                cmd.Parameters.AddWithValue("@CustType", summary.CustClassId);
                cmd.ExecuteNonQuery();
            }
        }


        internal static void DeleteCustomerOnHold(CustSummary summary)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["sage"].ConnectionString))
            {
                string sql = String.Format(@"DELETE tcpCustHold WHERE CustKey={0}", summary.CustKey);
                con.Open();
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }


        internal static bool HasLettersSent(CustSummary summary)
        {
            int retval;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["sage"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("spOPARStatusTestLetterFlags", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CustKey", SqlDbType.Int).Value = summary.CustKey;
                SqlParameter outputParam = cmd.Parameters.Add("@FlagCount", SqlDbType.Int);
                outputParam.Direction = ParameterDirection.Output;
                cmd.ExecuteScalar();
                retval = (int)cmd.Parameters["@FlagCount"].Value;
            }
            return retval == 0 ? false : true;
        }


        internal static void ClearLetterFlags(CustSummary summary)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["sage"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("spOPARStatusClearLetterFlags", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CustKey", summary.CustKey);
                cmd.ExecuteNonQuery();
            }
        }


        internal static List<CallingData> GetCallingData(string start, string end)
        {
            List<CallingData> calls = new List<CallingData>();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["sage"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("spCPCPamsCallingData", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@StartDate", start);
                cmd.Parameters.AddWithValue("@EndDate", end);
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    CallingData call = new CallingData();
                    call.Collector = dr["Collector"].ToString();
                    call.Time = Convert.ToDateTime(dr["Time"]);
                    call.CustId = dr["CustID"].ToString();
                    call.CustName = dr["CustName"].ToString();
                    call.RemarkText = dr["MemoText"].ToString();
                    calls.Add(call);
                }
            }
            return calls;
        }


        internal static void LogSummary(
            decimal _totalOwed,
            decimal _totalOver45,
            decimal _totalOver60,
            decimal _totalOver90,
            int _totalOnHold,
            int _totalNewHold,
            int _totalOffHold)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["sage"].ConnectionString))
            {
                string sql = String.Format(@"insert tcpARSummaryLog
                    (DateStamp, TotalOwed, Balance45, Balance60, Balance90, OnHold, NewHold, OffHold)
                    VALUES('{0}', {1}, {2}, {3}, {4}, {5}, {6}, {7})", DateTime.Now,
                    _totalOwed, _totalOver45, _totalOver60, _totalOver90, _totalOnHold, _totalNewHold, _totalOffHold);
                con.Open();
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }


        internal static void LogStatusChange(int custKey, AccountStatus prevStatus, AccountStatus newStatus)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["sage"].ConnectionString))
            {
                string sql = String.Format(@"INSERT tcpARStatusLog (DateStamp, CustKey, PrevStatus, NewStatus)
                    VALUES ('{0}', {1}, {2}, {3})", DateTime.Now, custKey, (int)prevStatus, (int)newStatus);
                con.Open();
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
