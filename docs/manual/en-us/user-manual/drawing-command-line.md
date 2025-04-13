# Drawing Command Line

This page will help you use drawing command line.

## Looking

<figure><img src="../.gitbook/assets/image.png" alt=""><figcaption><p>Page looking</p></figcaption></figure>

This page will allow you using command line to draw pictures.

## Command line syntax

As you can see, the right text box will allow you write command line to draw elements.

This program supports the following items to be drawn:

* Cell highlight
* Candidate highlight
* House highlight
* Chute highlight
* Icons
* Links
* Baba grouping

The basic command line syntax is based on normal command line. **However, it's not supported for using quotes `"` to escape whitespaces**.

For the detail of commands, I will list them below.

## Detail

For example, the following commands

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

can produce such output:

<figure><img src="../.gitbook/assets/image (1).png" alt=""><figcaption><p>Output</p></figcaption></figure>

### Load puzzle

You should use `load` command to load a puzzle, with an option to specify candidates displaying:

```bash
load [+c|-c] <puzzle>
```

where `+c` will display candidates, and `-c` won't. If omitted, `-c` will be default.

### Cell highlight

If you want to highlight a cell, you can use `cell` command:

```bash
cell <color-identifier> <cell|cell-group>
```

For the part `color-identfier`, I will mention it below.

### Candidate highlight

If you want to highlight a candidte, you should use `candidate` command:

```bash
candidate <color-identifier> <candidate|candidate-group>
```

In addition, if you want to append all candidates from a cell appeared in a grid, you can just use cell notation instead. For example, `r1c1` represents all candidates in `r1c1`, instead of writting multiple candidates like `r1c1(13579)`. However, this command must be placed after `load` command because it relies on a real grid to reference states of candidates.

### House highlight

If you want to highlight a house, you should use `house` command:

```bash
house <color-identifier> <house|house-group>
```

### Chute highlight

If you want to highlight a chute, you can use `chute` command:

```bash
chute <color-identifier> <chute|chute-group>
```

### Icon

Icons are some shapes displayed in a cell. You can use `icon` command to output an icon:

```bash
icon <color-identifier> <icon-shape> <cell|cell-group>
```

The supported icon shapes are:

* circle
* cross
* diamond
* heart
* square
* star
* triangle

### Baba grouping

Baba grouping allows you outputting a character into a cell, displaying as a variable indicating the possible candidates can be chosen in the containing cell.

You can use `baba` command:

```bash
baba <color-identifier> <character> <cell|cell-group>
```

### Links

The program also supports for outputting links. You can use `link` command:

```bash
link <color-identifier> <link-shape> <start-candidates> <end-candidates> [<extra>]
```

By the way, the supported link shapes are:

* cell: a cell link (used by Unique Loop)
* chain: a chain link
* conjugate: a conjugate pair link (used by Type 4 of Deadly Patterns)

For example, `link !n chain r1c1(1) r1c2(1) =` will create a strong link between `r1c1(1)` and `r1c2(1)`. The equals token `=` represents a strong link, and minus token `-` represents a weak link.

In addition, grouped nodes are also supported.

## Coordinate syntax

For the coordinates mentioned above, you should use one of RxCy, K9, Excel notation defined in settings page:

<figure><img src="../.gitbook/assets/image (2).png" alt=""><figcaption><p>Coordinate kind in settings page</p></figcaption></figure>

## Color identifier syntax

For color identifiers, the program supports for 3 kinds of identifiers:

* Hexadecimal-based color string (e.g. `#FFFFFF` indicating the color white)
* Aliased string (e.g. `!elimination` indicating the color used by an elimination)
* Palette ID (e.g. `&5` indicating the 5th color in palette)

### Hexadecimal-based color

You can declare a color by using 6 or 8 characters to describe a color by obeying RGB or ARGB color representing rule. Casing are ignored.

### Aliased string

You can also use aliased names to describe a color, meaning what environment the color is used.

The supported values are:

* **`normal` or `n`**: a normal color
* **`auxiliary` or `aux`**: an auxiliary color used. The possible values are `auxiliary1`, `auxiliary2`, `auxiliary3`, `aux1`, `aux2` and `aux3`
* **`assignment` or `a`**: an assignment
* **`overlapped_assignmented`, `overlapped` or `o`**: an assignment overlapping with another colored candidate
* **`elimination`, `elim` or `e`**: an elimination
* **`cannibalism`, `cannibal` or `c`**: an elimination overlapping with another colored candidate
* **`exofin` or `f`**: a fin
* **`endofin` or `ef`**: a fin overlapping with fish body
* **`link` or `l`**: a link color
* **`almost_locked_set` or `als`**: an ALS pattern. The possible values are `almost_locked_set1`, `almost_locked_set2`, `almost_locked_set3`, `almost_locked_set4`, `almost_locked_set5`, `als1`, `als2`, `als3`, `als4` and `als5`
* **`rectangle` or `r`**: a rectangle pattern. The possible values are `rectangle1`, `rectangle2`, `rectangle3`, `r1`, `r2` and `r3`

### Palette ID

This program also supports 15 different colors used in palette, which supports customization in settings page. You can change them to different colors in order to rich the looking.

The supported IDs are from 1 to 15. Hex characters `A` to `F` are also supported to represent IDs 10 to 15.

## Explanation of example

From the above syntax we learnt, the command lines can be easy-comprehended.

```bash
# load a puzzle, without candidates shown
load -c 1...8...4..7..28...2...4.5....3.....7.5...4.8.....6....1.6...4...61..5..3...5...2

# show candidate r23c5(1) with normal color
candidate !n r23c5(1)

# show candidate r78c5(2) with normal color
candidate !n r78c5(2)

# show block 2 and 8 with normal color
house !n b28

# show column 5 with auxiliary color 1
house !aux1 c5

# display circles with normal color to the following cells
icon !n circle r1c1,r2c6,r4c4,r5c1379,r6c6,r8c4,r9c9

# display a diamond to r5c5, with auxiliary color 3
icon !aux3 diamond r5c5

# display a cross sign to the following cells with auxiliary color 1
icon !aux1 cross r1c6,r123c4,r789c6,r9c4

# show candidate 1 and 2 from cell r5c5 with a color same as elimination
candidate !e r5c5(12)

# show candidate 9 from cell r5c5 with a color same as assignment,
# with other shown candidates overlapped
candidate !o r5c5(9)
```
