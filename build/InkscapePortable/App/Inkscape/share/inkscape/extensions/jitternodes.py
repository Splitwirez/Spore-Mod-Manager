#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2012 Juan Pablo Carbajal ajuanpi-dev@gmail.com
# Copyright (C) 2005 Aaron Spike, aaron@ekips.org
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 3 of the License, or
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
import random
import inkex


class JitterNodes(inkex.EffectExtension):
    """Jiggle nodes around"""
    def add_arguments(self, pars):
        pars.add_argument("--tab")
        pars.add_argument("--radiusx", type=float, default=10.0, help="Randum radius X")
        pars.add_argument("--radiusy", type=float, default=10.0, help="Randum radius Y")
        pars.add_argument("--ctrl", type=inkex.Boolean, default=True, help="Randomize ctrl points")
        pars.add_argument("--end", type=inkex.Boolean, default=True, help="Randomize nodes")
        pars.add_argument("--dist", type=self.arg_method('dist'),
                          default=self.dist_uniform, help="Distribution of displacement")

    def effect(self):
        for node in self.svg.selection.filter(inkex.PathElement).values():
            path = node.path.to_superpath()
            for subpath in path:
                closed = subpath[0] == subpath[-1]
                for index, csp in enumerate(subpath):
                    if closed and index == len(subpath) - 1:
                            subpath[index] = subpath[0]
                            break
                    if self.options.end:
                        delta = self.randomize([0, 0])
                        csp[0][0] += delta[0]
                        csp[0][1] += delta[1]
                        csp[1][0] += delta[0]
                        csp[1][1] += delta[1]
                        csp[2][0] += delta[0]
                        csp[2][1] += delta[1]
                    if self.options.ctrl:
                        csp[0] = self.randomize(csp[0])
                        csp[2] = self.randomize(csp[2])
            node.path = path

    def randomize(self, pos):
        """Randomise the given position [x, y] as set in the options"""
        delta = self.options.dist(self.options.radiusx, self.options.radiusy)
        return [pos[0] + delta[0], pos[1] + delta[1]]

    @staticmethod
    def dist_gaussian(x, y):
        """Gaussian distribution"""
        return random.gauss(0.0, x), random.gauss(0.0, y)

    @staticmethod
    def dist_pareto(x, y):
        """Pareto distribution"""
        # sign is used to fake a double sided pareto distribution.
        # for parameter value between 1 and 2 the distribution has infinite variance
        # I truncate the distribution to a high value and then normalize it.
        # The idea is to get spiky distributions, any distribution with long-tails is
        # good (ideal would be Levy distribution).
        sign = random.uniform(-1.0, 1.0)
        return x * math.copysign(min(random.paretovariate(1.0), 20.0) / 20.0, sign),\
               y * math.copysign(min(random.paretovariate(1.0), 20.0) / 20.0, sign)

    @staticmethod
    def dist_lognorm(x, y):
        """Log Norm distribution"""
        sign = random.uniform(-1.0, 1.0)
        return x * math.copysign(random.lognormvariate(0.0, 1.0) / 3.5, sign),\
               y * math.copysign(random.lognormvariate(0.0, 1.0) / 3.5, sign)

    @staticmethod
    def dist_uniform(x, y):
        """Uniform distribution"""
        return random.uniform(-x, x), random.uniform(-y, y)

if __name__ == '__main__':
    JitterNodes().run()
