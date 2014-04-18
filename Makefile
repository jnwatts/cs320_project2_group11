.PHONY: all run clean

TARGET = cs320_project2_group11.exe

SOURCES = \
	main.cs \
    mainview.cs

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
	$(MONO) $(TARGET)

clean:
	rm -f "$(TARGET)"
	rm -f *.mdb
