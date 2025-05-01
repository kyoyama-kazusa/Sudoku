# Table of contents

## 数独概述 <a href="#sudoku-basics" id="sudoku-basics"></a>

* [初来乍到](README.md)
* [坐标](sudoku-basics/02-coordinate.md)
* [数独历史](sudoku-basics/03-history-of-sudoku.md)

## 直观技巧 <a href="#direct-technique" id="direct-technique"></a>

* [排除](direct-technique/01-crosshatching.md)
* [唯一余数](direct-technique/02-naked-single.md)
* [剩余数的概念](direct-technique/03-concept-of-lasting.md)

## 局部标记技巧 <a href="#partial-marking-technique" id="partial-marking-technique"></a>

* [割补（LoL）](partial-marking-technique/01-law-of-leftover.md)
* [直观区块](partial-marking-technique/02-direct-locked-candidates.md)
* [直观数组](partial-marking-technique/03-direct-subset/README.md)
  * [直观隐性数组](partial-marking-technique/03-direct-subset/01-direct-hidden-subset.md)
  * [直观显性数组](partial-marking-technique/03-direct-subset/02-direct-naked-subset.md)
* [直观复杂出数](partial-marking-technique/04-direct-complex-single.md)

## 基础候选数技巧 <a href="#candidate-technique-basics" id="candidate-technique-basics"></a>

* [候选数的概念](candidate-technique-basics/01-concept-of-candidate.md)
* [直观和局标技巧在全标下的样子](candidate-technique-basics/02-looking-of-direct-and-partial-marking-techniques-in-full-marking-grids.md)
* [标准鱼](candidate-technique-basics/03-normal-fish/README.md)
  * [鱼的基本推理](candidate-technique-basics/03-normal-fish/01-reasoning-of-normal-fish.md)
  * [鳍鱼](candidate-technique-basics/03-normal-fish/02-finned-fish.md)
  * [退化鱼](candidate-technique-basics/03-normal-fish/03-sashimi-fish.md)
  * [孪生鱼](candidate-technique-basics/03-normal-fish/04-sashimi-fish.md)
  * [鱼的直观和互补性](candidate-technique-basics/03-normal-fish/05-direct-view-of-fish-and-law-of-complement-of-fish.md)
  * [鱼的命名](candidate-technique-basics/03-normal-fish/06-naming-of-fish.md)
* [XY-Wing 及推广](candidate-technique-basics/04-regular-wings/README.md)
  * [XY-Wing 及推广的基本推理](candidate-technique-basics/04-regular-wings/01-reasoning-of-regular-wings.md)
  * [XY-Wing 及推广的残缺逻辑](candidate-technique-basics/04-regular-wings/02-incomplete-regular-wings.md)
* [W-Wing](candidate-technique-basics/05-w-wing.md)
* [唯一矩形（UR）](candidate-technique-basics/06-unique-rectangle/README.md)
  * [唯一矩形的基本推理](candidate-technique-basics/06-unique-rectangle/01-reasoning-of-unique-rectangle.md)
  * [唯一矩形的类型](candidate-technique-basics/06-unique-rectangle/02-subtypes-of-unique-rectangle.md)
  * [残缺唯一矩形](candidate-technique-basics/06-unique-rectangle/03-incomplete-unique-rectangle.md)
* [可规避矩形（AR）](candidate-technique-basics/07-avoidable-rectangle.md)
* [唯一环（UL）](candidate-technique-basics/08-unique-loop/README.md)
  * [唯一环的基本推理](candidate-technique-basics/08-unique-loop/01-reasoning-of-unique-loop.md)
  * [唯一环的形成条件](candidate-technique-basics/08-unique-loop/02-condition-of-forming-unique-loop.md)
  * [唯一环的规格推广](candidate-technique-basics/08-unique-loop/03-size-extended-unique-loop.md)
* [拓展矩形（XR）](candidate-technique-basics/09-extended-rectangle/README.md)
  * [拓展矩形的基本推理](candidate-technique-basics/09-extended-rectangle/01-reasoning-of-extended-rectangle.md)
  * [拓展矩形的规格推广](candidate-technique-basics/09-extended-rectangle/02-size-extended-extended-rectangle.md)
