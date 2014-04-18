using System;
using System.Data;
using MySql.Data.MySqlClient;

public class DbModel
{
    public delegate void ResultHandler(DataTable result, string error);

    public string Username { get; set; }
    public string Password { get; set; }
    public string Hostname { get; set; }
    public string Database { get; set; }
    public bool Pooling { get; set; }

    private MySqlConnection _con = null;

    public DbModel()
    {
        Username = "";
        Password = "";
        Hostname = "";
        Database = "";
        Pooling = false;
    }

    public string Execute(string sql, ResultHandler handler)
    {
        string result = null;
        DataTable dt = null;
        if ((result = Connect()) != null) {
            return result;
        }

        try {
            MySqlDataAdapter da = new MySqlDataAdapter(sql, _con);
            dt = new DataTable();
            da.Fill(dt);
            da.Dispose();
            da = null;
        } catch (Exception e) {
            result = e.Message;
        }
        handler(dt, result);
        return result;
    }

    private string Connect()
    {
        if (_con == null) {
            try {
                string connectionString =
                    "Server=" + Hostname + ";" +
                    "Database=" + Database + ";" +
                    "User ID=" + Username + ";" +
                    "Password=" + Password + ";" +
                    "Pooling=" + (Pooling ? "true" : "false");
                _con = new MySqlConnection(connectionString);
                _con.Open();
            } catch (Exception e) {
                return e.Message;
            }
        }
        return null;
    }
}
