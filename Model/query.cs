using System;
using System.Data;
using System.Collections.Generic;

public class Query
{
    public String query{ get; set; }
    public String name { get; set; }

    public Query(String name, String query)
    {
        this.name = name;
        this.query = query;
    }


    public override String ToString()
    {
        return name;
    }

    public static List<Query> CreateQueries()
    {
        List<Query> QueryList = new List<Query>();

        QueryList.Add(new Query("Select a query...",
            ""));
        QueryList.Add(new Query("Vendor parts w/ mfg and vendor",
            "SELECT * FROM Vendor_parts AS VP INNER JOIN Manufacturers AS M ON VP.Manufacturer_id = M.Manufacturer_id INNER JOIN Vendors AS V ON VP.Vendor_id = V.Vendor_id;"));
        QueryList.Add(new Query("Manufacturers and parts",
            "SELECT * FROM Vendor_parts AS VP RIGHT OUTER JOIN Manufacturers AS M ON VP.Manufacturer_id = M.Manufacturer_id;"));
        QueryList.Add(new Query("Parts w/ type attributes",
            "SELECT * FROM Parts AS P NATURAL LEFT JOIN Capacitor_attributes AS A NATURAL LEFT JOIN Part_types AS T WHERE T.Type = 'Capacitor'"));
        QueryList.Add(new Query("Number of each type of part",
            "SELECT Type, COUNT(Part_num) FROM Parts NATURAL JOIN Part_types GROUP BY Part_type_id"));
        QueryList.Add(new Query("A list of all Manufacturers",
            "SELECT Name FROM Manufacturers"));
        QueryList.Add(new Query("A list of all Vendors",
            "SELECT Name FROM Vendors"));
        QueryList.Add(new Query("A list of what Vendors a single Manufacturer",
            "SELECT DISTINCT M.Name as Manufacturer, V.Name as Vendor FROM Manufacturers as M JOIN Vendor_parts JOIN Vendors as V Where M.Name = 'AVX'"));
        QueryList.Add(new Query("Find parts with no vendor supplier", 
            "SELECT Part_num, Type, Description FROM Parts AS P NATURAL JOIN Part_types WHERE P.Part_num NOT IN (SELECT Part_num FROM Vendor_parts) ORDER BY Part_num, Part_type_id"));
        QueryList.Add(new Query("Find parts with no pricing information",
            "SELECT Part_num, Type, Description FROM Parts AS P NATURAL JOIN Part_types WHERE P.Part_num NOT IN (SELECT Part_num FROM Vendor_price_breaks) ORDER BY Part_num, Part_type_id"));
        QueryList.Add(new Query("Find parts with no matching tuple in extended attributes", 
            " SELECT Part_num, Type, Description FROM Parts AS P NATURAL JOIN Part_types WHERE P.Part_num NOT IN ( SELECT Part_num FROM Capacitor_attributes UNION SELECT Part_num FROM Connector_attributes UNION SELECT Part_num FROM Memory_attributes ) ORDER BY Part_num, Part_type_id; "));


        return QueryList;
    }
}
