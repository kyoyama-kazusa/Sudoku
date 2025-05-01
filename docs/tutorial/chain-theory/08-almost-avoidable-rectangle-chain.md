---
description: Almost Avoidable Rectangle Chain
---

# 待定可规避矩形链（AAR 链）

今天来看一篇短小的内容。因为这种用法并不算多，所以本文只有一个例子。

## 待定可规避矩形的弱链关系 <a href="#weak-inference-in-almost-avoidable-rectangle" id="weak-inference-in-almost-avoidable-rectangle"></a>

<figure><img src="../.gitbook/assets/images_0330.png" alt="" width="375"><figcaption><p>弱链关系</p></figcaption></figure>

如图所示。链的写法如下：

```
9r4c6=9r4c2-9r6c7=9r5c8
```

其实还算一个比较简单的链，只有两个强链关系。不过这个链里用到的弱链关系是直接连接了完全不相关的候选数 9。这个其实是可规避矩形的功劳：如果 `r4c2` 和 `r6c7` 都填 9，则四个单元格 `r46c27` 将构成关于 8 和 9 的可规避矩形的矛盾情况。所以这两个 9 是不同真的。不同真即为弱链关系，所以我们可以直接串到链里使用。

我们把这个图里 `r46c27` 四个单元格的候选数 8 和 9 这种结构称为**待定可规避矩形**（Almost Avoidable Rectangle，简称 AAR）；而把使用了待定可规避矩形的链就称为**待定可规避矩形链**（Almost Avoidable Rectangle Chain，简称 AAR Chain）。

因为待定可规避矩形出现频次极低，在实战里也较少应用，因此例子并不多。

## 待定可规避矩形的强链关系 <a href="#strong-inference-in-almost-avoidable-rectangle" id="strong-inference-in-almost-avoidable-rectangle"></a>

<figure><img src="../.gitbook/assets/images_0331.png" alt="" width="375"><figcaption><p>强链关系</p></figcaption></figure>

如图所示。这甚至还是刚才那个题。链的写法如下：

```
5r6c7=4r4c2-4r1c2=5r23c1
```

这次我们用一下里面的强链关系。刚才我们不是用到了弱链关系不同真的逻辑吗？这次我们不看这两个 9 的关系。我们发现，这两个空格上都还包含一个多出来的候选数。显然，这两个候选数是不同假的。同假就意味着两个单元格变为唯一余数，则必须填 9 致使出现矛盾。

然后，我们利用这个强链就可以得到这个题里的删数结论了。
