

using LevenshteinDistanceAlgorithm;
using MySqlConnector;
using System;

public class ConnectionObject
{

    private DBModel maliplusDbModel = new() { Database = "KfaMain", Host = "192.168.1.239", Password = "HQisd2022", Port = "52232", Schema = "KFALTD", User = "kfaltd" };
    private DBModel zanasDbModel = new() { Database = "MOISB", Host = "192.168.1.10", Password = "Pa55word", Port = "50000", Schema = "ZANAS", User = "maliplus" };
    private string? message;
    private string? messageError;
    private string? messageTitle;
    public DBModel MaliplusDbModel { get => maliplusDbModel; set => maliplusDbModel = value; }
    public DBModel ZanasDbModel { get => zanasDbModel; set => zanasDbModel = value; }

    //public IBM.Data.DB2.Core.DB2Connection MaliplusConnection => new IBM.Data.DB2.Core.DB2Connection($"Server={MaliplusDbModel.Host}:{MaliplusDbModel.Port};Database={MaliplusDbModel.Database};UID={MaliplusDbModel.User};PWD={MaliplusDbModel.Password};");
    //public IBM.Data.DB2.Core.DB2Connection ZanasConnection => new IBM.Data.DB2.Core.DB2Connection($"Server={ZanasDbModel.Host}:{ZanasDbModel.Port};Database={ZanasDbModel.Database};UID={ZanasDbModel.User};PWD={ZanasDbModel.Password};");


    private DBModel mySQLSubServerDbModel = new() { Database = "kfa_sub_systems", Host = "192.168.1.239", Password = "isd@KFA2023#", Port = "3306", Schema = "KFALTD", User = "remoteuser" };
    private DBModel mySQLSubServerDbModel2 = new() { Database = "kfa_sub_systems", Host = "192.168.1.240", Password = "*654321.0kfa#", Port = "50000", Schema = "kfa_sub_systems", User = "remote_user" };
    private DBModel mySQLDbModel = new() { Database = "kfa_sub_systems", Host = "127.0.0.1", Password = "654321", Port = "3306", Schema = "ZANAS", User = "root" };

    public DBModel MySQLSubServerDbModel { get => mySQLSubServerDbModel; set => mySQLSubServerDbModel = value; }
    public DBModel MySQLSubServerDbModel2 { get => mySQLSubServerDbModel2; set => mySQLSubServerDbModel2 = value; }
    public DBModel MySQLDbModel { get => mySQLDbModel; set => mySQLDbModel = value; }

    MySqlConnection MySQLSubServerConnection => new($@"server={MySQLSubServerDbModel.Host};port={MySQLSubServerDbModel.Port};database={MySQLSubServerDbModel.Database};user={MySQLSubServerDbModel.User};password={MySQLSubServerDbModel.Password};ConvertZeroDateTime=True;AllowUserVariables=True;");
   MySqlConnection MySQLSubServerConnection2 => new($@"server={MySQLSubServerDbModel2.Host};port={MySQLSubServerDbModel2.Port};database={MySQLSubServerDbModel2.Database};user={MySQLSubServerDbModel2.User};password={MySQLSubServerDbModel2.Password};ConvertZeroDateTime=True;AllowUserVariables=True;");
//MySqlConnection MySQLDbConnection => new($@"server={MySQLDbModel.Host};port={MySQLDbModel.Port};database={MySQLDbModel.Database};user={MySQLDbModel.User};password={MySQLDbModel.Password};ConvertZeroDateTime=True;AllowUserVariables=True;");


    public MySqlConnection MySQLConnection
    {
        get
        {
            MySqlConnection? con = new ConnectionObject().MySQLSubServerConnection;
            try
            {
                con.Open();
            }
            catch (Exception ex)
            {
                con = new ConnectionObject().MySQLSubServerConnection2;
            }
            return con; ;
        }
    }
}