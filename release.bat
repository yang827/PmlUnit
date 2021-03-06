@echo off
setlocal EnableExtensions

set "BASE_DIR=%~dp0"

set "PmlUnit.sln=%BASE_DIR%\PmlUnit.sln"
set "PmlUnit=%BASE_DIR%\PmlUnit"
set "PmlUnit.SmokeTest=%BASE_DIR%\PmlUnit.SmokeTest"
set "PmlUnit.Tests=%BASE_DIR%\PmlUnit.Tests"

for /f "delims=" %%i in ('powershell.exe -ExecutionPolicy bypass "& '%BASE_DIR%\get-version.ps1' '%PmlUnit%'"') do (
    set "VERSION=%%i"
)
if not defined VERSION (
    call :write_error "Unable to determine PmlUnit version"
    goto end
)
call :write_info "Building PmlUnit %VERSION%"
set "BUILD_DIR=PmlUnit-%VERSION%"
set "RELEASE_FILE=PmlUnit-%VERSION%.zip"



for %%X in (msbuild.exe) do (
    set "msbuild.exe=%%~$PATH:X"
)
if defined msbuild.exe (
    if exist "%msbuild.exe" (
        goto msbuild_found
    )
)
for /f "delims=" %%i in ('powershell.exe -ExecutionPolicy bypass "& '%BASE_DIR%\find-msbuild.ps1'"') do (
    set "msbuild.exe=%%i"
)
if not defined msbuild.exe (
    call :write_error "Couldn't find msbuild.exe on your system"
    goto end
)
if not exist "%msbuild.exe%" (
    call :write_error "'%msbuild.exe%' not found"
    goto end
)
:msbuild_found
call :write_info "Using msbuild from '%msbuild.exe%'"



pushd %~dp0
nuget restore
set _restore_error_level=%errorlevel%
popd
if %_restore_error_level% neq 0 (
    call :write_error "Failed to restore dependencies"
    goto end
)



for %%X in (nunit3-console.exe) do (
    set "nunit.exe=%%~$PATH:X"
)
if defined nunit.exe (
    if exist "%nunit.exe%" (
        goto nunit_found
    )
)
pushd %~dp0
for /f "delims=" %%i in ('powershell.exe -ExecutionPolicy bypass "& '%BASE_DIR%\find-nunit.ps1'"') do (
    set "nunit.exe=%%i"
)
popd
if not defined nunit.exe (
    call :write_error "Couldn't find the NUnit console runner"
    goto end
)
if not exist "%nunit.exe%" (
    call :write_error "'%nunit.exe%' not found"
    goto end
)
:nunit_found
call :write_info "Using NUnit console runner from '%nunit.exe%'"



call :build "PDMS 12.1" "pdms-12.1"
if %errorlevel% neq 0 (
    goto end
)

mkdir "%BUILD_DIR%\oh-12.1"
if %errorlevel% neq 0 (
    call :write_error "Failed to create oh-12.1 directory"
    goto end
)
mkdir "%BUILD_DIR%\oh-12.1\bin"
if %errorlevel% neq 0 (
    call :write_error "Failed to create oh-12.1 binary directory"
    goto end
)
mkdir "%BUILD_DIR%\oh-12.1\caf"
if %errorlevel% neq 0 (
    call :write_error "Failed to create oh-12.1 caf directory"
    goto end
)
copy "%BUILD_DIR%\pdms-12.1\bin\*" "%BUILD_DIR%\oh-12.1\bin\"
if %errorlevel% neq 0 (
    call :write_error "Failed to copy pdms-12.1 binaries to oh-12.1"
    goto end
)
copy "%BASE_DIR%\caf\OH 12.1\*" "%BUILD_DIR%\oh-12.1\caf\"
if %errorlevel% neq 0 (
    call :write_error "Failed to copy oh-12.1 CAF files"
    goto end
)

call :build "E3D 1.1" "e3d-1.1"
if %errorlevel% neq 0 (
    goto end
)
call :build "E3D 2.1" "e3d-2.1"
if %errorlevel% neq 0 (
    goto end
)



