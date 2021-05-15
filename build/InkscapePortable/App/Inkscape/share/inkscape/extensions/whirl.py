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
"""Whirl path extension (modify path)"""

import math
import inkex

class Whirl(inkex.EffectExtension):
    """Modify a path by twisting the nodes around a point"""
    def add_arguments(self, pars):
        pars.add_argument("-t", "--whirl", type=float,\
            default=1.0, help="amount of whirl")
        pars.add_argument("-r", "--rotation", type=inkex.Boolean,\
            default=True, help="direction of rotation")

    def effect(self):
        view_center = self.svg.namedview.center
        rotation = 1 if self.options.rotation else -1
        whirl = self.options.whirl / 1000
        for node in self.svg.selection.filter(inkex.PathElement).values():
            self.whirl_node(view_center, rotation, whirl, node)

    @staticmethod
    def whirl_node(center, direction, ammount, node):
        """Apply a whirl to a path given the center, direction and amount"""
        path = node.path.to_superpath()
        for sub in path:
            for csp in sub:
                for point in csp:
                    point[0] -= center[0]
                    point[1] -= center[1]
                    dist = math.sqrt((point[0] ** 2) + (point[1] ** 2))
                    if dist != 0:
                        art = direction * dist * ammount
                        theta = math.atan2(point[1], point[0]) + art
                        point[0] = (dist * math.cos(theta))
                        point[1] = (dist * math.sin(theta))
                    point[0] += center[0]
                    point[1] += center[1]
        node.path = path


if __name__ == '__main__':
    Whirl().run()
