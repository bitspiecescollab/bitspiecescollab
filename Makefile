SHELL = /bin/bash
.ONESHELL:


LIBS = -pkg:dotnet -r:MonoMod.Utils.dll -r:MonoMod.RuntimeDetour.dll \
-r:CollabUtils2.dll -r:Celeste.exe -r:MMHOOK_Celeste.dll \
-r:FNA.dll -r:Mono.Cecil.dll
# Paths are relative to the Celeste/Mods/bitspiecescollab/ directory:
# - Celeste/ contains game & mono dlls
# - Code/lib contains CelesteNet references
LIB_PATH = -lib:../../ -lib:./Code/lib/

SOURCE=Code/src/*.cs
BINARY=Code/build/BitsPieces.dll

.PHONY: default clean debug release
default: release

clean:
	rm -rf Code/build/

debug: clean
	mkdir -p Code/build
	mcs ${LIB_PATH} ${LIBS} ${SOURCE} -t:library -out:${BINARY} -debug

release: clean
	mkdir -p Code/build
	mcs ${LIB_PATH} ${LIBS} ${SOURCE} -t:library -out:${BINARY} -optimize

# This strips out code from reference assemblies so only the symbols are
# present for linking.
strip:
	for file in Code/lib/*.dll; do mono-cil-strip "$$file"; done
