#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2005 Aaron Spike, aaron@ekips.org
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
Draws handles of selected paths.
"""

import inkex
from inkex.paths import Path, Curve, Move, Line, Quadratic
from inkex.transforms import Vector2d

class Handles(inkex.EffectExtension):
    """
    Renders the handle lines for the selected curves onto the canvas.
    """
    def effect(self):
        for node in self.svg.selection.filter(inkex.PathElement).values():
            result = Path()
            prev = Vector2d()
            start = None
            for seg in node.path.to_absolute():
                if start is None:
                    start = seg.end_point(start, prev)
                if isinstance(seg, Curve):
                    result += [
                        Move(seg.x2, seg.y2), Line(prev.x, prev.y),
                        Move(seg.x3, seg.y3), Line(seg.x4, seg.y4),
                    ]
                elif isinstance(seg, Quadratic):
                    result += [
                        Move(seg.x2, seg.y2), Line(prev.x, prev.y),
                        Move(seg.x2, seg.y2), Line(seg.x3, seg.y3)
                    ]
                prev = seg.end_point(start, prev)

            if not result:
                continue

            elem = node.getparent().add(inkex.PathElement())
            elem.path = result
            elem.style = {'stroke-linejoin': 'miter', 'stroke-width': '1.0px',
                          'stroke-opacity': '1.0', 'fill-opacity': '1.0',
                          'stroke': '#000000', 'stroke-linecap': 'butt',
                          'fill': 'none'}

if __name__ == '__main__':
    Handles().run()
