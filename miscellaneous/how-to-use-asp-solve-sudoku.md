# ASP 规约解题

## 1. ASP（clingo）是什么

**ASP**（Answer Set Programming，回答集编程）是一种声明式编程范式，起源于逻辑编程和非单调推理领域。它的核心思想是：你只需要描述“问题是什么”（规则和约束），求解器会自动找出所有满足条件的“回答集”（即解）。

**clingo** 是 ASP 领域最成熟的工具链之一，由德国波茨坦大学的 **Potassco**（Potsdam Answer Set Solving Collection）项目开发。clingo 这个名称由 `clasp`（底层 SAT-based 求解器）和 `gringo`（规则接地器）组合而来。它是众多学者和工业界解决组合搜索、规划、配置等问题的首选 ASP 工具。

具体而言，clingo 接受一个扩展后的逻辑程序作为输入，该程序由事实（facts）和规则（rules）构成，语法大致如下：

```prolog
% 事实：puzzle(1, 2, 5) 表示第 1 行第 2 列的已知数字为 5
puzzle(1, 2, 5).

% 规则：h :- b1, b2, ..., bm, not c1, ..., not cn.
% 含义：如果所有 b_i 为真且所有 c_j 为假，则 h 为真
% 「not」在 ASP 中表示“否定为失败”（negation as failure）
```

clingo 的工作分为两个阶段：

1. **接地（Grounding）**：由 `gringo` 完成，将带有变量的规则展开为无变量的命题逻辑程序。
2. **求解（Solving）**：由 `clasp` 完成，在接地后的程序上搜索稳定模型（回答集），本质上是 CDCL（Conflict-Driven Clause Learning，冲突驱动子句学习）机制的 SAT 求解器。

换句话说，**ASP ≈ SAT + 变量展开（接地）**，它的抽象层次介于 SAT 和 SMT 之间——比 SAT 的表达力强，比 SMT 的打通领域广。

---

## 2. 为什么数独适合使用 ASP 规约解题

数独是一种典型的**约束满足问题**（CSP）：81 个格子、9 个数字，行/列/宫三套唯一性约束。全部规则都可以用 ASP 的一阶逻辑语法简洁表达，本仓库的 `AspSolver` 正是这样做的：

### 2.1 每格恰好一个数字

```prolog
1 { cell(R, C, N) : num(N) } 1 :- row(R), col(C).
```

这就是 ASP **基数约束**（cardinality constraint）的典型用法——读作：在 `{cell(R, C, 1), ..., cell(R, C, 9)}` 中恰好选 1 个。clingo 接地器会自动为 9x9 网格展开全部 81 个实例。

### 2.2 行、列唯一性

```prolog
:- row(R), num(N), #count { C : cell(R, C, N) } != 1.
:- col(C), num(N), #count { R : cell(R, C, N) } != 1.
```

以 `:-` 开头的规则是**约束**（constraint），意为“不存在这样的解”。这里使用 ASP 的**聚合函数** `#count`：对每一行、每一个数字，统计该数字在该行出现的列数，不允许不等于 1 的情况——直接表达了“行内数字不重复”，无需手写嵌套循环。

### 2.3 宫（3x3 小方块）唯一性

```prolog
subgrid(SG_R, SG_C, R, C) :-
    row(R), col(C), SG_R = (R-1)/3, SG_C = (C-1)/3.
:- num(N), subgrid(SG_R, SG_C, _, _),
   #count { R, C : cell(R, C, N), subgrid(SG_R, SG_C, R, C) } != 1.
```

`subgrid/4` 是一条纯派生的规则：由格子坐标计算它所属的宫（行块、列块）。第二条约束再配合 `#count` 统计每个宫内每个数字的出现次数。整个“宫”的概念用两行规则定义完毕，无需任何手写索引表。

### 2.4 服从已知数字

```prolog
:- puzzle(R, C, N), not cell(R, C, N).
```

