#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2007 Tavmjong Bah, tavmjong@free.fr
# Copyright (C) 2006 Georg Wiora, xorx@quarkbox.de
# Copyright (C) 2006 Johan Engelen, johan@shouraizou.nl
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
# Changes:
#  * This program is a modified version of wavy.py by Aaron Spike.
#  * 22-Dec-2006: Wiora : Added axis and isotropic scaling
#  * 21-Jun-2007: Tavmjong: Added polar coordinates
#
import random
import math
from math import cos, pi, sin

import inkex
from inkex import ClipPath, Rectangle

EVAL_GLOBALS = {}
EVAL_GLOBALS.update(random.__dict__)
EVAL_GLOBALS.update(math.__dict__)


def drawfunction(xstart, xend, ybottom, ytop, samples, width, height, left, bottom,
                 fx="sin(x)", fpx="cos(x)", fponum=True, times2pi=False, polar=False, isoscale=True, drawaxis=True, endpts=False):
    if times2pi:
        xstart = 2 * pi * xstart
        xend = 2 * pi * xend

    # coords and scales based on the source rect
    if xstart == xend:
        inkex.errormsg("x-interval cannot be zero. Please modify 'Start X value' or 'End X value'")
        return []
    scalex = width / (xend - xstart)
    xoff = left
    coordx = lambda x: (x - xstart) * scalex + xoff  # convert x-value to coordinate
    if polar:  # Set scale so that left side of rectangle is -1, right side is +1.
        # (We can't use xscale for both range and scale.)
        centerx = left + width / 2.0
        polar_scalex = width / 2.0
        coordx = lambda x: x * polar_scalex + centerx  # convert x-value to coordinate

    if ytop == ybottom:
        inkex.errormsg("y-interval cannot be zero. Please modify 'Y value of rectangle's top' or 'Y value of rectangle's bottom'")
        return []
    scaley = height / (ytop - ybottom)
    yoff = bottom
    coordy = lambda y: (ybottom - y) * scaley + yoff  # convert y-value to coordinate

    # Check for isotropic scaling and use smaller of the two scales, correct ranges
    if isoscale and not polar:
        if scaley < scalex:
            # compute zero location
            xzero = coordx(0)
            # set scale
            scalex = scaley
            # correct x-offset
            xstart = (left - xzero) / scalex
            xend = (left + width - xzero) / scalex
        else:
            # compute zero location
            yzero = coordy(0)
            # set scale
            scaley = scalex
            # correct x-offset
            ybottom = (yzero - bottom) / scaley
            ytop = (bottom + height - yzero) / scaley

    # functions specified by the user
    try:
        if fx != "":
            f = eval('lambda x: ' + fx, EVAL_GLOBALS, {})
        if fpx != "":
            fp = eval('lambda x: ' + fpx, EVAL_GLOBALS, {})
    # handle incomplete/invalid function gracefully
    except SyntaxError:
        return []

    # step is the distance between nodes on x
    step = (xend - xstart) / (samples - 1)
    third = step / 3.0
    ds = step * 0.001  # Step used in calculating derivatives

    a = []  # path array
    # add axis
    if drawaxis:
        # check for visibility of x-axis
        if ybottom <= 0 <= ytop:
            # xaxis
            a.append(['M', [left, coordy(0)]])
            a.append(['l', [width, 0]])
        # check for visibility of y-axis
        if xstart <= 0 <= xend:
            # xaxis
            a.append(['M', [coordx(0), bottom]])
            a.append(['l', [0, -height]])

    # initialize function and derivative for 0;
    # they are carried over from one iteration to the next, to avoid extra function calculations.
    x0 = xstart
    y0 = f(xstart)
    if polar:
        xp0 = y0 * cos(x0)
        yp0 = y0 * sin(x0)
        x0 = xp0
        y0 = yp0
    if fponum or polar:  # numerical derivative, using 0.001*step as the small differential
        x1 = xstart + ds  # Second point AFTER first point (Good for first point)
        y1 = f(x1)
        if polar:
            xp1 = y1 * cos(x1)
            yp1 = y1 * sin(x1)
            x1 = xp1
            y1 = yp1
        dx0 = (x1 - x0) / ds
        dy0 = (y1 - y0) / ds
    else:  # derivative given by the user
        dx0 = 1  # Only works for rectangular coordinates
        dy0 = fp(xstart)

    # Start curve
    if endpts:
        a.append(['M', [left, coordy(0)]])
        a.append(['L', [coordx(x0), coordy(y0)]])
    else:
        a.append(['M', [coordx(x0), coordy(y0)]])  # initial moveto

    for i in range(int(samples - 1)):
        x1 = (i + 1) * step + xstart
        x2 = x1 - ds  # Second point BEFORE first point (Good for last point)
        y1 = f(x1)
        y2 = f(x2)
        if polar:
            xp1 = y1 * cos(x1)
            yp1 = y1 * sin(x1)
            xp2 = y2 * cos(x2)
            yp2 = y2 * sin(x2)
            x1 = xp1
            y1 = yp1
            x2 = xp2
            y2 = yp2
        if fponum or polar:  # numerical derivative
            dx1 = (x1 - x2) / ds
            dy1 = (y1 - y2) / ds
        else:  # derivative given by the user
            dx1 = 1  # Only works for rectangular coordinates
            dy1 = fp(x1)
        # create curve
        a.append(['C',
                  [coordx(x0 + (dx0 * third)), coordy(y0 + (dy0 * third)),
                   coordx(x1 - (dx1 * third)), coordy(y1 - (dy1 * third)),
                   coordx(x1), coordy(y1)]
                  ])
        x0 = x1  # Next segment's start is this segments end
        y0 = y1
        dx0 = dx1  # Assume the function is smooth everywhere, so carry over the derivative too
        dy0 = dy1
    if endpts:
        a.append(['L', [left + width, coordy(0)]])
    return a


