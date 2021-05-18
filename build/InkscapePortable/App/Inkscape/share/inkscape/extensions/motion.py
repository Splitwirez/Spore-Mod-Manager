#!/usr/bin/env python
# coding=utf-8
#
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

import math

import inkex
from inkex.paths import Move, Line, Curve, ZoneClose, Arc, Path, Vert, Horz, TepidQuadratic, Quadratic, Smooth
from inkex.transforms import Vector2d
from inkex.bezier import beziertatslope, beziersplitatt


class Motion(inkex.EffectExtension):
    """Generate a motion path"""

    def add_arguments(self, pars):
        pars.add_argument("-a", "--angle", type=float, default=45.0, \
                          help="direction of the motion vector")
        pars.add_argument("-m", "--magnitude", type=float, default=100.0, \
                          help="magnitude of the motion vector")

    @staticmethod
    def makeface(last, segment, facegroup, delx, dely):
        """translate path segment along vector"""
        elem = facegroup.add(inkex.PathElement())

        npt = segment.translate([delx, dely])

        # reverse direction of path segment
        if isinstance(segment, Curve):
            rev = Curve(npt.x3, npt.y3, npt.x2, npt.y2,
                        last[0] + delx, last[1] + dely
                        )
        elif isinstance(segment, Line):
            rev = Line(last[0] + delx, last[1] + dely)
        else:
            raise RuntimeError("Unexpected segment type {}".format(type(segment)))

        elem.path = inkex.Path([
            Move(last[0], last[1]),
            segment,
            npt.to_line(Vector2d()),
            rev,
            ZoneClose(),
        ])

    def effect(self):
        delx = math.cos(math.radians(self.options.angle)) * self.options.magnitude
        dely = math.sin(math.radians(self.options.angle)) * self.options.magnitude
        for node in self.svg.selection.filter(inkex.PathElement).values():
            group = node.getparent().add(inkex.Group())
            facegroup = group.add(inkex.Group())
            group.append(node)

            if node.transform:
                group.transform = node.transform
                node.transform = None

            facegroup.style = node.style

            for cmd_proxy in node.path.to_absolute().proxy_iterator():
                self.process_segment(cmd_proxy, facegroup, delx, dely)

    @staticmethod
    def process_segment(cmd_proxy, facegroup, delx, dely):
        """Process each segments"""

        segments = []
        if isinstance(cmd_proxy.command, (Curve, Smooth, TepidQuadratic, Quadratic, Arc)):
            prev = cmd_proxy.previous_end_point
            for curve in cmd_proxy.to_curves():
                bez = [prev] + curve.to_bez()
                prev = curve.end_point(cmd_proxy.first_point, prev)
                tees = [t for t in beziertatslope(bez, (dely, delx)) if 0 < t < 1]
                tees.sort()
                if len(tees) == 1:
                    one, two = beziersplitatt(bez, tees[0])
                    segments.append(Curve(*(one[1] + one[2] + one[3])))
                    segments.append(Curve(*(two[1] + two[2] + two[3])))
                elif len(tees) == 2:
                    one, two = beziersplitatt(bez, tees[0])
                    two, three = beziersplitatt(two, tees[1])
                    segments.append(Curve(*(one[1] + one[2] + one[3])))
                    segments.append(Curve(*(two[1] + two[2] + two[3])))
                    segments.append(Curve(*(three[1] + three[2] + three[3])))
                else:
                    segments.append(curve)
        elif isinstance(cmd_proxy.command, (Line, Curve)):
            segments.append(cmd_proxy.command)
        elif isinstance(cmd_proxy.command, ZoneClose):
            segments.append(Line(cmd_proxy.first_point.x, cmd_proxy.first_point.y))
        elif isinstance(cmd_proxy.command, (Vert, Horz)):
            segments.append(cmd_proxy.command.to_line(cmd_proxy.end_point))

        for seg in Path([Move(*cmd_proxy.previous_end_point)] + segments).proxy_iterator():
            if isinstance(seg.command, Move): continue
            Motion.makeface(seg.previous_end_point, seg.command, facegroup, delx, dely)


if __name__ == '__main__':
    Motion().run()
