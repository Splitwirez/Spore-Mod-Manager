#!/usr/bin/env python
# coding=utf-8
#
# Copyright (c) 2009 Bryan Hoyt (MIT License)
#               2011 Nicolas Dufour <nicoduf@yahoo.fr>
#               2013 Johan B. C. Engelen <j.b.c.engelen@alumnus.utwente.nl>
#               2019 Martin Owens <doctormo@gmail.com>
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
#
"""
TODO: mark elements that have previously been snapped, along with the settings
    used, so that the same settings can be used for that element next time when
    it's selected as part of a group (and add an option to the extension dialog
    "Use previous/default settings" which is selected by default)

TODO: make elem_offset return [x_offset, y_offset] so we can handle non-symetric scaling
      => will probably need to take into account non-symetric scaling on stroke-widths,
         too (horizontal vs vertical strokes)

TODO: Transforming points isn't quite perfect, to say the least. In particular,
    when translating a point on a bezier curve, we translate the handles by the same amount.
    BUT, some handles that are attached to a particular point are conceptually
    handles of the prev/next node.
    Best way to fix it would be to keep a list of the fractional_offsets[] of
    each point, without transforming anything. Then go through each point and
    transform the appropriate handle according to the relevant fraction_offset
    in the list.

    i.e. calculate first, then modify.

    In fact, that might be a simpler algorithm anyway -- it avoids having
    to keep track of all the first_xy/next_xy guff.

Note: This doesn't work very well on paths which have both straight segments
      and curved segments.
      The biggest three problems are:
        a) we don't take handles into account (segments where the nodes are
           aligned are always treated as straight segments, even where the
           handles make it curve)
        b) when we snap a straight segment right before/after a curve, it
           doesn't make any attempt to keep the transition from the straight
           segment to the curve smooth.
        c) no attempt is made to keep equal widths equal. (or nearly-equal
           widths nearly-equal). For example, font strokes.

Note: Paths that have curves & arcs on some sides of the bounding box won't
    be snapped correctly on that side of the bounding box, and nor will they
    be translated/resized correctly before the path is modified. Doesn't affect
    most applications of this extension, but it highlights the fact that we
    take a geometrically simplistic approach to inspecting & modifying the path.
"""

from __future__ import division
from __future__ import print_function

import sys

import inkex
from inkex import PathElement, Group, Image, Rectangle, ShapeElement, Transform

Precision = 5  # number of digits of precision for comparing float numbers

class TransformError(Exception):
    pass

def transform_point(transform, pt, inverse=False):
    """apply_to_point with inbuilt inverse"""
    if inverse:
        transform = -transform
    return transform.apply_to_point(pt)

def transform_dimensions(transform, width=None, height=None, inverse=False):
    """ Dimensions don't get translated. I'm not sure how much diff rotate/skew
        makes in this context, but we currently ignore anything besides scale.
    """
    if inverse:
        transform = -transform

    if width is not None:
        width *= transform.a
    if height is not None:
        height *= transform.d

    if width is not None and height is not None:
        return width, height
    if width is not None:
        return width
    if height is not None:
        return height


