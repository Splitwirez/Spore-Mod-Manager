#!/usr/bin/env python
#
# Copyright (C) 2013 Brett Graham (hahahaha @ hahaha.org)
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
Generate a gear rack as SVG.
"""

from math import acos, cos, radians, sin, sqrt, tan

import inkex

def involute_intersect_angle(Rb, R):
    Rb, R = float(Rb), float(R)
    return (sqrt(R ** 2 - Rb ** 2) / Rb) - (acos(Rb / R))


def point_on_circle(radius, angle):
    x = radius * cos(angle)
    y = radius * sin(angle)
    return x, y


def points_to_svgd(p):
    """
    p: list of 2 tuples (x, y coordinates)
    """
    f = p[0]
    p = p[1:]
    svgd = 'M{:.3f},{:.3f}'.format(f[0], f[1])
    for x in p:
        svgd += 'L{:.3f},{:.3f}'.format(x[0], x[1])
    return svgd


class RackGear(inkex.GenerateExtension):
    container_label = 'Rendered Gear Rack'
    def add_arguments(self, pars):
        pars.add_argument("--length", type=float, default=100.0, help="Rack Length")
        pars.add_argument("--spacing", type=float, default=10.0, help="Tooth Spacing")
        pars.add_argument("--angle", type=float, default=20.0, help="Contact Angle")

    def generate(self):
        length = self.svg.unittouu(str(self.options.length) + 'px')
        spacing = self.svg.unittouu(str(self.options.spacing) + 'px')
        angle = radians(self.options.angle)

        # generate points: list of (x, y) pairs
        points = []
        x = 0
        tas = tan(angle) * spacing
        while x < length:
            # move along path, generating the next 'tooth'
            points.append((x, 0))
            points.append((x + tas, spacing))
            points.append((x + spacing, spacing))
            points.append((x + spacing + tas, 0))
            x += spacing * 2.

        path = points_to_svgd(points)

        # Create SVG Path for gear
        style = {'stroke': '#000000', 'fill': 'none', 'stroke-width': str(self.svg.unittouu('1px'))}
        yield inkex.PathElement(style=str(inkex.Style(style)), d=str(path))

if __name__ == '__main__':
    RackGear().run()
