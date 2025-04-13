---
description: Drawing Command Line
---

# 绘图命令行

显示的部件该页面将帮助你如何使用绘图命令行。

## 外貌 <a href="#looking" id="looking"></a>

<figure><img src="../.gitbook/assets/images_0001.png" alt=""><figcaption><p>外貌</p></figcaption></figure>

## 命令行语法 <a href="#command-line-syntax" id="command-line-syntax"></a>

可以从图上看出，右侧的文本框是给你输入命令行，以命令行的形式输入绘图指令的。

在这个程序里，支持如下的一些绘图部件：

* 单元格
* 候选数
* 区域
* 大行列
* 图标
* 链
* 代数

这个页面使用的命令行语法基本基于命令行的输入语法规则。**只不过，他暂时还不支持使用引号字符 `"` 来转义空白字符。**

对于细节，我会马上在下面列举他们。

## 细节 <a href="#detail" id="detail"></a>

例如，下方的命令行

```bash
load -c 1...8...4..7..28...2...4.5....3.....7.5...4.8.....6....1.6...4...61..5..3...5...2
candidate !n r23c5(1)
candidate !n r78c5(2)
house !n b28
house !aux1 c5
icon !n circle r1c1,r2c6,r4c4,r5c1379,r6c6,r8c4,r9c9
icon !aux3 diamond r5c5
icon !aux1 cross r1c6,r123c4,r789c6,r9c4
candidate !e r5c5(12)
candidate !o r5c5(9)
```

可以产生如下的输出内容：

<figure><img src="../.gitbook/assets/images_0002.png" alt=""><figcaption></figcaption></figure>

### 载入题目 <a href="#load-puzzle" id="load-puzzle"></a>

你可以使用 `load` 指令来加载一个题目到页面上。他还支持使用额外选项控制候选数是否展示：

```bash
load [+c|-c] <puzzle>
```

这里的 `+c` 就表示展示候选数，而 `-c` 就表示不展示。如果省略不写，那么他等于是不显示候选数（和 `-c` 一样的效果）。

### 单元格高亮 <a href="#cell-highlight" id="cell-highlight"></a>

如果你要高亮一个单元格，请使用 `cell` 指令：

```bash
cell <color-identifier> <cell|cell-group>
```

对于这里的 `color-identfier`，我稍后会介绍。

### 候选数高亮 <a href="#candidate-highlight" id="candidate-highlight"></a>

如果你要展示候选数的高亮，请使用 `candidate` 指令：

```bash
candidate <color-identifier> <candidate|candidate-group>
```

另外，你也可以使用 `r1c1` 这种单元格语法来直接高亮这个单元格里的全部候选数。不过这个用法只支持在你前面用了 `load` 指令导入了盘面的情况。

### 区域高亮 <a href="#house-highlight" id="house-highlight"></a>

如果你要展示区域的高亮，就使用 `house` 指令：

```bash
house <color-identifier> <house|house-group>
```

### 大行列高亮 <a href="#chute-highlight" id="chute-highlight"></a>

如果你要高亮整个并排的三个宫，则使用 `chute` 指令：

```bash
chute <color-identifier> <chute|chute-group>
```

### 图标 <a href="#icon" id="icon"></a>

一个图标是显示在一个单元格里的一个形状。你可以使用 `icon` 指令来显示一个图标：

```bash
icon <color-identifier> <icon-shape> <cell|cell-group>
```

当前支持的图标有如下的一些：

* 圆（用 `circle` 表示）
* 打叉记号（用 `cross` 表示）
* 菱形（用 `diamond` 表示）
* 心形（用 `heart` 表示）
* 方块（用 `square` 表示）
* 五角星（用 `star` 表示）
* 三角形（用 `triangle` 表示）

### 代数 <a href="#baba-group" id="baba-group"></a>

代数允许你往单元格里显示一个字母，他用来代指这个单元格最终的填数等于这个字母表示的变量。这个使用的是 `baba` 指令：

```bash
baba <color-identifier> <character> <cell|cell-group>
```

### 链 <a href="#link" id="link"></a>

这个程序还支持往盘面上画链。你可以使用 `link` 来画链：

```bash
link <color-identifier> <link-shape> <start-candidates> <end-candidates> [<extra>]
```

目前程序支持的链的类型有如下一些：

* 单元格之间的连线（用在唯一环里，用 `cell` 表示）
* 强弱链（用 `chain` 表示）
* 共轭对（用于致命结构的类型 4，用 `conjugate` 表示）

另外，链的记号会用到 `=` 来表示强链，`-` 来表示弱链。比如说 `link !n chain r1c1(1) r1c2(1) =` 表示在 `r1c1(1)` 和 `r1c2(1)` 之间有一个强链。另外，这个语法还支持区块节点，如 `r1c123(1)` 这种组合候选数的情况。

