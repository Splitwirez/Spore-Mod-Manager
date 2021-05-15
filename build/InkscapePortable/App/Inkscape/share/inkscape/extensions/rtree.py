#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2005 Aaron Spike, aaron@ekips.org
# Copyright (C) 2015 su_v, suv-sf@users.sf.net
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

import inkex
from inkex import turtle as pturtle

class TurtleRtree(inkex.GenerateExtension):
    """Create RTree Turtle path"""
    def add_arguments(self, pars):
        pars.add_argument("--size", type=float, default=100.0,
                          help="initial branch size")
        pars.add_argument("--minimum", type=float, default=4.0,
                          help="minimum branch size")
        pars.add_argument("--pentoggle", type=inkex.Boolean, default=False,
                          help="Lift pen for backward steps")

    def generate(self):
        self.options.size = self.svg.unittouu(str(self.options.size) + 'px')
        self.options.minimum = self.svg.unittouu(str(self.options.minimum) + 'px')
        point = self.svg.namedview.center

        style = inkex.Style({
            'stroke-linejoin': 'miter', 'stroke-width': str(self.svg.unittouu('1px')),
            'stroke-opacity': '1.0', 'fill-opacity': '1.0',
            'stroke': '#000000', 'stroke-linecap': 'butt',
            'fill': 'none'
        })
        tur = pturtle.pTurtle()
        tur.pu()
        tur.setpos(point)
        tur.pd()
        tur.rtree(self.options.size, self.options.minimum, self.options.pentoggle)
        return inkex.PathElement(d=tur.getPath(), style=str(style))

if __name__ == '__main__':
    TurtleRtree().run()
