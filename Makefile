.PHONY: all run clean

TARGET = cs320_project2_group11.exe

SOURCES = \
	App/mainapp.cs \
    Model/dbmodel.cs \
    Model/preferencesmodel.cs \
    Model/parttypeentry.cs \
    Model/partentry.cs \
    View/mainview.cs \
    View/preferencesview.cs \
    View/rawsqlview.cs \
    View/partsview.cs \
    View/parteditview.cs \
    Util/util.cs

RESOURCES =

REFS = MySql.Data.dll System.Data.dll

LIBS = dotnet

all: $(TARGET)

debug: DEFINES += DEBUG
debug: all

MCS ?= gmcs
MONO ?= mono
MONOFLAGS ?= -debug -target:exe
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