**否定为失败**（negation as failure）的典型用法：若某个格子已知数字为 `N`，而求解结果中该格不是 `N`，则此候选解被排除。盘面的已知数字由 C# 侧生成 `puzzle/3` 事实提供。

### 2.5 总结

以上数独规则在 ASP 中只需 **约 10 行代码**即可完整表达。这是因为 ASP 天生擅长：

- **基数约束**——每格恰好一个数字
- **聚合（`#count`）**——行/列/宫唯一性，一个聚合函数代替手写循环
- **否定为失败**——服从已知数字，一行约束完成

相比之下，在命令式语言（C#、C++）中实现同样的约束验证，需要为行、列、宫编写 27 组循环检查；本仓库的手写求解器（如 `BacktrackingSolver`、`DancingLinksSolver`）则需数百行代码实现完整的回溯或精确覆盖搜索。

---

## 3. ASP 与 SAT、SMT 的异同

| 维度 | SAT | SMT | ASP |
|------|-----|-----|-----|
| **核心层** | 命题逻辑 | SAT + 背景理论 | SAT + 稳定模型语义 |
| **输入语言** | CNF（合取范式） | SMT-LIB（带类型的表达式） | 逻辑程序（事实 + 规则） |
| **变量展开** | 无（全为命题） | 由求解器在需要时实例化 | `gringo` 接地器全量展开 |
| **表达力** | NP 完全 | NP + 特定理论（如线性算术） | Σ₂^P（第二级多项式层级） |
| **传递闭包** | 需要手动编码 | 依赖理论（无通用方案） | 递归规则原生支持 ✅ |
| **基数约束** | 需要编码为子句 | 依赖理论/编码 | `1 { ... } N` 原生支持 ✅ |
| **否定** | 无（仅 ¬ 文字） | 背景理论内的否定 | “否定为失败”（非单调）✅ |
| **优化/最小化** | MaxSAT 扩展 | 优化模块 | `#minimize` / `#maximize` ✅ |

### 3.1 SAT 的局限

SAT 求解器只接受命题逻辑公式（CNF 格式）。本仓库的 `SatisfiabilitySolver` 需要为每个 `(格子, 数字)` 对建立一个布尔变量，然后把“每格恰好一个数字”“行/列/宫数字唯一”展开为显式子句：at-least-one（每格至少一个）与 at-most-one（每格至多一个，通常用 pairwise 编码，每格 36 对、每个单元组 36 对）。这种编码规模随约束数量急剧膨胀，且子句是机器生成的，难以阅读和维护。

### 3.2 SMT 的方案

本仓库的 `SmtSolver` 使用 Z3 的整数理论：为每个格子分配一个 `1..9` 的整数变量，通过断言行/列/宫互异来表达唯一性。这样写比 CNF 直观，但仍然需要为每个约束组显式构造 AllDifferent 或 pairwise 不等式。相比之下，ASP 的基数约束和聚合函数是语言原生构造，表达更直接。

### 3.3 为什么 ASP 更适合“规则型”谜题

数墙、数独、数间（Hitori）、信号旗等笔谜题的共同特征是：**规则可以用一阶逻辑风格的递归规则和聚合约束自然表达**。这些恰好是 ASP 的长项。相反，SMT 更适合需要大量算术运算的问题（如调度、线性规划），SAT 更适合组合爆炸但结构扁平的纯布尔问题（如硬件验证）。

---

## 4. ASP 为什么能这么快

clingo 的高效来自多个层次的优化：

### 4.1 CDCL 求解引擎

`clasp` 的核心是 CDCL SAT 求解器，与当今最快的 SAT 求解器共享同一套底层技术：
- **冲突驱动子句学习**：每次发现矛盾，分析冲突图并学习一条新的禁止子句，避免未来重复探索同样的死路
- **回跳（Backjumping）**：不是回溯单步，而是直接跳回导致冲突的决策变量
- **VSIDS 分支启发式**：优先分配出现频率高的变量
- **重启策略**：定期遗忘部分搜索状态，避免陷入局部死角（Luby 序列）
- **两观察文字（Two Watched Literals）**：高效的单元传播机制

