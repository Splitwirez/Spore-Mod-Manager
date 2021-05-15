#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2006 Aaron Spike, aaron@ekips.org
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
import inkex
from inkex import bezier

class Flatten(inkex.EffectExtension):
    """Flattern a path"""
    def add_arguments(self, pars):
        pars.add_argument("--flatness", type=float, default=10.0, help="Minimum flattness")

    def effect(self):
        for node in self.svg.selection.filter(inkex.PathElement).values():
            path = node.path.to_superpath()
            bezier.cspsubdiv(path, self.options.flatness)
            newpath = []
            for subpath in path:
                first = True
                for csp in subpath:
                    cmd = 'L'
                    if first:
                        cmd = 'M'
                    first = False
                    newpath.append([cmd, [csp[1][0], csp[1][1]]])
            node.path = newpath

if __name__ == '__main__':
    Flatten().run()
