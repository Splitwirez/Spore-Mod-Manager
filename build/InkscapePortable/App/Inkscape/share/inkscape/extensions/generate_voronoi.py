#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2010 Alvin Penner, penner@vaxxine.com
#
# - Voronoi Diagram algorithm and C code by Steven Fortune, 1987, http://ect.bell-labs.com/who/sjf/
# - Python translation to file voronoi.py by Bill Simons, 2005, http://www.oxfish.com/
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

import random

import inkex
from inkex import PathElement, Pattern

import voronoi

def clip_line(x1, y1, x2, y2, w, h):
    if x1 < 0 and x2 < 0:
        return [0, 0, 0, 0]
    if x1 > w and x2 > w:
        return [0, 0, 0, 0]
    if x1 < 0:
        y1 = (y1 * x2 - y2 * x1) / (x2 - x1)
        x1 = 0
    if x2 < 0:
        y2 = (y1 * x2 - y2 * x1) / (x2 - x1)
        x2 = 0
    if x1 > w:
        y1 = y1 + (w - x1) * (y2 - y1) / (x2 - x1)
        x1 = w
    if x2 > w:
        y2 = y1 + (w - x1) * (y2 - y1) / (x2 - x1)
        x2 = w
    if y1 < 0 and y2 < 0:
        return [0, 0, 0, 0]
    if y1 > h and y2 > h:
        return [0, 0, 0, 0]
    if x1 == x2 and y1 == y2:
        return [0, 0, 0, 0]
    if y1 < 0:
        x1 = (x1 * y2 - x2 * y1) / (y2 - y1)
        y1 = 0
    if y2 < 0:
        x2 = (x1 * y2 - x2 * y1) / (y2 - y1)
        y2 = 0
    if y1 > h:
        x1 = x1 + (h - y1) * (x2 - x1) / (y2 - y1)
        y1 = h
    if y2 > h:
        x2 = x1 + (h - y1) * (x2 - x1) / (y2 - y1)
        y2 = h
    return [x1, y1, x2, y2]


class GenerateVoronoi(inkex.EffectExtension):
    def add_arguments(self, pars):
        pars.add_argument("--tab")
        pars.add_argument("--size", type=int, default=10, help="Average size of cell (px)")
        pars.add_argument("--border", type=int, default=0, help="Size of Border (px)")

    def effect(self):
        if not self.options.ids:
            return inkex.errormsg(_("Please select an object"))
        scale = self.svg.unittouu('1px')  # convert to document units
        self.options.size *= scale
        self.options.border *= scale
        obj = self.svg.selection.first()
        bbox = obj.bounding_box()
        mat = obj.composed_transform().matrix
        pattern = self.svg.defs.add(Pattern())
        pattern.set_random_id('Voronoi')
        pattern.set('width', str(bbox.width))
        pattern.set('height', str(bbox.height))
        pattern.set('patternUnits', 'userSpaceOnUse')
        pattern.patternTransform.add_translate(bbox.left - mat[0][2], bbox.top - mat[1][2])

        # generate random pattern of points
        c = voronoi.Context()
        pts = []
        b = float(self.options.border)  # width of border
        for i in range(int(bbox.width * bbox.height / self.options.size / self.options.size)):
            x = random.random() * bbox.width
            y = random.random() * bbox.height
            if b > 0:  # duplicate border area
                pts.append(voronoi.Site(x, y))
                if x < b:
                    pts.append(voronoi.Site(x + bbox.width, y))
                    if y < b:
                        pts.append(voronoi.Site(x + bbox.width, y + bbox.height))
                    if y > bbox.height - b:
                        pts.append(voronoi.Site(x + bbox.width, y - bbox.height))
                if x > bbox.width - b:
                    pts.append(voronoi.Site(x - bbox.width, y))
                    if y < b:
                        pts.append(voronoi.Site(x - bbox.width, y + bbox.height))
                    if y > bbox.height - b:
                        pts.append(voronoi.Site(x - bbox.width, y - bbox.height))
                if y < b:
                    pts.append(voronoi.Site(x, y + bbox.height))
                if y > bbox.height - b:
                    pts.append(voronoi.Site(x, y - bbox.height))
            elif x > -b and y > -b and x < bbox.width + b and y < bbox.height + b:
                pts.append(voronoi.Site(x, y))  # leave border area blank
            # dot = pattern.add(inkex.Rectangle())
            # dot.set('x', str(x-1))
            # dot.set('y', str(y-1))
            # dot.set('width', '2')
            # dot.set('height', '2')
        if len(pts) < 3:
            return inkex.errormsg("Please choose a larger object, or smaller cell size")

        # plot Voronoi diagram
        sl = voronoi.SiteList(pts)
        voronoi.voronoi(sl, c)
        path = ""
        for edge in c.edges:
            if edge[1] >= 0 and edge[2] >= 0:  # two vertices
                [x1, y1, x2, y2] = clip_line(c.vertices[edge[1]][0], c.vertices[edge[1]][1], c.vertices[edge[2]][0], c.vertices[edge[2]][1], bbox.width, bbox.height)
            elif edge[1] >= 0:  # only one vertex
                if c.lines[edge[0]][1] == 0:  # vertical line
                    xtemp = c.lines[edge[0]][2] / c.lines[edge[0]][0]
                    if c.vertices[edge[1]][1] > bbox.height / 2:
                        ytemp = bbox.height
                    else:
                        ytemp = 0
                else:
                    xtemp = bbox.width
                    ytemp = (c.lines[edge[0]][2] - bbox.width * c.lines[edge[0]][0]) / c.lines[edge[0]][1]
                [x1, y1, x2, y2] = clip_line(c.vertices[edge[1]][0], c.vertices[edge[1]][1], xtemp, ytemp, bbox.width, bbox.height)
            elif edge[2] >= 0:  # only one vertex
                if edge[0] >= len(c.lines):
                    xtemp = 0
                    ytemp = 0
                elif c.lines[edge[0]][1] == 0:  # vertical line
                    xtemp = c.lines[edge[0]][2] / c.lines[edge[0]][0]
                    if c.vertices[edge[2]][1] > bbox.height / 2:
                        ytemp = bbox.height
                    else:
                        ytemp = 0
                else:
                    xtemp = 0
                    ytemp = c.lines[edge[0]][2] / c.lines[edge[0]][1]
                [x1, y1, x2, y2] = clip_line(xtemp, ytemp, c.vertices[edge[2]][0], c.vertices[edge[2]][1], bbox.width, bbox.height)
            if x1 or x2 or y1 or y2:
                path += 'M %.3f,%.3f %.3f,%.3f ' % (x1, y1, x2, y2)

        patternstyle = {'stroke': '#000000', 'stroke-width': str(scale)}
        attribs = {'d': path, 'style': str(inkex.Style(patternstyle))}
        pattern.append(PathElement(**attribs))

        # link selected object to pattern
        style = {}
        if 'style' in obj.attrib:
            style = dict(inkex.Style.parse_str(obj.attrib['style']))
        style['fill'] = 'url(#%s)' % pattern.get('id')
        obj.attrib['style'] = str(inkex.Style(style))
        if isinstance(obj, inkex.Group):
            for node in obj:
                style = {}
                if 'style' in node.attrib:
                    style = dict(inkex.Style.parse_str(node.attrib['style']))
                style['fill'] = 'url(#%s)' % pattern.get('id')
                node.attrib['style'] = str(inkex.Style(style))

if __name__ == '__main__':
    GenerateVoronoi().run()
