.PHONY: all run clean

TARGET = cs320_project2_group11.exe

SOURCES = \
	App/mainapp.cs \
    Model/dbmodel.cs \
    Model/preferencesmodel.cs \
    Model/parttypeentry.cs \
    View/mainview.cs \
    View/preferencesview.cs \
    View/rawsqlview.cs \
    View/partsview.cs \
    Util/util.cs

RESOURCES =

REFS = MySql.Data.dll System.Data.dll

LIBS = dotnet

all: $(TARGET)

MCS ?= gmcs
MONO ?= mono
MONOFLAGS ?= -debug -target:exe
MONOFLAGS += $(foreach lib,$(LIBS),-pkg:$(lib))
MONOFLAGS += $(foreach ref,$(REFS),-r:$(ref))

$(TARGET): $(SOURCES)
	$(MCS) $(MONOFLAGS) -out:"$@" $^ 

run: $(TARGET)
	$(MONO) --debug $(TARGET)

clean:
	rm -f "$(TARGET)"
	rm -f *.mdb
