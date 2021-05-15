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
import math

import inkex
from inkex import TextElement, Circle

class NumberNodes(inkex.EffectExtension):
    """Replace the selection's nodes with numbered dots according to the options"""
    def add_arguments(self, pars):
        pars.add_argument("--dotsize", default="10px", help="Size of the dots on the path nodes")
        pars.add_argument("--fontsize", default="20px", help="Size of node labels")
        pars.add_argument("--start", type=int, default=1, help="First number in the sequence")
        pars.add_argument("--step", type=int, default=1, help="Numbering step between two nodes")
        pars.add_argument("--tab", help="The selected UI-tab when OK was pressed")

    def effect(self):
        if not self.svg.selected:
            raise inkex.AbortExtension("Please select an object.")
        for node in self.svg.selection.filter(inkex.PathElement).values():
            self.add_dot(node)

    def add_dot(self, node):
        """Add a dot label for this path element"""
        group = node.getparent().add(inkex.Group())
        dot_group = group.add(inkex.Group())
        num_group = group.add(inkex.Group())
        group.transform = node.transform

        style = inkex.Style({'stroke': 'none', 'fill': '#000'})

        for step, (x, y) in enumerate(node.path.end_points):
            circle = dot_group.add(Circle(cx=str(x), cy=str(y),\
                    r=str(self.svg.unittouu(self.options.dotsize) / 2)))
            circle.style = style
            num_group.append(self.add_text(
                x + (self.svg.unittouu(self.options.dotsize) / 2),
                y - (self.svg.unittouu(self.options.dotsize) / 2),
                self.options.start + (self.options.step * step)))

        node.delete()

    def add_text(self, x, y, text):
        """Add a text label at the given location"""
        elem = TextElement(x=str(x), y=str(y))
        elem.text = str(text)
        elem.style = {
            'font-size': self.svg.unittouu(self.options.fontsize),
            'fill-opacity': '1.0',
            'stroke': 'none',
            'font-weight': 'normal',
            'font-style': 'normal',
            'fill': '#999'}
        return elem

if __name__ == '__main__':
    NumberNodes().run()
