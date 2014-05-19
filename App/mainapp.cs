using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

public class MainApp : Form
{
    private PartsDb dbm = null;
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
        //dbm = new MySqlPartsDb();
        dbm = new SqlPartsDb();
        mv = new MainView();
        pm.Changed += new EventHandler(PrefsChanged);
        mv.PrefsView.OnSavePrefs += pm.Save;
        mv.RawSqlView.OnSQLExecute += SQLExecute;
        mv.PartsView.OnShowParts += ShowParts;
        mv.PartsView.OnEditPart += EditPart;
        mv.PartsView.OnNewPart += NewPart;
        mv.PartsView.OnDeletePart += DeletePart;
        mv.PartsView.OnRepairMissingAttributes += RepairMissingAttributes;
        mv.PartEditView.OnSavePart += SavePart;

        pm.Load();

        dbm.GetPartTypes(delegate(List<PartType> partTypes) {
            mv.PartsView.PartTypes = partTypes;
        }, null);
    }

    private void PrefsChanged(object sender, EventArgs e)
    {
        DataTable dt = pm.CurrentPrefs;
        mv.PrefsView.DataSource = dt;
        dbm.Username = Util.GetColumnString(dt, "Username", "");
        dbm.Password = Util.GetColumnString(dt, "Password", "");
        dbm.Hostname = Util.GetColumnString(dt, "Hostname", "");
        dbm.Database = Util.GetColumnString(dt, "Database", "");
        dbm.GetPartTypes(delegate(List<PartType> partTypes) {
            mv.PartsView.PartTypes = partTypes;
        }, null);
    }

    private void SQLExecute(string sql)
    {
        dbm.Execute(sql, SQLResult, SQLError);
    }

    private void SQLResult(DataTable result)
    {
        mv.RawSqlView.DataSource = result;
    }

    private void SQLError(string message, Exception exception)
    {
        mv.RawSqlView.DataSource = null;
        mv.RawSqlView.Message = message;
        mv.RawSqlView.OperationFailed = (exception != null);
    }

    private void ShowParts(PartType partType)
    {
        dbm.GetParts(partType, delegate(PartCollection parts) {
            mv.PartsView.Parts = parts;
        }, null);
    }

    private void EditPart(DataRow row)
    {
        dbm.GetPart((string)row["Part_num"], delegate(Part part) {
            if (part != null) {
                mv.PartEditView.Part = part;
                mv.ActiveTab = MainView.Tabs.Edit;
            }
        }, null);
    }

    private void NewPart(PartType partType)
    {
        dbm.NewPart(partType, null);
        ShowParts(mv.PartsView.SelectedPartType);
    }

    private void SavePart(Part part)
    {
        dbm.UpdatePart(part, null);
        mv.ActiveTab = MainView.Tabs.Parts;
        ShowParts(mv.PartsView.SelectedPartType);
    }

    private void DeletePart(DataRow row)
    {
        //((MySqlPartsDb)dbm).DeletePart((string)row["Part_num"], null);
        //ShowParts(mv.PartsView.SelectedPartType);
    }

    private void RepairMissingAttributes()
    {
        //((MySqlPartsDb)dbm).RepairMissingAttributes();
    }
}
