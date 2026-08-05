# clingo 外部依赖

本目录存放 [clingo](https://github.com/potassco/clingo)（Potassco 的 Answer Set Programming 求解器）的原生共享库，供 `Nurikabe.AspSolver`（`src/Nurikabe/Solving/AspSolver.cs`）通过 P/Invoke 调用。

## 目录内容

| 路径 | 平台 | 提供方式 |
|------|------|----------|
| `linux-x64/libclingo.so` | Linux x64 | 脚本下载 |
| `linux-arm64/libclingo.so` | Linux arm64 | 脚本下载 |
| `win-x64/clingo.dll` | Windows x64 | 脚本下载 |
| `osx-x64/libclingo.dylib` | macOS x64 | 脚本下载 |
| `osx-arm64/libclingo.dylib` | macOS arm64 | 脚本下载 |
| `LICENSE.md` | — | clingo 的 MIT 许可证原文，随库一并分发（MIT 许可要求） |

二进制文件不入库（见仓库根 `.gitignore`）。克隆仓库后请先运行下载脚本一次：

- Linux / macOS: `bash scripts/fetch-externals.sh`（需要 `zstd` 命令，Debian/Ubuntu 安装：`sudo apt install zstd`；macOS 15+ 自带）
- Windows: `powershell -ExecutionPolicy Bypass -File scripts/fetch-externals.ps1`（自动下载临时 zstd，无需安装）

脚本会自动检测当前平台，只下载对应平台的库。构建时（`src/Nurikabe/Nurikabe.csproj`）会把库复制到程序输出目录，`Clingo` 类的 DllImport 解析器（`src/Nurikabe/Solving/Asp/Clingo.cs`）会优先从程序目录加载，找不到时回退到系统库搜索路径，无需手工配置。

## 版本

当前版本: **clingo 5.8.0**（与 `Clingo.cs` 的 P/Invoke 封装对应）。

## 获取流程

日常使用只需运行下载脚本（见上）。以下为脚本执行的具体步骤，供了解或手动复现（例如脚本失效时排查）。

### 1. 确定平台包文件名

构建号（如 `py310hf71b8c6_0`）随 conda-forge 发布变动，升级时到 <https://anaconda.org/conda-forge/clingo/files> 查询最新文件名：

| conda 平台 | 包文件名（clingo 5.8.0） | externals 目标目录 | 包内提取路径 |
|-----------|--------------------------|--------------------|--------------|
| `linux-64` | `clingo-5.8.0-py310hf71b8c6_0.conda` | `linux-x64/` | `lib/libclingo.so` |
| `linux-aarch64` | `clingo-5.8.0-py310he30c3ed_0.conda` | `linux-arm64/` | `lib/libclingo.so` |
| `win-64` | `clingo-5.8.0-py310h26ee641_0.conda` | `win-x64/` | `Library/bin/clingo.dll` |
| `osx-64` | `clingo-5.8.0-py310h6954a95_0.conda` | `osx-x64/` | `lib/libclingo.dylib` |
| `osx-arm64` | `clingo-5.8.0-py310h853098b_0.conda` | `osx-arm64/` | `lib/libclingo.dylib` |

### 2. 下载并解包（以 Linux x64 为例，其余平台同理）

`.conda` 文件是 zip 容器，内含 zstd 压缩的 tar；`zstd` 命令缺失时安装（Debian/Ubuntu: `sudo apt install zstd`）：

```bash
curl -fL -O https://conda.anaconda.org/conda-forge/linux-64/clingo-5.8.0-py310hf71b8c6_0.conda
unzip clingo-5.8.0-py310hf71b8c6_0.conda
zstd -d pkg-clingo-5.8.0-py310hf71b8c6_0.tar.zst -o pkg.tar
tar -xf pkg.tar lib        # 提取整个 lib 目录：libclingo.so 是符号链接（→ .so.4 → .so.4.0），需连同链接目标一并提取
```

### 3. 复制共享库到 externals

`cp` 会自动跟随符号链接，复制出真实文件：

```bash
mkdir -p externals/clingo/linux-x64
cp lib/libclingo.so externals/clingo/linux-x64/
```

### 4. 获取许可证

MIT 许可要求随库一并分发许可证文本：

```bash
curl -fL -o externals/clingo/LICENSE.md https://raw.githubusercontent.com/potassco/clingo/v5.8.0/LICENSE.md
```

### 5. 验证

`dotnet build` 后库会出现在输出目录（如 `src/Nurikabe/bin/Debug/net11.0/libclingo.so`），`Clingo` 类的 DllImport 解析器（`src/Nurikabe/Solving/Asp/Clingo.cs`）会从程序目录加载它；随后运行程序（如 `dotnet run --project src/_shared/ConsoleTest`）即可用 `AspSolver` 求解验证。

## 来源与升级

库文件取自 [conda-forge](https://anaconda.org/conda-forge/clingo/files) 的 clingo 包（`.conda` 文件，内部为 zip 容器 + zstd 压缩的 tar）。conda 包内是独立的纯 C++ 共享库（`lib/libclingo.so` / `lib/libclingo.dylib` / `Library/bin/clingo.dll`），无 Python 依赖，可直接供 P/Invoke 加载。clingo 的 GitHub release 不再附带预编译二进制，PyPI 的 wheel 内嵌共享库则依赖 Python 运行时符号（无法独立加载），故均不采用。

升级步骤：

1. 修改 `scripts/fetch-externals.sh` 与 `scripts/fetch-externals.ps1` 顶部的版本号，并到 conda-forge 页面查询最新包文件名（构建号随发布变动）同步更新；
2. 重新运行下载脚本；
3. 确认 `Clingo.cs` 中的 API 与新版 clingo 兼容（clingo 6.x 的 C API 存在破坏性变更，请勿直接升级）。
