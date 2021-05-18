#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2007 John Beard john.j.beard@gmail.com
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
This extension allows you to draw a polar grid in Inkscape.
There is a wide range of options including subdivision and labels.
"""

from math import cos, log, pi, sin

import inkex
from inkex import Group, Circle, TextElement

def draw_circle(r, cx, cy, width, fill, name, parent):
    """Draw an SVG circle"""
    circle = parent.add(Circle(cx=str(cx), cy=str(cy), r=str(r)))
    circle.style = {'stroke': '#000000', 'stroke-width': str(width), 'fill': fill}
    circle.label = name

def draw_line(x1, y1, x2, y2, width, name, parent):
    """Draw an SVG line"""
    line = parent.add(inkex.PathElement())
    line.style = {'stroke': '#000000', 'stroke-width': str(width), 'fill': 'none'}
    line.path = 'M {},{} L {},{}'.format(x1, y1, x2, y2)
    line.label = name

def draw_label(x, y, string, font_size, name, parent):
    """Draw a centered label"""
    label = parent.add(TextElement(x=str(x), y=str(y)))
    label.style = {'text-align': 'center', 'vertical-align': 'top',
                   'text-anchor': 'middle', 'font-size': str(font_size) + 'px',
                   'fill-opacity': '1.0', 'stroke': 'none',
                   'font-weight': 'normal', 'font-style': 'normal', 'fill': '#000000'}
    label.text = string
    label.label = name


class GridPolar(inkex.GenerateExtension):
    def add_arguments(self, pars):
        pars.add_argument("--tab")
        pars.add_argument("--r_divs", type=int, default=5, help="Circular Divisions")
        pars.add_argument("--dr", type=float, default=50, help="Circular Division Spacing")
        pars.add_argument("--r_subdivs", type=int, default=3, help="Subdivisions per Major div")
        pars.add_argument("--r_log", type=inkex.Boolean, default=False, help="Logarithmic subdiv")
        pars.add_argument("--r_divs_th", type=float, default=2, help="Major Line thickness")
        pars.add_argument("--r_subdivs_th", type=float, default=1, help="Minor Line thickness")
        pars.add_argument("--a_divs", type=int, default=24, help="Angle Divisions")
        pars.add_argument("--a_divs_cent", type=int, default=4, help="Angle Divisions at Centre")
        pars.add_argument("--a_subdivs", type=int, default=1, help="Angcular Subdivisions")
        pars.add_argument("--a_subdivs_cent", type=int, default=1, help="Angular Subdivisions end")
        pars.add_argument("--a_divs_th", type=float, default=2, help="Major Angular thickness")
        pars.add_argument("--a_subdivs_th", type=float, default=1, help="Minor Angular thickness")
        pars.add_argument("--c_dot_dia", type=float, default=5.0, help="Diameter of Centre Dot")
        pars.add_argument("--a_labels", default='deg', help="The kind of labels to apply")
        pars.add_argument("--a_label_size", type=int, default=18, help="Pixel size of the labels")
        pars.add_argument("--a_label_outset", type=float, default=24, help="Label Radial outset")

    def generate(self):
        self.options.dr = self.svg.unittouu(str(self.options.dr) + 'px')
        self.options.r_divs_th = self.svg.unittouu(str(self.options.r_divs_th) + 'px')
        self.options.r_subdivs_th = self.svg.unittouu(str(self.options.r_subdivs_th) + 'px')
        self.options.a_divs_th = self.svg.unittouu(str(self.options.a_divs_th) + 'px')
        self.options.a_subdivs_th = self.svg.unittouu(str(self.options.a_subdivs_th) + 'px')
        self.options.c_dot_dia = self.svg.unittouu(str(self.options.c_dot_dia) + 'px')
        self.options.a_label_size = self.svg.unittouu(str(self.options.a_label_size) + 'px')
        self.options.a_label_outset = self.svg.unittouu(str(self.options.a_label_outset) + 'px')

        # Embed grid in group
        grid = Group.new("GridPolar:R{0.r_divs}:A{0.a_divs}".format(self.options))

        (pos_x, pos_y) = self.svg.namedview.center
        grid.transform.add_translate(pos_x, pos_y)

        dr = self.options.dr  # Distance between neighbouring circles
        dtheta = 2 * pi / self.options.a_divs_cent  # Angular change between adjacent radial lines at centre
        rmax = self.options.r_divs * dr

        # Create SVG circles
        for i in range(1, self.options.r_divs + 1):
            draw_circle(i * dr, 0, 0,  # major div circles
                            self.options.r_divs_th, 'none',
                            'MajorDivCircle' + str(i) + ':R' + str(i * dr), grid)

            if self.options.r_log:  # logarithmic subdivisions
                for j in range(2, self.options.r_subdivs):
                    draw_circle(i * dr - (1 - log(j, self.options.r_subdivs)) * dr,  # minor div circles
                                    0, 0, self.options.r_subdivs_th, 'none',
                                    'MinorDivCircle' + str(i) + ':Log' + str(j), grid)
            else:  # linear subdivs
                for j in range(1, self.options.r_subdivs):
                    draw_circle(i * dr - j * dr / self.options.r_subdivs,  # minor div circles
                                    0, 0, self.options.r_subdivs_th, 'none',
                                    'MinorDivCircle' + str(i) + ':R' + str(i * dr), grid)

        if self.options.a_divs == self.options.a_divs_cent:  # the lines can go from the centre to the edge
            for i in range(0, self.options.a_divs):
                draw_line(0, 0, rmax * sin(i * dtheta), rmax * cos(i * dtheta),
                              self.options.a_divs_th, 'RadialGridline' + str(i), grid)

        else:  # we need separate lines
            for i in range(0, self.options.a_divs_cent):  # lines that go to the first circle
                draw_line(0, 0, dr * sin(i * dtheta), dr * cos(i * dtheta),
                              self.options.a_divs_th, 'RadialGridline' + str(i), grid)

            dtheta = 2 * pi / self.options.a_divs  # work out the angle change for outer lines

            for i in range(0, self.options.a_divs):  # lines that go from there to the edge
                draw_line(dr * sin(i * dtheta + pi / 2.0), dr * cos(i * dtheta + pi / 2.0),
                              rmax * sin(i * dtheta + pi / 2.0), rmax * cos(i * dtheta + pi / 2.0),
                              self.options.a_divs_th, 'RadialGridline' + str(i), grid)

        if self.options.a_subdivs > 1:  # draw angular subdivs
            for i in range(0, self.options.a_divs):  # for each major division
                for j in range(1, self.options.a_subdivs):  # draw the subdivisions
                    angle = i * dtheta - j * dtheta / self.options.a_subdivs + pi / 2.0  # the angle of the subdivion line
                    draw_line(dr * self.options.a_subdivs_cent * sin(angle),
                                  dr * self.options.a_subdivs_cent * cos(angle),
                                  rmax * sin(angle), rmax * cos(angle),
                                  self.options.a_subdivs_th, 'RadialMinorGridline' + str(i), grid)

        if self.options.c_dot_dia != 0:  # if a non-zero diameter, draw the centre dot
            draw_circle(self.options.c_dot_dia / 2.0,
                            0, 0, 0, '#000000', 'CentreDot', grid)

        if self.options.a_labels == 'deg':
            label_radius = rmax + self.options.a_label_outset  # radius of label centres
            label_size = self.options.a_label_size
            numeral_size = 0.73 * label_size  # numerals appear to be 0.73 the height of the nominal pixel size of the font in "Sans"

            for i in range(0, self.options.a_divs):  # self.options.a_divs): #radial line labels
                draw_label(sin(i * dtheta + pi / 2.0) * label_radius,  # 0 at the RHS, mathematical style
                                       cos(i * dtheta + pi / 2.0) * label_radius + numeral_size / 2.0,  # centre the text vertically
                                       str(i * 360 / self.options.a_divs),
                                       label_size, 'Label' + str(i), grid)

        return grid

if __name__ == '__main__':
    GridPolar().run()
