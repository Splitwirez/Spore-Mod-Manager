#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2005,2007 Aaron Spike, aaron@ekips.org
# - template dxf_outlines.dxf added Feb 2008 by Alvin Penner, penner@vaxxine.com
# - layers, transformation, flattening added April 2008 by Bob Cook, bob@bobcookdev.com
# - added support for dxf R12, Nov. 2008 by Switcher
# - brought together to replace ps2edit version 2018 by Martin Owens
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
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
#

from __future__ import absolute_import, print_function, unicode_literals

import re
import inkex
from inkex.bezier import cspsubdiv

r12_header = ''' 0 
SECTION
 2 
HEADER
 9 
$ACADVER
 1 
AC1009
 9 
$EXTMIN
 10 
 0 
 20 
 0 
 9 
$EXTMAX
 10 
 8.5 
 20 
 11 
 0 
ENDSEC
 0 
SECTION
 2 
ENTITIES
'''

r12_footer = ''' 0 
ENDSEC
 0 
EOF'''


class DxfTwelve(inkex.OutputExtension):
    """Create dxf12 output from the svg"""
    def __init__(self):
        super(DxfTwelve, self).__init__()
        self.handle = 255
        self.flatness = 0.1

    def dxf_add(self, line):
        self._stream.write(line.encode('utf-8'))

    def dxf_insert_code(self, code, value):
        self.dxf_add(code + "\n" + value + "\n")

    def dxf_line(self, layer, csp):
        self.dxf_insert_code('0', 'LINE')
        self.dxf_insert_code('8', layer)
        # self.dxf_insert_code(  '62', '1' )  #Change the Line Color
        self.dxf_insert_code('10', '{:f}'.format(csp[0][0]))
        self.dxf_insert_code('20', '{:f}'.format(csp[0][1]))
        self.dxf_insert_code('11', '{:f}'.format(csp[1][0]))
        self.dxf_insert_code('21', '{:f}'.format(csp[1][1]))

    def dxf_path_to_lines(self, layer, p):
        f = self.flatness
        is_flat = 0
        while is_flat < 1:
            try:
                cspsubdiv(p, self.flatness)
                is_flat = 1
            except:
                f += 0.1

        for sub in p:
            for i in range(len(sub) - 1):
                self.handle += 1
                s = sub[i]
                e = sub[i + 1]
                self.dxf_line(layer, [s[1], e[1]])

    def dxf_path_to_point(self, layer, p):
        bbox = inkex.Path(p).bounding_box() or inkex.BoundingBox(0, 0)
        x, y = bbox.center
        self.dxf_point(layer, x, y)

    def save(self, stream):
        self._stream = stream
        self.dxf_insert_code('999', '"DXF R12 Output" (www.mydxf.blogspot.com)')
        self.dxf_add(r12_header)

        scale = 25.4 / 90.0
        h = self.svg.height

        path = '//svg:path'
        for node in self.svg.xpath(path):

            layer = node.getparent().label
            if layer is None:
                layer = 'Layer 1'

            node.transform *= inkex.Transform([[scale, 0, 0], [0, -scale, h * scale]])
            node.apply_transform()
            path = node.path.to_superpath()

            if re.search('drill$', layer, re.I) is None:
                # if layer == 'Brackets Drill':
                self.dxf_path_to_lines(layer, path)
            else:
                self.dxf_path_to_point(layer, path)

        self.dxf_add(r12_footer)


if __name__ == '__main__':
    DxfTwelve().run()
