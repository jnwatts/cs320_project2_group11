using System;
using System.Data;
using System.Collections.Generic;

public class QueryEntry
{
    public String query{ get; set; }
    public String name { get; set; }

    public QueryEntry(String name, String query)
    {
        this.name = name;
        this.query = query;
    }


    public override String ToString()
    {
        return name;
    }

    public static List<QueryEntry> CreateQueries()
    {
        List<QueryEntry> QueryList = new List<QueryEntry>();

        QueryList.Add(new QueryEntry("Select a query...",""));
        QueryList.Add(new QueryEntry("Vendor parts w/ mfg and vendor",
            "SELECT * FROM Vendor_parts AS VP INNER JOIN Manufacturers AS M ON VP.Manufacturer_id = M.Manufacturer_id INNER JOIN Vendors AS V ON VP.Vendor_id = V.Vendor_id;"));
        QueryList.Add(new QueryEntry("Manufacturers and parts","SELECT * FROM Vendor_parts AS VP RIGHT OUTER JOIN Manufacturers AS M ON VP.Manufacturer_id = M.Manufacturer_id;"));
        QueryList.Add(new QueryEntry("Parts w/ type attributes", "SELECT * FROM Parts AS P NATURAL LEFT JOIN Capacitor_attributes AS A NATURAL LEFT JOIN Part_types AS T WHERE T.Type = 'Capacitor'"));

        return QueryList;
    }
}
