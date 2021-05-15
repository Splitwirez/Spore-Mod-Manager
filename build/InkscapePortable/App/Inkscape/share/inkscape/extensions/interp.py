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

import copy
from collections import namedtuple
from itertools import combinations

import inkex
from inkex.styles import Style
from inkex.utils import pairwise
from inkex.paths import CubicSuperPath
from inkex.tween import interppoints
from inkex.bezier import csplength, cspbezsplitatlength, cspbezsplit, bezlenapprx

class Interp(inkex.EffectExtension):
    def add_arguments(self, pars):
        pars.add_argument("-e", "--exponent", type=float, default=0.0,\
            help="values other than zero give non linear interpolation")
        pars.add_argument("-s", "--steps", type=int, default=5,\
            help="number of interpolation steps")
        pars.add_argument("-m", "--method", type=int, default=2,\
            help="method of interpolation")
        pars.add_argument("-d", "--dup", type=inkex.Boolean, default=True,\
            help="duplicate endpaths")
        pars.add_argument("--style", type=inkex.Boolean, default=True,\
            help="try interpolation of some style properties")
        pars.add_argument("--zsort", type=inkex.Boolean, default=False,\
            help="use z-order instead of selection order")

    def effect(self):
        exponent = self.options.exponent
        if exponent >= 0:
            exponent += 1.0
        else:
            exponent = 1.0 / (1.0 - exponent)
        steps = [1.0 / (self.options.steps + 1.0)]
        for i in range(self.options.steps - 1):
            steps.append(steps[0] + steps[-1])
        steps = [step ** exponent for step in steps]

        if self.options.zsort:
            # work around selection order swapping with Live Preview
            objects = self.svg.get_z_selected()
        else:
            # use selection order (default)
            objects = self.svg.selected

        objects = [node for node in objects.values() if isinstance(node, inkex.PathElement)]

        # prevents modification of original objects
        objects = copy.deepcopy(objects)

        for node in objects:
            node.apply_transform()

        objectpairs = pairwise(objects, start=False) 

        for (elem1, elem2) in objectpairs:
            start = elem1.path.to_superpath()
            end = elem2.path.to_superpath()
            sst = copy.deepcopy(elem1.style)
            est = copy.deepcopy(elem2.style)
            basestyle = copy.deepcopy(sst)

            if 'stroke-width' in basestyle:
                basestyle['stroke-width'] = sst.interpolate_prop(est, 0, 'stroke-width')

            # prepare for experimental style tweening
            if self.options.style:
                styledefaults = Style(
                    {'opacity': 1.0,
                     'stroke-opacity': 1.0,
                     'fill-opacity': 1.0,
                     'stroke-width': 1.0,
                     'stroke': None,
                     'fill': None})
                for key in styledefaults:
                    sst.setdefault(key, styledefaults[key])
                    est.setdefault(key, styledefaults[key])

                isnotplain = lambda x: not (x == 'none' or x[:1] == '#')
                isgradient = lambda x: x.startswith('url(#')

                if isgradient(sst['stroke']) and isgradient(est['stroke']):
                    strokestyle = 'gradient' 
                elif isnotplain(sst['stroke']) or isnotplain(est['stroke']) or (sst['stroke'] == 'none' and est['stroke'] == 'none'):
                    strokestyle = 'notplain' 
                else:
                    strokestyle = 'color' 

                if isgradient(sst['fill']) and isgradient(est['fill']):
                    fillstyle = 'gradient' 
                elif isnotplain(sst['fill']) or isnotplain(est['fill']) or (sst['fill'] == 'none' and est['fill'] == 'none'):
                    fillstyle = 'notplain' 
                else:
                    fillstyle = 'color'

                if strokestyle is 'color':
                    if sst['stroke'] == 'none':
                        sst['stroke-width'] = '0.0'
                        sst['stroke-opacity'] = '0.0'
                        sst['stroke'] = est['stroke']
                    elif est['stroke'] == 'none':
                        est['stroke-width'] = '0.0'
                        est['stroke-opacity'] = '0.0'
                        est['stroke'] = sst['stroke']

                if fillstyle is 'color':
                    if sst['fill'] == 'none':
                        sst['fill-opacity'] = '0.0'
                        sst['fill'] = est['fill']
                    elif est['fill'] == 'none':
                        est['fill-opacity'] = '0.0'
                        est['fill'] = sst['fill']

            if self.options.method == 2:
                # subdivide both paths into segments of relatively equal lengths
                slengths, stotal = csplength(start)
                elengths, etotal = csplength(end)
                lengths = {}
                t = 0
                for sp in slengths:
                    for l in sp:
                        t += l / stotal
                        lengths.setdefault(t, 0)
                        lengths[t] += 1
                t = 0
                for sp in elengths:
                    for l in sp:
                        t += l / etotal
                        lengths.setdefault(t, 0)
                        lengths[t] += -1
                sadd = [k for (k, v) in lengths.items() if v < 0]
                sadd.sort()
                eadd = [k for (k, v) in lengths.items() if v > 0]
                eadd.sort()

                t = 0
                s = [[]]
                for sp in slengths:
                    if not start[0]:
                        s.append(start.pop(0))
                    s[-1].append(start[0].pop(0))
                    for l in sp:
                        pt = t
                        t += l / stotal
                        if sadd and t > sadd[0]:
                            while sadd and sadd[0] < t:
                                nt = (sadd[0] - pt) / (t - pt)
                                bezes = cspbezsplitatlength(s[-1][-1][:], start[0][0][:], nt)
                                s[-1][-1:] = bezes[:2]
                                start[0][0] = bezes[2]
                                pt = sadd.pop(0)
                        s[-1].append(start[0].pop(0))
                t = 0
                e = [[]]
                for sp in elengths:
                    if not end[0]:
                        e.append(end.pop(0))
                    e[-1].append(end[0].pop(0))
                    for l in sp:
                        pt = t
                        t += l / etotal
                        if eadd and t > eadd[0]:
                            while eadd and eadd[0] < t:
                                nt = (eadd[0] - pt) / (t - pt)
                                bezes = cspbezsplitatlength(e[-1][-1][:], end[0][0][:], nt)
                                e[-1][-1:] = bezes[:2]
                                end[0][0] = bezes[2]
                                pt = eadd.pop(0)
                        e[-1].append(end[0].pop(0))
                start = s[:]
                end = e[:]
            else:
                # which path has fewer segments?
                lengthdiff = len(start) - len(end)
                # swap shortest first
                if lengthdiff > 0:
                    start, end = end, start
                # subdivide the shorter path
                for x in range(abs(lengthdiff)):
                    maxlen = 0
                    subpath = 0
                    segment = 0
                    for y in range(len(start)):
                        for z in range(1, len(start[y])):
                            leng = bezlenapprx(start[y][z - 1], start[y][z])
                            if leng > maxlen:
                                maxlen = leng
                                subpath = y
                                segment = z
                    sp1, sp2 = start[subpath][segment - 1:segment + 1]
                    start[subpath][segment - 1:segment + 1] = cspbezsplit(sp1, sp2)
                # if swapped, swap them back
                if lengthdiff > 0:
                    start, end = end, start

            # break paths so that corresponding subpaths have an equal number of segments
            s = [[]]
            e = [[]]
            while start and end:
                if start[0] and end[0]:
                    s[-1].append(start[0].pop(0))
                    e[-1].append(end[0].pop(0))
                elif end[0]:
                    s.append(start.pop(0))
                    e[-1].append(end[0][0])
                    e.append([end[0].pop(0)])
                elif start[0]:
                    e.append(end.pop(0))
                    s[-1].append(start[0][0])
                    s.append([start[0].pop(0)])
                else:
                    s.append(start.pop(0))
                    e.append(end.pop(0))

            if self.options.dup:
                steps = [0] + steps + [1]
            # create an interpolated path for each interval
            group = self.svg.get_current_layer().add(inkex.Group())
            for time in steps:
                interp = []
                # process subpaths
                for ssp, esp in zip(s, e):
                    if not (ssp or esp):
                        break
                    interp.append([])
                    # process superpoints
                    for sp, ep in zip(ssp, esp):
                        if not (sp or ep):
                            break
                        interp[-1].append([])
                        # process points
                        for p1, p2 in zip(sp, ep):
                            if not (sp or ep):
                                break
                            interp[-1][-1].append(interppoints(p1, p2, time))

                # remove final subpath if empty.
                if not interp[-1]:
                    del interp[-1]

                # basic style interpolation
                if self.options.style:
                    basestyle.update(sst.interpolate(est, time))
                    for prop in ['stroke', 'fill']:
                        if isgradient(sst[prop]) and isgradient(est[prop]):
                            gradid1 = sst[prop][4:-1]
                            gradid2 = est[prop][4:-1]
                            grad1 = self.svg.getElementById(gradid1)
                            grad2 = self.svg.getElementById(gradid2)
                            newgrad = grad1.interpolate(grad2, time)
                            stops, orientation = newgrad.stops_and_orientation()
                            self.svg.defs.add(orientation)
                            basestyle[prop] = 'url(#{})'.format(orientation.get_id())
                            if len(stops):
                                self.svg.defs.add(stops, orientation)
                                orientation.set('xlink:href', '#{}'.format(stops.get_id()))

                new = group.add(inkex.PathElement())
                new.style = basestyle
                new.path = CubicSuperPath(interp)


if __name__ == '__main__':
    Interp().run()
