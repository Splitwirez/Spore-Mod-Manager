#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2005, 2007 Aaron Spike, aaron@ekips.org
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
"""
This extension either adds nodes to a path so that

  No segment is longer than a maximum value OR that each segment is divided
  into a given number of equal segments.

"""
import math
import inkex

from inkex import bezier, PathElement, CubicSuperPath


class AddNodes(inkex.EffectExtension):
    """Extension to split a path by adding nodes to it"""
    def add_arguments(self, pars):
        pars.add_argument("--segments", type=int, default=2,
                          help="Number of segments to divide the path into")
        pars.add_argument("--max", type=float, default=2.0,
                          help="Number of segments to divide the path into")
        pars.add_argument("--method", help="The kind of division to perform")

    def effect(self):
        for node in self.svg.selection.filter(PathElement).values():
            new = []
            for sub in node.path.to_superpath():
                new.append([sub[0][:]])
                i = 1
                while i <= len(sub) - 1:
                    length = bezier.cspseglength(new[-1][-1], sub[i])

                    if self.options.method == 'bynum':
                        splits = self.options.segments
                    else:
                        splits = math.ceil(length / self.options.max)

                    for sel in range(int(splits), 1, -1):
                        result = bezier.cspbezsplitatlength(new[-1][-1], sub[i], 1.0 / sel)
                        better_result = [[list(el) for el in elements] for elements in result]
                        new[-1][-1], nxt, sub[i] = better_result
                        new[-1].append(nxt[:])
                    new[-1].append(sub[i])
                    i += 1
            node.path = CubicSuperPath(new).to_path(curves_only=True)

if __name__ == '__main__':
    AddNodes().run()
