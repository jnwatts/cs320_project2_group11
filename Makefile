.PHONY: all run clean

TARGET = cs320_project2_group2.exe

SOURCES = \
	main.cs \
	test.cs

RESOURCES =

REFS = 

LIBS = dotnet

all: $(TARGET)

MCS ?= mcs
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
