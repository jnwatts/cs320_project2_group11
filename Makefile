.PHONY: all run clean

TARGET = cs320_project2_group11.exe

SOURCES = \
	App/mainapp.cs \
    Model/partsdb.cs \
    Model/mysqlpartsdb.cs \
    Model/preferencesmodel.cs \
    Model/parttype.cs \
    Model/part.cs \
    Model/partcollection.cs \
    Model/query.cs \
    View/mainview.cs \
    View/preferencesview.cs \
    View/rawsqlview.cs \
    View/partsview.cs \
    View/parteditview.cs \
    Util/util.cs

RESOURCES =

REFS = Libs/MySql.Data.dll System.Data.dll

LIBS = dotnet

APP_TYPE = winexe

all: $(TARGET)

debug: DEFINES += DEBUG TRACE
debug: APP_TYPE = exe
debug: all

MCS ?= gmcs
MONO ?= mono
MONOFLAGS ?= -debug -target:$(APP_TYPE)
MONOFLAGS += $(foreach lib,$(LIBS),-pkg:$(lib))
MONOFLAGS += $(foreach ref,$(REFS),-r:$(ref))
MONOFLAGS += $(foreach def,$(DEFINES),-d:$(def))

$(TARGET): $(SOURCES)
	$(MCS) $(MONOFLAGS) -out:"$@" $^ 

run: $(TARGET)
	$(MONO) --debug $(TARGET)

clean:
	rm -f "$(TARGET)"
	rm -f *.mdb
