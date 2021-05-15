Inkscape 1.0.2
------------

Released on **2021-01-17**

Release highlights
------------------

This is a bugfix release:

- More granular controls for canvas zooming and rotation
- Fixes extensions popping up when a clipboard manager was used
- Several crashes fixed





Inkscape 1.0.1
------------

Released on **2020-09-07**.

Release highlights
------------------

- Selectors/CSS dialog is now available
- Experimental color-managed PDF export through Scribus
- Many crash fixes and bugs fixed



### Selectors and CSS dialog

The Selectors and CSS dialog that was hidden and labelled as 'experimental' in
Inkscape 1.0 is now available from the Object menu in 1.0.1. This new dialog
allows users to edit a CSS stylesheet for the document and also to select all
objects with a certain CSS selector, thus providing a replacement for the
Selection Sets dialog, that had to be removed for Inkscape 1.0.

### Scribus PDF export

An experimental PDF export extensions was added that uses Scribus 1.5.5+ if it
can be found in the path. You'll need to use a color profile in the SVG file
and make sure to double-check the exported image, as there are many SVG
features not supported by Scribus.




Inkscape 1.0
------------

Released on **2020-05-01**.

Release highlights
------------------

-   [Theming support and more new customization
    options](#customization)
-   [Better HiDPI (high resolution) screen support](#high-resolution-screens)
-   [Native support for macOS with a signed and notarized .dmg
    file](#mac-application)
-   [Coordinate origin in top left corner by
    default](#y-axis-inversion)
-   [Canvas rotation and mirroring](#canvas)
-   [On-Canvas alignment of objects](#on-canvas-alignment)
-   [Split view and X-Ray modes](#view)
-   [PowerPencil for drawing editable, variable width strokes with a
    pressure sensitive graphics tablet](#powerpencil)
-   [New PNG export options](#export-png-images)
-   [Integrated centerline tracing for vectorizing line
    drawings](#trace-bitmap)
-   [Searchable Symbols dialog](#symbols)
-   [New Live Path Effect (LPE) selection
    dialog](#live-path-effects)
-   [New Corners (Fillet/chamfer) LPE, (lossless) Boolean Operation LPE
    (experimental), Offset LPE and Measure Segments LPE (and
    more!)](#live-path-effects)
-   [Path operations, deselection of a large number of paths as well as
    grouping/ungrouping are much faster now](#performance)
-   [Much improved text line-height settings](#text-tool)
-   [Variable fonts support (only if compiled with pango library
    version >= 1.41.1)](#variable-font-support)
-   [Browser-compatible flowed text](#browser-compatible-flowed-text)
-   [Extensions programming interface updated, with many new
    options](#third-party-extensions) - *Note: this introduces
    breaking changes, some third-party extensions will have to be
    updated to work with Inkscape 1.0*
-   [Python 3 support for extensions](#for-extension-writers)

General: Application
--------------------

### Performance

Lots of small performance improvements in various areas combine to make
Inkscape run smoother than before:

-   Accelerated breaking apart of paths and Boolean operations (by
    disabling intermittent canvas rendering during these operations).
-   Accelerated "deselect" speed (by improving internal data structure
    algorithms).
-   Faster grouping and ungrouping of groups with a large number of
    objects
-   Speed boost to operations on large sets of objects in the Objects
    dialog (Issue [\#392](https://gitlab.com/inkscape/inbox/-/issues/392))
-   Faster lookup of attributes (MR
    [\#448](https://gitlab.com/inkscape/inkscape/-/merge_requests/448))

### Mac Application

Inkscape is now a first-rate native macOS application, and no longer
requires XQuartz to operate. The minimum required operating system
version is OS X El Capitan 10.11.

It has a standard Mac-style menu bar (rather than a menu bar within the
window). Keyboard shortcuts now use the command (<kbd>⌘</kbd>) key
rather than the control key. Retina display screen resolution is now
supported. The build is now cleanly 64-bit, a prerequisite for macOS
Catalina 10.15 and beyond. It comes bundled with Python 3 to power
Inkscape extensions.

General User Interface
----------------------

### Modernized GUI Toolkit

The user interface has been changed to utilize a more recent version of
GTK+ (GTK+ 3). This is a collection of user interface building blocks
that Inkscape uses to draw the user interface on the screen. This new
version brings many improvements, especially for users of HiDPI (high
resolution) screens.

Bringing the newer GTK+ toolkit to Inkscape has been an eagerly
anticipated goal for a long time. It has taken much effort by the team
involved and it was a focus point of the [Boston
Hackfest](https://inkscape.org/en/news/2018/05/22/2018-boston-inkscape-hackfest/).

### Window position / size / behavior

Improvements and fixes to the code for handling/restoring window size
and position
[1](https://gitlab.com/inkscape/inkscape/-/merge_requests/180) were made.
The window manager handles most of the jobs now, which should make it
much more robust. If you still encounter problems with this, please
report those [to our bug tracker](https://inkscape.org/report).

The dialog opacity options have been removed, because they were causing
crashes that could not be fixed otherwise.

### High Resolution Screens

Inkscape now supports HiDPI (high resolution) screens natively. This
means that icons and node handles will no longer be tiny on monitors
with large pixel dimensions, and the canvas will no longer look blurry.

### Tool box

The tools have been reordered and grouped into more logical groups.
Groups from top to bottom:

-   Edit tools
-   Create Shapes
-   Create Shapes from scratch
-   Color Editing
-   Other tools
-   Canvas tools.

Users can customize the order of the tools by putting a customized
version of the file `inkscape/share/ui/toolbar-tool.ui` into a folder
named `ui` in their Inkscape user configuration folder, if they prefer a
different order.

### Improved number entry fields

Many numeric entry fields no longer change their value when scrolling
while the mouse is hovering over them. They now need to be selected
(clicked into). Then scrolling will change the value as previously. This
change was made, because scrolling docked dialogs easily led to
accidental change of values.

Additionally, **<kbd>Ctrl + Scroll</kbd>** on a selected numeric entry
field will now change the value in larger steps.

Canvas
------

### Y-Axis Inversion

During the [Inkscape Hackfest in
Kiel](https://inkscape.org/en/news/2018/09/20/inkscape-hackfest-kiel-2018-what-happened/),
a significant change by Thomas Holder was integrated into the Inkscape
codebase. It sets the origin of the document to the top left corner of
the page. This finally makes the coordinates that a user can see in the
interface match the ones that are saved in the SVG data (unit
conversions/transformations may be required), and makes working in
Inkscape more comfortable for people who are used to this more standard
behavior. The old behavior can be reactivated in the preferences, at
`Edit → Preferences → Interface` (disable 'Origin in upper left with
y-axis pointing down').

### Canvas Rotation

With **<kbd>Ctrl + Shift + Scroll wheel</kbd>** or **<kbd>Ctrl + Scroll
wheel dragging</kbd>** the drawing area can be rotated and viewed from
different angles. In the bottom right corner of the window, the viewing
angle can be entered manually. **<kbd>Right-click</kbd>** to select
between a set of preset values. You can also reset canvas rotation in
`View → Canvas orientation → Reset Rotation`. Keyboard shortcuts for
clockwise/counter-clockwise/no rotation can be set in the preferences.

### Canvas Mirroring

The canvas can now be flipped, to ensure that the drawing does not lean
to one side, and looks good either way. The vertical/horizontal flipping
is available from the menu
`View → Canvas orientation → Flip horizontally / Flip vertically`.
Keyboard shortcuts for flipping the canvas can be set in the preferences
(`Edit → Preferences → Interface → Keyboard shortcuts`).

### Pinch-to-zoom

On supported hardware (trackpad, touchpad, multi-touch screen), the
canvas can be zoomed with the two-finger pinch gesture.

### Duplicate Guides

A new option for duplicating guides was added to the 'Guideline' dialog
(access by double-clicking on a guide line). This new feature, when used
before the 'relative change' option (dialog needs to be opened once for
each step), makes it easier to create guides that are offset by a
certain distance / angle from an existing guide.

### Context menu

The menu that appears after a right-click on the canvas has been
extended with the following items, which makes locking/unlocking and
hiding/unhiding individual objects much more comfortable:

-   Hide selected objects
-   Unhide objects below (the mouse cursor)
-   Lock selected objects
-   Unlock objects below (the mouse cursor)

### Snapping

-   In `Edit → Preferences → Behavior → Snapping`, a new option was
    added to **disable snapping** in new documents or files that are
    opened with Inkscape for the first time.

View
----

### Center view on page

You can now center the view on the page, without changing the zoom
level.

This function is available via:

-   `View → Zoom`
-   the Zoom tool's tool controls bar
-   Keyboard shortcut **<kbd>Ctrl + 4</kbd>**

### Split View Mode

The new Split View Mode features a draggable separator that becomes
visible as soon as the Split view mode has been activated. On one side
of the separator, the canvas will look just like before, while on the
other side, everything will be displayed in outline mode, and objects
can more easily be grabbed with the mouse or edited with the node tool.

It can be moved on the canvas by grabbing either the separation line or
the central handle. The sides can be switched by clicking on one of the
little arrows on the handle.

Activate it with one of:

-   `View → Split View Mode`
-   **<kbd>Ctrl + 6</kbd>**

To deativate the mode, either deactivate the checkbox in the menu again,
use the keyboard shortcut a second time, or drag the separator off the
canvas.

### X-Ray Mode

When the X-Ray mode is active, a circular area that shows objects on the
canvas in outline mode will follow the mouse pointer. This makes editing
complex drawings with many objects layered on top of one another much
easier, and is especially useful when used with the node tool.

Activate it with:

-   `View → XRay Mode`
-   <kbd>Alt + 6</kbd>

The size of the X-Ray circle can be adjusted in
`Edit → Preferences → Rendering → Rendering XRay radius`.

To deactivate the mode, either deactivate the checkbox in the menu
again, or use the keyboard shortcut a second time.

### Visible Hairlines Display Mode

This new display mode is available in the menu under
`View → Display Mode → Visible Hairlines`. It ensures that all lines of
nonzero width are visible (with a minimum visible line width),
regardless of zoom level, while still drawing everything else normally.

This is especially useful if you need to assign very small line widths
for use with CNC machines like laser cutters and vinyl cutters which use
hairlines to denote cut lines. In that case, you will still be able to
see the lines, even when zoomed out.

Paths
-----

### Changed behavior of Stroke to Path

The 'Stroke to Path' command now not only converts the stroke of a shape
to path, but effectively splits it into its components.

In the case of applying it to a path that only has a stroke, the
behavior is unchanged.

For paths that don't only have a stroke, but also a fill and/or markers,
the result will be a group consisting of:

-   Stroke outline turned to path
-   Fill (if there was one)
-   A group of all markers (if applicable; one group per marker,
    consisting of its outline and its fill turned into a path)

### Unlinking Clones for Path Operations

Clones and Symbols are now automatically unlinked, before a Boolean
operation (union, difference, etc.), or one of the Path operations
'Combine', 'Break apart', or 'Stroke to Path' is performed.

A setting in the preferences at `Behavior → Clones → Unlink Clones`
allows the User to disable the automatic unlinking.

Tools
-----

### Bézier Tool

The tool mode 'Create sequence of paraxial segments', which previously
would only draw lines parallel to the x and y axis of the document, now
supports arbitrary starting angles. After the first segment, all further
angles will use the current angle ±90°. If you want your first segment
to work as in previous versions, you need to press **<kbd>Ctrl</kbd>**
after the first click. This will restrict your options to (by default)
15° steps.

### Calligraphy Tool

A new option to add dots has been added to the tool. Click in place
without moving the mouse to create a dot, **<kbd>Shift + Click</kbd>**
to create a larger dot *\[Known issues:
[inbox\#2368](https://gitlab.com/inkscape/inbox/-/issues/2368),
[inbox\#2342](https://gitlab.com/inkscape/inbox/-/issues/2342)\]*.

### Circle Tool

The circle tool can now also create closed ("filleted") circle shapes
(closed arcs) with the click of a button.

### Eraser

-   Added option to control eraser width with a pressure sensitive input
    device.
-   Added thinning, caps, and tremor options (as used for calligraphy
    tool).
-   New option to erase as clip, which allows the User to
    non-destructively erase (parts of) various types of elements,
    including raster images and clones.

### Measurement Tool

Hovering over a path with the tool now displays the length, height,
width, and position of the path. If you hover over a group, it will show
the width, height, and position of the group. Holding
**<kbd>Shift</kbd>** switches to showing info about the constituents of
the group.

The tool also has a new option to only measure selected objects when
using click-and-drag.

### Pencil Tool

#### PowerPencil

Pressure sensitivity can now be enabled for the Pencil tool. This
feature makes use of the PowerStroke Live Path Effect (LPE).

New settings for the tool are available for tweaking the behavior of the
PowerStroke LPE when it is being created with the Pencil tool (and a
graphics tablet/stylus):

-   **Use pressure input** (in the tool controls bar): activates the
    PowerStroke feature, if a pressure sensitive device is available.
-   **Min/Max** (in the tool controls bar): determines the minimal and
    maximal stroke width (0–100%). This does not change the number of
    available pressure levels, but spreads them out in the available
    line width interval.
-   **Caps** (in the tool controls bar): choose between the options
    'butt', 'square', 'round', 'peak' and 'zero width' for the end caps
    of the stroke.
-   Additionally, the PowerStroke LPE itself has been improved, to
    better work when used in this new way, see [the section about LPE
    updates](#powerstroke-lpe-improvements).

### Selection Tool

#### On-Canvas Alignment

When the option "Enable on-canvas alignment" is active in the "Align and
Distribute" dialog, a new set of handles will appear on canvas when an
object is clicked three times (first click: scale handles, second click:
rotation/skew handles).

The handles can be used to align the selected objects relative to the
area of the current selection.

-   **<kbd>Shift + click</kbd>** on the outer handles aligns on the
    outside of the selection area.
-   Clicking on the central handle will align the selected objects on
    the horizontal axis. On **<kbd>Shift + click</kbd>** objects will be
    aligned on the vertical axis.
-   With **<kbd>Ctrl + Shift </kbd>** the whole selection can be aligned
    to its outer boundaries / corners as a group (i.e. it is moved by
    its own `width` and / or `height`).

### Text Tool

#### More Compact Tool Controls Bar

-   Some control buttons that have been mutually exclusive (e.g.
    right-aligned, left-aligned, justified) have been combined into
    drop-down lists, so they now take up less space.
-   Removed the 'Show style of outermost text element' button that made
    settings confusing.

#### Better Line Height Settings

-   Line spacing settings got an overhaul and are now much easier to
    use. Gone is the unwieldy `?` button; this is now handled by
    selecting text on canvas.
-   Setting line height works like this now:
    -   For the whole text: click anywhere into the text without
        selecting anything, then adjust the line height in the tool
        controls bar.
    -   For a specific line: select the text in that line, then adjust
        the line height in the tool controls bar.
    -   Set the global line height first, and the specific line heights
        last. Changing global line height later will remove any
        line-specific line-heights (even just changing the unit will
        unset those ).
    -   Line heights for single selected lines cannot go below the
        height set globally.
    -   If more than one line is selected, the line height in between
        those lines can go below the line height set for the whole text.
-   The line height settings work in all four text types.

#### Improved Text Selection

When clicking on a text object, any click within the whole area of a
text line now selects the text with both selection and text tool
(previously, clicking into the space between two letters did not select
the text).

#### Variable Font Support

If Inkscape has been compiled with a Pango library version that is at
least 1.41.1 (which is the case for the Windows and macOS versions, and
for Ubuntu versions starting from 19.04), it comes with [ support for
variable fonts](https://wiki.inkscape.org/wiki/index.php/Variable_Fonts_support) ([more info about how
this looks](https://www.patreon.com/posts/font-variations-17644963)).
These are fonts that do not come with different faces, but often with
various sliding scales for different font properties, like 'boldness',
or 'condensing', or even playful features.

#### Basic Support for SVG in OpenType

Some OpenType fonts with glyphs saved as SVG render now in Inkscape.

#### Flow order

With `Text → Flow into Frame`, text now flows into the selected frames
in selection order (instead of reversed order previously).

#### Browser-compatible flowed text

**Flowed text** (created by clicking and dragging a text frame) in
previous Inkscape versions was not compatible with browsers, and
rendered as 'black boxes' in web browsers and other SVG viewers, or was
just missing.

The old flowed text can still be enabled by unchecking
`Edit → Preferences → Tools → Text: Use SVG2 auto-flowed text`.

Now, Inkscape offers to use **two new types of flowed text**, that have
a compatible fallback for use with modern web browsers:

1.  **SVG 2 text using the CSS `shape-inside` property:** This new
    flowed text automatically **includes a fallback option** (in SVG
    1.1), and will show up in web browsers and other software. This type
    of text can be created by **clicking and dragging a box** on canvas,
    **if the option 'Use SVG 2 auto-flowed text' is enabled** in the
    Text tool's preferences. When the option is enabled, it will also be
    used for `Text → Flow into frame`. Kerning and letter rotation do
    not work with this type of text.
2.  **SVG 2 text with `inline-size` property:** This is the new "Column
    mode" of the text tool, that can be created by clicking on the
    canvas, typing, and then dragging the diamond-shaped handle at the
    end of the text to determine the width of the text. It creates
    flowed text columns without a predetermined height. Note that this
    text can be left-, center- or right-aligned, but that justification
    is not possible. Kerning and letter rotation also do not work with
    this type of text. These texts include an SVG 1.1 fallback, too. To
    convert the text back to SVG 1.1 text, **<kbd>Ctrl-click</kbd>** on
    the diamond (this will result in all the text being placed on a
    single line).

The fallback option can be disabled in
`Edit → Preferences → Input/Output → SVG export: SVG 2: Insert SVG 1.1 fallback in text`.

#### Overview of available text types

#### SVG 2 Text Support in Detail

Inkscape supports SVG 2 text (multi-line and text in a shape), both
rendering and creating.

There are several types of SVG 2 text:

##### 1. Multi-line text via the CSS 'inline-size' property

The CSS property `inline-size` defines the width (height) of a block of
horizontal (vertical) text. Inkscape supports both rendering and
generating multi-line text via this property. Flowed text using
'inline-size' is not justifiable (it uses the 'text-anchor' property).
This text type has an SVG 1.1 fallback, that is included by default.

##### 2. Multi-line text via the CSS 'shape-inside' property

The CSS property `shape-inside` allows placing text inside a shape.
Inkscape supports this property in both rendering and creating. Inkscape
also supports in rendering the CSS `shape-subtract`, `shape-margin`, and
`shape-padding` properties.

Multi-line text via the `shape-inside` property is a direct replacement
for SVG 1.2 flowed text. SVG 1.2 never became a final W3C standard and
only the Batik SVG renderer besides Inkscape provided any support for
SVG 1.2 flowed text. SVG 2 is not yet a final W3C standard but SVG 2
flowed text has one very important advantage over SVG 1.2 flowed text in
that it is easy to structure the SVG 2 text such that it will be
rendered (almost) correctly by an SVG 1.1 renderer. This means that
browsers which do not support SVG 2 text will still render the text.

##### 3. Multi-line text via the CSS 'white-space' property (only rendering)

The CSS property
['white-space'](https://developer.mozilla.org/en-US/docs/Web/CSS/white-space)
controls how white space is handled. By default, SVG collapses all
adjacent white-space including newlines into a single space. By setting
the `white-space` value to `pre`, `pre-line`, or `pre-wrap`, Inkscape
will respect newlines and generate multi-line text. At the moment, one
must use the XML Editor dialog to change the `white-space` property
value. (Supported by Firefox. No SVG 1.1 fallback created by Inkscape.)

One disadvantage of using SVG 2 text is that it will not be editable as
multi-line text in Inkscape 0.92 (it will still be editable as single
lines of text).

Clipping / Masking
------------------

Clip paths and masks now have an inverse mode in the menu, using the
PowerClip and PowerMask LPEs.

Filters
-------

The size of the filter region can now be adjusted by dragging on two new
diamond-shaped nodes with the node tool. This is especially useful for
blur filters, whose area has been affected by a transformation of the
filtered object.

Live Path Effects
-----------------

### General

Live Path Effects received a major overhaul, with lots of improvements
and new features. The main changes are:

-   **New LPE selection dialog**: the LPE
    list is now made available as a searchable dialog with an icon for
    each path effect, and options to favorite, to switch between list
    and tile view, to display info about the selected effect and to
    apply it. Several additional, experimental path effects are
    available after the corresponding switch has been activated.
-   **Set default parameters**: default values for any LPE can be set in
    the respective LPE's dialogue, when it is applied to an object

(*Note: we have the 'multiple desktop preferences' problem here: If you
have multiple Inkscape windows open, the last one will determine what
will be saved to the preferences file, as preferences changes are only
saved when Inkscape is closed, and the settings are only loaded from
file when a new window is opened.*)

-   **Clip and Mask**: improved handling
-   **Fix multiple LPE BBox**: a problem with the size of the bounding
    box when applying multiple LPEs to an object has been fixed.
    Sometimes you need to add a intermediate LPE bounding box between.
-   **Knots on shapes**: show edit knots in LPE shapes
-   **Switch knots**: change the handles to the correct LPE handles when
    one selects an LPE in the list of active LPEs for the selected
    object.

### New Live Path Effects

#### Dashed Stroke LPE

This new LPE creates uniformly dashed paths, optionally by subdividing
the path's segments, or including dashes that are symmetrically wrapped
around corners.

[Demo Video](https://archive.org/details/dash-stroke-lpe)

#### Ellipse from Points

This new LPE creates an optimally fitted ellipse from a path's nodes.

In contrast to the already existing LPE "Ellipse by 5 points", this LPE
is more flexible (since, depending on the number of points available, it
can fit both circles and ellipses) and has more features). Technical
illustrators in particular can benefit from these features.

See [LPE:\_Ellipse\_from\_Points](https://wiki.inkscape.org/wiki/index.php/LPE:_Ellipse_from_Points)
for more documentation.

#### Corners (Fillet/Chamfer) LPE

This new LPE adds fillet and chamfer to paths. Also adds a new internal
class that allows to handle extra info per node, the LPE itself is an
example of use the new classes.

[Demo video](https://www.youtube.com/watch?v=wJKzGhJULfc)

#### Measure Segments LPE

This new path effect adds DIN and custom style measuring lines to
"straight" segments in a path.

[Demo video](https://www.youtube.com/watch?v=ppgt2GPm1IY)

#### Offset

Use this to add an offset to your paths, shapes and groups. Compared
with the 'Dynamic Offset' available from the menu, this allows you to:

-   define the offset distance numerically and to choose the unit, e.g.
    to offset an object by 3 mm.
-   keep sharp corners sharp (or to make them round, or beveled, if you
    want to), by using different methods for calculating the corners.
-   same on-canvas control handle for changing the offset when using the
    node tool

#### Power Clip and Power Mask

These two new LPEs can be applied to objects by choosing
`Object → Clip → Set Inverse (LPE)` or
`Object → Mask → Set Inverse (LPE)`.

They can also be used to invert a clip that is already set on an object,
by adding the LPE to that object via
`Path → Path Effects → + → Power Clip / Power Mask`

An Inversed Clip is cut out from the object it's applied to. With an
Inversed Mask it's possible to modify only the parts of the object that
are directly below the mask, and to leave the rest of the object
unchanged (and visible).

### New Experimental LPEs

#### Angle Bisector

Draws a line that halves the angle between the first three nodes of the
path.

#### Boolean Operation LPE

The Boolean Operation LPE finally makes non-destructive boolean
operations available in Inkscape. That way, two paths can be combined to
a single shape, and both are still editable:

1.  Start with two paths. Rectangles or other shapes are also okay.
    Groups are not yet supported ([Issue
    \#1352](https://gitlab.com/inkscape/inkscape/-/issues/1352)).
2.  Copy the second path into the clipboard (`Edit → Copy`).
3.  Select the first path and add the Boolean Operation LPE
    (`Path → Path Effects → Add path effect (Plus) → Boolean operation`).
4.  Link the second path to the LPE by clicking on
    `Operand Path - Link to path in clipboard`.

Available options:

-   union
-   symmetric difference
-   intersection
-   division
-   difference
-   cut

#### Circle by 3 points

Draws a circle whose circumference passes through the first three nodes
of the path.

#### Circle (by center and radius)

Draws a circle where the first node of the path is the center, and the
last node determines the radius.

#### Extrude

Extrudes the path, creating a face for each path segment.

#### Line segment

Draws a straight line that connects the first and last node of the path.

#### Parallel

Creates a draggable line that will always be parallel to a two-node
path.

#### Perpendicular Bisector

Draws a perpendicular line in the middle of the (imaginary) line that
connects the start and end nodes.

#### Tangent to Curve

Draws a tangent with variable length and an additional angle that can be
moved along the path.

### Improved LPEs

#### BSPline and Spiro

Improvements in Pen/Pencil mode. With **<kbd>Alt</kbd>**, you can move
the previous node.

#### Clone Original

This path effect now allows various objects instead of only paths and is
even more powerful.

[Demo Video](https://www.youtube.com/watch?v=JAJAxKNY8lA)

[Demo
Video](https://ia601501.us.archive.org/34/items/00003303/0000-3303.ogv)

#### Fill Between Many / Fill Between Strokes LPE

New options added:

-   Fuse coincident points
-   Join subpaths: fill each path separately / connect all the fills
-   Close: close the fill path that is created, so it can have a stroke
    on all sides
-   LPEs on linked: take the applied live path effects of the filled
    paths into account (Fill Between Many only)

#### Knot LPE

New options added:

-   Inverse: use the stroke width *of the other path* as basis for
    calculating the gap length
-   Both gaps: interrupt both paths at a crossing

#### Mirror Symmetry and Rotate Copies LPE

-   Split feature: This new feature allows custom styles for each part
    of the resulting drawing without unlinking the LPE. [Demo
    Video](https://www.youtube.com/watch?v=mIzrQ2lpzuw)
-   The LPE rendering on the canvas now updates accordingly when there
    are objects added or removed.

#### PowerStroke LPE Improvements

-   **Width scale** setting added: adjust the overall width of the
    stroke after it has been drawn.
-   **Closed paths**: PowerStroke now works much better on closed paths.

Import / Export
---------------

### Linking and embedding SVG files

On import of an SVG file, there is now a dialog that asks whether

-   the user would like to link to the SVG file
-   to embed it (base64 encoded) into an `<img>` tag,
-   or if the objects in the SVG file should be imported into the
    document (which was how Inkscape handled importing SVG files
    previously).

The dpi value for displaying embedded SVG files can be set in the import
dialog or changed in the object properties dialog.

This makes importing SVG files work (almost) the same as importing
raster images.

The 'Embed' and 'Extract' options in the context menu for linked SVG
files work the same as they do for raster images. The 'Edit externally'
option will open the linked SVG file with Inkscape per default. This
setting can be changed in the preferences' 'Imported Images' section.

The displaying of the dialog can be disabled by checking the 'Don’t ask
me again' option.

Linked and embedded SVG images are displayed as their raster
representations, so they will become blurry when zoomed in too far.

### Mesh Gradient Polyfill

SVG files that include a mesh gradient now automatically include a
Javascript polyfill that allows the mesh gradient to display correctly
in web browsers.

### SVG 1.1 compatibility

Inkscape includes SVG 1.1 fallbacks for text by default (see [the
section about text tool updates](#text-tool)).

When exporting as SVG 1.1 explicitly, using the checkbox in the export
dialog, some settings are available in
`Edit → Preferences → Input/Output → SVG Export`, in order to allow for
correct rendering of markers in other software.

### Export PNG images

The export dialog has received several new options which are available
when you expand the 'Advanced' section.

-   Enable interlacing (ADAM7): when loading images, they will be
    displayed faster
-   Bit depth: set the number of bits that code for the color of a
    pixel, supports grayscale and up to 16-bit
-   Compression type: choose strength of lossless compression
-   pHYs dpi: force-set a dpi value for the image
-   Antialiasing: choose type of anti-aliasing or disable it
-   The option for "Cairo PNG" has been removed from the "Save as"
    dialog, as it was often confused with the "Export PNG image" option,
    but only supported a small subset of PNG rendering features.

### PDF Export

-   External links in the SVG file are now kept when the file is
    exported to pdf (requires Cairo in version 1.15.4 or higher).
-   Some Inkscape file metadata (<code>File → Document properties : Metadata</code>) are now exported to PDF (title, subject, creator, keywords).

### (E)PS Export

-   The title and copyright ('rights') info from the document's metadata
    is now exported to (E)PS.

### OpenClipart Import Removed

The dialog and settings for importing images from OpenClipart have been
removed due to the openclipart.org API being non-functional since
mid-April 2019 with no information about a return date.

Extensions
----------

### Extension dialogs

Extension dialogs can now have clickable links, images, a better layout
with separators and indentation, multiline text fields, file chooser
fields and more. For detailed info for development see the [developer
section](https://wiki.inkscape.org/wiki/index.php/Release_notes/1.0#For_extension_writers) above.

### Export Layer Slices

`Extensions → Export → Export Layer Slices`

The new 'Export Layer Slices' extension allows you to export PNG
"slices" from your image by creating a new layer and drawing rectangles
to denote the area of the export.

If you create a layer (default name "slices") with rectangles in it,
this extension will export a PNG file for each rectangle into the
directory with the name of the {rectangle ID}.png (use Object Properties
to set this).

If the export already exists, it will skip it and color the rectangle
GREY. If the "Overwrite existing exports" checkbox is selected, and the
file was previously generated, it will color the rectangle RED. For new
exports that did not previously exist, the rectangle will be GREEN.

If you want to create (square) icons at different sizes, select "Icon
mode". Icon mode will create a square export for each dimension in
"Sizes".

### Frame

`Extensions → Render → Frame`

This new extension will add a rectangular frame with a specified stroke
width, and specified stroke and fill colors to each object in the
selection. Optionally, corners can be rounded, and the frame can be
positioned inside or outside the selection. The result can be grouped
with the frame, and the object can be clipped to the size of the frame
(for further editing).

### Hershey Text

`Extensions → Text → Hershey Text`

The "Hershey Text" extension, a utility for replacing text by
stroke-based paths, has been rewritten. The most significant
improvements are:

-   The new version converts all or only the selected text objects in
    place. This means that it is possible to convert text with
    paragraphs and to convert multiple text objects at once.
-   It now uses **SVG fonts**. This means that:
    -   It is now possible to easily add and use **custom stroke
        fonts**.
    -   **Unicode characters** are now supported.
    -   Stroke fonts now support **arbitrary curves** rather than only
        straight segments.
-   **Improved font selection** with basic international characters
-   Automated **font-mapping**: each text will be converted to the
    available Hershey font with the same font file name (e.g. 'Fancy
    Font.svg') as the current font (e.g. 'Fancy Font'), if one is
    available to the extension. The automated mapping overrides any
    other Hershey font settings.
-   An option to **generate font samples** in all available SVG fonts is
    available.
-   An **extensive help text** is built into the extension.
-   Hershey Text is now located in the *Extensions → Text* sub-menu.


### Interactive Mockup

`Extensions → Web → Interactive Mockup`

The new Interactive Mockup extension is intended for use by UI/UX
designers. It can help to visualize mockups and create user flows to
make interactive demos for approvals.

To use it, select two or more objects: the first one(s) will be the
active one (button, area, image, link…), and the last selected will be
the element that should be displayed after activation.

Apply the extension, then save as SVG and open it in a browser for
demonstrating the mockup to potential users.

`Extensions → Modify Path → Mesh → Meshgradient to Path / Path to Meshgradient`
\[doesn't work: <https://gitlab.com/inkscape/extensions/-/issues/216>\]

A set of two extensions that convert mesh gradient geometry to paths and
back.


`Extensions → Export → Plot`

The new option 'Convert objects to paths' will take care of converting
everything to a path non-destructively before the data is sent to the
plotter. \[Known issue: [does not
work](https://gitlab.com/inkscape/extensions/-/issues/211)\]

`Extensions → Render → Barcode → QR Code`

Options for choosing the shape of single QR code dots were added ([but
do not work](https://gitlab.com/inkscape/extensions/-/issues/150)).


Palettes
--------

New palettes:

-   The **Munsell** palette
-   the **Bootstrap 5** palette and
-   the palette for the **new GNOME Human Interface Guidelines** (GNOME
    HIG)

have been added to Inkscape's set of stock palettes.

Users can now also drag the `none` color field from the palette bar at
the bottom onto objects to set their color to `none`.

Templates
---------

-   The **Desktop** template has new options for 4k, 5k, and 8k screens.
-   Some new page sizes were added to the **(Blank) Page** template.
-   New: template for an **A4 3-fold roll flyer**
-   New: template for **Envelope** with 2 standard envelope formats

SVG and CSS
-----------

-   **Dashes**: Inkscape can now load and display files with dashes
    and/or dash offsets defined in units other than the unitless user
    unit (e.g. `%`, `mm`) correctly. There is no user interface for
    editing these values currently, except for the XML editor. Values
    for `stroke-dasharray` that are entered in other units (except for
    `%`) will be converted to user units when the new values are set.


-   **Blend modes** applied via Layers and Objects dialog no longer use
    SVG filters, but CSS blend modes. These can be exported to PDF
    without rasterization.


-   **Hairlines** can now be rendered in Inkscape. There is currently no
    user interface for adding them yet, though. Hairlines can be
    specified by adding
    `stroke-width:1px; stroke-width: -ink-hairline; vector-effect:non-scaling-stroke;`
    into the <code>style</code> tag of the object via the XML editor or the Style dialog.
    Hairlines will be exported to PNG correctly. Export to PDF requires
    that the Cairo library that your Inkscape program was compiled with
    contains the additional code from
    <https://gitlab.freedesktop.org/cairo/cairo/merge_requests/21> (as
    of April 29, 2020, there's no official Cairo version available yet
    that contains the patch).

Dialogs
-------

### About

`Help → About Inkscape`

The Inkscape 1.0 About screen features the winning entry of the
[Inkscape 1.0 About Screen
Contest](https://inkscape.org/gallery/=about-screen-contest/contest-for-10/),
["Island of
Creativity"](https://inkscape.org/~bayubayu/%E2%98%85island-of-creativity)
by [Bayu Rizaldhan
Rayes](https://inkscape.org/news/2020/02/11/inkscape-brings-enjoyment-and-freedom-creativity/),
and its layout has changed a little.

### Arrange

`Object → Arrange`

The 'Polar coordinates' functionality now arranges objects clockwise (in
selection order) around the circle/ellipse. This should better
correspond to user expectations.

### Document Properties

-   When resizing the page, the page margin fields can now be
    **locked**, so the same value will be used for all margins, but only
    needs to be entered once.
-   The guides panel now has controls to lock or unlock all guides,
    create guides around the page, and delete all guides. These actions
    also appear on the Edit menu, making it possible to assign custom
    keyboard shortcuts.
-   **Grids can now be aligned** to the corners, edge midpoints, or
    centre of the page with a button click in the grids panel.
-   Checkerboard patterns can now have a color (for updating the current
    view, check and uncheck the box for the checkerboard background,
    [Issue \#2561](https://gitlab.com/inkscape/inbox/-/issues/2561)).
    This color will also be used as a (non-checkered) background for PNG
    export.
-   A set of new page formats for different Video resolutions (SD/PAL,
    SD-Widescreen/PAL, SD/NTSC, SD-Widescreen/NTSC, HD 720p, HD 1080p,
    DCI 2k (Full Frame), UHD 4k, DCI 4k (Full Frame), UHD 8k) has been
    added.

### Fill and Stroke

`Object → Fill and stroke`

-   The RGBA code entry field now also accepts values like `#123` and
    autocompletes them to `#112233`, automatically removes hash signs in
    pasted codes, and keeps the old alpha value if the pasted code does
    not contain any.
-   The blur slider is now scaled quadratically as you drag the slider.
    This makes it easier to apply and adjust smaller blur values.

### Filter Editor

`Filters → Filter Editor`

The filter primitives now have a symbolic icon (one whose color can be
changed).

### Objects

`Object → Objects`

The context (right-click) menu now also contains an entry for
**deleting** the selected object.

### Paint Servers

`Object → Paint Servers`

-   New dialog that allows you to see a list of patterns and SVG2 hatch
    fills used in the current document (or available by default) and to
    assign those to objects. The hatch fills can be modified by their
    handles on canvas [in the
    future](https://gitlab.com/inkscape/inbox/-/issues/2526).
-   `Server` field contains the following options: `All paint servers`,
    `Current Document` and each document's title from the user's
    `/paint` configuration folder.
-   The `Change` field decides whether the fill or the stroke of the
    object will be set to the paint server on click.
-   You can select multiple objects or a group of objects and they will
    all get their fill or stroke changed to the selected paint server.
-   To add a new paint server, you need to add an `.svg` document in the
    `/paint` user configuration folder with the following restrictions:
    -   the svg must be valid
    -   it must have a unique <code>title</code> property
    -   and it must have patterns or hatches with unique ids in the
        defs section.

This functionality was added to Inkscape as a Google Summer of Code
project. More details on it are available [in the project
description](https://gitlab.com/vanntile/inkscape-gsoc-application#paint-servers-dialog)
and our [news
article](https://inkscape.org/news/2020/02/21/valentin-wrangled-meshes-hatches-and-gtk-during-su/).

### Preferences

`Edit → Preferences`

-   The **Bitmaps** subsection has been renamed to **Imported Images**,
    as it now applies to both imported (embedded or linked) raster
    images as well as to imported (embedded or linked) SVG images (i.e.
    to everything in `<img>` tags).
-   The **System** subsection lists more relevant folders and offers
    buttons to open those folders with the system's file browser. This
    makes it easier to find the correct folder, e.g. for resetting the
    preferences or for adding an extension or a new icon set.
-   The **System** subsection now has a button for quickly resetting all
    Inkscape preferences, which also automatically creates a backup of
    the current preferences.
-   An option for **scaling a stroke's dash pattern when scaling the
    stroke width** has been added and can be found at
    `Behaviour → Dashes`. It is activated by default.
-   **Autosave** is now enabled by default. The default directory has
    changed (the path is displayed in
    `Edit → Preferences → Input/Output → Autosave: Autosave directory`).
-   The setting for **Handle size** has been moved from **Input
    devices** to **Interface** to make it more discoverable.

### Selection Sets Dialog Hidden

The **Selection Sets** dialog is deprecated and has been hidden from the
menus. It will be removed in Inkscape 1.1 and sets created with this
option might not work in a future Inkscape version.

It can be un-hidden by assigning a keyboard shortcut to it in the
Inkscape preferences, or by editing the file `menus.xml` in Inkscape's
`ui` folder to uncomment the `DialogTags` entry, and saving the edited
file in your user preferences' `ui` folder.

### Selectors and CSS \[Experimental, hidden\]

-   New dialog for adding classes and CSS styles to elements of the
    drawing
-   It is currently in experimental status, thus hidden from the menu
    (`Edit → Selectors and CSS`)
-   The keyboard shortcut **<kbd>Ctrl + Shift + Q</kbd>** can be used to
    open the dialog
-   Among the [known
    issues](https://gitlab.com/groups/inkscape/-/issues?scope=all&utf8=%E2%9C%93&state=all&search=Selectors),
    there are a couple crashes, which is the reason why the dialog has
    been hidden for the 1.0 release.

### Symbols

`Object → Symbols`

The Symbols dialog can now handle a lot of symbols without delay on
startup, and also allows searching. Symbols and symbol sets are now
displayed in alphabetical order.

### Trace Bitmap

`Path → Trace Bitmap`

A new, unified dialog for vectorizing raster graphics is now available
from `Path → Trace Bitmap`. It contains the previously separate **Trace
pixelart** dialog and comes with a new option for **centerline
tracing**.

### Unicode Characters

-   The **'Glyphs' dialog** has been **renamed to 'Unicode
    Characters**'.
-   The characters in the dialog's character list now **use the selected
    font**.
-   Each character now has a **tooltip** that shows a larger version of
    the character, so one can more easily find the correct character.

### XML Editor

`Edit → XML Editor`

The side of the editor that allows one to set, edit or delete attributes
can now be panned both horizontally and vertically, or be hidden
entirely. Long items can more easily be edited in a little popup dialog
with a new monospaced font.

Menus
-----

-   New option to `Unlink clones recursively` added into sub-menu at
    `Edit → Clone`

Customization
----------------------------------------------

### Customize many files in the share folder

Many files in `/share` can be over-ridden by placing files in the user's
configuration folder (e.g. `~/.config/inkscape`). Configurable contents
now includes extensions, filters, fonts, gradients, icons, keyboard
shortcuts, \[preset markers (not yet: [Issue
\#211](https://gitlab.com/inkscape/inbox/-/issues/211))\], user paint
servers (SVG hatches, patterns, ...), palettes, about screen, symbol
sets, templates, tutorials and some user interface configuration files.
Only the file 'units.xml' cannot be overridden.

### Fonts

#### Load additional fonts

Inkscape can now load fonts that are not installed on the system. By
default Inkscape will load additional fonts from Inkscape's share folder
(`/share/inkscape/fonts`) and the user's configuration folder
(`~/.config/inkscape/fonts`). Custom folders can be set in preferences
(see `Tools → Text → Additional font directories`).

### Keyboard shortcuts

-   Allow to use "Super", "Hyper" and "Meta" modifier keys
-   Improve shortcut handling code. This should fix a lot of issues and
    allow the use of many shortcuts which were inaccessible before,
    especially on non-English keyboard layouts.
-   The Keyboard shortcut editor now issues a warning when the entered
    shortcut is already in use.
-   It is now possible to assign keyboard shortcuts that align an object
    to the top-left, top-right, bottom-left or bottom-right corners of
    the anchor (determined via the 'relative to:' field), or to align
    the objects' top-left, top-right, bottom-left or bottom-right
    corners with the opposite corner of the anchor.

### User interface customization

-   Inkscape is starting to use glade files for its dialogues, so they
    can be reconfigured by users. Only one is currently supported
    (filter editor).
-   The contents of the menus can be configured by customising the
    `menus.xml` file.
-   Toolbar contents for the command bar (`commands-toolbar.ui`), the
    snap bar (`snap-toolbar.ui`), the tool controls bars for each tool
    (`select-toolbar.ui`), the toolbox (`tool-toolbar.ui`) is now
    configurable.
-   The interface colors and some more UI styles can be customized in
    <preferences folder>`/ui/style.css` (very raw theming support).

### Theme selection

In `Edit → Preferences → User Interface → Theme`, users can set a custom
GTK3 theme for Inkscape. If the theme comes with a dark variant,
activating the 'Use dark theme' checkbox will result in the dark variant
being used. The new theme will be applied immediately.

New themes can be installed on your system to be made available in the
list to choose from. A large selection of (more or less current) GTK3
themes are available for download at
[gnome-look.org](https://www.gnome-look.org/browse/cat/135/ord/top/) .
On Windows, the new themes can be placed in
<var>`%AppData%`</var>`\Local\themes\`, so that the full path to the
theme's CSS files will be
<var>`%AppData%`</var>`\Local\themes\`<theme name>`\gtk-3.0\`.

### Icon set selection

In `Edit → Preferences → User Interface → Theme`, the icon set to use
can be selected. By default, Inkscape comes with 'hicolor', 'Tango', and
the new 'Multicolor' icons. In addition to this, it offers to use the
system's icons.

The symbolic icon set that is part of the 'hicolor' icon set as well as
the new Multicolor icons can be colorized with custom colors.

### Saving the current file as template

A new entry for saving the current file as a template has been added to
the File menu. You need to specify a name for it, and optionally, you can add the template's author, a description and some keywords. A checkbox allows you to set the new template as the default template. 

### Custom page sizes in Document Properties

Inkscape now creates a CSV file (comma separated values) called
`pages.csv`. It is located in your Inkscape user preferences folder,
next to your `preferences.xml` file. This file contains the default page
sizes that you can choose from in the 'Page' tab of the 'Document
properties' dialog. You can edit the `pages.csv` file to remove the page
sizes you won't use, or to add new ones.

Command Line
--------------------------------------------

The Inkscape command line has undergone a major overhaul with the goal
of making it more powerful and flexible for the user and easier to
enhance for the developer. The most important changes are:

-   Each command-line argument can now be used only once. To specify
    multiple *actions* (*verbs*), use semicolons (e.g.
    <code>--actions='ObjectFlipVertically;FileSave;FileClose'</code>).
-   Many *actions* can now take arguments (separated from the *action*
    name by a colon.
-   [xverbs](https://wiki.inkscape.org/wiki/index.php/Using_xverbs) have been removed from Inkscape
    (command line commands that take parameters from a file, e.g. for
    saving the selection under a specified filename as SVG file)
    ([mailing list
    thread](https://sourceforge.net/p/inkscape/mailman/inkscape-devel/thread/33487d06-e3c1-a4e5-1496-7b370d672d2f%40gmail.com/#msg35392523)).
-   -   Multiple objects in single file can be saved into individual
    files by giving a comma separated list of objects to the command:
    `--export-id`.
-   Inkscape can now import a specific page of a PDF file from the
    command line, for batch processing (new option:
    `--pdf-page `<var>`N`</var>).
-   For importing a PDF, the option to import via poppler is now
    available for the command line as `pdf-poppler`.
-   New verb allows one to swap fill and stroke style from the command
    line: `EditSwapFillStroke` (a keyboard shortcut can now be assigned
    to it) ([Issue \#675690
    (lp)](https://bugs.launchpad.net/inkscape/+bug/675690))
-   The shell mode syntax has changed, too.
-   The file name can now be specified with `--export-filename`
-   The command `-x` / `--extension-directory` has been removed.
    Replaced with: `--system-data-directory` and
    `--user-data-directory`.

More information about usage and how to update your commands can be
found at [Using the Command Line](https://wiki.inkscape.org/wiki/index.php/Using_the_Command_Line).

Tutorials / Documentation
-------------------------

-   Some small updates were made to tutorial texts
-   Tutorial files got a new header / footer design, using [Esteban
    Capella's
    entry](https://inkscape.org/~esteban/%E2%98%85inksscreen-10-by-esteban-capella)
    for our About Screen Contest.

Translations \[as of 2019-12-18\]
---------------------------------

Translations were updated for:

-   Basque
-   British English
-   Brazilian Portuguese
-   Catalan
-   Croatian
-   Czech
-   Dutch
-   Finnish
-   French
-   German
-   Greek
-   Hindi
-   Hungarian
-   Icelandic
-   Indonesian
-   Italian
-   Korean
-   Latvian
-   Norwegian (Bokmål)
-   Polish
-   Romanian
-   Russian
-   Slovak
-   Spanish
-   Swedish
-   Turkish
-   Ukrainian
-   Urdu
-   Swedish

Tutorial translations were added for:

-   Korean

Translations were dropped for:

-   Amharic

Notable Bugfixes
----------------

-   Symbols: Visio Stencils loaded from `.vss` files now use their
    actual name instead of a placeholder derived from the symbol file's
    name ([Issue \#1676144
    (lp)](https://bugs.launchpad.net/inkscape/+bug/1676144))
-   Shapes on Pen and Pencil tools now retain color and width ([Issue
    \#1707899 (lp)](https://bugs.launchpad.net/inkscape/+bug/1707899)).
-   Text and Font dialog: The font selection no longer jumps to the top
    of the list when clicking Apply.
-   Docked dialogs now open on their own when the corresponding
    functionality is called from a menu or button
-   The icon preview dialog now correctly shows the page background
    (Issue \#[1537497
    (lp)](https://bugs.launchpad.net/inkscape/+bug/1537497)).
-   As of Windows 10 (version 1809) fonts are installed into a new
    user-specific folder by default. Allow Inkscape to recognize those
    fonts. ([Iusse
    \#50](https://gitlab.com/inkscape/inkscape/-/issues/50))
-   The default Perl interpreter executable on Windows was changed from
    `perl.exe` to `wperl.exe` which should usually avoid flashing a
    console window. ([Issue
    \#66](https://gitlab.com/inkscape/inkscape/-/issues/66))
-   Some printers who don't correctly recognize the page formats sent by
    Inkscape, printed only square excerpts of the whole image that was
    supposed to be printed. Now they print the whole image ([Merge
    request
    \#407](https://gitlab.com/inkscape/inkscape/-/merge_requests/407)).

For an exhaustive list of bugs that have been fixed, please see the
[milestones page for Inkscape 1.0 on
Launchpad](https://launchpad.net/inkscape/1.0.x) and the [list of
milestoned issues on
GitLab](https://gitlab.com/inkscape/inkscape/-/issues?scope=all&utf8=%E2%9C%93&state=closed&milestone_title=Inkscape%201.0).

Breaking changes / Action required
----------------------------------

### For users

#### Custom Icon Sets

Icon sets no longer consist of a single file containing all icons.
Instead each icon is allocated its own file. The directory structure
must follow the [standard structure for Gnome
icons](https://developer.gnome.org/icon-theme-spec/).

If you would like to create or convert your own icon set to the new
format, please compare the 'hicolor' and 'Tango' icon theme folders, in
your Inkscape installation's 'share' directory for suitable examples and
check out [ our guide to making a new multicolor icon
theme](https://wiki.inkscape.org/wiki/index.php/Creating_a_new_multicolor_icon_theme).

As a side effect of a bug fix to the icon preview dialog (see below),
custom UI icon SVG files need to be updated to have their background
color alpha channel set to 0 so that they display correctly (see Issue
\#[1661989 (lp)](https://bugs.launchpad.net/inkscape/+bug/1661989)).

#### Third-party extensions

Most extensions that are maintained by a third-party developer (i.e.
that are not an Inkscape stock extension) need to be updated to work
with this version of Inkscape. Inkscape contributors have contacted many
extension authors already to let them know about the upcoming changes.
If your favorite third-party extension still needs to be updated to be
compatible with Inkscape 1.0, please point its author to [the section
about updating one's extension in the
Wiki](https://wiki.inkscape.org/wiki/index.php/Release_notes/1.0#For_extension_writers).

#### Dropped / Replaced Extensions

Extensions that previously used the UniConvertor library for
saving/opening various file formats have been removed, as well as some
extensions that depended on third-party programs:

***Import extensions that have been removed:***

-   Adobe Illustrator 8.0 and below (UC) (`*.ai`) (Workaround: rename
    the file extension to `.eps`. Newer versions can still be imported
    if they contain an embedded PDF.)
-   Corel DRAW Compressed Exchange files (UC) (`*.ccx`)
-   Corel DRAW 7-X4 files (UC) (`*.cdr`) (`*.cdr` in general can still
    be imported)
-   Corel DRAW 7-13 template files (UC) (`*.cdt`)
-   Computer Graphics Metafile files (UC) (`*.cgm`)
-   Corel DRAW Presentation Exchange files (UC) (`*.cmx`)
-   HP Graphics Language Plot file \[AutoCAD\] (UC) (`*.plt`)
-   sK1 vector graphics files (UC) (`*.sk1`)
-   Dia Diagram (`*.dia`) (Workaround: export as a different format from
    [Dia](https://wiki.gnome.org/Apps/Dia/Download))

***Export extensions that have been removed:***

-   HP Graphics Language Plot file \[AutoCAD\] (UC) (`*.plt`)
-   sK1 vector graphics files (UC) (`*.sk1`)

***Extensions that have been replaced:***

-   `Render → LaTeX`: The [EQTeXSVG
    extension](https://www.julienvitard.eu/en/eqtexsvg_en.html)
    (`Render → LaTeX`) that could be used to convert an inline LaTeX
    equation into SVG paths using Python was dropped, due to its
    external dependencies. It has been replaced by the extension
    `Render → Mathematics → LaTeX (pdflatex)` which serves the same
    purpose.

#### Command line changes

The Inkscape command line options [have changed significantly (see
below)](#command-line). Any command line scripts that you
have used will need to be updated for Inkscape 1.0.

### For extension writers

Extensions have undergone some fundamental changes.

Inkscape's stock extensions **have been moved to [their own
repository](https://gitlab.com/inkscape/extensions)** and have been
updated for compatibility with **Python 3**. Internally, extensions have
been reorganized, many functions have been deprecated, and new functions
have been added.

#### General

-   Extensions were updated to be compatible
    with Python 3. While we'll be migrating away from Python 2,
    extension writers should aim for support of Python 2.7 and Python
    3.5+ for maximum compatibility.
-   Windows packages now ship with Python 3 (currently Python 3.7).
    Python 2 is not bundled anymore, so make sure to update your
    extension to be compatible.
-   Inkscape now adds itself to search path on startup, so you should
    always be able to call it from your extension by simply calling
    `inkscape`, without the need to add it to search path manually, or
    worrying about other potentially incompatible versions of Inkscape
    being available on search path.
-   The folder structure of Windows packages was updated: Binaries were
    moved from the installation root to `bin/`, Inkscape's shared files
    where moved from `share/` to a `share/inkscape` subfolder.
-   The underscores that were previously necessary to mark elements as
    translatable are no longer needed. Elements that are usually
    translated are now by default included in translations. Elements
    that are usually not translated, are not included. This can be
    overridden by setting the `translatable="yes/no"` attribute.
-   Extensions (including their `.inx` files) can now be put into a
    subdirectory of the `extensions/` folder to allow for better
    structuring and separation of extensions.

#### Extension dialogs

Extension dialogs have some new input types and layout options:

-   new multiline text fields
-   new file chooser fields
-   new `appearance="url"` for [INX
    Parameters](https://wiki.inkscape.org/wiki/index.php/INX_Parameters) of type `"description"`. You
    can now add clickable links to your extension UI.
-   a simpler version of the color chooser field has been added
-   new layout options (separator, table-like layouts)
-   all [INX Parameters](https://wiki.inkscape.org/wiki/index.php/INX_Parameters) now have the common
    attribute `indent="`<var>`n`</var>`"` where <var>`n`</var> specifies
    the level of indentation in the extension UI.
-   new `label` parameter
-   new option to include an image
-   some confusing options have been merged
-   new effect extension attribute `implements-custom-gui` is
    [available](http://wiki.inkscape.org/wiki/index.php/INX_extension_descriptor_format#Attributes_description)
    to hide the 'Extension is working' dialog.

#### More info

Please also note the changed [command line
options](#command-line), if your extension calls another
instance of Inkscape.

More detailed instructions for updating old extensions are available at
[Updating your Extension for
1.0](https://wiki.inkscape.org/wiki/index.php/Updating_your_Extension_for_1.0). Also check the
[extension (`inkex`) API
documentation](https://inkscape.gitlab.io/extensions/documentation/) and
the [stock extensions
repository](https://gitlab.com/inkscape/extensions) for finding
comparable extensions. If you have questions about extension
development, you can join us in our [extension development chat
channel](https://chat.inkscape.org/channel/inkscape_extensions).

### For packagers and those who compile Inkscape

-   `autotools` builds have been dropped. Please use `CMake` for
    building Inkscape from now on. More info is available [on our
    website](https://inkscape.org/develop/getting-started/#092-onwards).
-   `libsoup` dependency added: we use `libsoup` for making HTTP
    requests without a need for `dbus` and `gvfs`.
-   double-conversion [2](https://github.com/google/double-conversion)
    dependency added: `lib2geom` now depends on an external version of
    the library.
-   Inkscape now uses a git submodule for the `extensions/` directory.
    If you have cloned the repository and are not building from the
    release source code tarball, please note the [updated build
    instructions](https://inkscape.org/en/develop/getting-started/)
-   On Ubuntu 18.04, Gnome's fallback icon set (package
    'adwaita-icon-theme-full'), that is needed to display Inkscape's
    default icons completely, is no longer automatically installed. It
    has been added as a 'recommends' level dependency.
-   lib2geom: \[insert up-to-date info here\]
-   The environment variable `INKSCAPE_PORTABLE_PROFILE_DIR` has been
    removed. Please use the equivalent `INKSCAPE_PROFILE_DIR` instead
    for changing the location of the profile directory at run time. (see
    also [\#114](https://gitlab.com/inkscape/inkscape/-/issues/114))
-   Inkscape extensions have been updated to work with Python 3, they no
    longer depend on Python 2 (but still work with it)

Known Issues
------------

See [our list of confirmed and ready-to-be-worked-on
issues](https://gitlab.com/inkscape/inkscape/-/issues) and [the
list of new user submitted issues, questions and feature
requests](https://gitlab.com/inkscape/inbox/-/issues).

Inkview
-------

**Inkview** (a simple SVG viewer) was considerably improved and got some
new features:

-   Support folders as input (will load all SVG files from the specified
    folder)  
    The `-r` or `--recursive` option will even allow to search
    subfolders recursively.
-   Implement `-t` or `--timer` option which allows to set a time after
    which the next file will be automatically loaded.
-   Add `-s` or `--scale` option to set a factor by which to scale the
    displayed image.
-   Add `-f` or `--fullscreen` option to launch Inkview in fullscreen
    mode
-   Many smaller fixes and improvements