class FuncPlot(inkex.EffectExtension):
    def add_arguments(self, pars):
        pars.add_argument("--tab")
        pars.add_argument("--xstart", type=float, default=0.0, help="Start x-value")
        pars.add_argument("--xend", type=float, default=1.0, help="End x-value")
        pars.add_argument("--times2pi", type=inkex.Boolean, default=True, help="* x-range by 2*pi")
        pars.add_argument("--polar", type=inkex.Boolean, default=False, help="Use polar coords")
        pars.add_argument("--ybottom", type=float, default=-1.0, help="y-value of rect's bottom")
        pars.add_argument("--ytop", type=float, default=1.0, help="y-value of rectangle's top")
        pars.add_argument("--samples", type=int, default=8, help="Samples")
        pars.add_argument("--fofx", default="sin(x)", help="f(x) for plotting")
        pars.add_argument("--fponum", type=inkex.Boolean, default=True, help="Numerical 1st deriv")
        pars.add_argument("--fpofx", default="cos(x)", help="f'(x) for plotting")
        pars.add_argument("--clip", type=inkex.Boolean, default=False, help="Clip with source rect")
        pars.add_argument("--remove", type=inkex.Boolean, default=True, help="Remove source rect")
        pars.add_argument("--isoscale", type=inkex.Boolean, default=True, help="Isotropic scaling")
        pars.add_argument("--drawaxis", type=inkex.Boolean, default=True, help="Draw axis")
        pars.add_argument("--endpts", type=inkex.Boolean, default=False, help="Add end points")

    def effect(self):
        newpath = None
        for node in self.svg.selected.values():
            if isinstance(node, Rectangle):
                # create new path with basic dimensions of selected rectangle
                newpath = inkex.PathElement()
                x = float(node.get('x'))
                y = float(node.get('y'))
                w = float(node.get('width'))
                h = float(node.get('height'))

                # copy attributes of rect
                newpath.style = node.style
                newpath.transform = node.transform

                # top and bottom were exchanged
                newpath.path = \
                        drawfunction(self.options.xstart,
                                     self.options.xend,
                                     self.options.ybottom,
                                     self.options.ytop,
                                     self.options.samples,
                                     w, h, x, y + h,
                                     self.options.fofx,
                                     self.options.fpofx,
                                     self.options.fponum,
                                     self.options.times2pi,
                                     self.options.polar,
                                     self.options.isoscale,
                                     self.options.drawaxis,
                                     self.options.endpts)
                newpath.set('title', self.options.fofx)

                # add path into SVG structure
                node.getparent().append(newpath)
                # option whether to clip the path with rect or not.
                if self.options.clip:
                    clip = self.svg.defs.add(ClipPath())
                    clip.set_random_id()
                    clip.append(node.copy())
                    newpath.set('clip-path', 'url(#' + clip.get_id() + ')')
                # option whether to remove the rectangle or not.
                if self.options.remove:
                    node.getparent().remove(node)
        if newpath is None:
            inkex.errormsg("Please select a rectangle")


if __name__ == '__main__':
    FuncPlot().run()
