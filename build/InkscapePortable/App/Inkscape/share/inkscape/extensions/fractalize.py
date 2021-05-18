#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2005 Carsten Goetze c.goetze@tu-bs.de
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
import math
import random
import inkex
from inkex.paths import Move, Line

def calculate_subdivision(smoothness, x1, y1, x2, y2):
    # Calculate the vector from (x1,y1) to (x2,y2)
    x3 = x2 - x1
    y3 = y2 - y1
    # Calculate the point half-way between the two points
    hx = x1 + x3 / 2
    hy = y1 + y3 / 2
    # Calculate normalized vector perpendicular to the vector (x3,y3)
    length = math.sqrt(x3 * x3 + y3 * y3)
    if length != 0:
        nx = -y3 / length
        ny = x3 / length
    else:
        nx = 1
        ny = 0
    # Scale perpendicular vector by random factor """
    r = random.uniform(-length / (1 + smoothness), length / (1 + smoothness))
    nx = nx * r
    ny = ny * r
    # add scaled perpendicular vector to the half-way point to get the final displaced subdivision point
    x = hx + nx
    y = hy + ny
    return (x, y)


class Fractalize(inkex.EffectExtension):
    def add_arguments(self, pars):
        pars.add_argument("-s", "--subdivs", type=int, default="6",
                          help="Number of subdivisons")
        pars.add_argument("-f", "--smooth", type=float, default="4.0",
                          help="Smoothness of the subdivision")

    def effect(self):
        for node in self.svg.selection.filter(inkex.PathElement).values():
            path = node.path.to_absolute()
            result = []
            for cmd_proxy in path.proxy_iterator():  # type: inkex.Path.PathCommandProxy
                prev = cmd_proxy.previous_end_point
                end = cmd_proxy.end_point
                if cmd_proxy.letter == 'M':
                    result.append(Move(*cmd_proxy.args))
                else:
                    for seg in self.fractalize((prev.x, prev.y, end.x, end.y), self.options.subdivs,
                                               self.options.smooth):
                        result.append(Line(*seg))
                    result.append(Line(end.x, end.y))

            node.path = result

    def fractalize(self, coords, subdivs, smooth):
        """recursively subdivide the segments left and right of the subdivision"""
        subdiv_point = calculate_subdivision(smooth, *coords)

        if subdivs:
            # recursively subdivide the segment left of the subdivision point
            for left_seg in self.fractalize(coords[:2] + subdiv_point[-2:], subdivs - 1, smooth):
                yield left_seg

            yield subdiv_point

            # recursively subdivide the segment right of the subdivision point
            for right_seg in self.fractalize(subdiv_point[-2:] + coords[-2:], subdivs - 1, smooth):
                yield right_seg

if __name__ == '__main__':
    Fractalize().run()
