using System.Text.RegularExpressions;
namespace LevenshteinDistanceAlgorithm;

public static class CustomValidations
{
    public static bool IsValidLedgerCode(string code) =>
        !string.IsNullOrWhiteSpace(code)
         && Regex.IsMatch(code, "(^[sS][a-zA-Z0-9]{2}[0-9]{3}$)|(^08[0-9]{4}$)|(^[1-9][0-9]{4}$)");

    public static bool IsValidEmail(string code) =>
        !string.IsNullOrWhiteSpace(code)
         && Regex.IsMatch(code, "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$");
    public static bool IsValidTelephone(string code) =>
        !string.IsNullOrWhiteSpace(code)
         && Regex.IsMatch(code, "(?:\\+?(\\d{1,3}))?[-. (]*(\\d{3})?[-. )]*(\\d{3})[-. ]*(\\d{4,5})(?: *x(\\d+))?");
    public static bool IsValidSupplierCode(string code) =>
        !string.IsNullOrWhiteSpace(code)
         && Regex.IsMatch(code, "^[sS][a-zA-Z0-9]{2}[0-9]{3}$");

    public static bool IsValidNonApCode(string code) =>
      !string.IsNullOrWhiteSpace(code)
       && Regex.IsMatch(code, "^[1-9][0-9]{4}$");

    public static bool IsValidItemCode(string code) =>
     !string.IsNullOrWhiteSpace(code)
      && Regex.IsMatch(code, "^(0[1-9]|[1-3][0-9]|(42|45))[0-9]{4}$");

    public static bool IsValidLeaseAccountCode(string code) =>
        !string.IsNullOrWhiteSpace(code)
         && Regex.IsMatch(code, "^08[0-9]{4}$");

    public static bool IsValidCashSaleNumber(string code) =>
   !string.IsNullOrWhiteSpace(code)
    && Regex.IsMatch(code, "^0*[1-9][0-9]{3,5}$");
}