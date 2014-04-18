using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

public class MainApp : Form
{
    private MainView mv = null;
    private string connectionString = "";

    public static void Main()
    {
        MainApp ma = new MainApp();
        Application.Run(ma.mv);
    }

    public MainApp()
    {
        mv = new MainView();
        mv.OnSQLConnect += dbconnect;
        mv.OnSQLExecute += dbexec;
        //mv.Show();
    }

    private void dbconnect(string connectionString)
    {
        connectionString =
            "Server=localhost;" +
            "Database=cs320_project2;" +
            "User ID=cs320;" +
            "Password=aiN3xei3;" +
            "Pooling=false";
    }

    private void dbexec(string command)
    {
        try {
            MySqlConnection dbcon;
            dbcon = new MySqlConnection(connectionString);
            dbcon.Open();
            MySqlDataAdapter da = new MySqlDataAdapter(command, dbcon);
            DataTable dt = new DataTable();
            da.Fill(dt);
            mv.DataSource = dt;
            dbcon.Close();
            dbcon = null;
        } catch(Exception e) {
            mv.Error = e.ToString();
        }
    }

}
