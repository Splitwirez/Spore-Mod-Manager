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
"""Straintens path segments (but doesn't turn them into lines)"""
import inkex

from inkex.bezier import percent_point

class SegmentStraightener(inkex.EffectExtension):
    """Make segments straiter"""
    def add_arguments(self, pars):
        pars.add_argument("-p", "--percent", type=float, default=10.0,\
            help="move curve handles PERCENT percent closer to a straight line")
        pars.add_argument("-b", "--behavior", type=int, default=1,\
            help="straightening behavior for cubic segments")

    def effect(self):
        for node in self.svg.selection.get(inkex.PathElement).values():
            path = node.path.to_arrays()
            last = []
            sub_start = []
            for cmd, params in path:
                if cmd == 'C':
                    if self.options.behavior <= 1:
                        #shorten handles towards end points
                        params[:2] = percent_point(params[:2], last[:], self.options.percent)
                        params[2:4] = percent_point(params[2:4], params[-2:], self.options.percent)
                    else:
                        #shorten handles towards thirds of the segment
                        dest1 = percent_point(last[:], params[-2:], 33.3)
                        dest2 = percent_point(params[-2:], last[:], 33.3)
                        params[:2] = percent_point(params[:2], dest1[:], self.options.percent)
                        params[2:4] = percent_point(params[2:4], dest2[:], self.options.percent)
                elif cmd == 'Q':
                    dest = percent_point(last[:], params[-2:], 50)
                    params[:2] = percent_point(params[:2], dest, self.options.percent)
                if cmd == 'M':
                    sub_start = params[-2:]
                if cmd == 'Z':
                    last = sub_start[:]
                else:
                    last = params[-2:]
            node.path = path

if __name__ == '__main__':
    SegmentStraightener().run()
