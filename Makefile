.PHONY: all run clean

TARGET = cs320_project2_group11.exe

SOURCES = \
    dbmodel.cs \
	main.cs \
    mainview.cs \
    preferencesview.cs \
    preferencesmodel.cs \
    rawsqlview.cs \
    util.cs

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
