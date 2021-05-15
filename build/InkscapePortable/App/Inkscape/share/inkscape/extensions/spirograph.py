#! /usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2007 Joel Holdsworth joel@airwebreathe.org.uk
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

class Spirograph(inkex.EffectExtension):
    def add_arguments(self, pars):
        pars.add_argument("--primaryr", type=float, default=60.0,
                          help="The radius of the outer gear")
        pars.add_argument("--secondaryr", type=float, default=100.0,
                          help="The radius of the inner gear")
        pars.add_argument("--penr", type=float, default=50.0,
                          help="The distance of the pen from the inner gear")
        pars.add_argument("--gearplacement", default="inside",
                          help="Selects whether the gear is inside or outside the ring")
        pars.add_argument("--rotation", type=float, default=0.0,
                          help="The number of degrees to rotate the image by")
        pars.add_argument("--quality", type=int, default=16,
                          help="The quality of the calculated output")

    def effect(self):
        self.options.primaryr = self.svg.unittouu(str(self.options.primaryr) + 'px')
        self.options.secondaryr = self.svg.unittouu(str(self.options.secondaryr) + 'px')
        self.options.penr = self.svg.unittouu(str(self.options.penr) + 'px')

        if self.options.secondaryr == 0:
            return
        if self.options.quality == 0:
            return

        if self.options.gearplacement.strip(' ').lower().startswith('outside'):
            a = self.options.primaryr + self.options.secondaryr
            flip = -1
        else:
            a = self.options.primaryr - self.options.secondaryr
            flip = 1

        ratio = a / self.options.secondaryr
        if ratio == 0:
            return
        scale = 2 * math.pi / (ratio * self.options.quality)

        rotation = - math.pi * self.options.rotation / 180

        new = inkex.PathElement()
        new.style = inkex.Style(stroke='#000000', fill='none', stroke_width='1.0')

        path_string = ''
        maxPointCount = 1000

        for i in range(maxPointCount):

            theta = i * scale

            view_center = self.svg.namedview.center
            x = a * math.cos(theta + rotation) + \
                self.options.penr * math.cos(ratio * theta + rotation) * flip + \
                view_center[0]
            y = a * math.sin(theta + rotation) - \
                self.options.penr * math.sin(ratio * theta + rotation) + \
                view_center[1]

            dx = (-a * math.sin(theta + rotation) - ratio * self.options.penr * math.sin(ratio * theta + rotation) * flip) * scale / 3
            dy = (a * math.cos(theta + rotation) - ratio * self.options.penr * math.cos(ratio * theta + rotation)) * scale / 3

            if i <= 0:
                path_string += 'M {},{} C {},{} '.format(str(x), str(y), str(x + dx), str(y + dy))
            else:
                path_string += '{},{} {},{}'.format(str(x - dx), str(y - dy), str(x), str(y))

                if math.fmod(i / ratio, self.options.quality) == 0 and i % self.options.quality == 0:
                    path_string += 'Z'
                    break
                else:
                    if i == maxPointCount - 1:
                        pass  # we reached the allowed maximum of points, stop here
                    else:
                        path_string += ' C {},{} '.format(str(x + dx), str(y + dy))

        new.path = path_string
        self.svg.get_current_layer().append(new)

if __name__ == '__main__':
    Spirograph().run()