call :write_info "Copying PMLLIB files to output directory"
xcopy /S /E /F /I "%BASE_DIR%\pmllib" "%BUILD_DIR%\pmllib"
if %errorlevel% neq 0 (
    call :write_error "Failed to copy PMLLIB to output directory"
    goto end
)
xcopy /S /E /F /I "%BASE_DIR%\pmllib-tests" "%BUILD_DIR%\pmllib-tests"
if %errorlevel% neq 0 (
    call :write_error "Failed to copy PMLLIB tests to output directory"
    goto end
)

call :write_info "Copying README, CHANGELOG, and LICENSE to output directory"
copy "%BASE_DIR%\README.md" "%BUILD_DIR%\README.txt"
if %errorlevel% neq 0 (
    call :write_error "Failed to copy README to output directory"
    goto end
)
copy "%BASE_DIR%\CHANGELOG.md" "%BUILD_DIR%\CHANGELOG.txt"
if %errorlevel% neq 0 (
    call :write_error "Failed to copy CHANGELOG to output directory"
    goto end
)
copy "%BASE_DIR%\LICENSE" "%BUILD_DIR%\LICENSE.txt"
if %errorlevel% neq 0 (
    call :write_error "Failed to copy LICENSE to output directory"
    goto end
)



call :write_info "Creating zip file"
"C:\Program Files\7-Zip\7z.exe" a "%RELEASE_FILE%" "%BUILD_DIR%"
if %errorlevel% neq 0 (
    call :write_error "Failed to zip build directory"
    goto end
)
rmdir /S /Q "%BUILD_DIR%"

call :write_success "%RELEASE_FILE% created sucessfully"
goto end



:build
set "platform=%~1"
set "bin_dir=%BUILD_DIR%\%~2\bin\"
set "caf_dir=%BUILD_DIR%\%~2\caf\"

call :write_info "Building solution for %platform%"
"%msbuild.exe%" /p:Configuration=Release "/p:Platform=%platform%" "%PmlUnit.sln%"
if %errorlevel% neq 0 (
    call :write_error "Failed to build solution for %platform%"
    exit /B 1
)
call :write_info "Running tests for %platform%"
rem We cannot run the tests from PmlUnit.SmokeTest and PmlUnit.Tests together
rem because they may have different target framework versions. (The SmokeTest
rem uses .NET 3.5 for PDMS and .NET 4.0 for E3D, while the unit tests always
rem .NET 4.5 or higher.)
rem The NUnit console runner cannot load multiple runtimes at once but we want
rem to execute the smoke tests in the correct target framework. So we have to
rem execute them separately.
"%nunit.exe%" --noresult "%PmlUnit.SmokeTest%\bin\Release\%platform%\PmlUnit.SmokeTest.dll"
if %errorlevel% neq 0 (
    call :write_error "Smoke test for %platform% failed"
    exit /B 2
)
"%nunit.exe%" --noresult "%PmlUnit.Tests%\bin\Release\%platform%\PmlUnit.Tests.dll"
if %errorlevel% neq 0 (
    call :write_error "Tests for %platform% failed"
    exit /B 3
)

if not exist %bin_dir% (
    mkdir %bin_dir%
    if %errorlevel% neq 0 (
        call :write_error "Unable to create output directory %bin_dir%"
        exit /B 11
    )
)
if not exist %caf_dir% (
    mkdir %caf_dir%
    if %errorlevel% neq 0 (
        call :write_error "Unable to create output directory %caf_dir%"
        exit /B 12
    )
)

copy "%PmlUnit%\bin\Release\%platform%\*" "%bin_dir%"
if %errorlevel% neq 0 (
    call :write_error "Failed to copy PmlUnit.dll to output directory %bin_dir%"
    exit /B 13
)
copy "%BASE_DIR%\caf\%platform%\*" %caf_dir%
if %errorlevel% neq 0 (
    call :write_error "Failed to copy CAF XML files to output directory %caf_dir%"
    exit /B 14
)

goto :eof



:write_info
echo.
echo [97m[INFO] %~1[0m
echo.
goto :eof



:write_error
echo.
echo [91m[ERROR] %~1[0m
goto :eof



:write_success
echo.
echo [92m[SUCCESS] %~1[0m
goto :eof



:end
(((echo.%cmdcmdline%) | find /I "%~0") > nul)
if %errorlevel% equ 0 (
    rem We were called from Windows Explorer. Pause so that people can actually
    rem look at the command output
    pause
)
