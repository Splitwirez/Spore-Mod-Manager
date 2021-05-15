#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2012 Alvin Penner, penner@vaxxine.com
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
This extension will generate vector graphics printout, specifically for Windows GDI32.

This is a modified version of the file dxf_outlines.py by Aaron Spike, aaron@ekips.org
It will write only to the default printer.
The printing preferences dialog will be called.
In order to ensure a pure vector output, use a linewidth < 1 printer pixel

- see http://www.lessanvaezi.com/changing-printer-settings-using-the-windows-api/
- get GdiPrintSample.zip at http://archive.msdn.microsoft.com/WindowsPrintSample

"""

import sys
import ctypes

import inkex
from inkex import PathElement, Rectangle, Group, Use, Transform
from inkex.paths import Path

if sys.platform.startswith('win'):
    myspool = ctypes.WinDLL("winspool.drv")
    mygdi = ctypes.WinDLL("gdi32.dll")
else:
    myspool = None
    mygdi = None

LOGBRUSH = ctypes.c_long * 3
DM_IN_PROMPT = 4                        # call printer property sheet
DM_OUT_BUFFER = 2                       # write to DEVMODE structure

class PrintWin32Vector(inkex.EffectExtension):
    def __init__(self):
        super(PrintWin32Vector, self).__init__()
        self.visibleLayers = True       # print only visible layers

    def process_shape(self, node, mat):
        """Process shape"""
        rgb = (0, 0, 0)                 # stroke color
        fillcolor = None                # fill color
        stroke = 1                      # pen width in printer pixels
        # Very NB : If the pen width is greater than 1 then the output will Not be a vector output !
        style = node.style
        if style:
            if 'stroke' in style:
                if style['stroke'] and style['stroke'] != 'none' and style['stroke'][0:3] != 'url':
                    rgb = inkex.Color(style['stroke']).to_rgb()
            if 'stroke-width' in style:
                stroke = self.svg.unittouu(style['stroke-width'])/self.svg.unittouu('1px')
                stroke = int(stroke*self.scale)
            if 'fill' in style:
                if style['fill'] and style['fill'] != 'none' and style['fill'][0:3] != 'url':
                    fill = inkex.Color(style['fill']).to_rgb()
                    fillcolor = fill[0] + 256*fill[1] + 256*256*fill[2]
        color = rgb[0] + 256*rgb[1] + 256*256*rgb[2]
        if isinstance(node, PathElement):
            p = node.path.to_superpath()
            if not p:
                return
        elif isinstance(node, Rectangle):
            x = float(node.get('x'))
            y = float(node.get('y'))
            width = float(node.get('width'))
            height = float(node.get('height'))
            p = [[[x, y],[x, y],[x, y]]]
            p.append([[x + width, y],[x + width, y],[x + width, y]])
            p.append([[x + width, y + height],[x + width, y + height],[x + width, y + height]])
            p.append([[x, y + height],[x, y + height],[x, y + height]])
            p.append([[x, y],[x, y],[x, y]])
            p = [p]
        else:
            return
        mat += node.transform
        p = Path(p).transform(Transform(mat)).to_arrays()
        hPen = mygdi.CreatePen(0, stroke, color)
        mygdi.SelectObject(self.hDC, hPen)
        self.emit_path(p)
        if fillcolor is not None:
            brush = LOGBRUSH(0, fillcolor, 0)
            hBrush = mygdi.CreateBrushIndirect(ctypes.addressof(brush))
            mygdi.SelectObject(self.hDC, hBrush)
            mygdi.BeginPath(self.hDC)
            self.emit_path(p)
            mygdi.EndPath(self.hDC)
            mygdi.FillPath(self.hDC)
        return

    def emit_path(self, p):
        for sub in p:
            mygdi.MoveToEx(self.hDC, int(sub[0][1][0]), int(sub[0][1][1]), None)
            POINTS = ctypes.c_long*(6*(len(sub)-1))
            points = POINTS()
            for i in range(len(sub)-1):
                points[6*i]     = int(sub[i][2][0])
                points[6*i + 1] = int(sub[i][2][1])
                points[6*i + 2] = int(sub[i + 1][0][0])
                points[6*i + 3] = int(sub[i + 1][0][1])
                points[6*i + 4] = int(sub[i + 1][1][0])
                points[6*i + 5] = int(sub[i + 1][1][1])
            mygdi.PolyBezierTo(self.hDC, ctypes.addressof(points), 3*(len(sub)-1))
        return

    def process_clone(self, node):
        trans = node.get('transform')
        x = node.get('x')
        y = node.get('y')
        mat = Transform([[1.0, 0.0, 0.0], [0.0, 1.0, 0.0]])
        if trans:
            mat *= Transform(trans)
        if x:
            mat *= Transform([[1.0, 0.0, float(x)], [0.0, 1.0, 0.0]])
        if y:
            mat *= Transform([[1.0, 0.0, 0.0], [0.0, 1.0, float(y)]])
        # push transform
        if trans or x or y:
            self.groupmat.append(Transform(self.groupmat[-1]) * mat)
        # get referenced node
        refnode = node.href
        if refnode is not None:
            if isinstance(refnode, inkex.Group):
                self.process_group(refnode)
            elif refnode.tag == 'svg:use':
                self.process_clone(refnode)
            else:
                self.process_shape(refnode, self.groupmat[-1])
        # pop transform
        if trans or x or y:
            self.groupmat.pop()

    def process_group(self, group):
        if isinstance(group, inkex.Layer):
            style = group.style
            if 'display' in style:
                if style['display'] == 'none' and self.visibleLayers:
                    return
        trans = group.get('transform')
        if trans:
            self.groupmat.append(Transform(self.groupmat[-1]) * Transform(trans))
        for node in group:
            if isinstance(node, Group):
                self.process_group(node)
            elif isinstance(node, Use):
                self.process_clone(node)
            else:
                self.process_shape(node, self.groupmat[-1])
        if trans:
            self.groupmat.pop()

    def effect(self):
        pcchBuffer = ctypes.c_long()
        myspool.GetDefaultPrinterA(None, ctypes.byref(pcchBuffer))     # get length of printer name
        pname = ctypes.create_string_buffer(pcchBuffer.value)
        myspool.GetDefaultPrinterA(pname, ctypes.byref(pcchBuffer))    # get printer name
        hPrinter = ctypes.c_long()
        if myspool.OpenPrinterA(pname.value, ctypes.byref(hPrinter), None) == 0:
            return inkex.errormsg(_("Failed to open default printer"))

        # get printer properties dialog

        pcchBuffer = myspool.DocumentPropertiesA(0, hPrinter, pname, None, None, 0)
        pDevMode = ctypes.create_string_buffer(pcchBuffer + 100) # allocate extra just in case
        pcchBuffer = myspool.DocumentPropertiesA(0, hPrinter, pname, ctypes.byref(pDevMode), None, DM_IN_PROMPT + DM_OUT_BUFFER)
        myspool.ClosePrinter(hPrinter)
        if pcchBuffer != 1:             # user clicked Cancel
            exit()

        # initiallize print document

        docname = self.svg.xpath('@sodipodi:docname')
        if not docname:
            docname = ['New document 1']
        lpszDocName = ctypes.create_string_buffer('Inkscape ' + docname[0].split('\\')[-1])
        DOCINFO = ctypes.c_long * 5
        docInfo = DOCINFO(20, ctypes.addressof(lpszDocName), 0, 0, 0)
        self.hDC = mygdi.CreateDCA(None, pname, None, ctypes.byref(pDevMode))
        if mygdi.StartDocA(self.hDC, ctypes.byref(docInfo)) < 0:
            exit()                      # user clicked Cancel

        self.scale = (ord(pDevMode[58]) + 256.0*ord(pDevMode[59]))/96    # use PrintQuality from DEVMODE
        self.scale /= self.svg.unittouu('1px')
        h = self.svg.unittouu(self.svg.xpath('@height')[0])
        doc = self.document.getroot()
        # process viewBox height attribute to correct page scaling
        viewBox = doc.get('viewBox')
        if viewBox:
            viewBox2 = viewBox.split(',')
            if len(viewBox2) < 4:
                viewBox2 = viewBox.split(' ')
            self.scale *= h / self.svg.unittouu(self.addDocumentUnit(viewBox2[3]))
        self.groupmat = [[[self.scale, 0.0, 0.0], [0.0, self.scale, 0.0]]]
        self.process_group(doc)
        mygdi.EndDoc(self.hDC)

if __name__ == '__main__':
    PrintWin32Vector().run()
