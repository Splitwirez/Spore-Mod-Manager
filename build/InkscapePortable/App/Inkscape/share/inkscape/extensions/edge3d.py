#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2007 Terry Brown, terry_n_brown@yahoo.com
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

from math import atan2, degrees

import inkex
from inkex import ClipPath, Filter


class Edge3D(inkex.EffectExtension):
    """Generate a 3d edge"""
    def add_arguments(self, pars):
        pars.add_argument('--angle', type=float, default=45.0,
                          help='angle of illumination, clockwise, 45 = upper right')
        pars.add_argument('--stddev', type=float, default=5.0, help='Gaussian Blur stdDeviation')
        pars.add_argument('--blurheight', type=float, default=2.0, help='Gaussian Blur height')
        pars.add_argument('--blurwidth', type=float, default=2.0, help='Gaussian Blur width')
        pars.add_argument('--shades', type=int, default=2, help="Number of shades")
        pars.add_argument('--bw', type=inkex.Boolean, help="Black and white")
        pars.add_argument('--thick', type=float, default=10.0, help='stroke-width for pieces')

    def angle_between(self, start, end, angle):
        """Return true if angle (degrees, clockwise, 0 = up/north) is between
           angles start and end"""

        def f(x):
            """Add 360 to x if x is less than 0"""
            if x < 0:
                return x + 360.
            return x

        # rotate all inputs by start, => start = 0
        a = f(f(angle) - f(start))
        e = f(f(end) - f(start))
        return a < e

    def effect(self):
        """Check each internode to see if it's in one of the wedges
           for the current shade.  shade is a floating point 0-1 white-black"""
        # size of a wedge for shade i, wedges come in pairs
        delta = 360. / self.options.shades / 2.
        for node in self.svg.selection.filter(inkex.PathElement).values():
            array = node.path.to_arrays()
            group = None
            filt = None
            for shade in range(0, self.options.shades):
                if self.options.bw and 0 < shade < self.options.shades - 1:
                    continue
                start = [self.options.angle - delta * (shade + 1)]
                end = [self.options.angle - delta * shade]
                start.append(self.options.angle + delta * shade)
                end.append(self.options.angle + delta * (shade + 1))
                level = float(shade) / float(self.options.shades - 1)
                last = []
                result = []
                for cmd, params in array:
                    if cmd == 'Z':
                        last = []
                        continue
                    if last:
                        if cmd == 'V':
                            point = [last[0], params[-2:][0]]
                        elif cmd == 'H':
                            point = [params[-2:][0], last[1]]
                        else:
                            point = params[-2:]
                        ang = degrees(atan2(point[0] - last[0], point[1] - last[1]))
                        if (self.angle_between(start[0], end[0], ang) or \
                            self.angle_between(start[1], end[1], ang)):
                            result.append(('M', last))
                            result.append((cmd, params))
                        ref = point
                    else:
                        ref = params[-2:]
                    last = ref
                if result:
                    if group is None:
                        group, filt = self.get_group(node)
                    new_node = group.add(node.copy())
                    new_node.path = result
                    col = 255 - int(255. * level)
                    new_node.style = 'fill:none;stroke:#%02x%02x%02x;stroke-opacity:1;stroke-width:10;%s' % ((col,) * 3 + (filt,))

    def get_group(self, node):
        """
        make a clipped group, clip with clone of original, clipped group
        include original and group of paths.
        """
        defs = self.svg.defs
        clip = defs.add(ClipPath())
        new_node = clip.add(node.copy())
        clip_group = node.getparent().add(inkex.Group())
        group = clip_group.add(inkex.Group())
        clip_group.set('clip-path', 'url(#' + clip.get_id() + ')')

        # make a blur filter reference by the style of each path
        filt = defs.add(Filter(x='-0.5', y='-0.5',\
            height=str(self.options.blurheight),\
            width=str(self.options.blurwidth)))

        filt.add_primitive('feGaussianBlur', stdDeviation=self.options.stddev)
        return group, 'filter:url(#%s);' % filt.get_id()

if __name__ == '__main__':
    Edge3D().run()