### 4.2 智能接地（Grounding）

`gringo` 不是简单地将所有变量替换为所有可能的值（那样会产生指数级的事实）。它使用了多项优化：
- **半朴素求值（Semi-naive Evaluation）**：只对增量变化的部分求值
- **依赖图分析**：尽早绑定变量，缩小笛卡尔积空间
- **安全变量限制**：规则必须“安全”——每个变量必须出现在正文字中，这确保了接地后的程序是有限的

### 4.3 基数约束的原生处理

`1 { a; b; c } 2` 这类基数约束在 SAT 中需要编码为几十条子句；在 clingo 中被实现为专用的**基数网络**（基于排序网络或计数器），传播速度远快于等价的子句编码。

### 4.4 增量求解

clingo 支持增量模式（`#program` 指令），允许分步骤增加事实和规则。每一步的求解可以复用前一步的子句数据库，对“逐步添加约束”的解题场景尤其高效。

### 4.5 实际表现

对于典型的 9x9 数独谜题，clingo 通常在 **毫秒级**内完成求解，足以满足交互式使用。相比本仓库的 `BitwiseSolver` 等高度优化的手写求解器，ASP 的优势不在于极致的求解速度，而在于**用最少的代码表达全部规则**——规则的正确性一目了然，且可轻松扩展新的约束。

---

## 5. 约束演示：数独

以下是经典 9x9 数独的完整 ASP 程序，展示了 ASP 如何用极简代码表达复杂约束。这段程序与本仓库 `AspSolver` 内嵌的规则完全一致：

```prolog
% ---- 结构事实 ----
row(1..9).  col(1..9).
num(1..9).

% ---- 初始盘面 ----
% puzzle(行, 列, 数字).
puzzle(1, 1, 5).  puzzle(1, 2, 3).  ...（部分已知数字）

% ---- 核心约束 ----

% 1. 每格恰好一个数字
1 { cell(R, C, N) : num(N) } 1 :- row(R), col(C).

% 2. 每行每个数字恰好一次
:- row(R), num(N), #count { C : cell(R, C, N) } != 1.

% 3. 每列每个数字恰好一次
:- col(C), num(N), #count { R : cell(R, C, N) } != 1.

% 4. 每宫每个数字恰好一次
subgrid(SG_R, SG_C, R, C) :-
    row(R), col(C), SG_R = (R-1)/3, SG_C = (C-1)/3.
:- num(N), subgrid(SG_R, SG_C, _, _),
   #count { R, C : cell(R, C, N), subgrid(SG_R, SG_C, R, C) } != 1.

% 5. 必须满足初始盘面
:- puzzle(R, C, N), not cell(R, C, N).

% ---- 输出 ----
#show cell/3.
```

关键点：
- `1 { cell(R, C, N) : num(N) } 1` — 基数约束，每格恰好选一个数字
- `#count { C : cell(R, C, N) } != 1` — 聚合函数，表达唯一性
- `:- puzzle(R, C, N), not cell(R, C, N).` — 否定为失败，强制服从已知数字

同样的模式也可以用于数间（Hitori）、信号旗（Tents）、数方（Shikaku）等逻辑谜题。

---

## 6. 进一步学习