## 坐标语法 <a href="#coordinate-syntax" id="coordinate-syntax"></a>

对于上述提及的坐标，你需要使用要么 RxCy 表示、要么 K9 表示，要么 Excel 表示的方式来呈现他们。具体哪一个取决于你在设置页里配置的那个。

<figure><img src="../.gitbook/assets/images_0003.png" alt=""><figcaption><p>设置页里配置的坐标类型</p></figcaption></figure>

## 颜色标识符语法 <a href="#color-identifier-syntax" id="color-identifier-syntax"></a>

对于颜色标识符，程序使用了三种不同的方式表示他们：

* 十六进制数（如 `#FFFFFF` 表示白色）
* 涂色名称（如 `!elimination` 表示涂色跟删数的配色一样）
* 画板配色编号（如 `&5` 表示你在自定义画板设置里配置的第 5 个颜色）

### 十六进制数 <a href="#hexadecimal-based-color" id="hexadecimal-based-color"></a>

你可以使用 6 到 8 个十六进制数表示出一个颜色。其中 6 个十六进制数会被转为 RGB 表示，而 8 个十六进制数则表示的是 ARGB。不区分大小写。

### 涂色名称 <a href="#aliased-string" id="aliased-string"></a>

你也可以使用别名来称呼一个颜色，表示这个颜色一般用在软件里充当什么角色的时候才会用到他。

支持的值有如下的一些：

* **`normal` 或 `n`**：表示通常的配色
* **`auxiliary` 或 `aux`**：表示辅助配色。辅助配色包含三种，所以可以填写的写法有 `auxiliary1`、`auxiliary2`、`auxiliary3`、`aux1`、`aux2` 和 `aux3`
* **`assignment` 或 `a`**：表示一个出数
* **`overlapped_assignmented`、`overlapped` 或 `o`**：表示一个出数，并且这个出数也同时被结构所用到（效果就是叠起来了）
* **`elimination`、`elim` 或 `e`**：表示一个删数
* **`cannibalism`、`cannibal` 或 `c`**：表示一个删数，但这个删数在结构里也用到（效果也是叠起来了）
* **`exofin` 或 `f`**：一个（外部）鱼鳍
* **`endofin` 或 `ef`**：一个内部鱼鳍
* **`link` 或 `l`**：跟链一样的颜色
* **`almost_locked_set` 或 `als`**：表示 ALS 结构的配色。这种配色方案一共有 5 个，所以可以用的写法有 `almost_locked_set1`、`almost_locked_set2`、`almost_locked_set3`、`almost_locked_set4`、`almost_locked_set5`、`als1`、`als2`、`als3`、`als4` 和 `als5`
* **`rectangle` 或 `r`**：一个唯一矩形。这种配色方案下一共支持 3 个不同的配色，所以可以用的写法有 `rectangle1`、`rectangle2`、`rectangle3`、`r1`、`r2` 和 `r3`

### 画板配色编号 <a href="#palette-id" id="palette-id"></a>

程序还支持 15 种不同的自定义画板配色。这个配置项可以在设置里自由配置。你可以改变他们，以便自定义出区别于程序的不同的配色。

因为支持 15 个颜色，所以 ID 的范围就是 1 到 15。你也可以用字母 `A` 到 `F` 来表示编号 10 到 15。

## 对前文命令行的解释 <a href="#explanation-of-example" id="explanation-of-example"></a>

前面讲过了命令行的语法，所以我们再来看命令行就比较好理解了。

```bash
# 载入题目，不显示候选数
load -c 1...8...4..7..28...2...4.5....3.....7.5...4.8.....6....1.6...4...61..5..3...5...2

# 用普通的配色来高亮 r23c5(1)
candidate !n r23c5(1)

# 用普通的配色来高亮 r78c5(2)
candidate !n r78c5(2)

# 用普通的配色来高亮第 2、8 宫
house !n b28

# 用辅助颜色 #1 来高亮第 5 列
house !aux1 c5

# 往如下配置的这些单元格上添加一个圆圈图标，并使用普通的配色
icon !n circle r1c1,r2c6,r4c4,r5c1379,r6c6,r8c4,r9c9

# 使用辅助颜色 #3 显示一个菱形图标到 r5c5 上
icon !aux3 diamond r5c5

# 在如下这些单元格上画叉，并使用的是辅助颜色 #1
icon !aux1 cross r1c6,r123c4,r789c6,r9c4

# 将 r5c5(12) 按删数颜色高亮
candidate !e r5c5(12)

# 将 r5c5(9) 按出数颜色高亮，并暗示它是推理过程中也会用到的候选数，用重叠的配色来强调
candidate !o r5c5(9)
```
