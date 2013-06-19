# MonoGame Model Pipeline

This is just a setup for a simpler workflow while I write the necessary parts for an XNA-compatible Model implementation for MonoGame.

## Getting Started

* Run CMake for assimp. Because both 32-bit and 64-bit DLL's are needed, run CMake twice in different directories under assimp/build. Ex: assimp/build/win32. Also, set ASSIMP_LIBRARY_SUFFIX to 32 and 64 so both libraries can be included in your pipeline project.

* Build assimp for both architectures.

* Copy the assimp outputs to the assimp-net, pipeline, and XNA projects (TODO: Automate this or fixup in some upstream-friendly way)

* Build assimp-net