class PixelSnap(inkex.EffectExtension):
    def add_arguments(self, pars):
        """Add inx options"""
        pars.add_argument("-a", "--snap_ancestors", type=inkex.Boolean, default=True,\
            help="Snap unselected ancestors' translations "\
                 "(groups, layers, document height) first")
        pars.add_argument("-t", "--ancestor_offset", type=inkex.Boolean, default=True,\
            help="Calculate offset relative to unselected ancestors' "\
                 "transforms (includes document height offset)")
        pars.add_argument("-g", "--max_gradient", type=float, default=0.5,\
            help="Maximum slope to consider straight (%)")

    def vertical(self, pt1, pt2):
        hlen = abs(pt1[0] - pt2[0])
        vlen = abs(pt1[1] - pt2[1])
        if vlen == 0 and hlen == 0:
            return True
        elif vlen == 0:
            return False
        return (hlen / vlen) < self.options.max_gradient / 100

    def horizontal(self, pt1, pt2):
        hlen = round(abs(pt1[0] - pt2[0]), Precision)
        vlen = round(abs(pt1[1] - pt2[1]), Precision)
        if hlen == 0 and vlen == 0:
            return True
        elif hlen == 0:
            return False
        return (vlen / hlen) < self.options.max_gradient / 100

    def stroke_width_offset(self, elem, parent_transform=None):
        """ Returns the amount the bounding-box is offset due to the stroke-width.
            Transform is taken into account.
        """
        stroke_width = self.stroke_width(elem)
        if stroke_width == 0:
            return 0  # if there's no stroke, no need to worry about the transform

        transform = (elem.transform * Transform(parent_transform))

        if abs(abs(transform.a) - abs(transform.d)) > (10 ** -Precision):
            raise TransformError("Selection contains non-symetric scaling")  # *** wouldn't be hard to get around this by calculating vertical_offset & horizontal_offset separately, maybe 1 functions, or maybe returning a tuple

        stroke_width = transform_dimensions(transform, width=stroke_width)

        return stroke_width / 2

    def stroke_width(self, elem, setval=None):
        """Get/set stroke-width in pixels, untransformed"""
        style = dict(inkex.Style.parse_str(elem.attrib.get('style', '')))
        stroke = style.get('stroke', None)
        if stroke == 'none':
            stroke = None

        stroke_width = 0
        if stroke and setval is None:
            stroke_width = self.svg.unittouu(style.get('stroke-width', '').strip())

        if setval:
            style['stroke-width'] = str(setval)
            elem.attrib['style'] = str(inkex.Style(style))
        else:
            return stroke_width

    def transform_path_node(self, transform, path, i):
        """ Modifies a segment so that every point is transformed, including handles
        """
        segtype = path[i][0].lower()

        if segtype == 'z':
            return
        elif segtype == 'h':
            path[i][1][0] = transform_point(transform, [path[i][1][0], 0])[0]
        elif segtype == 'v':
            path[i][1][0] = transform_point(transform, [0, path[i][1][0]])[1]
        else:
            first_coordinate = 0
            if segtype == 'a':
                first_coordinate = 5  # for elliptical arcs, skip the radius x/y, rotation, large-arc, and sweep
            for j in range(first_coordinate, len(path[i][1]), 2):
                x, y = path[i][1][j], path[i][1][j + 1]
                x, y = transform_point(transform, (x, y))
                path[i][1][j] = x
                path[i][1][j + 1] = y

    def pathxy(self, path, i, setval=None):
        """ Return the endpoint of the given path segment.
            Inspects the segment type to know which elements are the endpoints.
        """
        segtype = path[i][0].lower()
        x = y = 0

        if segtype == 'z':
            i = 0

        if segtype == 'h':
            if setval:
                path[i][1][0] = setval[0]
            else:
                x = path[i][1][0]

        elif segtype == 'v':
            if setval:
                path[i][1][0] = setval[1]
            else:
                y = path[i][1][0]
        else:
            if setval and segtype != 'z':
                path[i][1][-2] = setval[0]
                path[i][1][-1] = setval[1]
            else:
                x = path[i][1][-2]
                y = path[i][1][-1]

        if setval is None:
            return [x, y]

    def snap_path_scale(self, elem, parent_transform=None):

        path = elem.original_path.to_arrays()
        transform = elem.transform * Transform(parent_transform)
        bbox = elem.bounding_box()

        # In case somebody tries to snap a 0-high element,
        # or a curve/arc with all nodes in a line, and of course
        # because we should always check for divide-by-zero!
        if not bbox.width or not bbox.height:
            return

        width, height = bbox.width, bbox.height
        min_xy, max_xy = bbox.minimum, bbox.maximum
        rescale = round(width) / width, round(height) / height

        min_xy = transform_point(transform, min_xy, inverse=True)
        max_xy = transform_point(transform, max_xy, inverse=True)

        for i in range(len(path)):
            translate = Transform(translate=min_xy)
            self.transform_path_node(-translate, path, i)  # center transform
            self.transform_path_node(Transform(scale=rescale), path, i)
            self.transform_path_node(translate, path, i)  # uncenter transform

        elem.original_path = path

    def snap_path_pos(self, elem, parent_transform=None):
        path = elem.original_path.to_arrays()
        transform = elem.transform * Transform(parent_transform)
        bbox = elem.bounding_box()
        min_xy, max_xy = bbox.minimum, bbox.maximum

        fractional_offset = min_xy[0] - round(min_xy[0]), min_xy[1] - round(min_xy[1]) - self.document_offset
        fractional_offset = transform_dimensions(transform, fractional_offset[0], fractional_offset[1], inverse=True)

        for i in range(len(path)):
            self.transform_path_node(-Transform(translate=fractional_offset), path, i)

        path = str(inkex.Path(path))
        if elem.get('inkscape:original-d'):
            elem.set('inkscape:original-d', path)
        else:
            elem.set('d', path)

    def snap_transform(self, elem):
        # Only snaps the x/y translation of the transform, nothing else.
        # Doesn't take any parent_transform into account -- assumes
        # that the parent's transform has already been snapped.
        transform = elem.transform
        # if we've got any skew/rotation, get outta here
        if transform.c or transform.b:
            raise TransformError("TR: Selection contains transformations with skew/rotation")

        trm = list(transform.to_hexad())
        trm[4] = round(transform.e)
        trm[5] = round(transform.f)
        elem.transform *= Transform(trm)

    def snap_stroke(self, elem, parent_transform=None):
        transform = elem.transform * Transform(parent_transform)

        stroke_width = self.stroke_width(elem)
        if (stroke_width == 0): return                                          # no point raising a TransformError if there's no stroke to snap

        if abs(abs(transform.a) - abs(transform.d)) > (10**-Precision):
            raise TransformError("Selection contains non-symetric scaling, can't snap stroke width")

        if stroke_width:
            stroke_width = transform_dimensions(transform, width=stroke_width)
            stroke_width = round(stroke_width)
            stroke_width = transform_dimensions(transform, width=stroke_width, inverse=True)
            self.stroke_width(elem, stroke_width)

    def snap_path(self, elem, parent_transform=None):
        path = elem.original_path.to_arrays()

        transform = (elem.transform * Transform(parent_transform))

        if transform.c or transform.b:  # if we've got any skew/rotation, get outta here
            raise TransformError("Path: Selection contains transformations with skew/rotation")

        offset = self.stroke_width_offset(elem, parent_transform) % 1

        prev_xy = self.pathxy(path, -1)
        first_xy = self.pathxy(path, 0)
        for i in range(len(path)):
            segtype = path[i][0].lower()
            xy = self.pathxy(path, i)
            if segtype == 'z':
                xy = first_xy
            if (i == len(path) - 1) or \
                    ((i == len(path) - 2) and path[-1][0].lower() == 'z'):
                next_xy = first_xy
            else:
                next_xy = self.pathxy(path, i + 1)

            if not (xy and prev_xy and next_xy):
                prev_xy = xy
                continue

            xy_untransformed = tuple(xy)
            xy = list(transform_point(transform, xy))
            prev_xy = transform_point(transform, prev_xy)
            next_xy = transform_point(transform, next_xy)

            on_vertical = on_horizontal = False

            if self.horizontal(xy, prev_xy):
                # on 2-point paths, first.next==first.prev==last and last.next==last.prev==first
                if len(path) > 2 or i == 0:
                    # make the almost-equal values equal, so they round in the same direction
                    xy[1] = prev_xy[1]
                on_horizontal = True
            if self.horizontal(xy, next_xy):
                on_horizontal = True

            if self.vertical(xy, prev_xy):  # as above
                if len(path) > 2 or i == 0:
                    xy[0] = prev_xy[0]
                on_vertical = True
            if self.vertical(xy, next_xy):
                on_vertical = True

            prev_xy = tuple(xy_untransformed)

            fractional_offset = [0, 0]
            if on_vertical:
                fractional_offset[0] = xy[0] - (round(xy[0] - offset) + offset)
            if on_horizontal:
                fractional_offset[1] = xy[1] - (round(xy[1] - offset) + offset)\
                    - self.document_offset

            fractional_offset = transform_dimensions(
                transform, fractional_offset[0], fractional_offset[1], inverse=True)
            self.transform_path_node(-Transform(translate=fractional_offset), path, i)

        elem.original_path = path

    def snap_rect(self, elem, parent_transform=None):
        transform = (elem.transform * Transform(parent_transform))

        if transform.c or transform.b:  # if we've got any skew/rotation, get outta here
            raise TransformError("Rect: Selection contains transformations with skew/rotation")

        offset = self.stroke_width_offset(elem, parent_transform) % 1

        width = self.svg.unittouu(elem.attrib['width'])
        height = self.svg.unittouu(elem.attrib['height'])
        x = self.svg.unittouu(elem.attrib['x'])
        y = self.svg.unittouu(elem.attrib['y'])

        width, height = transform_dimensions(transform, width, height)
        x, y = transform_point(transform, [x, y])

        # Snap to the nearest pixel
        height = round(height)
        width = round(width)
        x = round(x - offset) + offset  # If there's a stroke of non-even width, it's shifted by half a pixel
        y = round(y - offset) + offset

        width, height = transform_dimensions(transform, width, height, inverse=True)
        x, y = transform_point(transform, [x, y], inverse=True)

        y += self.document_offset / transform.d

        # Position the elem at the newly calculate values
        elem.attrib['width'] = str(width)
        elem.attrib['height'] = str(height)
        elem.attrib['x'] = str(x)
        elem.attrib['y'] = str(y)

    def snap_image(self, elem, parent_transform=None):
        self.snap_rect(elem, parent_transform)

    def pixel_snap(self, elem, parent_transform=None):
        if not isinstance(elem, (Group, Image, Rectangle, PathElement)):
            return

        if isinstance(elem, Group):
            self.snap_transform(elem)
            transform = elem.transform * Transform(parent_transform)
            for child in elem:
                try:
                    self.pixel_snap(child, transform)
                except TransformError as err:
                    raise inkex.AbortExtension(str(err))
            return

        # If we've been given a parent_transform, we can assume that the
        # parents have already been snapped, or don't need to be
        if self.options.snap_ancestors and parent_transform is None:
            # Loop through ancestors from outermost to innermost, excluding this element.
            for child in elem.ancestors().values():
                self.snap_transform(child)

        # If we haven't been given a parent_transform, then we need to calculate it
        if self.options.ancestor_offset and parent_transform is None:
            if isinstance(elem.getparent(), ShapeElement):
                parent_transform = elem.getparent().composed_transform()

        self.snap_transform(elem)
        try:
            self.snap_stroke(elem, parent_transform)
        except TransformError as err:
            raise inkex.AbortExtension(str(err))

        if isinstance(elem, PathElement):
            self.snap_path_scale(elem, parent_transform)
            self.snap_path_pos(elem, parent_transform)
            self.snap_path(elem, parent_transform)  # would be quite useful to make this an option, as scale/pos alone doesn't mess with the path itself, and works well for sans-serif text
        elif isinstance(elem, Rectangle):
            self.snap_rect(elem, parent_transform)
        elif isinstance(elem, Image):
            self.snap_image(elem, parent_transform)

    def effect(self):
        svg = self.document.getroot()

        self.document_offset = self.svg.unittouu(svg.attrib['height']) % 1  # although SVG units are absolute, the elements are positioned relative to the top of the page, rather than zero

        for elem in self.svg.selected.values():
            try:
                self.pixel_snap(elem)
            except TransformError as err:
                raise inkex.AbortExtension(str(err))


if __name__ == '__main__':
    PixelSnap().run()
