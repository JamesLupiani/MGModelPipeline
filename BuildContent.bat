@rem Copyright 2014 James T. Lupiani
@rem 
@rem Licensed under the Apache License, Version 2.0 (the "License");
@rem you may not use this file except in compliance with the License.
@rem You may obtain a copy of the License at
@rem
@rem   http://www.apache.org/licenses/LICENSE-2.0
@rem
@rem Unless required by applicable law or agreed to in writing, software
@rem distributed under the License is distributed on an "AS IS" BASIS,
@rem WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
@rem See the License for the specific language governing permissions and
@rem limitations under the License.

@rem Batch script for building game content
@rem Also happens to set up the MonoGame submodule if you haven't yet...

@echo off
@set MODE=Debug
@set MONOGAME_ROOT=%~dp0%~1MonoGame
@set GAME_PATH=%~dp0%~1ModelViewer

@rem Do we have MonoGame set up yet?
@if not exist "%MONOGAME_ROOT%" (
    @echo Getting MonoGame...
    git submodule update --init

    @echo Getting MonoGame dependencies...
    @pushd %MONOGAME_ROOT%
    git submodule update --init --recursive
    Protobuild.exe -generate Windows
    @popd
)

@rem Make sure the MonoGame content builder is up to date first
@echo Building MGCB...
@call "%VS120COMNTOOLS%vsvars32.bat"
@pushd %MONOGAME_ROOT%\Tools\MGCB
msbuild /nologo /v:quiet MGCB.sln /p:Configuration=%MODE% /p:PlatformTarget=x64 /t:Clean,Build
@popd

@rem Build the actual assets and put them in the game's directory.
@pushd Content
@echo Building content...
%MONOGAME_ROOT%\Tools\MGCB\bin\x64\%MODE%\MGCB.exe ^
/rebuild ^
/config:%MODE% ^
/platform:Windows ^
/reference:..\SkinnedModelPipeline\bin\%MODE%\SkinnedModel.dll ^
/reference:..\SkinnedModelPipeline\bin\%MODE%\SkinnedModelPipeline.dll ^
/intermediateDir:%GAME_PATH%\obj\%MODE%\Content ^
/outputDir:%GAME_PATH%\Content ^
/@:content.mgcb %1
@popd

@echo Dumping experimental data...
ParseXnb.exe ModelViewer\Content\Dude\dude.xnb > experiment-dude.txt
ParseXnb.exe ModelViewer\Content\Ship\ship.xnb > experiment-ship.txt

@echo Done.
@pause
