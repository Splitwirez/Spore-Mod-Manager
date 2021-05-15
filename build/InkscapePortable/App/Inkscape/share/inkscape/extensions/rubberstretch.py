#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2006 Jean-Francois Barraud, barraud@math.univ-lille1.fr
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
Distorts selected paths, stretching it vertically while squeezing horizontally.

Amount is controlled by ratio parameter.

Curve gives further effect by bunching the surface towards the ends.
"""

from inkex.utils import X, Y
from pathmodifier import Diffeo


class RubberStretch(Diffeo):
    """Distort selected paths"""
    ratio = property(lambda self: -(self.options.ratio / 100))
    curve = property(lambda self: min(self.options.curve / 100, 0.99))

    def add_arguments(self, pars):
        pars.add_argument("-r", "--ratio", type=float, default=0.5)
        pars.add_argument("-c", "--curve", type=float, default=0.5)

    def applyDiffeo(self, bpt, vects=()):
        for vect in vects:
            vect[0] -= bpt[0]
            vect[1] -= bpt[1]
            vect[1] *= -1
        bpt[1] *= -1
        bx0 = self.bbox.center.x
        by0 = -self.bbox.center.y

        x, y = (bpt[0] - bx0), (bpt[1] - by0)
        sx1 = (1 + self.curve * (x / self.bbox.width + 1) * \
              (x / self.bbox.width - 1)) * 2 ** self.ratio
        sy1 = (1 + self.curve * (y / self.bbox.height + 1) * \
              (y / self.bbox.height - 1)) * 2 ** self.ratio
        bpt[0] = bx0 + x * sy1
        bpt[1] = by0 + y / sx1
        for vect in vects:
            dx_dx = sy1
            dx_dy = x * 2 * self.curve * y / self.bbox.height / self.bbox.height * 2 ** self.ratio
            dy_dx = -y * 2 * self.curve * x / self.bbox.width / \
                        self.bbox.width * 2 ** self.ratio / sx1 / sx1
            dy_dy = 1 / sx1
            vect[0] = dx_dx * vect[X] + dx_dy * vect[Y]
            vect[1] = dy_dx * vect[X] + dy_dy * vect[Y]

        # --spherify
        # s=((x*x+y*y)/(w*w+h*h))**(-a/2)
        # bpt[0]=x0+s*x
        # bpt[1]=y0+s*y
        # for v in vects:
        #    dx,dy=v
        #    v[0]=(1-a/2/(x*x+y*y)*2*x*x)*s*dx+( -a/2/(x*x+y*y)*2*y*x)*s*dy
        #    v[1]=( -a/2/(x*x+y*y)*2*x*y)*s*dx+(1-a/2/(x*x+y*y)*2*y*y)*s*dy

        for vect in vects:
            vect[0] += bpt[0]
            vect[1] += bpt[1]
            vect[1] *= -1
        bpt[1] *= -1


if __name__ == "__main__":
    RubberStretch().run()
