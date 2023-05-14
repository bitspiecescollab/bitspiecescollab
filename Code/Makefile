SHELL = /bin/bash
.ONESHELL:


LIBS = -pkg:dotnet -r:MonoMod.Utils.dll -r:MonoMod.RuntimeDetour.dll \
-r:CollabUtils2.dll -r:Celeste.exe -r:MMHOOK_Celeste.dll
# Paths are relative to the Celeste/Mods/bitspiecescollab/Code/ directory.
LIB_PATH = -lib:../../../ -lib:./lib/


.PHONY: default clean debug release
default: release

clean:
	rm -rf build/

debug: clean
	mkdir -p build
	mcs ${LIB_PATH} ${LIBS} src/BitsPieces.cs -t:library -o:BitsPieces.dll -debug
	mv src/BitsPieces.dll build/BitsPieces.dll
	mv src/BitsPieces.dll.mdb build/BitsPieces.dll.mdb

release: clean
	mkdir -p build
	mcs ${LIB_PATH} ${LIBS} src/BitsPieces.cs -t:library -o:BitsPieces.dll -optimize
	mv src/BitsPieces.dll build/BitsPieces.dll

# This strips out code from reference assemblies so only the symbols are
# present for linking.
strip:
	for file in ./lib/*.dll; do mono-cil-strip "$$file"; done
