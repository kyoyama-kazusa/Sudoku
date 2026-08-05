# 下载 clingo（Potassco）原生共享库并放入 miscellaneous/dll/clingo/。
# Windows x64 专用；Linux / macOS 请运行 scripts/fetch-externals.sh。
#
# 用法: powershell -ExecutionPolicy Bypass -File scripts/fetch-externals.ps1
#
# 来源说明:
#   clingo 的 GitHub release 不再附带预编译二进制。本脚本从 conda-forge 下载 clingo 的
#   conda 包（.conda 文件，内部为 zip 容器 + zstd 压缩的 tar），提取其中独立的
#   Library/bin/clingo.dll（Windows 的 C API 共享库，无 Python 依赖），可直接作为
#   P/Invoke 目标使用。
#   解压 zstd 所需的 zstd.exe 由脚本自动从 zstd 官方 release 下载（临时使用，不安装）。

$ErrorActionPreference = "Stop"

# clingo 版本号。升级依赖时仅需修改此变量（并确认 Clingo.cs 注释中的 API 版本一致）。
$ClingoVersion = "5.8.0"

# conda-forge 包文件名。文件名中的构建号（如 py310h26ee641_0）随 conda-forge 发布变动，
# 升级版本时若下载失败，可到 https://anaconda.org/conda-forge/clingo/files 查询最新文件名。
$CondaPkg = "clingo-$ClingoVersion-py310h26ee641_0.conda"

# zstd 官方 release（Windows 二进制，仅临时使用）
$ZstdVersion = "1.5.7"
$ZstdUrl = "https://github.com/facebook/zstd/releases/download/v$ZstdVersion/zstd-v$ZstdVersion-win64.zip"

# 兼容旧版 Windows PowerShell 的 TLS 设置
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$RepoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$ExternalsDir = Join-Path $RepoRoot "miscellaneous\dll\clingo"

$DownloadUrl = "https://conda.anaconda.org/conda-forge/win-64/$CondaPkg"

$TempDir = Join-Path $env:TEMP ("clingo-fetch-" + [System.Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $TempDir | Out-Null
try {
	# 下载 zstd（临时使用）
	Write-Host "下载 zstd（临时） ..."
	Invoke-WebRequest -Uri $ZstdUrl -OutFile (Join-Path $TempDir "zstd.zip")
	Expand-Archive -Path (Join-Path $TempDir "zstd.zip") -DestinationPath (Join-Path $TempDir "zstd") -Force
	$ZstdPath = Get-ChildItem -Path (Join-Path $TempDir "zstd") -Recurse -File -Filter "zstd.exe" | Select-Object -First 1
	if (-not $ZstdPath) {
		throw "zstd 解压失败。"
	}

	# 下载 clingo conda 包
	Write-Host "下载 $CondaPkg ..."
	Invoke-WebRequest -Uri $DownloadUrl -OutFile (Join-Path $TempDir "clingo.conda")

	Write-Host "解压 ..."
	Expand-Archive -Path (Join-Path $TempDir "clingo.conda") -DestinationPath (Join-Path $TempDir "conda") -Force
	$PkgTarZst = Get-ChildItem -Path (Join-Path $TempDir "conda") -File -Filter "pkg-*.tar.zst" | Select-Object -First 1
	if (-not $PkgTarZst) {
		throw "conda 包内未找到 pkg-*.tar.zst。"
	}

	# zstd 解压 tar
	$PkgTar = Join-Path $TempDir "pkg.tar"
	& $ZstdPath.FullName -d -f -o $PkgTar $PkgTarZst.FullName
	if ($LASTEXITCODE -ne 0) {
		throw "zstd 解压失败（退出码 $LASTEXITCODE）。"
	}

	# 提取 Windows C API 共享库（Library/bin/clingo.dll）
	$ExtractDir = Join-Path $TempDir "extracted"
	New-Item -ItemType Directory -Path $ExtractDir | Out-Null
	tar.exe -xf $PkgTar -C $ExtractDir "Library/bin/clingo.dll"
	if ($LASTEXITCODE -ne 0) {
		throw "tar 提取失败（退出码 $LASTEXITCODE）。"
	}

	$TargetDir = Join-Path $ExternalsDir "win-x64"
	New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
	Copy-Item (Join-Path $ExtractDir "Library\bin\clingo.dll") (Join-Path $TargetDir "clingo.dll") -Force

	# 许可证（MIT，Potassco）——随共享库一同分发是 MIT 许可的要求
	$LicenseUrl = "https://raw.githubusercontent.com/potassco/clingo/v$ClingoVersion/LICENSE.md"
	try {
		Invoke-WebRequest -Uri $LicenseUrl -OutFile (Join-Path $ExternalsDir "LICENSE.md")
		Write-Host "已复制许可证文件 LICENSE.md"
	} catch {
		Write-Warning "无法从 GitHub 下载 LICENSE.md，请手动补充（$LicenseUrl）。"
	}

	Write-Host ""
	Write-Host "完成: $(Join-Path $TargetDir 'clingo.dll')"
} finally {
	Remove-Item -Path $TempDir -Recurse -Force -ErrorAction SilentlyContinue
}
