using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

public class MainApp : Form
{
    private DbModel dbm = null;
    private PreferencesModel pm = null;
    private MainView mv = null;

    private string prefsPath = "preferences.xml";

    public static void Main()
    {
        //TODO: Read settings path from arg or env?
        MainApp ma = new MainApp();
        Application.EnableVisualStyles();
        Application.Run(ma.mv);
    }

    public MainApp()
    {
        pm = new PreferencesModel(prefsPath);
        dbm = new DbModel();
        mv = new MainView();
        pm.Changed += new EventHandler(PrefsChanged);
        mv.PrefsView.OnSavePrefs += pm.Save;
        mv.RawSqlView.OnSQLExecute += SQLExecute;
        mv.PartsView.PartTypes = dbm.PartTypes();

        pm.Load();
    }

    private void PrefsChanged(object sender, EventArgs e)
    {
        DataTable dt = pm.CurrentPrefs;
        mv.PrefsView.DataSource = dt;
        dbm.Username = Util.GetColumnString(dt, "Username", "");
        dbm.Password = Util.GetColumnString(dt, "Password", "");
        dbm.Hostname = Util.GetColumnString(dt, "Hostname", "");
        dbm.Database = Util.GetColumnString(dt, "Database", "");
        dbm.Pooling = Util.GetColumnBool(dt, "Pooling", false); 
    }

    private void SQLExecute(string sql)
    {
        string result = null;
        result = dbm.Execute(sql, SQLResult);
        if (result != null) {
            mv.RawSqlView.DataSource = null;
            mv.RawSqlView.Message = result;
            mv.RawSqlView.OperationFailed = true;
        }
    }

    private void SQLResult(DataTable result, string message, Exception exception)
    {
        mv.RawSqlView.DataSource = result;
        mv.RawSqlView.Message = message;
        mv.RawSqlView.OperationFailed = (exception != null);
    }
}
