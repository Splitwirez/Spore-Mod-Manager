#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2007 John Bintz, jcoswell@cosellproductions.org
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
Greate perfect bound cover
"""

import inkex
from inkex import Guide


def caliper_to_ppi(caliper):
    return 2 / caliper


def bond_weight_to_ppi(bond_weight):
    return caliper_to_ppi(bond_weight * .0002)


def points_to_ppi(points):
    return caliper_to_ppi(points / 1000.0)


class PerfectBoundCover(inkex.EffectExtension):
    def add_arguments(self, pars):
        pars.add_argument("--width", type=float, default=6.0, help="cover width (in)")
        pars.add_argument("--height", type=float, default=9.0, help="cover height (in)")
        pars.add_argument("--pages", type=int, default=64, help="number of pages")
        pars.add_argument("--paperthicknessmeasurement", default=100.0,
                          help="paper thickness measurement")
        pars.add_argument("--paperthickness", type=float, default=0.0, help="paper thickness")
        pars.add_argument("--coverthicknessmeasurement", default=100.0,
                          help="cover thickness measurement")
        pars.add_argument("--coverthickness", type=float, default=0.0, help="cover thickness")
        pars.add_argument("--bleed", type=float, default=0.25, help="cover bleed (in)")
        pars.add_argument("--removeguides", type=inkex.Boolean, default=False, help="remove guide")

    def effect(self):
        switch = {
            "ppi": lambda x: x,
            "caliper": caliper_to_ppi,
            "bond_weight": bond_weight_to_ppi,
            "points": points_to_ppi,
            "width": lambda x: x
        }

        if self.options.paperthickness > 0:
            if self.options.paperthicknessmeasurement == "width":
                paper_spine = self.options.paperthickness
            else:
                paper_spine = self.options.pages / switch[self.options.paperthicknessmeasurement](self.options.paperthickness)
        else:
            paper_spine = 0

        if self.options.coverthickness > 0:
            if self.options.coverthicknessmeasurement == "width":
                cover_spine = self.options.coverthickness
            else:
                cover_spine = 4.0 / switch[self.options.coverthicknessmeasurement](self.options.coverthickness)
        else:
            cover_spine = 0

        spine_width = paper_spine + cover_spine

        document_width = (self.options.bleed + self.options.width * 2) + spine_width
        document_height = self.options.bleed * 2 + self.options.height

        root = self.document.getroot()

        root.set("width", "%sin" % document_width)
        root.set("height", "%sin" % document_height)

        guides = []

        guides.append(["horizontal", self.options.bleed])
        guides.append(["horizontal", document_height - self.options.bleed])
        guides.append(["vertical", self.options.bleed])
        guides.append(["vertical", document_width - self.options.bleed])
        guides.append(["vertical", (document_width / 2) - (spine_width / 2)])
        guides.append(["vertical", (document_width / 2) + (spine_width / 2)])

        namedview = self.svg.namedview
        if namedview is not None:
            if self.options.removeguides:
                for node in namedview.get_guides():
                    node.delete()
            for guide in guides:
                newguide = namedview.add(Guide())
                newguide.set("orientation", guide[0])
                newguide.set("position", "%f" % (guide[1] * 96))

if __name__ == '__main__':
    PerfectBoundCover().run()
