#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2008, 2009 Alvin Penner, penner@vaxxine.com
# Copyright (C) 2009 Christian Mayer, inkscape@christianmayer.de
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
Input a DXF file >= (AutoCAD Release 13 == AC1012)
"""
# thanks to Aaron Spike for inkex without which this would not have been possible

from __future__ import absolute_import, unicode_literals

import os
import re
import sys
import math

from lxml import etree

import inkex

if sys.version_info[0] < 3:
    from urllib import quote
else:
    from urllib.parse import quote

    unichr = chr


def re_hex2unichar(m):
    return unichr(int(m.group(1), 16))


def formatStyle(style):
    return str(inkex.Style(style))


def export_MTEXT():
    # mandatory group codes : (1 or 3, 10, 20) (text, x, y)
    if (vals[groups['1']] or vals[groups['3']]) and vals[groups['10']] and vals[groups['20']]:
        x = vals[groups['10']][0]
        y = vals[groups['20']][0]
        # optional group codes : (21, 40, 50) (direction, text height mm, text angle)
        size = 12  # default fontsize in px
        if vals[groups['40']] and vals[groups['40']][0]:
            size = scale * vals[groups['40']][0]
        attribs = {'x': '%f' % x, 'y': '%f' % y, 'style': 'font-size: %.1fpx; fill: %s; font-family: %s' % (size, color, options.font)}
        angle = 0  # default angle in degrees
        if vals[groups['50']]:
            angle = vals[groups['50']][0]
            attribs.update({'transform': 'rotate (%f %f %f)' % (-angle, x, y)})
        elif vals[groups['21']]:
            if vals[groups['21']][0] == 1.0:
                attribs.update({'transform': 'rotate (%f %f %f)' % (-90, x, y)})
            elif vals[groups['21']][0] == -1.0:
                attribs.update({'transform': 'rotate (%f %f %f)' % (90, x, y)})
        node = layer.add(inkex.Text(**attribs))
        node.set('sodipodi:linespacing', '125%')
        text = ''
        if vals[groups['3']]:
            for i in range(0, len(vals[groups['3']])):
                text += vals[groups['3']][i]
        if vals[groups['1']]:
            text += vals[groups['1']][0]
        found = text.find(r'\P')  # new line
        while found > -1:
            tspan = node.add(inkex.Tspan())
            tspan.set('sodipodi:role', 'line')
            tspan.text = text[:found]
            text = text[(found + 2):]
            found = text.find(r'\P')
        tspan = node.add(inkex.Tspan())
        tspan.set('sodipodi:role', 'line')
        tspan.text = text


def export_POINT(w):
    # mandatory group codes : (10, 20) (x, y)
    if vals[groups['10']] and vals[groups['20']]:
        if options.gcodetoolspoints:
            generate_gcodetools_point(vals[groups['10']][0], vals[groups['20']][0])
        else:
            generate_ellipse(vals[groups['10']][0], vals[groups['20']][0], w / 2, 0.0, 1.0, 0.0, 0.0)


def export_LINE():
    # mandatory group codes : (10, 11, 20, 21) (x1, x2, y1, y2)
    if vals[groups['10']] and vals[groups['11']] and vals[groups['20']] and vals[groups['21']]:
        path = 'M %f,%f %f,%f' % (vals[groups['10']][0], vals[groups['20']][0], scale * (extrude * vals[groups['11']][0] - xmin), height - scale * (vals[groups['21']][0] - ymin))
        attribs = {'d': path, 'style': style}
        etree.SubElement(layer, 'path', attribs)


def export_SPLINE():
    # see : http://www.mactech.com/articles/develop/issue_25/schneider.html
    # mandatory group codes : (10, 20, 40, 70) (x[], y[], knots[], flags)
    if vals[groups['70']] and len(vals[groups['10']]) == len(vals[groups['20']]) and vals[groups['10']] and vals[groups['20']] and vals[groups['40']]:
        knots = len(vals[groups['40']])
        ctrls = len(vals[groups['10']])
        if ctrls > 3 and knots == ctrls + 4:  # cubic
            if ctrls > 4:
                for i in range(knots - 5, 3, -1):
                    if vals[groups['40']][i] != vals[groups['40']][i - 1] and vals[groups['40']][i] != vals[groups['40']][i + 1]:
                        a0 = (vals[groups['40']][i] - vals[groups['40']][i - 2]) / (vals[groups['40']][i + 1] - vals[groups['40']][i - 2])
                        a1 = (vals[groups['40']][i] - vals[groups['40']][i - 1]) / (vals[groups['40']][i + 2] - vals[groups['40']][i - 1])
                        vals[groups['10']].insert(i - 1, (1.0 - a1) * vals[groups['10']][i - 2] + a1 * vals[groups['10']][i - 1])
                        vals[groups['20']].insert(i - 1, (1.0 - a1) * vals[groups['20']][i - 2] + a1 * vals[groups['20']][i - 1])
                        vals[groups['10']][i - 2] = (1.0 - a0) * vals[groups['10']][i - 3] + a0 * vals[groups['10']][i - 2]
                        vals[groups['20']][i - 2] = (1.0 - a0) * vals[groups['20']][i - 3] + a0 * vals[groups['20']][i - 2]
                        vals[groups['40']].insert(i, vals[groups['40']][i])
                knots = len(vals[groups['40']])
                for i in range(knots - 6, 3, -2):
                    if vals[groups['40']][i] != vals[groups['40']][i + 2] and vals[groups['40']][i - 1] != vals[groups['40']][i + 1] and vals[groups['40']][i - 2] != vals[groups['40']][i]:
                        a1 = (vals[groups['40']][i] - vals[groups['40']][i - 1]) / (vals[groups['40']][i + 2] - vals[groups['40']][i - 1])
                        vals[groups['10']].insert(i - 1, (1.0 - a1) * vals[groups['10']][i - 2] + a1 * vals[groups['10']][i - 1])
                        vals[groups['20']].insert(i - 1, (1.0 - a1) * vals[groups['20']][i - 2] + a1 * vals[groups['20']][i - 1])
            ctrls = len(vals[groups['10']])
            path = 'M %f,%f' % (vals[groups['10']][0], vals[groups['20']][0])
            for i in range(0, (ctrls - 1) // 3):
                path += ' C %f,%f %f,%f %f,%f' % (vals[groups['10']][3 * i + 1], vals[groups['20']][3 * i + 1], vals[groups['10']][3 * i + 2], vals[groups['20']][3 * i + 2], vals[groups['10']][3 * i + 3], vals[groups['20']][3 * i + 3])
            if vals[groups['70']][0] & 1:  # closed path
                path += ' z'
            attribs = {'d': path, 'style': style}
            etree.SubElement(layer, 'path', attribs)
        if ctrls == 3 and knots == 6:  # quadratic
            path = 'M %f,%f Q %f,%f %f,%f' % (vals[groups['10']][0], vals[groups['20']][0], vals[groups['10']][1], vals[groups['20']][1], vals[groups['10']][2], vals[groups['20']][2])
            attribs = {'d': path, 'style': style}
            etree.SubElement(layer, 'path', attribs)
        if ctrls == 5 and knots == 8:  # spliced quadratic
            path = 'M %f,%f Q %f,%f %f,%f Q %f,%f %f,%f' % (vals[groups['10']][0], vals[groups['20']][0], vals[groups['10']][1], vals[groups['20']][1], vals[groups['10']][2], vals[groups['20']][2], vals[groups['10']][3], vals[groups['20']][3], vals[groups['10']][4], vals[groups['20']][4])
            attribs = {'d': path, 'style': style}
            etree.SubElement(layer, 'path', attribs)


def export_CIRCLE():
    # mandatory group codes : (10, 20, 40) (x, y, radius)
    if vals[groups['10']] and vals[groups['20']] and vals[groups['40']]:
        generate_ellipse(vals[groups['10']][0], vals[groups['20']][0], scale * vals[groups['40']][0], 0.0, 1.0, 0.0, 0.0)


def export_ARC():
    # mandatory group codes : (10, 20, 40, 50, 51) (x, y, radius, angle1, angle2)
    if vals[groups['10']] and vals[groups['20']] and vals[groups['40']] and vals[groups['50']] and vals[groups['51']]:
        generate_ellipse(vals[groups['10']][0], vals[groups['20']][0], scale * vals[groups['40']][0], 0.0, 1.0, vals[groups['50']][0] * math.pi / 180.0, vals[groups['51']][0] * math.pi / 180.0)


def export_ELLIPSE():
    # mandatory group codes : (10, 11, 20, 21, 40, 41, 42) (xc, xm, yc, ym, width ratio, angle1, angle2)
    if vals[groups['10']] and vals[groups['11']] and vals[groups['20']] and vals[groups['21']] and vals[groups['40']] and vals[groups['41']] and vals[groups['42']]:
        generate_ellipse(vals[groups['10']][0], vals[groups['20']][0], scale * vals[groups['11']][0], scale * vals[groups['21']][0], vals[groups['40']][0], vals[groups['41']][0], vals[groups['42']][0])


def export_LEADER():
    # mandatory group codes : (10, 20) (x, y)
    if vals[groups['10']] and vals[groups['20']]:
        if len(vals[groups['10']]) > 1 and len(vals[groups['20']]) == len(vals[groups['10']]):
            path = 'M %f,%f' % (vals[groups['10']][0], vals[groups['20']][0])
            for i in range(1, len(vals[groups['10']])):
                path += ' %f,%f' % (vals[groups['10']][i], vals[groups['20']][i])
            attribs = {'d': path, 'style': style}
            etree.SubElement(layer, 'path', attribs)


def export_LWPOLYLINE():
    # mandatory group codes : (10, 20, 70) (x, y, flags)
    if vals[groups['10']] and vals[groups['20']] and vals[groups['70']]:
        if len(vals[groups['10']]) > 1 and len(vals[groups['20']]) == len(vals[groups['10']]):
            # optional group codes : (42) (bulge)
            iseqs = 0
            ibulge = 0
            if vals[groups['70']][0] & 1:  # closed path
                seqs.append('20')
                vals[groups['10']].append(vals[groups['10']][0])
                vals[groups['20']].append(vals[groups['20']][0])
            while seqs[iseqs] != '20':
                iseqs += 1
            path = 'M %f,%f' % (vals[groups['10']][0], vals[groups['20']][0])
            xold = vals[groups['10']][0]
            yold = vals[groups['20']][0]
            for i in range(1, len(vals[groups['10']])):
                bulge = 0
                iseqs += 1
                while seqs[iseqs] != '20':
                    if seqs[iseqs] == '42':
                        bulge = vals[groups['42']][ibulge]
                        ibulge += 1
                    iseqs += 1
                if bulge:
                    sweep = 0  # sweep CCW
                    if bulge < 0:
                        sweep = 1  # sweep CW
                        bulge = -bulge
                    large = 0  # large-arc-flag
                    if bulge > 1:
                        large = 1
                    r = math.sqrt((vals[groups['10']][i] - xold) ** 2 + (vals[groups['20']][i] - yold) ** 2)
                    r = 0.25 * r * (bulge + 1.0 / bulge)
                    path += ' A %f,%f 0.0 %d %d %f,%f' % (r, r, large, sweep, vals[groups['10']][i], vals[groups['20']][i])
                else:
                    path += ' L %f,%f' % (vals[groups['10']][i], vals[groups['20']][i])
                xold = vals[groups['10']][i]
                yold = vals[groups['20']][i]
            if vals[groups['70']][0] & 1:  # closed path
                path += ' z'
            attribs = {'d': path, 'style': style}
            etree.SubElement(layer, 'path', attribs)


def export_HATCH():
    # mandatory group codes : (10, 20, 70, 72, 92, 93) (x, y, fill, Edge Type, Path Type, Number of edges)
    if vals[groups['10']] and vals[groups['20']] and vals[groups['70']] and vals[groups['72']] and vals[groups['92']] and vals[groups['93']]:
        if len(vals[groups['10']]) > 1 and len(vals[groups['20']]) == len(vals[groups['10']]):
            # optional group codes : (11, 21, 40, 50, 51, 73) (x, y, r, angle1, angle2, CCW)
            i10 = 1  # count start points
            i11 = 0  # count line end points
            i40 = 0  # count circles
            i72 = 0  # count edge type flags
            path = ''
            for i in range(0, len(vals[groups['93']])):
                xc = vals[groups['10']][i10]
                yc = vals[groups['20']][i10]
                if vals[groups['72']][i72] == 2:  # arc
                    rm = scale * vals[groups['40']][i40]
                    a1 = vals[groups['50']][i40]
                    path += 'M %f,%f ' % (xc + rm * math.cos(a1 * math.pi / 180.0), yc + rm * math.sin(a1 * math.pi / 180.0))
                else:
                    a1 = 0
                    path += 'M %f,%f ' % (xc, yc)
                for j in range(0, vals[groups['93']][i]):
                    if vals[groups['92']][i] & 2:  # polyline
                        if j > 0:
                            path += 'L %f,%f ' % (vals[groups['10']][i10], vals[groups['20']][i10])
                        if j == vals[groups['93']][i] - 1:
                            i72 += 1
                    elif vals[groups['72']][i72] == 2:  # arc
                        xc = vals[groups['10']][i10]
                        yc = vals[groups['20']][i10]
                        rm = scale * vals[groups['40']][i40]
                        a2 = vals[groups['51']][i40]
                        diff = (a2 - a1 + 360) % 360
                        sweep = 1 - vals[groups['73']][i40]  # sweep CCW
                        large = 0  # large-arc-flag
                        if diff:
                            path += 'A %f,%f 0.0 %d %d %f,%f ' % (rm, rm, large, sweep, xc + rm * math.cos(a2 * math.pi / 180.0), yc + rm * math.sin(a2 * math.pi / 180.0))
                        else:
                            path += 'A %f,%f 0.0 %d %d %f,%f ' % (rm, rm, large, sweep, xc + rm * math.cos((a1 + 180.0) * math.pi / 180.0), yc + rm * math.sin((a1 + 180.0) * math.pi / 180.0))
                            path += 'A %f,%f 0.0 %d %d %f,%f ' % (rm, rm, large, sweep, xc + rm * math.cos(a1 * math.pi / 180.0), yc + rm * math.sin(a1 * math.pi / 180.0))
                        i40 += 1
                        i72 += 1
                    elif vals[groups['72']][i72] == 1:  # line
                        path += 'L %f,%f ' % (scale * (extrude * vals[groups['11']][i11] - xmin), height - scale * (vals[groups['21']][i11] - ymin))
                        i11 += 1
                        i72 += 1
                    i10 += 1
                path += "z "
            if vals[groups['70']][0]:
                style = formatStyle({'fill': '%s' % color})
            else:
                style = formatStyle({'fill': 'url(#Hatch)', 'fill-opacity': '1.0'})
            attribs = {'d': path, 'style': style}
            etree.SubElement(layer, 'path', attribs)


def export_DIMENSION():
    # mandatory group codes : (10, 11, 13, 14, 20, 21, 23, 24) (x1..4, y1..4)
    if vals[groups['10']] and vals[groups['11']] and vals[groups['13']] and vals[groups['14']] and vals[groups['20']] and vals[groups['21']] and vals[groups['23']] and vals[groups['24']]:
        dx = abs(vals[groups['10']][0] - vals[groups['13']][0])
        dy = abs(vals[groups['20']][0] - vals[groups['23']][0])
        if (vals[groups['10']][0] == vals[groups['14']][0]) and dx > 0.00001:
            d = dx / scale
            dy = 0
            path = 'M %f,%f %f,%f' % (vals[groups['10']][0], vals[groups['20']][0], vals[groups['13']][0], vals[groups['20']][0])
        elif (vals[groups['20']][0] == vals[groups['24']][0]) and dy > 0.00001:
            d = dy / scale
            dx = 0
            path = 'M %f,%f %f,%f' % (vals[groups['10']][0], vals[groups['20']][0], vals[groups['10']][0], vals[groups['23']][0])
        else:
            return
        attribs = {'d': path, 'style': style + '; marker-start: url(#DistanceX); marker-end: url(#DistanceX); stroke-width: 0.25px'}
        etree.SubElement(layer, 'path', attribs)
        x = scale * (extrude * vals[groups['11']][0] - xmin)
        y = height - scale * (vals[groups['21']][0] - ymin)
        size = 12  # default fontsize in px
        if vals[groups['3']]:
            if vals[groups['3']][0] in DIMTXT:
                size = scale * DIMTXT[vals[groups['3']][0]]
                if size < 2:
                    size = 2
        attribs = {'x': '%f' % x, 'y': '%f' % y, 'style': 'font-size: %.1fpx; fill: %s; font-family: %s; text-anchor: middle; text-align: center' % (size, color, options.font)}
        if dx == 0:
            attribs.update({'transform': 'rotate (%f %f %f)' % (-90, x, y)})
        node = etree.SubElement(layer, 'text', attribs)
        tspan = node.add(inkex.Tspan())
        tspan.set('sodipodi:role', 'line')
        tspan.text = str(float('%.2f' % d))


def export_INSERT():
    # mandatory group codes : (2, 10, 20) (block name, x, y)
    if vals[groups['2']] and vals[groups['10']] and vals[groups['20']]:
        x = vals[groups['10']][0] + scale * xmin
        y = vals[groups['20']][0] - scale * ymin - height
        elem = layer.add(inkex.Use())
        elem.set('xlink:href', '#' + quote(vals[groups['2']][0].replace(" ", "_").encode("utf-8")))
        elem.transform = 'translate(%f, %f)' % (x, y)
        if vals[groups['41']] and vals[groups['42']]:
            elem.transform.add_scale(vals[groups['41']][0], vals[groups['42']][0])


def export_BLOCK():
    # mandatory group codes : (2) (block name)
    if vals[groups['2']]:
        global block
        block = etree.SubElement(defs, 'symbol', {'id': vals[groups['2']][0].replace(" ", "_")})


def export_ENDBLK():
    global block
    block = defs  # initiallize with dummy


def export_ATTDEF():
    # mandatory group codes : (1, 2) (default, tag)
    if vals[groups['1']] and vals[groups['2']]:
        vals[groups['1']][0] = vals[groups['2']][0]
        export_MTEXT()


def generate_ellipse(xc, yc, xm, ym, w, a1, a2):
    rm = math.sqrt(xm * xm + ym * ym)
    a = math.atan2(ym, xm)
    diff = (a2 - a1 + 2 * math.pi) % (2 * math.pi)
    if abs(diff) > 0.0000001 and abs(diff - 2 * math.pi) > 0.0000001:  # open arc
        large = 0  # large-arc-flag
        if diff > math.pi:
            large = 1
        xt = rm * math.cos(a1)
        yt = w * rm * math.sin(a1)
        x1 = xt * math.cos(a) - yt * math.sin(a)
        y1 = xt * math.sin(a) + yt * math.cos(a)
        xt = rm * math.cos(a2)
        yt = w * rm * math.sin(a2)
        x2 = xt * math.cos(a) - yt * math.sin(a)
        y2 = xt * math.sin(a) + yt * math.cos(a)
        path = 'M %f,%f A %f,%f %f %d 0 %f,%f' % (xc + x1, yc - y1, rm, w * rm, -180.0 * a / math.pi, large, xc + x2, yc - y2)
    else:  # closed arc
        path = 'M %f,%f A %f,%f %f 1 0 %f,%f %f,%f %f 1 0 %f,%f z' % (xc + xm, yc - ym, rm, w * rm, -180.0 * a / math.pi, xc - xm, yc + ym, rm, w * rm, -180.0 * a / math.pi, xc + xm, yc - ym)
    attribs = {'d': path, 'style': style}
    etree.SubElement(layer, 'path', attribs)


def generate_gcodetools_point(xc, yc):
    elem = layer.add(inkex.PathElement())
    elem.style = 'stroke:none;fill:#ff0000'
    elem.set('inkscape:dxfpoint', '1')
    elem.path = 'm %s,%s 2.9375,-6.34375 0.8125,1.90625 6.84375,-6.84375 0,0 0.6875,0.6875 -6.84375,6.84375 1.90625,0.8125 z' % (xc, yc)


#   define DXF Entities and specify which Group Codes to monitor

entities = {'MTEXT': export_MTEXT, 'TEXT': export_MTEXT, 'POINT': export_POINT, 'LINE': export_LINE, 'SPLINE': export_SPLINE, 'CIRCLE': export_CIRCLE, 'ARC': export_ARC, 'ELLIPSE': export_ELLIPSE, 'LEADER': export_LEADER, 'LWPOLYLINE': export_LWPOLYLINE, 'HATCH': export_HATCH, 'DIMENSION': export_DIMENSION, 'INSERT': export_INSERT, 'BLOCK': export_BLOCK, 'ENDBLK': export_ENDBLK, 'ATTDEF': export_ATTDEF, 'VIEWPORT': False, 'ENDSEC': False}
groups = {'1': 0, '2': 1, '3': 2, '6': 3, '8': 4, '10': 5, '11': 6, '13': 7, '14': 8, '20': 9, '21': 10, '23': 11, '24': 12, '40': 13, '41': 14, '42': 15, '50': 16, '51': 17, '62': 18, '70': 19, '72': 20, '73': 21, '92': 22, '93': 23, '230': 24, '370': 25}
colors = {1: '#FF0000', 2: '#FFFF00', 3: '#00FF00', 4: '#00FFFF', 5: '#0000FF', 6: '#FF00FF', 7: '#000000', 8: '#808080',
          9: '#C0C0C0', 10: '#FF0000', 11: '#FF7F7F', 12: '#CC0000', 13: '#CC6666', 14: '#990000', 15: '#994C4C', 16: '#7F0000', 17: '#7F3F3F',
          18: '#4C0000', 19: '#4C2626', 20: '#FF3F00', 21: '#FF9F7F', 22: '#CC3300', 23: '#CC7F66', 24: '#992600', 25: '#995F4C', 26: '#7F1F00',
          27: '#7F4F3F', 28: '#4C1300', 29: '#4C2F26', 30: '#FF7F00', 31: '#FFBF7F', 32: '#CC6600', 33: '#CC9966', 34: '#994C00', 35: '#99724C',
          36: '#7F3F00', 37: '#7F5F3F', 38: '#4C2600', 39: '#4C3926', 40: '#FFBF00', 41: '#FFDF7F', 42: '#CC9900', 43: '#CCB266', 44: '#997200',
          45: '#99854C', 46: '#7F5F00', 47: '#7F6F3F', 48: '#4C3900', 49: '#4C4226', 50: '#FFFF00', 51: '#FFFF7F', 52: '#CCCC00', 53: '#CCCC66',
          54: '#989800', 55: '#98984C', 56: '#7F7F00', 57: '#7F7F3F', 58: '#4C4C00', 59: '#4C4C26', 60: '#BFFF00', 61: '#DFFF7F', 62: '#99CC00',
          63: '#B2CC66', 64: '#729800', 65: '#85984C', 66: '#5F7F00', 67: '#6F7F3F', 68: '#394C00', 69: '#424C26', 70: '#7FFF00', 71: '#BFFF7F',
          72: '#66CC00', 73: '#99CC66', 74: '#4C9800', 75: '#72984C', 76: '#3F7F00', 77: '#5F7F3F', 78: '#264C00', 79: '#394C26', 80: '#3FFF00',
          81: '#9FFF7F', 82: '#33CC00', 83: '#7FCC66', 84: '#269800', 85: '#5F984C', 86: '#1F7F00', 87: '#4F7F3F', 88: '#134C00', 89: '#2F4C26',
          90: '#00FF00', 91: '#7FFF7F', 92: '#00CC00', 93: '#66CC66', 94: '#009800', 95: '#4C984C', 96: '#007F00', 97: '#3F7F3F', 98: '#004C00',
          99: '#264C26', 100: '#00FF3F', 101: '#7FFF9F', 102: '#00CC33', 103: '#66CC7F', 104: '#009826', 105: '#4C985F', 106: '#007F1F',
          107: '#3F7F4F', 108: '#004C13', 109: '#264C2F', 110: '#00FF7F', 111: '#7FFFBF', 112: '#00CC66', 113: '#66CC99', 114: '#00984C',
          115: '#4C9872', 116: '#007F3F', 117: '#3F7F5F', 118: '#004C26', 119: '#264C39', 120: '#00FFBF', 121: '#7FFFDF', 122: '#00CC99',
          123: '#66CCB2', 124: '#009872', 125: '#4C9885', 126: '#007F5F', 127: '#3F7F6F', 128: '#004C39', 129: '#264C42', 130: '#00FFFF',
          131: '#7FFFFF', 132: '#00CCCC', 133: '#66CCCC', 134: '#009898', 135: '#4C9898', 136: '#007F7F', 137: '#3F7F7F', 138: '#004C4C',
          139: '#264C4C', 140: '#00BFFF', 141: '#7FDFFF', 142: '#0099CC', 143: '#66B2CC', 144: '#007298', 145: '#4C8598', 146: '#005F7F',
          147: '#3F6F7F', 148: '#00394C', 149: '#26424C', 150: '#007FFF', 151: '#7FBFFF', 152: '#0066CC', 153: '#6699CC', 154: '#004C98',
          155: '#4C7298', 156: '#003F7F', 157: '#3F5F7F', 158: '#00264C', 159: '#26394C', 160: '#003FFF', 161: '#7F9FFF', 162: '#0033CC',
          163: '#667FCC', 164: '#002698', 165: '#4C5F98', 166: '#001F7F', 167: '#3F4F7F', 168: '#00134C', 169: '#262F4C', 170: '#0000FF',
          171: '#7F7FFF', 172: '#0000CC', 173: '#6666CC', 174: '#000098', 175: '#4C4C98', 176: '#00007F', 177: '#3F3F7F', 178: '#00004C',
          179: '#26264C', 180: '#3F00FF', 181: '#9F7FFF', 182: '#3300CC', 183: '#7F66CC', 184: '#260098', 185: '#5F4C98', 186: '#1F007F',
          187: '#4F3F7F', 188: '#13004C', 189: '#2F264C', 190: '#7F00FF', 191: '#BF7FFF', 192: '#6600CC', 193: '#9966CC', 194: '#4C0098',
          195: '#724C98', 196: '#3F007F', 197: '#5F3F7F', 198: '#26004C', 199: '#39264C', 200: '#BF00FF', 201: '#DF7FFF', 202: '#9900CC',
          203: '#B266CC', 204: '#720098', 205: '#854C98', 206: '#5F007F', 207: '#6F3F7F', 208: '#39004C', 209: '#42264C', 210: '#FF00FF',
          211: '#FF7FFF', 212: '#CC00CC', 213: '#CC66CC', 214: '#980098', 215: '#984C98', 216: '#7F007F', 217: '#7F3F7F', 218: '#4C004C',
          219: '#4C264C', 220: '#FF00BF', 221: '#FF7FDF', 222: '#CC0099', 223: '#CC66B2', 224: '#980072', 225: '#984C85', 226: '#7F005F',
          227: '#7F3F6F', 228: '#4C0039', 229: '#4C2642', 230: '#FF007F', 231: '#FF7FBF', 232: '#CC0066', 233: '#CC6699', 234: '#98004C',
          235: '#984C72', 236: '#7F003F', 237: '#7F3F5F', 238: '#4C0026', 239: '#4C2639', 240: '#FF003F', 241: '#FF7F9F', 242: '#CC0033',
          243: '#CC667F', 244: '#980026', 245: '#984C5F', 246: '#7F001F', 247: '#7F3F4F', 248: '#4C0013', 249: '#4C262F', 250: '#333333',
          251: '#5B5B5B', 252: '#848484', 253: '#ADADAD', 254: '#D6D6D6', 255: '#FFFFFF'}


class DxfInput(inkex.InputExtension):
    def add_arguments(self, pars):
        pars.add_argument("--tab", default="Options")
        pars.add_argument("--scalemethod", default="manual")
        pars.add_argument("--scale", default="1.0")
        pars.add_argument("--xmin", default="0.0")
        pars.add_argument("--ymin", default="0.0")
        pars.add_argument("--gcodetoolspoints", default=True, type=inkex.Boolean)
        pars.add_argument("--encoding", dest="input_encode", default="latin_1")
        pars.add_argument("--font", default="Arial")

    def load(self, stream):
        return stream

    def effect(self):
        global options
        global defs
        global entity
        global vals
        global seqs
        global style
        global layer
        global scale
        global color
        global extrude
        global xmin
        global ymin
        global height

        options = self.options

        doc = self.get_template(width=210 * 96 / 25.4, height=297 * 96 / 25.4)
        svg = doc.getroot()
        defs = svg.defs
        marker = etree.SubElement(defs, 'marker', {'id': 'DistanceX', 'orient': 'auto', 'refX': '0.0', 'refY': '0.0', 'style': 'overflow:visible'})
        etree.SubElement(marker, 'path', {'d': 'M 3,-3 L -3,3 M 0,-5 L  0,5', 'style': 'stroke:#000000; stroke-width:0.5'})
        pattern = etree.SubElement(defs, 'pattern', {'id': 'Hatch', 'patternUnits': 'userSpaceOnUse', 'width': '8', 'height': '8', 'x': '0', 'y': '0'})
        etree.SubElement(pattern, 'path', {'d': 'M8 4 l-4,4', 'stroke': '#000000', 'stroke-width': '0.25', 'linecap': 'square'})
        etree.SubElement(pattern, 'path', {'d': 'M6 2 l-4,4', 'stroke': '#000000', 'stroke-width': '0.25', 'linecap': 'square'})
        etree.SubElement(pattern, 'path', {'d': 'M4 0 l-4,4', 'stroke': '#000000', 'stroke-width': '0.25', 'linecap': 'square'})

        def _get_line():
            return self.document.readline().strip().decode(options.input_encode)

        def get_line():
            return _get_line(), _get_line()

        def get_group(group):
            line = get_line()
            if line[0] == group:
                return float(line[1])
            return 0.0

        xmax = xmin = ymin = 0.0
        height = 297.0 * 96.0 / 25.4  # default A4 height in pixels
        measurement = 0  # default inches
        line = get_line()
        polylines = 0
        flag = 0  # (0, 1, 2, 3) = (none, LAYER, LTYPE, DIMTXT)
        layer_colors = {}  # store colors by layer
        layer_nodes = {}  # store nodes by layer
        linetypes = {}  # store linetypes by name
        DIMTXT = {}  # store DIMENSION text sizes

        while line[0] and line[1] != 'BLOCKS':
            line = get_line()
            if options.scalemethod == 'file':
                if line[1] == '$MEASUREMENT':
                    measurement = get_group('70')
            elif options.scalemethod == 'auto':
                if line[1] == '$EXTMIN':
                    xmin = get_group('10')
                    ymin = get_group('20')
                if line[1] == '$EXTMAX':
                    xmax = get_group('10')
            if flag == 1 and line[0] == '2':
                layername = line[1]
                layer_nodes[layername] = svg.add(inkex.Layer.new(layername))
            if flag == 2 and line[0] == '2':
                linename = line[1]
                linetypes[linename] = []
            if flag == 3 and line[0] == '2':
                stylename = line[1]
            if line[0] == '2' and line[1] == 'LAYER':
                flag = 1
            if line[0] == '2' and line[1] == 'LTYPE':
                flag = 2
            if line[0] == '2' and line[1] == 'DIMSTYLE':
                flag = 3
            if flag == 1 and line[0] == '62':
                layer_colors[layername] = int(line[1])
            if flag == 2 and line[0] == '49':
                linetypes[linename].append(float(line[1]))
            if flag == 3 and line[0] == '140':
                DIMTXT[stylename] = float(line[1])
            if line[0] == '0' and line[1] == 'ENDTAB':
                flag = 0

        if options.scalemethod == 'file':
            scale = 25.4  # default inches
            if measurement == 1.0:
                scale = 1.0  # use mm
        elif options.scalemethod == 'auto':
            scale = 1.0
            if xmax > xmin:
                scale = 210.0 / (xmax - xmin)  # scale to A4 width
        else:
            scale = float(options.scale)  # manual scale factor
            xmin = float(options.xmin)
            ymin = float(options.ymin)
        svg.description('%s - scale = %f, origin = (%f, %f), method = %s' % (
            os.path.basename(options.input_file), scale, xmin, ymin, options.scalemethod))
        scale *= 96.0 / 25.4  # convert from mm to pixels

        if '0' not in layer_nodes:
            layer_nodes['0'] = svg.add(inkex.Layer.new('0'))

            layer_colors['0'] = 7

        for linename in linetypes.keys():  # scale the dashed lines
            linetype = ''
            for length in linetypes[linename]:
                if length == 0:  # test for dot
                    linetype += ' 0.5,'
                else:
                    linetype += '%.4f,' % math.fabs(length * scale)
            if linetype == '':
                linetypes[linename] = 'stroke-linecap: round'
            else:
                linetypes[linename] = 'stroke-dasharray:' + linetype

        entity = ''
        inENTITIES = False
        block = defs  # initiallize with dummy
        while line[0] and (line[1] != 'ENDSEC' or not inENTITIES):
            line = get_line()
            if line[1] == 'ENTITIES':
                inENTITIES = True
            elif line[1] == 'POLYLINE':
                polylines += 1
            if entity and line[0] in groups:
                seqs.append(line[0])  # list of group codes
                if line[0] in ('1', '2', '3', '6', '8'):  # text value
                    val = line[1].replace(r'\~', ' ')
                    val = re.sub(r'\\A.*;', '', val)
                    val = re.sub(r'\\H.*;', '', val)
                    val = re.sub(r'\^I', '', val)
                    val = re.sub(r'{\\L', '', val)
                    val = re.sub(r'}', '', val)
                    val = re.sub(r'\\S.*;', '', val)
                    val = re.sub(r'\\W.*;', '', val)
                    val = val
                    val = re.sub(r'\\U\+([0-9A-Fa-f]{4})', re_hex2unichar, val)
                elif line[0] == '62' or line[0] == '70' or line[0] == '92' or line[0] == '93':
                    val = int(line[1])
                else:  # unscaled float value
                    val = float(line[1])
                vals[groups[line[0]]].append(val)
            elif line[1] in entities:
                if entity in entities:
                    if block != defs:  # in a BLOCK
                        layer = block
                    elif vals[groups['8']]:  # use Common Layer Name
                        if not vals[groups['8']][0]:
                            vals[groups['8']][0] = '0'  # use default name
                        if vals[groups['8']][0] not in layer_nodes:
                            layer_nodes[vals[groups['8']][0]] = svg.add(inkex.Layer.new(vals[groups['8']][0]))
                        layer = layer_nodes[vals[groups['8']][0]]
                    color = '#000000'  # default color
                    if vals[groups['8']]:
                        if vals[groups['8']][0] in layer_colors:
                            if layer_colors[vals[groups['8']][0]] in colors:
                                color = colors[layer_colors[vals[groups['8']][0]]]
                    if vals[groups['62']]:  # Common Color Number
                        if vals[groups['62']][0] in colors:
                            color = colors[vals[groups['62']][0]]
                    style = formatStyle({'stroke': '%s' % color, 'fill': 'none'})
                    w = 0.5  # default lineweight for POINT
                    if vals[groups['370']]:  # Common Lineweight
                        if vals[groups['370']][0] > 0:
                            w = 96.0 / 25.4 * vals[groups['370']][0] / 100.0
                            if w < 0.5:
                                w = 0.5
                            style = formatStyle({'stroke': '%s' % color, 'fill': 'none', 'stroke-width': '%.1f' % w})
                    if vals[groups['6']]:  # Common Linetype
                        if vals[groups['6']][0] in linetypes:
                            style += ';' + linetypes[vals[groups['6']][0]]
                    extrude = 1.0
                    if vals[groups['230']]:
                        extrude = float(vals[groups['230']][0])
                    for xgrp in ['10', '13', '14']:  # scale/reflect x values
                        if vals[groups[xgrp]]:
                            for i in range(0, len(vals[groups[xgrp]])):
                                vals[groups[xgrp]][i] = scale * (extrude * vals[groups[xgrp]][i] - xmin)
                    for ygrp in ['20', '23', '24']:  # scale y values
                        if vals[groups[ygrp]]:
                            for i in range(0, len(vals[groups[ygrp]])):
                                vals[groups[ygrp]][i] = height - scale * (vals[groups[ygrp]][i] - ymin)
                    if extrude == -1.0:  # reflect angles
                        if vals[groups['50']] and vals[groups['51']]:
                            temp = vals[groups['51']][0]
                            vals[groups['51']][0] = 180.0 - vals[groups['50']][0]
                            vals[groups['50']][0] = 180.0 - temp
                    if entities[entity]:
                        if entity == 'POINT':
                            entities[entity](w)
                        else:
                            entities[entity]()
                entity = line[1]
                vals = [[], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], []]
                seqs = []

        if polylines:
            inkex.errormsg(_('%d ENTITIES of type POLYLINE encountered and ignored. Please try to convert to Release 13 format using QCad.') % polylines)
        self.document = doc


if __name__ == '__main__':
    DxfInput().run()
