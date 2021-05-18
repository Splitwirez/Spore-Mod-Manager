#!/usr/bin/env python
# coding=utf-8
# Copyright (C) 2011 Karlisson Bezerra <contact@hacktoon.com>
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
Save an SVG file into an html canvas file.
"""

import inkex

import ink2canvas_lib.svg as svg
from ink2canvas_lib.canvas import Canvas

class Html5Canvas(inkex.OutputExtension):
    """Creates a canvas output"""
    def save(self, stream):
        svg_root = self.document.getroot()
        width = self.svg.unittouu(svg_root.get("width"))
        height = self.svg.unittouu(svg_root.get("height"))
        canvas = Canvas(self, width, height)
        self.walk_tree(svg_root, canvas)
        stream.write(canvas.output().encode('utf-8'))

    def get_gradient_defs(self, elem):
        """Return the gradient information"""
        url_id = elem.get_gradient_href()
        # get the gradient element
        gradient = self.svg.getElementById(url_id)
        # get the color stops
        gstops = gradient.href
        colors = []
        for stop in gstops:
            colors.append(stop.get("style"))
        if gradient.get("r"):
            return svg.RadialGradientDef(gradient, colors)
        return svg.LinearGradientDef(gradient, colors)

    @staticmethod
    def _shape_from_node(node, canvas):
        """
        Make a canvas shape object for the given node. Returns `None` if
        the node is not an SVG shape element.
        @rtype svg.AbstractShape or NoneType
        """
        prefix, _brace_, command = node.tag.partition('}')
        if prefix != '{http://www.w3.org/2000/svg':
            return None

        # makes pylint happy
        assert _brace_ == '}'

        cls = getattr(svg, command.capitalize(), None)

        if not (isinstance(cls, type) and issubclass(cls, svg.AbstractShape)):
            return None

        return cls(command, node, canvas)

    def walk_tree(self, root, canvas):
        """Walk throug the whole svg tree"""
        for node in root:
            elem = self._shape_from_node(node, canvas)
            if elem is None:
                continue
            gradient = None
            if elem.has_gradient():
                gradient = self.get_gradient_defs(elem)
            elem.start(gradient)
            elem.draw()
            self.walk_tree(node, canvas)
            elem.end()


if __name__ == "__main__":
    Html5Canvas().run()
