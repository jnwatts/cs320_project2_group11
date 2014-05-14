using System;
using System.Data;
using System.Collections.Generic;

public delegate void ErrorHandler(string message, Exception exception);

public delegate void PartTypesHandler(List<PartType> partTypes);
public delegate void PartsHandler(PartCollection part);
public delegate void PartHandler(Part part);

public abstract class PartsDb {
    public string Username { get; set; }
    public string Password { get; set; }
    public string Hostname { get; set; }
    public string Database { get; set; }

    public abstract void GetPartTypes(PartTypesHandler partTypeHandler, ErrorHandler errorHandler);

    public abstract void GetPart(string Part_num, PartHandler partHandler, ErrorHandler errorHandler);
    public abstract void GetParts(PartType partType, PartsHandler partsHandler, ErrorHandler errorHandler);
    //TODO: This should be based off a new part. The *controller* should figure out how to make the new part, the DB should only dutifully insert it.
    public abstract void NewPart(PartType partType, ErrorHandler errorHandler);
    public abstract void UpdatePart(Part part, ErrorHandler errorHandler);
}
