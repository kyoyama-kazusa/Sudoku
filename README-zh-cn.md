<center>Language switch to: <a href="README.md">English</a><br/><img src="miscellaneous/pic/icon.png" /></center>

[![stars](https://img.shields.io/github/stars/kyoyama-kazusa/Sudoku?color=097abb)](https://github.com/kyoyama-kazusa/Sudoku/stargazers)
[![issues](https://img.shields.io/github/issues/kyoyama-kazusa/Sudoku?color=097abb)](https://github.com/kyoyama-kazusa/Sudoku/issues)
[![license](https://img.shields.io/github/license/kyoyama-kazusa/Sudoku?color=097abb)](https://github.com/kyoyama-kazusa/Sudoku/blob/main/LICENSE)

![](https://img.shields.io/badge/%E7%BC%96%E7%A8%8B%E8%AF%AD%E8%A8%80-C%23%2014-%23178600)
![](https://img.shields.io/badge/%E5%9F%BA%E6%9C%AC%E6%A1%86%E6%9E%B6-.NET%209-blueviolet)
![](https://img.shields.io/badge/%E7%BC%A9%E8%BF%9B-Tab-lightgrey)
![](https://img.shields.io/badge/IDE-Visual%20Studio%202022%20v17.14%20%E9%A2%84%E8%A7%88%E7%89%88%207-%23cf98fb)
![](https://img.shields.io/badge/%E8%AF%AD%E8%A8%80-%E8%8B%B1%E8%AF%AD%E3%80%81%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-success)
[![](https://img.shields.io/badge/UI%20%E9%A1%B9%E7%9B%AE-SudokuStudio-%230d1117)](https://github.com/kyoyama-kazusa/Sudoku/tree/main/src/SudokuStudio/SudokuStudio)

[![](https://img.shields.io/badge/GitHub-%E7%82%B9%E6%88%91%EF%BC%81-%230d1117?logo=github)](https://github.com/kyoyama-kazusa/Sudoku)
[![](https://img.shields.io/badge/Gitee-%E7%82%B9%E6%88%91%EF%BC%81-%230d1117?logo=gitee)](https://gitee.com/KyouyamaKazusa/Sudoku)

[![哔哩哔哩](https://img.shields.io/badge/dynamic/json?color=%23fb7299&label=%E5%93%94%E5%93%A9%E5%93%94%E5%93%A9&logo=bilibili&query=%24.data.follower&suffix=%20%E5%85%B3%E6%B3%A8&url=https%3A%2F%2Fapi.bilibili.com%2Fx%2Frelation%2Fstat%3Fvmid%3D23736703)](https://space.bilibili.com/23736703)

## 简介

一个使用暴力破解和普通逻辑算法解题的数独分析解题 SDK。

## 界面

![](miscellaneous/pic/ui.png)

## 项目定位

这个仓库的代码适用于下面这些人群：

* **喜欢钻研数独算法的人**：如果你想要钻研数独有关的实现机制和背后运行机制的话，你可以在这个项目里找到对应的答案。
* **尝试学习 C# 有关特性的人**：这个项目使用了一些较新的 C# 语言特性，并且你可以在这里学到一些 .NET 相关的技术，比如 Windows UI。
* **我自己**：我自己会提前对之前想到的代码和算法进行一些实现，这样就防止我忘记它们的原理和处理规则。

## API 支持

* 题目生成
  * 标准出题器
  * 基于图案的出题器
  * 基于难题图案的出题器
  * 基于必含技巧的出题器
  * 出题筛选器
* 解题
  * 位运算算法
  * 舞蹈链算法
  * 回溯算法（深度优先）
    * 也提供了广度优先的等价实现
  * LINQ 算法
  * 字典查询算法
  * 布尔可满足性归约算法（SAT 归约算法）
* 分析题目
  * 题目难度分析
  * 题目步骤分析
  * 卡点分析
  * 直观技巧特殊支持（如余数性质等）
  * 盘面差异 & 技巧使用分析
  * 对部分超难和仅供研究的技巧的支持
* 绘图
  * GDI+ 项目盘面绘图功能（增删绘图节点、盘面背景色配置、底部文字配置）
  * UI 项目增量绘图性能优化
* 文本处理
  * 各种盘面字符串导入导出
  * JSON 可序列化众多数独相关数据结构
* 其他分析软件的兼容
  * HoDoKu 所有技巧信息的集成，以及绝大部分技巧的实现
  * Sudoku Explainer 绝大部分技巧信息的集成，以及绝大部分技巧的实现

### 技巧支持

* 直观技巧
  * 同区九缺一（Full House）
  * 同数九缺一（Last Digit）
  * 排除
  * 唯一余数
* 局标候选数技巧
  * 直观区块（区块排除、区块唯一余数）
  * 直观数组（数组排除、数组唯一余数）
* 全标候选数技巧
  * 区块类技巧
    * 区块
    * 割补（LoL）
    * 欠一数组（ALC）
    * 烟花数组
  * 数组
    * 显性数组
      * 死锁数组
      * 区块数组
      * 显性数组
    * 隐性数组
      * 隐性死锁数组
      * 隐性数组
  * 鱼（含鱼鳍逻辑）
    * 普通鱼
    * 复杂鱼
      * 宫内鱼
      * 交叉鱼
  * 链式同数结构
    * 双强链
      * 摩天楼
      * 双线风筝
      * 多宝鱼
    * 空矩形（ER）
  * 短链结构
    * 规则 Wing（XY-Wing、XYZ-Wing 等）
    * 不规则 Wing
      * W-Wing
      * M-Wing
      * 其他（S-Wing、H-Wing、L-Wing）
    * XYZ 环
  * 致命结构
    * 无条件致命结构
      * 唯一矩形（UR）
      * 唯一环（UL）
      * 拓展矩形（XR）
      * 探长致命结构
      * 淑芬致命结构
      * 唯一矩阵
      * 唯一性提示信息覆盖（UCC）
    * 条件致命结构
      * 旋转致命结构
      * 其他致命结构（规格 = 8）
    * 其他
      * 全双值格致死解法（BUG）
      * 反转双数致命结构（反 BUG）
  * 涂色（**只有 API 支持**）
    * 简易涂色
      * 色链规则
      * 色分规则
  * 链
    * 非区块链
      * 标准链（AIC）
      * 连续环（CNL）
    * 有名字的链
      * 远程数对
        * 标准远程数对
        * 复杂远程数对
    * 区块链
      * 区块链（GAIC）
      * 区块环（GCNL）
      * 节点重叠
    * 绽放逻辑
      * 绽放环
    * 鳍逻辑
      * 鳍链
      * 区块鳍链
  * 强制链
    * 区域强制链
    * 单元格强制链
    * 矩形强制链
    * 全双值格致死解法强制链
  * 动态链
    * 线性动态链
      * 线性动态链
      * 线性动态区块链
      * Bowman's Bingo
    * 动态强制链
      * 动态单元格强制链
      * 动态区域强制链
      * 动态矛盾强制链
      * 动态二值矛盾强制链
  * 待定数组（ALS）
    * 链式待定数组
      * 待定数组・双强链
      * 待定数组・XY-Wing
      * 待定数组・W-Wing
      * 待定数组・链
    * 伪数组
    * 胖姨环
    * 对交空矩形
    * 死亡绽放
      * 单元格绽放
      * 区域绽放
      * n 次 ALS 绽放
  * 秩逻辑
    * 0 秩逻辑
      * 融合待定数组（SDC，标准类型、孤立数类型和自噬类型）
      * 三维融合待定数组
      * 多米诺环
      * 网（MSLS）
    * 负秩逻辑
      * 守护者
      * 双值死环（即死环）
      * 三值死环（“三顺一逆”规则）
  * 飞鱼
    * 初级飞鱼（JE）
    * 高级飞鱼（SE）
    * 双飞鱼
    * 衰弱飞鱼（WE）
    * 复杂飞鱼
      * 复杂初级飞鱼
      * 复杂高级飞鱼
  * 对称性
    * 宇宙（GSP，标准和反演类型）
  * 枚举类技巧
    * 均衡数组
  * 爆破类技巧
    * 图案叠加删数
    * 模板
    * 计算机试数（BF）

> 其中的“不规则 Wing”技巧就是除了 Broken Wing（守护者）以外的所有名字带 Wing 的技巧类型。这些技巧一般长度为 5，三强两弱，只比双强链多了一个强链关系，因此在国内一般称为“短链结构”，包含 W-Wing、M-Wing、Hybrid-Wing、Local-Wing 和 Split-Wing 五种，以及一个叫 Purple Cow 的技巧（这个技巧名称特殊，但也归在这个范围之中）。其中，只有 W-Wing 单独有实现规则的代码，剩下的短链结构全部在“交替推导链”（即 AIC）的代码里实现。

## 关于该仓库的复刻（Fork）及代码拉取请求（Pull Requests）

当然，你可以复制这个仓库到你的账号下，然后做你想做的任何事情。你可以在基于 [MIT](https://github.com/kyoyama-kazusa/Sudoku/blob/main/LICENSE) 开源协议的情况下做你任何想做的事情。不过，由于 Gitee 是从 GitHub 拷贝过来的，所以 Gitee 项目暂时不支持任何的代码拉取请求，敬请谅解；不过这两个仓库都可以创建 issue。

## 开源协议

### `Sudoku.Drawing.Ocr` 代码项目

该项目比较特殊。该项目的代码使用到了 [EmguCV](https://github.com/emgucv/emgucv) 仓库里的 API 以及部分源代码。根据源仓库的开源协议规则和规范，该项目也需要严格遵守此点。因此它采用的是独立的开源协议。请查看该项目在仓库里的 `LICENSE-OCR` 文件。

### 仓库文件夹 docs

仓库里还有一个文件夹 `docs` 放的是文档项目：

* 完整的数独教程（docs/tutorial）
* Sudoku Studio 软件使用手册（docs/manual）

那两个项目因为是文档项目，所以采用不同的协议 - [CC-BY-4.0 International](https://github.com/kyoyama-kazusa/Sudoku/blob/main/LICENSE-WIKI) 协议。

### 其他项目

本仓库的其他项目都采用 MIT 开源协议。MIT 开源协议意味着**你需要在你的应用程序里声明本项目的作者信息**。

## 行为准则

为了建立好良好的讨论氛围和代码提交规范，本项目使用了默认规定的行为准则。请查看[行为准则](CODE_OF_CONDUCT)文件。

## 作者

一个热爱开发的非开发人员。

如果要联系我，请查看 [CONTACTS-zh-cn](CONTACTS-zh-cn.md) 文件了解我的联系方式。
