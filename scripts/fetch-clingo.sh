#!/usr/bin/env bash
#
# 下载 clingo（Potassco）原生共享库并放入 miscellaneous/dll/clingo/。
# 支持 Linux（x64 / arm64）与 macOS（x64 / arm64）；Windows 请运行 scripts/fetch-externals.ps1。
#
# 用法: bash scripts/fetch-externals.sh
#
# 依赖: curl、unzip、zstd（Debian/Ubuntu: sudo apt install zstd；macOS 15+ 自带）
#
# 来源说明:
#   clingo 的 GitHub release 不再附带预编译二进制。本脚本从 conda-forge 下载 clingo 的
#   conda 包（.conda 文件，内部为 zip 容器 + zstd 压缩的 tar），提取其中独立的
#   lib/libclingo.so（Linux/macOS）共享库——该库是纯 C++ 共享库，无 Python 依赖，
#   可直接作为 P/Invoke 目标使用。
#   conda 包内 info/ 目录附带的 LICENSE 即 clingo 的 MIT 许可证原文，一并提取。

set -euo pipefail

# clingo 版本号。升级依赖时仅需修改此变量（并确认 Clingo.cs 注释中的 API 版本一致）。
CLINGO_VERSION="5.8.0"

# conda-forge 包文件名。文件名中的构建号（如 py310hf71b8c6_0）随 conda-forge 发布变动，
# 升级版本时若下载失败，可到 https://anaconda.org/conda-forge/clingo/files 查询最新文件名。
case "$(uname -s)" in
	Linux)
		case "$(uname -m)" in
			x86_64)
				CONDA_PLATFORM="linux-64"
				CONDA_PKG="clingo-${CLINGO_VERSION}-py310hf71b8c6_0.conda"
				TARGET_DIR="linux-x64"
				LIB_NAME="libclingo.so"
				;;
			aarch64)
				CONDA_PLATFORM="linux-aarch64"
				CONDA_PKG="clingo-${CLINGO_VERSION}-py310he30c3ed_0.conda"
				TARGET_DIR="linux-arm64"
				LIB_NAME="libclingo.so"
				;;
			*)
				echo "不支持的架构: $(uname -m)（仅支持 x86_64 / aarch64）" >&2
				exit 1
				;;
		esac
		;;
	Darwin)
		case "$(uname -m)" in
			x86_64)
				CONDA_PLATFORM="osx-64"
				CONDA_PKG="clingo-${CLINGO_VERSION}-py310h6954a95_0.conda"
				TARGET_DIR="osx-x64"
				LIB_NAME="libclingo.dylib"
				;;
			arm64)
				CONDA_PLATFORM="osx-arm64"
				CONDA_PKG="clingo-${CLINGO_VERSION}-py310h853098b_0.conda"
				TARGET_DIR="osx-arm64"
				LIB_NAME="libclingo.dylib"
				;;
			*)
				echo "不支持的架构: $(uname -m)（仅支持 x86_64 / arm64）" >&2
				exit 1
				;;
		esac
		;;
	*)
		echo "仅支持 Linux 与 macOS。Windows 请运行 scripts/fetch-externals.ps1。" >&2
		exit 1
		;;
esac

for cmd in curl unzip zstd; do
	if ! command -v "$cmd" >/dev/null 2>&1; then
		echo "缺少命令: $cmd" >&2
		echo "  Debian/Ubuntu: sudo apt install $cmd" >&2
		echo "  macOS (Homebrew): brew install $cmd" >&2
		exit 1
	fi
done

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
EXTERNALS_DIR="$REPO_ROOT/miscellaneous/dll/clingo"

DOWNLOAD_URL="https://conda.anaconda.org/conda-forge/$CONDA_PLATFORM/$CONDA_PKG"

TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

echo "下载 ${CONDA_PKG} ..."
curl -sfL -o "$TMP_DIR/clingo.conda" "$DOWNLOAD_URL" || {
	echo "下载失败（$DOWNLOAD_URL）。若为文件名问题，请到 https://anaconda.org/conda-forge/clingo/files 查询最新文件名并更新脚本。" >&2
	exit 1
}

echo "解压 ..."
unzip -q "$TMP_DIR/clingo.conda" -d "$TMP_DIR/conda"

PKG_TAR_ZST="$(find "$TMP_DIR/conda" -type f -name 'pkg-*.tar.zst' | head -1)"
if [ -z "$PKG_TAR_ZST" ]; then
	echo "conda 包内未找到 pkg-*.tar.zst。" >&2
	exit 1
fi

zstd -q -d -f -o "$TMP_DIR/pkg.tar" "$PKG_TAR_ZST"

# 提取 lib 目录中的共享库（lib/libclingo.so 或 lib/libclingo.dylib）
# 该文件是符号链接（→ .so.4 → .so.4.0），需连同链接目标一并提取，cp 时自动跟随链接得到真实文件
if ! tar -xf "$TMP_DIR/pkg.tar" -C "$TMP_DIR" lib; then
	echo "conda 包内未找到 lib/$LIB_NAME。" >&2
	exit 1
fi

if [ ! -f "$TMP_DIR/lib/$LIB_NAME" ]; then
	echo "conda 包内未找到 lib/$LIB_NAME。" >&2
	exit 1
fi

mkdir -p "$EXTERNALS_DIR/$TARGET_DIR"
cp "$TMP_DIR/lib/$LIB_NAME" "$EXTERNALS_DIR/$TARGET_DIR/$LIB_NAME"

# 许可证（MIT，Potassco）——随共享库一同分发是 MIT 许可的要求
# conda 包内不附带 clingo 的许可证文本，从 GitHub 获取
LICENSE_URL="https://raw.githubusercontent.com/potassco/clingo/v${CLINGO_VERSION}/LICENSE.md"
if curl -sfL -o "$EXTERNALS_DIR/LICENSE.md" "$LICENSE_URL"; then
	echo "已复制许可证文件 LICENSE.md"
else
	echo "警告: 无法从 GitHub 下载 LICENSE.md（${LICENSE_URL}），请手动补充。" >&2
fi

echo
echo "完成: $EXTERNALS_DIR/$TARGET_DIR/$LIB_NAME"
ls -lh "$EXTERNALS_DIR/$TARGET_DIR/$LIB_NAME"