| 资源 | 说明 |
|------|------|
| [Potassco 官方网站](https://potassco.org/) | clingo 下载、文档、教程入口 |
| [Potassco User Guide](https://github.com/potassco/guide/releases/) | clingo 的官方用户指南（PDF），约 100 页，全面覆盖语法与 API |
| [Answer Set Solving in Practice](https://potassco.org/book/) | Gebser/Kaminski/Kaufmann/Schaub 著，ASP 和 clingo 的权威教材，免费 PDF |
| [Clingo Python API](https://potassco.org/clingo/python-api/5.7/) | 在 Python 中调用 clingo 求解器的 API 文档 |
| [ASP 标准](https://www.mat.unical.it/aspcomp2011/ASPStandardization/) | ASP-Core-2 语言规范，定义了跨求解器的标准语法 |
| [clasp 论文](https://www.cs.uni-potsdam.de/wv/publications/DBLP_journals/tplp/GebserKKS12.pdf) | clasp 求解器的学术论文（2012），详解 CDCL 在 ASP 中的实现 |

---

## 7. 安装 clingo

> **本仓库的求解器不需要系统级 clingo。** 运行 `scripts/fetch-clingo.sh`（Linux/macOS）或 `scripts/fetch-clingo.ps1`（Windows）即可将 libclingo 共享库下载到 `miscellaneous/dll/clingo/`，`AspSolver` 通过 P/Invoke 直接加载。以下命令行工具的安装方法仅用于学习、调试 ASP 程序。

clingo 是命令行工具，无需图形界面。安装后，将 `clingo` 加入系统 PATH 即可正常使用。

### 7.1 Linux

**Ubuntu / Debian（APT）：**

```bash
sudo apt install gringo
```

APT 仓库中的包名称为 `gringo`，但实际包含了完整的 clingo 工具链。

**Fedora / RHEL：**

```bash
sudo dnf install clingo
```

**Arch Linux：**

```bash
sudo pacman -S clingo
```

**通过 conda（跨发行版）：**

```bash
conda install -c potassco clingo
```

**通过 pip（Python 绑定）：**

```bash
pip install clingo
```

Python 包安装后可在命令行直接使用 `clingo` 命令，也可在 Python 中通过 `import clingo` 调用。

### 7.2 Windows

**方法一：下载预编译二进制（推荐）**

1. 前往 [Potassco 发布页面](https://github.com/potassco/clingo/releases)
2. 下载最新的 `clingo-{version}-win64.zip`
3. 解压到一个固定目录（例如 `C:\Program Files\clingo\`）
4. 将该目录添加到系统环境变量 PATH 中：
   - 打开“设置” → “系统” → “关于” → “高级系统设置”
   - 点击“环境变量”
   - 在“系统变量”中找到 `Path`，点击“编辑”
   - 添加 clingo 所在目录路径
   - 点击“确定”保存
5. 打开新的命令提示符或 PowerShell，运行 `clingo --version` 验证安装

**方法二：通过 WSL**

```bash
wsl sudo apt install gringo
```

然后在 WSL 中直接使用 `clingo`，在 Windows 端调用时可加 `wsl clingo` 前缀。

**方法三：通过 Python**

```bash
pip install clingo
```

安装完成后 `clingo` 命令自动可用。

### 7.3 验证安装

```bash
clingo --version
# 期望输出类似：clingo version 5.7.x

# 试运行一个简单的 ASP 程序
echo "a.  b :- a.  :- not b." | clingo
# 期望输出：Answer: 1  a b
```

---

## 附录：数独求解器的 ASP 编码（本仓库实现）

本仓库数独求解器中的 `AspSolver` 类（`src/Sudoku.Core/Solving/Asp/AspSolver.cs`）将上述 ASP 约束以常量字符串内嵌，运行时由 C# 代码生成盘面事实（`puzzle(R, C, N)`），拼接后通过 `ClingoInterop`（`src/Sudoku.Core/Solving/Asp/ClingoInterop.cs`，libclingo 5.8 C API 的 P/Invoke 封装）直接求解，并从模型输出解析 `cell(R, C, N)` 原子还原完整盘面。

与命令行工具不同，本实现不通过 `Process` 启动外部进程，而是直接加载 `miscellaneous/dll/clingo/` 下的共享库（`libclingo.so` / `libclingo.dylib` / `clingo.dll`，随构建复制到输出目录），求解开销更小，也更便于集成。

ASP 程序约 10 行，C# 调用代码（`AspSolver` 与 `ClingoInterop`）总计约 400 行。整个求解器支持三种求解结果判定：唯一解、多解、无解（通过枚举至多 2 个模型判断唯一性）。这正是 ASP 的核心优势：**用最少的代码达成复杂的逻辑推理效果**。
