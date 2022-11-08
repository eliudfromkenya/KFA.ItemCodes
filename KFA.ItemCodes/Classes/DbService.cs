using LevenshteinDistanceAlgorithm;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFA.ItemCodes.Classes
{
    internal static class DbService
    {
        // internal static List<LevenshteinDistanceAlgorithm.ItemCode> AllItemCodes; 

        public static async Task SaveItem(string itemCode,string itemName, string supplier)
        {
            try
            {
                if (!CustomValidations.IsValidItemCode(itemCode))
                    throw new Exception("Invalid item code");
                if (string.IsNullOrWhiteSpace(itemName))
                    throw new Exception("Item name is required please");

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
                cmd.CommandText = $"INSERT INTO `tbl_stock_items`(`date_added`, `date_updated`, `originator_id`, `is_currently_enabled`, `item_code`, `distributor`, `group_id`, `item_name`, `is_active`) VALUES ({DateTime.Now.ToString("yyyyMMdd")}100756977, {DateTime.Now.ToString("yyyyMMdd")}100756977, 30000000123, 1, '{itemCode}', '{supplier.Replace("'", "''")}','{itemCode.Substring(0,2)}', '{itemName.Replace("'", "''")}', 1);";
                    cmd.ExecuteNonQuery();


                var sql = @"INSERT INTO item_master(ITEM_CODE, ITEM_TYPE, ITEM_NAME, BARCODE, ITEM_GROUP, SUB_GROUP, PACK_TYPE, IUOM, SUOM, PUOM, WUOM, ITEM_ACCOUNT, SUPPLIER, MANUFACTURER, BUY_PRICE, PROFIT_MARGIN, SALE_PRICE, WHOLE_PRICE, STOCK_PRICE, DISCOUNT_RATE, TAXABLE, TAX_RATE, TAX_GROUP, PRICE_GROUP, DESCRIPTION, LEVY_RATE1, LEVY_RATE2, MAX_STOCK, REORDER_LEVEL, CONVERSION, P_CONVERSION, S_CONVERSION, W_CONVERSION, SHELF_LIFE, ON_PROMOTION, PARENT_ITEM, ITEM_STATUS, BARCODE_TYPE, CATEGORY1, CATEGORY2, OPTION1, OPTION2, PACKAGE, UNIT_MEASURE, MEASURE_TYPE, BASE_MEASURE, PART_NUMBER, OEM_NUMBER, DEBIT_CHECK, DATA_OPTION, STOCK_PARENT, STOCK_RULE, STOCK_CODE, STOCK_CODE1, STOCK_CODE2, STOCK_CODE3, CHECK_OPTION1, CHECK_OPTION2, CHECK_OPTION3, CHECK_OPTION4, VALUE_OPTION1, VALUE_OPTION2, VALUE_OPTION3, VALUE_OPTION4, VALUE_RATE1, VALUE_RATE2, START_DATE, EXPIRE_DATE, VALUE_DATE, PROMOTION_TYPE, CHECK_CATEGORY, CONFIGURATION, DATE_CREATED, CREATED_BY, DATE_MODIFIED, MODIFIED_BY, EXPORT_INDICATOR, DATE_EXPORTED, EXPORTED_BY, PURCHASE_ACCOUNT, INVENTORY_ACCOUNT, CASH_ACCOUNT, BARCODE1, BARCODE2, BARCODE_TYPE1, BARCODE_TYPE2, SYNC_STATUS, TRADE_PRICE, TRADE_PRICE_TRIGGER, HAS_LEVIES, USE_DEFAULT_LOCATION, DEFAULT_LOCATION, DEFAULT_PACK, SALES_ACCOUNT, COS_ACCOUNT, GAIN_ACCOUNT, SALEABLE, PURCHASEABLE, PRODUCED, CONVERSION_OPTION, HAS_HEADERS, ITEM_CLASS, ECOMMERCE_SHOW, APPLIED_MODELS, APPLIED_ENGINES, ALTERNATE_PART_NUMBERS, BVP_PART_NUMBERS, TPROFIT_MARGIN, WPROFIT_MARGIN, SPROFIT_MARGIN) VALUES ('020028', 'STK', 'LARGE BAGS', NULL, 'S/H', 'GEN', 'EAC', 'EAC', 'EAC', 'EAC', 'EAC', '600100', NULL, NULL, 0.00, 0.000000, 0.00, 0.00, 0.00, 0.000, 'N', 16.000, 'NON', NULL, NULL, 0.000, 0.000, 0.000, 0.000, 1.00000000, 1.00000000, 1.00000000, 0.00, 0.00, 'N', NULL, 'ACT', 'EAN', NULL, NULL, 'N', 'N', NULL, 0.000, 'KG', 0.00000000, NULL, NULL, 'N', 'N', 'N', '*', NULL, NULL, NULL, NULL, 'N', 'N', 'N', 'N', 0.00, 0.00, 0.00, 0.00, 0.00000000, 0.00000000, NULL, NULL, NULL, '*', '*', 'CONFIGURATION', '2022-07-23 12:44:41', NULL, '2022-07-28 15:46:57', NULL, 'N', NULL, NULL, '710100', '220100', NULL, NULL, NULL, NULL, NULL, 'ADD', 0.00, 0.00, 'N', 'N', 'NON', 'EAC', '600100', '700100', '220300', 'Y', 'Y', 'N', 'A', 'N', 'NRM', NULL, NULL, NULL, NULL, NULL, 0.000000, 0.000000, 0.000000);";

            }
            catch (Exception ex)
            {

                Functions.NotifyError(ex);
                throw;
            }
        }
        internal static  List<LevenshteinDistanceAlgorithm.ItemCode> RefreshMySQLItems()
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

                var sql = @"SELECT
	tbl_stock_items.item_code, 
	tbl_stock_items.item_name, 
	tbl_stock_items.is_active,
    tbl_stock_items.distributor
FROM
	tbl_stock_items
ORDER BY item_code";
               using var ds =  Functions.GetDbDataSet(con, sql);
               using var table= ds.Tables[0];
                var items = table.AsEnumerable().Select(row => new { ItemCode = row[0].ToString(), ItemName = row[1].ToString(), IsActive = bool.TryParse(row[2].ToString(), out bool nn) && nn, Distributor = row[3].ToString() })
                    .OrderBy(c => !string.IsNullOrWhiteSpace(c.ItemName))
                    .ThenBy(c => c.IsActive)
                    .ThenBy(c => c.ItemCode?.Length)
                    .GroupBy(c => c.ItemCode?.Trim())
                    .Select(m =>
                    {
                        var itm = m.FirstOrDefault();
                        return new ItemCode
                        {
                            Code = m.Key,
                            Name = itm?.ItemName,
                            Distributor = itm?.Distributor,
                            IsVerified = itm?.IsActive,
                            ItemGroup =  itm?.ItemCode?.Length > 2? itm?.ItemCode?[..2] :""
                        };
                    }).Where(m => !string.IsNullOrWhiteSpace(m.Name) && CustomValidations.IsValidItemCode(m?.Code)).ToList();

                Matcher.CheckCodes(ref items);
                return items;
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
    }
}
