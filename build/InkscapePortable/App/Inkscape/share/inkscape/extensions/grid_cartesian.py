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
This extension allows you to draw a Cartesian grid in Inkscape.

There is a wide range of options including subdivision, subsubdivions and
logarithmic scales. Custom line widths are also possible.

All elements are grouped with similar elements (eg all x-subdivs)
"""

from math import log

import inkex
from inkex import Group, PathElement, Rectangle

def draw_line(x1, y1, x2, y2, width, name, parent):
    """Draw an SVG line"""
    line = parent.add(PathElement())
    line.style = {'stroke': '#000000', 'stroke-width': str(width), 'fill': 'none'}
    line.path = 'M {},{} L {},{}'.format(x1, y1, x2, y2)
    line.label = name


def draw_rect(x, y, w, h, width, fill, name, parent):
    """Draw an SVG Rectangle"""
    rect = parent.add(Rectangle(x=str(x), y=str(y), width=str(w), height=str(h)))
    rect.style = {'stroke': '#000000', 'stroke-width': str(width), 'fill': fill}
    rect.label = name


class GridCartesian(inkex.GenerateExtension):
    def add_arguments(self, pars):
        pars.add_argument("--border_th", type=float, default=3)
        pars.add_argument("--border_th_unit", default="cm")
        pars.add_argument("--tab", default="x_tab")
        pars.add_argument("--x_divs", type=int, default=6)
        pars.add_argument("--dx", type=float, default=100.0)
        pars.add_argument("--dx_unit", default="cm")
        pars.add_argument("--x_subdivs", type=int, default=2)
        pars.add_argument("--x_log", type=inkex.Boolean, default="false")
        pars.add_argument("--x_subsubdivs", type=int, default=5)
        pars.add_argument("--x_half_freq", type=int, default=4)
        pars.add_argument("--x_divs_th", type=float, default=2)
        pars.add_argument("--x_subdivs_th", type=float, default=1)
        pars.add_argument("--x_subsubdivs_th", type=float, default=0.3)
        pars.add_argument("--x_div_unit", default="cm")
        pars.add_argument("--y_divs", type=int, default=5)
        pars.add_argument("--dy", type=float, default=100.0)
        pars.add_argument("--dy_unit", default="cm")
        pars.add_argument("--y_subdivs", type=int, default=1)
        pars.add_argument("--y_log", type=inkex.Boolean, default="false")
        pars.add_argument("--y_subsubdivs", type=int, default=5)
        pars.add_argument("--y_half_freq", type=int, default=4)
        pars.add_argument("--y_divs_th", type=float, default=2)
        pars.add_argument("--y_subdivs_th", type=float, default=1)
        pars.add_argument("--y_subsubdivs_th", type=float, default=0.3)
        pars.add_argument("--y_div_unit", default="cm")

    def generate(self):
        self.options.border_th = self.svg.unittouu(str(self.options.border_th) + self.options.border_th_unit)

        self.options.dx = self.svg.unittouu(str(self.options.dx) + self.options.dx_unit)
        self.options.x_divs_th = self.svg.unittouu(str(self.options.x_divs_th) + self.options.x_div_unit)
        self.options.x_subdivs_th = self.svg.unittouu(str(self.options.x_subdivs_th) + self.options.x_div_unit)
        self.options.x_subsubdivs_th = self.svg.unittouu(str(self.options.x_subsubdivs_th) + self.options.x_div_unit)

        self.options.dy = self.svg.unittouu(str(self.options.dy) + self.options.dy_unit)
        self.options.y_divs_th = self.svg.unittouu(str(self.options.y_divs_th) + self.options.y_div_unit)
        self.options.y_subdivs_th = self.svg.unittouu(str(self.options.y_subdivs_th) + self.options.y_div_unit)
        self.options.y_subsubdivs_th = self.svg.unittouu(str(self.options.y_subsubdivs_th) + self.options.y_div_unit)

        # find the pixel dimensions of the overall grid
        ymax = self.options.dy * self.options.y_divs
        xmax = self.options.dx * self.options.x_divs

        # Embed grid in group
        # Put in in the centre of the current view

        grid = Group.new("GridCartesian:X{0.x_divs}:Y{0.y_divs}".format(self.options))

        (pos_x, pos_y) = self.svg.namedview.center
        grid.transform.add_translate(pos_x - xmax / 2.0, pos_y - ymax / 2.0)

        # Group for major x gridlines
        majglx = grid.add(Group.new("MajorXGridlines"))
        # Group for major y gridlines
        majgly = grid.add(Group.new("MajorYGridlines"))

        # Group for minor x gridlines
        if self.options.x_subdivs > 1:  # if there are any minor x gridlines
            minglx = grid.add(Group.new("MinorXGridlines"))

        # Group for subminor x gridlines
        if self.options.x_subsubdivs > 1:  # if there are any minor minor x gridlines
            mminglx = grid.add(Group.new("SubMinorXGridlines"))

        # Group for minor y gridlines
        if self.options.y_subdivs > 1:  # if there are any minor y gridlines
            mingly = grid.add(Group.new("MinorYGridlines"))

        # Group for subminor y gridlines
        if self.options.y_subsubdivs > 1:  # if there are any minor minor x gridlines
            mmingly = grid.add(Group.new("SubMinorYGridlines"))

        draw_rect(0, 0, xmax, ymax, self.options.border_th,
                      'none', 'Border', grid)  # border rectangle

        # DO THE X DIVISIONS======================================
        sd = self.options.x_subdivs  # sub divs per div
        ssd = self.options.x_subsubdivs  # subsubdivs per subdiv

        for i in range(0, self.options.x_divs):  # Major x divisions
            if i > 0:  # don't draw first line (we made a proper border)
                draw_line(self.options.dx * i, 0,
                              self.options.dx * i, ymax,
                              self.options.x_divs_th,
                              'MajorXDiv' + str(i), majglx)

            if self.options.x_log:  # log x subdivs
                for j in range(1, sd):
                    if j > 1:  # the first loop is only for subsubdivs
                        draw_line(self.options.dx * (i + log(j, sd)), 0,
                                      self.options.dx * (i + log(j, sd)), ymax,
                                      self.options.x_subdivs_th,
                                      'MinorXDiv' + str(i) + ':' + str(j), minglx)

                    for k in range(1, ssd):  # subsub divs
                        if (j <= self.options.x_half_freq) or (k % 2 == 0):  # only draw half the subsubdivs past the half-freq point
                            if (ssd % 2 > 0) and (j > self.options.y_half_freq):  # half frequency won't work with odd numbers of subsubdivs,
                                ssd2 = ssd + 1  # make even
                            else:
                                ssd2 = ssd  # no change
                            draw_line(self.options.dx * (i + log(j + k / float(ssd2), sd)), 0,
                                          self.options.dx * (i + log(j + k / float(ssd2), sd)), ymax,
                                          self.options.x_subsubdivs_th, 'SubminorXDiv' + str(i) + ':' + str(j) + ':' + str(k), mminglx)

            else:  # linear x subdivs
                for j in range(0, sd):
                    if j > 0:  # not for the first loop (this loop is for the subsubdivs before the first subdiv)
                        draw_line(self.options.dx * (i + j / float(sd)), 0,
                                      self.options.dx * (i + j / float(sd)), ymax,
                                      self.options.x_subdivs_th,
                                      'MinorXDiv' + str(i) + ':' + str(j), minglx)

                    for k in range(1, ssd):  # subsub divs
                        draw_line(self.options.dx * (i + (j * ssd + k) / (float(sd) * ssd)), 0,
                                      self.options.dx * (i + (j * ssd + k) / (float(sd) * ssd)), ymax,
                                      self.options.x_subsubdivs_th,
                                      'SubminorXDiv' + str(i) + ':' + str(j) + ':' + str(k), mminglx)

        # DO THE Y DIVISIONS========================================
        sd = self.options.y_subdivs  # sub divs per div
        ssd = self.options.y_subsubdivs  # subsubdivs per subdiv

        for i in range(0, self.options.y_divs):  # Major y divisions
            if i > 0:  # don't draw first line (we will make a border)
                draw_line(0, self.options.dy * i,
                              xmax, self.options.dy * i,
                              self.options.y_divs_th,
                              'MajorYDiv' + str(i), majgly)

            if self.options.y_log:  # log y subdivs
                for j in range(1, sd):
                    if j > 1:  # the first loop is only for subsubdivs
                        draw_line(0, self.options.dy * (i + 1 - log(j, sd)),
                                      xmax, self.options.dy * (i + 1 - log(j, sd)),
                                      self.options.y_subdivs_th,
                                      'MinorXDiv' + str(i) + ':' + str(j), mingly)

                    for k in range(1, ssd):  # subsub divs
                        if (j <= self.options.y_half_freq) or (k % 2 == 0):  # only draw half the subsubdivs past the half-freq point
                            if (ssd % 2 > 0) and (j > self.options.y_half_freq):  # half frequency won't work with odd numbers of subsubdivs,
                                ssd2 = ssd + 1
                            else:
                                ssd2 = ssd  # no change
                            draw_line(0, self.options.dx * (i + 1 - log(j + k / float(ssd2), sd)),
                                          xmax, self.options.dx * (i + 1 - log(j + k / float(ssd2), sd)),
                                          self.options.y_subsubdivs_th,
                                          'SubminorXDiv' + str(i) + ':' + str(j) + ':' + str(k), mmingly)
            else:  # linear y subdivs
                for j in range(0, self.options.y_subdivs):
                    if j > 0:  # not for the first loop (this loop is for the subsubdivs before the first subdiv)
                        draw_line(0, self.options.dy * (i + j / float(sd)),
                                      xmax, self.options.dy * (i + j / float(sd)),
                                      self.options.y_subdivs_th,
                                      'MinorXYiv' + str(i) + ':' + str(j), mingly)

                    for k in range(1, ssd):  # subsub divs
                        draw_line(0, self.options.dy * (i + (j * ssd + k) / (float(sd) * ssd)),
                                      xmax, self.options.dy * (i + (j * ssd + k) / (float(sd) * ssd)),
                                      self.options.y_subsubdivs_th,
                                      'SubminorXDiv' + str(i) + ':' + str(j) + ':' + str(k), mmingly)

        return grid


if __name__ == '__main__':
    GridCartesian().run()
