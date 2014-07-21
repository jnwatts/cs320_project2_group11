.PHONY: all run clean

TARGET = partsdbgui.exe

SOURCES = \
	App/mainapp.cs \
    Model/partsdb.cs \
    Model/mysqlpartsdb.cs \
    Model/sqlpartsdb.cs \
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
    Util/util.cs \
	AssemblyInfo.cs

RESOURCES =

REFS = Libs/MySql.Data.dll System.Data.dll

LIBS = dotnet

APP_TYPE = winexe

all: $(TARGET)
.PHONY: update_build update_major update_minor
update_major:
	@./version_increment.sh -f .version -M -r git >/dev/null;
	@echo Version updated to $$(cat .version);

update_minor:
	@./version_increment.sh -f .version -m -r git >/dev/null;
	@echo Version updated to $$(cat .version);

update_build:
	@./version_increment.sh -f .version -r git >/dev/null;
	@echo Version updated to $$(cat .version);

.version:
	+$(MAKE) update_build

AssemblyInfo.cs: .version
	@./version_cs.sh -i .version -o AssemblyInfo.cs

debug: DEFINES += DEBUG TRACE
debug: APP_TYPE = exe
debug: all

MCS ?= gmcs
MONO ?= mono
MONOFLAGS ?= -debug -target:$(APP_TYPE)
MONOFLAGS += $(foreach lib,$(LIBS),-pkg:$(lib))
MONOFLAGS += $(foreach ref,$(REFS),-r:$(ref))
MONOFLAGS += $(foreach def,$(DEFINES),-d:$(def))

$(TARGET): AssemblyInfo.cs $(SOURCES)
	$(MCS) $(MONOFLAGS) -out:"$@" $^ 

run: $(TARGET)
	$(MONO) --debug $(TARGET)

clean:
	rm -f "$(TARGET)"
	rm -f *.mdb