* [全双值格致死解法（BUG）](candidate-technique-basics/10-bivalue-universal-grave/README.md)
  * [全双值格致死解法的基本推理](candidate-technique-basics/10-bivalue-universal-grave/01-reasoning-of-bivalue-universal-grave.md)
  * [全双值格致死解法的其他类型](candidate-technique-basics/10-bivalue-universal-grave/02-other-types-of-bivalue-universal-grave.md)
* [欠一数组（ALC）](candidate-technique-basics/11-almost-locked-candidates.md)
* [融合待定数组（SdC）](candidate-technique-basics/12-sue-de-coq.md)
* [跨区数组（DDS）](candidate-technique-basics/13-distributed-disjoint-subset.md)
* [伪数组（ESP）](candidate-technique-basics/14-extended-subset-principle.md)
* [均衡数组](candidate-technique-basics/15-aligned-exclusion.md)
* [烟花数组](candidate-technique-basics/16-firework-subset/README.md)
  * [烟花数组的基本推理](candidate-technique-basics/16-firework-subset/01-reasoning-of-firework-subset.md)
  * [烟花数组的各种用法](candidate-technique-basics/16-firework-subset/02-usages-on-firework-subset.md)

## 链理论 <a href="#chain-theory" id="chain-theory"></a>

* [双强链](chain-theory/01-two-strong-link-chain.md)
* [同数链和异数链](chain-theory/02-x-chain-and-multidigit-chain/README.md)
  * [同数链和异数链的定义](chain-theory/02-x-chain-and-multidigit-chain/01-definition-of-x-chain-and-multidigit-chain.md)
  * [头尾异数链的删数规则](chain-theory/02-x-chain-and-multidigit-chain/02-elimination-rule-on-nodes-with-different-digits.md)
  * [不连续环的两种模式](chain-theory/02-x-chain-and-multidigit-chain/03-two-types-of-discontinuous-nice-loop.md)
  * [有技巧名的异数链](chain-theory/02-x-chain-and-multidigit-chain/04-named-multidigit-chain.md)
* [区块链](chain-theory/03-grouped-chain.md)
* [待定数组链（ALS 链）](chain-theory/04-almost-locked-set-chain/README.md)
  * [链关系的第二定义](chain-theory/04-almost-locked-set-chain/01-the-second-definition-of-inferences.md)
  * [有技巧名的待定数组结构](chain-theory/04-almost-locked-set-chain/02-named-almost-locked-set.md)
  * [在链里的待定数组](chain-theory/04-almost-locked-set-chain/03-other-use-cases-of-almost-locked-set.md)
* [隐性待定数组链（AHS 链）](chain-theory/05-almost-hidden-set-chain.md)
* [毛刺数组链](chain-theory/06-burred-subset-chain.md)
* [待定唯一矩形链（AUR 链）](chain-theory/07-almost-unique-rectangle-chain.md)
* [待定可规避矩形链（AAR 链）](chain-theory/08-almost-avoidable-rectangle-chain.md)
* [环](chain-theory/09-loop/README.md)
  * [环的基本推理](chain-theory/09-loop/01-reasoning-of-loop.md)

## 附录 <a href="#appendix" id="appendix"></a>

* [术语索引](appendix/01-terms.md)

## 逻辑学基础 <a href="#logic-basics" id="logic-basics"></a>

* [逻辑学简要介绍](logic-basics/01-brief-introduction-to-logic.md)
* [分情况讨论和析取消去](logic-basics/02-proof-of-cases-and-disjunction-elimination.md)
* [反证法](logic-basics/03-proof-by-contradiction.md)

## 组合数学基础 <a href="#combinatorics" id="combinatorics"></a>

* [抽屉原理/鸽巢原理](combinatorics/01-pigeonhole-principle.md)

## 其他 <a href="#miscellaneous" id="miscellaneous"></a>

* [作者介绍](miscellaneous/author-information.md)
* [版权声明](miscellaneous/copyright-statement.md)
