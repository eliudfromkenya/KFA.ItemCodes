using KFA.ItemCodes.LevenshteinDistanceAlgorithm;
using KFA.ItemCodes.ViewModels;
using KFA.ItemCodes.Views;
using LevenshteinDistanceAlgorithm;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KFA.ItemCodes.Classes
{
    internal static class SupplierDbService
    {
        // internal static List<LevenshteinDistanceAlgorithm.SupplierCode> AllSupplierCodes; 
        public static string? user = null;
        private static string GetUserName()
        {
            var sql = $@"DROP TABLE IF EXISTS deviceNames;
CREATE TEMPORARY TABLE deviceNames AS 
SELECT DISTINCT device_name, device_code FROM tbl_data_devices;

-- SELECT (SELECT login_id FROM tbl_user_logins WHERE deviceNames.device_code = tbl_user_logins.device_id ORDER BY tbl_user_logins.from_date DESC LIMIT 1) FROM deviceNames;

DROP TABLE IF EXISTS XUserLogins;
CREATE TEMPORARY TABLE XUserLogins AS 
SELECT (SELECT login_id FROM tbl_user_logins WHERE deviceNames.device_code = tbl_user_logins.device_id ORDER BY tbl_user_logins.from_date DESC LIMIT 1) login_id FROM deviceNames;

SELECT
	tbl_system_users.name_of_the_user
	-- tbl_system_users.username, 
	-- tbl_data_devices.device_number, 
	-- tbl_data_devices.device_name, 
	-- tbl_data_devices.device_code, 
	-- tbl_data_devices.device_caption
FROM
	tbl_data_devices
	INNER JOIN
	tbl_user_logins
	ON 
		tbl_data_devices.device_id = tbl_user_logins.device_id
	INNER JOIN
	tbl_system_users
	ON 
		tbl_user_logins.user_id = tbl_system_users.user_id
	WHERE tbl_user_logins.login_id In (SELECT login_id FROM XUserLogins)
	AND device_name  = '{Environment.MachineName}';
	";
            var name = GetMySqlScalar(sql)?.ToString();
            if (string.IsNullOrWhiteSpace(name))
                name = Environment.MachineName;

            return name;
        }
        public static async Task SaveSupplier(string supplierCode,string supplierName, string telephone, string email, string address, Branch branch, bool isUpdate)
        {
            try
            {
                if (!CustomValidations.IsValidSupplierCode(supplierCode))
                    throw new Exception("Invalid supplier code");
                if (string.IsNullOrWhiteSpace(supplierName))
                    throw new Exception("Supplier name is required please");

                user ??= GetUserName();

                string sql;
                if (!isUpdate)
                {
                    var duplicate = MainSupplierWindowViewModel.models.FirstOrDefault(n => (n.Code?.StartsWith(n.Branch?.Prefix??"") ?? false) && n.Name.HarmonizeName()   ==  supplierName.HarmonizeName());
                    if (duplicate != null)
                    {
                        throw new Exception($"Supplier with the same name already exists: (Supplier {duplicate.Code}: {duplicate.Name})");
                    }

                    duplicate = MainSupplierWindowViewModel.models.FirstOrDefault(n => (n.Code?.StartsWith(n.Branch?.Prefix??"") ?? false) && Matcher.LaveteshinDistanceAlgorithmBody(n.Name.HarmonizeName() ?? "", supplierName.HarmonizeName() ?? "") == 0);
                    if (duplicate != null)
                    {
                        throw new Exception($"Supplier with the same name already exists: (Supplier {duplicate.Code}: {duplicate.Name})");
                    }

                    sql = @"SELECT
	tbl_ledger_accounts.ledger_account_code, 
	tbl_ledger_accounts.description
FROM
	tbl_ledger_accounts WHERE ledger_account_code LIKE CONCAT(@prefix,'%')
	AND description = @supplierName";
                    var duplicateSupplier = GetMySqlScalar(sql, new MySqlParameter("@prefix", supplierCode?[..3]), new MySqlParameter("@supplierName", supplierName))?.ToString();
                    if (!string.IsNullOrWhiteSpace(duplicateSupplier))
                        throw new Exception($"Supplier with the same name already exists: (Supplier Code {duplicateSupplier})");
                }

                if (isUpdate)
                {
                    ExecuteMySqlNonQuery($@"UPDATE tbl_ledger_accounts SET description = @supplierName, `date_updated`={DateTime.Now:yyyyMMdd}100756977, cost_centre_code = {branch.Code} WHERE ledger_account_code = '{supplierCode}';");

                    ExecuteMySqlNonQuery($@"UPDATE tbl_ledger_accounts SET description = @supplierName, `date_updated`={DateTime.Now:yyyyMMdd}100756977, cost_centre_code = {branch.Code} WHERE ledger_account_code = '{supplierCode}';");
                }
                else
                {
                    sql = $@"INSERT INTO `tbl_stock_suppliers`(ledger_account_id, 
    is_currently_enabled, 
	date_added, 
	date_updated, 
	originator_id,
    cost_centre_code, 
	description, 
	group_name, 
	ledger_account_code, 
	increase_with_debit, 
	main_group, 
	is_active 
	) VALUES('{supplierCode}',1, {DateTime.Now:yyyyMMdd}100756977, {DateTime.Now:yyyyMMdd}100756977, 30000000123, 1, '{branch.Code}', @supplierName, 'Suppliers', '{supplierCode}', 1, 'Stocks Suppliers', 1);";

                    ExecuteMySqlNonQuery(sql);

                    sql = $@"INSERT INTO `tbl_stock_suppliers`(supplier_id
	supplier_code, 
	telephone, 
	address, 
	email, 
	cost_centre_code, 
	description, 
	is_currently_enabled, 
	date_added, 
	date_updated, 
	originator_id, 
	) VALUES('{supplierCode}','{supplierCode}','{telephone}',@address, '{email}','{branch.Code}', @supplierName, 1, {DateTime.Now:yyyyMMdd}100756977, {DateTime.Now:yyyyMMdd}100756977, 30000000123);";

                    ExecuteMySqlNonQuery(sql);
                }

            }
            catch (Exception ex)
            {
                Functions.NotifyError(ex);
                throw;
            }
        }
        internal static  
            (List<SupplierCode> suppliers, List<Branch> groups) RefreshMySQLSuppliers()
        {
            MySqlConnection? con = null;
            try
            {
                con = new ConnectionObject().MySQLSubServerConnection;
                try
                {
                    con.Open();
                }
                catch (Exception)
                {
                    con = new ConnectionObject().MySQLDbConnection;
                }

                var sql = @"
DROP TABLE IF EXISTS supp_codes;
DROP TABLE IF EXISTS supp_ledgers;
DROP TABLE IF EXISTS not_existing_ledgers;

CREATE TEMPORARY TABLE supp_codes AS SELECT
	tbl_suppliers.supplier_code, 
	tbl_suppliers.description, 
	tbl_suppliers.telephone, 
	tbl_suppliers.email, 
	tbl_suppliers.address
FROM
	tbl_suppliers;
	
CREATE TEMPORARY TABLE supp_ledgers AS SELECT
	tbl_ledger_accounts.ledger_account_code, 
	tbl_ledger_accounts.description
FROM
	tbl_ledger_accounts;
	
CREATE TEMPORARY TABLE not_existing_ledgers AS SELECT
	tbl_ledger_accounts.ledger_account_code, 
	tbl_ledger_accounts.description
FROM
	tbl_ledger_accounts WHERE ledger_account_code NOT IN (SELECT supplier_code FROM supp_codes) ;
	
UPDATE supp_codes, supp_ledgers SET supp_codes.description = supp_ledgers.description
	WHERE supp_codes.supplier_code = supp_ledgers.ledger_account_code;
	
INSERT INTO supp_codes (supplier_code, description) SELECT ledger_account_code supplier_code, description FROM not_existing_ledgers; 

SELECT
	supplier_code, 
	description, 
	telephone, 
	email, 
	address
FROM
	supp_codes;
	































SELECT
	-- tbl_ledger_accounts.ledger_account_id, 
	tbl_ledger_accounts.ledger_account_code, 
	tbl_ledger_accounts.description supplier_name, 
	tbl_cost_centres.cost_centre_code, 
	tbl_cost_centres.description branch_name, 
	tbl_cost_centres.supplier_code_prefix
FROM
	tbl_ledger_accounts
	INNER JOIN
	tbl_cost_centres
	ON 
		SUBSTRING(tbl_ledger_accounts.ledger_account_code, 1, 3) = tbl_cost_centres.supplier_code_prefix;

SELECT
	tbl_cost_centres.cost_centre_code, 
	tbl_cost_centres.description, 
	tbl_cost_centres.supplier_code_prefix
FROM
	tbl_cost_centres WHERE tbl_cost_centres.supplier_code_prefix IS NOT NULL;";
                using var ds =  Functions.GetDbDataSet(con, sql);
                using var table= ds.Tables[0];
                var suppliers = table.AsEnumerable().Select(row => new { SupplierCode = row[0].ToString(), SupplierName = row[1].ToString(),  CostCentreCode = row[2].ToString(),  BranchName = row[3].ToString(),  Prefix = row[4].ToString() })
                    .OrderBy(c => !string.IsNullOrWhiteSpace(c.SupplierName))
                    .ThenBy(c => c.SupplierCode)
                    .ThenBy(c => c.SupplierCode?.Length)
                    .GroupBy(c => c.SupplierCode?.Trim())
                    .Select(m =>
                    {
                        var itm = m.FirstOrDefault();
                        return new SupplierCode
                        {
                            Code = m.Key,
                            Name = itm?.SupplierName,
                            OriginalName= itm?.SupplierName,
                            Branch =  new Branch
                            {
                                 BranchName= itm?.BranchName,
                                 Code=itm?.CostCentreCode,
                                 Prefix= itm?.Prefix
                            }
                        };
                    }).Where(m => !string.IsNullOrWhiteSpace(m.Name) && CustomValidations.IsValidSupplierCode(m?.Code)).ToList();

                using var table2 = ds.Tables[1];
                var groups = table2.AsEnumerable().Select(row => new Branch
                {
                    Code = row[0].ToString(),
                    BranchName = row[1].ToString(),
                    Prefix = row[2].ToString(),
                }).ToList();

                foreach (var supplier in suppliers)
                {
                    try
                    {
                        var branch = groups.FirstOrDefault(c => c.Prefix == supplier?.Code?[..3]);
                        if (branch != null)
                            supplier.Branch = branch;
                    }
                    catch { }
                }

                return (suppliers, groups);
            }
            catch (Exception ex)
            {
                Functions.NotifyError(ex);
            }
            finally
            {
                con?.Dispose();
            }
            return new();
        }
 
        public static DataSet GetDb2DataSet(string sql, params IDbDataParameter[] parameters)
        {
            var con = new ConnectionObject().MaliplusConnection;
            if (con.State != ConnectionState.Open)
                con.Open();

             return Functions.GetDbDataSet(con, sql);
        }

        public static object GetDb2Scalar(string sql, params IDbDataParameter[] parameters)
        {
            var con = new ConnectionObject().MaliplusConnection;
            if (con.State != ConnectionState.Open)
                con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = sql;

            if (parameters != null)
                foreach (var par in parameters)
                    cmd.Parameters.Add(par);

            return cmd.ExecuteScalar();
        }

        public static int ExecuteDb2NonQuery(string sql, params IDbDataParameter[] parameters)
        {
            var con = new ConnectionObject().MaliplusConnection;            
            if (con.State != ConnectionState.Open)
                con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = sql;

            if (parameters != null)
                foreach (var par in parameters)
                    cmd.Parameters.Add(par);

            return cmd.ExecuteNonQuery();
        }

        public static DataSet GetMySqlDataSet(string sql, params IDbDataParameter[] parameters)
        {
            var con = new ConnectionObject().MySQLSubServerConnection;
            try
            {
                con.Open();
            }
            catch (Exception)
            {
                con = new ConnectionObject().MySQLDbConnection;
            }

            if (con.State != ConnectionState.Open)
                con.Open();

            return Functions.GetDbDataSet(con, sql);
        }

        public static object? GetMySqlScalar(string sql, params IDbDataParameter[] parameters)
        {
            var con = new ConnectionObject().MySQLSubServerConnection;
            try
            {
                con.Open();
            }
            catch (Exception)
            {
                con = new ConnectionObject().MySQLDbConnection;
            }

            if (con.State != ConnectionState.Open)
                con.Open();

            using var cmd = con.CreateCommand();

            cmd.CommandText = sql;

            if (parameters != null)
                foreach (var par in parameters)
                    cmd.Parameters.Add(par);

            return cmd.ExecuteScalar();
        }

        public static int ExecuteMySqlNonQuery(string sql, params IDbDataParameter[] parameters)
        {
            var con = new ConnectionObject().MySQLSubServerConnection;
            try
            {
                con.Open();
            }
            catch (Exception)
            {
                con = new ConnectionObject().MySQLDbConnection;
            }

            if (con.State != ConnectionState.Open)
                con.Open();

            using var cmd = con.CreateCommand();
            cmd.CommandText = sql;

            if (parameters != null)
                foreach (var par in parameters)
                    cmd.Parameters.Add(par);

            return cmd.ExecuteNonQuery();
        }
    }
}
