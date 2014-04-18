using System;
using System.Data;

public class PreferencesModel
{
    public event EventHandler Changed;

    public string Filename { get; set; }
    public DataTable CurrentPrefs {
        get
        {
            return prefs;
        }
    }

    private DataTable prefs = null;

    public PreferencesModel(string filename)
    {
        Filename = filename;
        setPrefs(Defaults());
    }

    private void setPrefs(DataTable prefs)
    {
        this.prefs = prefs;
        if (Changed != null) {
            Changed(this, new EventArgs());
        }
    }

    public DataTable Load()
    {
        DataTable dt = new DataTable();
        try {
            dt.ReadXml(Filename);
        } catch(Exception) {
            dt.Dispose();
            dt = Defaults();
        }
        setPrefs(dt);
        return dt;
    }

    public DataTable Defaults()
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("Name", Type.GetType("System.String"));
        dt.Columns.Add("Value", Type.GetType("System.String"));
        dt.Rows.Add("Username", "");
        dt.Rows.Add("Password", "");
        dt.Rows.Add("Hostname", "");
        dt.Rows.Add("Database", "");
        dt.Rows.Add("Pooling", false);
        return dt;
    }

    public void Save(DataTable prefs)
    {
        //TODO: The output from this looks like crap... but it works as required.
        prefs.WriteXml(Filename, XmlWriteMode.WriteSchema);
    }
}
