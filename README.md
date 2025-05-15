<center>语言切换：<a href="README-zh-cn.md">简体中文</a><br/><img src="miscellaneous/pic/icon.png" /></center>

[![stars](https://img.shields.io/github/stars/kyoyama-kazusa/Sudoku?color=097abb)](https://github.com/kyoyama-kazusa/Sudoku/stargazers)
[![issues](https://img.shields.io/github/issues/kyoyama-kazusa/Sudoku?color=097abb)](https://github.com/kyoyama-kazusa/Sudoku/issues)
[![license](https://img.shields.io/github/license/kyoyama-kazusa/Sudoku?color=097abb)](https://github.com/kyoyama-kazusa/Sudoku/blob/main/LICENSE)

![](https://img.shields.io/badge/Programming%20Language-C%23%2014-%23178600)
![](https://img.shields.io/badge/Framework-.NET%209-blueviolet)
![](https://img.shields.io/badge/Indenting-Tabs-lightgrey)
![](https://img.shields.io/badge/IDE-Visual%20Studio%202022%20v17.14%20Preview%207-%23cf98fb?logo=Visual%20Studio)
![](https://img.shields.io/badge/Language-English%2C%20Simplified%20Chinese-success)
[![](https://img.shields.io/badge/UI%20Project-SudokuStudio-%230d1117)](https://github.com/kyoyama-kazusa/Sudoku/tree/main/src/SudokuStudio/SudokuStudio)

[![](https://img.shields.io/badge/GitHub%20repo-Here!-%230d1117?logo=github)](https://github.com/kyoyama-kazusa/Sudoku)
[![](https://img.shields.io/badge/Gitee%20repo-Here!-%230d1117?logo=gitee)](https://gitee.com/KyouyamaKazusa/Sudoku)

[![bilibili](https://img.shields.io/badge/dynamic/json?color=%23fb7299&label=bilibili&logo=bilibili&query=%24.data.follower&suffix=%20followers&url=https%3A%2F%2Fapi.bilibili.com%2Fx%2Frelation%2Fstat%3Fvmid%3D23736703)](https://space.bilibili.com/23736703)

## Introduction

A sudoku handling SDK using brute forces and logical techniques.

## Preface

![](miscellaneous/pic/ui-en.png)

## Repository Positioning

This repository is created for the following users:

* **who likes sudoku algorithms**: If you like to study for sudoku techniques and its backing implementation and running mechanism, you can find answer in this repository.
* **who wants to learn about C#-related features**: This repository uses some C# newer features, which can help you learn about it.
* **myself**: The codes are very important for me to learn sudoku and related algorithm.

## API Supports

* Puzzle generation
  * Standard generator
  * Pattern-based generator
  * Hard-pattern-based generator
  * Technique-based generator
    * Generated puzzle filters
* Puzzle solving
  * Bitwise
  * Dancing Links (*DLX*)
  * Backtracking
    * Also provides BFS equivalent implementation
  * LINQ-based
  * Dictionary-based
  * Boolean Satisfiability Reduction (*SAT*)
* Puzzle analysis
  * Difficulty rating
  * Step-by-step analysis
  * Bottleneck analysis
  * Diff analysis & technique usage analysis
  * Specialized supports on direct & partial-marking techniques
  * Partial implementation on some extremely hard techniques
* Drawing
  * GDI+ drawing functions
* Text
  * I/O handling on variant grid text types (Susser, HoDoKu library format, etc.)
  * JSON serialization and deserialization on most of data structures
* Compatibility with other programs
  * Integrated information on all HoDoKu techniques, and analysis supports
  * Integrated information on most of Sudoku Explainer techniques, and analysis supports

## Technique Supports

This solution supports many kinds of human-friendly techniques. Here I will list them.

* Direct Techniques
  * Full House
  * Last Digit
  * Hidden Single (Crosshatching)
  * Naked Single
* Partial-Marking Techniques
  * Direct Intersection
  * Direct Subset
  * Complex Singles
* Full-Marking Techniques
  * Intersections
    * Locked Candidates (*LC*)
    * Law of Leftover (*LoL*)
    * Almost Locked Candidates (*ALC*)
    * Firework Subsets
  * Subsets
    * Naked Subsets
      * Locked Subsets
      * Semi-Locked Subsets
      * Normal Naked Subsets
    * Hidden Subsets
      * Locked Hidden Subsets
      * Normal Hidden Subsets
  * Fishes
    * Normal Fishes
    * Complex Fishes
      * Franken Fishes
      * Mutant Fishes
  * Single-Digit Patterns (*SDP*)
    * Two Strong Links
      * Skyscraper
      * Two-String Kite
      * Turbot Fish
    * Empty Rectangle
  * Wings
    * Regular Wing (XY-Wing, XYZ-Wing, etc.)
    * Irregular Wing[^1]
      * Woods' Wing (*W-Wing*)
      * Medusa Wing (*M-Wing*)
      * Split/Hybrid/Local Wing (*S/H/L-Wing*)
    * XYZ-Loop
  * Deadly Patterns
    * Unconditional Deadly Patterns
      * Unique Rectangle (*UR*)
      * Unique Loop (*UL*)
      * Extended Rectangle (*XR*)
      * Borescoper's Deadly Pattern
      * Qiu's Deadly Pattern
      * Unique Matrix (*UM*)
      * Uniqueness Clue Cover (*UCC*)
    * Conditional Deadly Patterns
      * Rotating Deadly Pattern
      * Anonymous Deadly Pattern (Size = 8)
    * Miscellaneous
      * Bi-value Universal Grave (*BUG*)
      * Reverse Bi-value Universal Grave (*Reverse BUG*)
  * Coloring (**Only in API**)
    * Simple Coloring
      * Simple Coloring Wrap
      * Simple Coloring Trap
  * Chains
    * Non-grouped Chains
      * Alternating Inference Chains (*AIC*)
      * Continuous Nice Loops (*CNL*)
    * Well-known Chains
      * Remote Pair
        * Standard Remote Pair
        * Complex Remote Pair
    * Grouped Chains
      * Grouped Alternating Inference Chains (*GAIC*)
      * Grouped Continuous Nice Loops (*GCNL*)
      * Node Collision
    * Blossom logic
      * Blossom Loop
    * Finned logic
      * Finned Chain
      * Grouped Finned Chain
  * Forcing Chains
    * Region Forcing Chains
    * Cell Forcing Chains
    * Rectangle Forcing Chains
    * Bi-value Universal Grave + n Forcing Chains
  * Dynamic Chains
    * Sequential Dynamic Chains
      * Whip
      * Grouped Whip
    * Dynamic Forcing Chains
      * Dynamic Cell Forcing Chains
      * Dynamic Region Forcing Chains
      * Dynamic Contradiction Forcing Chains
      * Dynamic Double Forcing Chains
  * Almost Locked Sets (*ALS*)
    * Chaining ALSes
      * Almost Locked Sets XZ Rule (*ALS-XZ*)
      * Almost Locked Sets XY-Wing (*ALS-XY-Wing*)
      * Almost Locked Sets W-Wing (*ALS-W-Wing*)
    * Extended Subset Principle (*ESP*)
    * Fat Ring
    * Empty Rectangle Intersection Pair (*ERIP*)
    * Death Blossom
  * Rank Logic
    * 0 Ranks
      * Sue de Coq (SdC)
      * 3-Dimensional Sue de Coq (*3D SdC*)
      * Domino Loop
      * Multi-sector Locked Sets
    * Negative Ranks
      * Guardian
      * Bi-value Oddagon
      * Chromatic Pattern (i.e. Tri-value Oddagon)
  * Exocets
    * Junior Exocet (*JE*)
    * Senior Exocet (*SE*)
    * Double Junior Exocet (*Double JE*)
    * Weak Exocet (*WE*)
    * Complex Exocet
      * Complex Junior Exocet (*Complex JE*)
      * Complex Senior Exocet (*Complex SE*)
  * Symmetrical Placements
    * Gurth's Symmetrical Placement (*GSP*)
      * Standard Type
      * Anti Type
  * Permutations
    * Aligned Exclusion
  * Last Resorts
    * Bowman's Bingo
    * Pattern Overlay
    * Templating
    * Brute Force

## Forks & PRs (Pull Requests) for This Repo

Of course you can fork my repo and do whatever you want. You can do whatever you want to do under the [MIT license](https://github.com/kyoyama-kazusa/Sudoku/blob/main/LICENSE-CODE). However, due to the copy of the GitHub repo, Gitee repo doesn't support any PRs. I'm sorry. But you can create the issue on both two platforms. Please visit the following part "Basic Information" for learning about more details.

## Open source license

### Special project: `Sudoku.Drawing.Ocr`

This project uses some APIs and source code from repo [EmguCV](https://github.com/emgucv/emgucv). Therefore, the project uses a standalone open-source license. For more information please visit the file `LICENSE-OCR` in that project in source code.

### Repository folder `docs`

This folder contains two projects:

* Sudoku Tutorial (docs/tutorial)
* SudokuStudio Manual (docs/manual)

Two projects uses markdown rendering engine instead of code. Therefore, those two projects will adopt different license - [CC-BY-4.0 International](https://github.com/kyoyama-kazusa/Sudoku/blob/main/LICENSE-WIKI).

### The others

All the other projects use **MIT license**, which means you should mention the copyright of the author of the repository if you want to use code snippet in this repository.

## Code of Conduct

Please see file [CODE_OF_CONDUCT](CODE_OF_CONDUCT).

## Author

I'm not a developer, but I like it!

Please visit file [CONTACTS](CONTACTS.md) to contact me if you want.

[^1]: W-Wing is implemented by a single technique searcher type, while other irregular wing types are implemented by AIC searchers.
