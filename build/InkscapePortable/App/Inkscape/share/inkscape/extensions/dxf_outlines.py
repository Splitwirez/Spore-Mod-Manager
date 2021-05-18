#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2005,2007,2008 Aaron Spike, aaron@ekips.org
# Copyright (C) 2008,2010 Alvin Penner, penner@vaxxine.com
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
This file output script for Inkscape creates a AutoCAD R14 DXF file.
The spec can be found here: http://www.autodesk.com/techpubs/autocad/acadr14/dxf/index.htm.

 File history:
 - template dxf_outlines.dxf added Feb 2008 by Alvin Penner
- ROBO-Master output option added Aug 2008
- ROBO-Master multispline output added Sept 2008
- LWPOLYLINE output modification added Dec 2008
- toggle between LINE/LWPOLYLINE added Jan 2010
- support for transform elements added July 2010
- support for layers added July 2010
- support for rectangle added Dec 2010
"""

from __future__ import print_function

import inkex
from inkex import colors, bezier, Transform, Group, Layer, Use, PathElement, \
    Rectangle, Line, Circle, Ellipse


def get_matrix(u, i, j):
    if j == i + 2:
        return (u[i]-u[i-1])*(u[i]-u[i-1])/(u[i+2]-u[i-1])/(u[i+1]-u[i-1])
    elif j == i + 1:
        return ((u[i]-u[i-1])*(u[i+2]-u[i])/(u[i+2]-u[i-1]) \
             + (u[i+1]-u[i])*(u[i]-u[i-2])/(u[i+1]-u[i-2]))/(u[i+1]-u[i-1])
    elif j == i:
        return (u[i+1]-u[i])*(u[i+1]-u[i])/(u[i+1]-u[i-2])/(u[i+1]-u[i-1])
    else:
        return 0

def get_fit(u, csp, col):
    return (1-u)**3*csp[0][col] + 3*(1-u)**2*u*csp[1][col] \
        + 3*(1-u)*u**2*csp[2][col] + u**3*csp[3][col]

class DxfOutlines(inkex.OutputExtension):
    def add_arguments(self, pars):
        pars.add_argument("--tab")
        pars.add_argument("-R", "--ROBO", type=inkex.Boolean, default=False)
        pars.add_argument("-P", "--POLY", type=inkex.Boolean, default=False)
        pars.add_argument("--units", default="72./96")  # Points
        pars.add_argument("--encoding", dest="char_encode", default="latin_1")
        pars.add_argument("--layer_option", default="all")
        pars.add_argument("--layer_name")

        self.dxf = []
        self.handle = 255  # handle for DXF ENTITY
        self.layers = ['0']
        self.layer = '0'  # mandatory layer
        self.layernames = []
        self.csp_old = [[0.0, 0.0]] * 4  # previous spline
        self.d = [0.0] # knot vector
        self.poly = [[0.0, 0.0]]  # LWPOLYLINE data

    def save(self, stream):
        stream.write(b''.join(self.dxf))

    def dxf_add(self, str):
        self.dxf.append(str.encode(self.options.char_encode))

    def dxf_line(self, csp):
        """Draw a line in the DXF format"""
        self.handle += 1
        self.dxf_add("  0\nLINE\n  5\n%x\n100\nAcDbEntity\n  8\n%s\n 62\n%d\n100\nAcDbLine\n" % (self.handle, self.layer, self.color))
        self.dxf_add(" 10\n%f\n 20\n%f\n 30\n0.0\n 11\n%f\n 21\n%f\n 31\n0.0\n" % (csp[0][0], csp[0][1], csp[1][0], csp[1][1]))

    def LWPOLY_line(self, csp):
        if (abs(csp[0][0] - self.poly[-1][0]) > .0001
                or abs(csp[0][1] - self.poly[-1][1]) > .0001
                or self.color_LWPOLY != self.color): # THIS LINE IS NEW
            self.LWPOLY_output()  # terminate current polyline
            self.poly = [csp[0]]  # initiallize new polyline
            self.color_LWPOLY = self.color
            self.layer_LWPOLY = self.layer
        self.poly.append(csp[1])

    def LWPOLY_output(self):
        if len(self.poly) == 1:
            return
        self.handle += 1
        closed = 1
        if (abs(self.poly[0][0] - self.poly[-1][0]) > .0001
                or abs(self.poly[0][1] - self.poly[-1][1]) > .0001):
            closed = 0
        self.dxf_add("  0\nLWPOLYLINE\n  5\n%x\n100\nAcDbEntity\n  8\n%s\n 62\n%d\n100\nAcDbPolyline\n 90\n%d\n 70\n%d\n" % (self.handle, self.layer_LWPOLY, self.color_LWPOLY, len(self.poly) - closed, closed))
        for i in range(len(self.poly) - closed):
            self.dxf_add(" 10\n%f\n 20\n%f\n 30\n0.0\n" % (self.poly[i][0], self.poly[i][1]))

    def dxf_spline(self, csp):
        knots = 8
        ctrls = 4
        self.handle += 1
        self.dxf_add("  0\nSPLINE\n  5\n%x\n100\nAcDbEntity\n  8\n%s\n 62\n%d\n100\nAcDbSpline\n" % (self.handle, self.layer, self.color))
        self.dxf_add(" 70\n8\n 71\n3\n 72\n%d\n 73\n%d\n 74\n0\n" % (knots, ctrls))
        for i in range(2):
            for j in range(4):
                self.dxf_add(" 40\n%d\n" % i)
        for i in csp:
            self.dxf_add(" 10\n%f\n 20\n%f\n 30\n0.0\n" % (i[0], i[1]))

    def ROBO_spline(self, csp):
        """this spline has zero curvature at the endpoints, as in ROBO-Master"""
        if (abs(csp[0][0] - self.csp_old[3][0]) > .0001
                or abs(csp[0][1] - self.csp_old[3][1]) > .0001
                or abs((csp[1][1] - csp[0][1]) * (self.csp_old[3][0] - self.csp_old[2][0]) - (csp[1][0] - csp[0][0]) * (self.csp_old[3][1] - self.csp_old[2][1])) > .001):
            self.ROBO_output()  # terminate current spline
            self.xfit = [csp[0][0]]  # initiallize new spline
            self.yfit = [csp[0][1]]
            self.d = [0.0]
            self.color_ROBO = self.color
            self.layer_ROBO = self.layer
        self.xfit += 3 * [0.0]
        self.yfit += 3 * [0.0]
        self.d += 3 * [0.0]
        for i in range(1, 4):
            j = len(self.d) + i - 4
            self.xfit[j] = get_fit(i / 3.0, csp, 0)
            self.yfit[j] = get_fit(i / 3.0, csp, 1)
            self.d[j] = self.d[j - 1] + bezier.pointdistance((self.xfit[j - 1], self.yfit[j - 1]), (self.xfit[j], self.yfit[j]))
        self.csp_old = csp

    def ROBO_output(self):
        try:
            import numpy
            from numpy.linalg import solve
        except ImportError:
            inkex.errormsg("Failed to import the numpy or numpy.linalg modules. These modules are required by the ROBO option. Please install them and try again.")
            return

        if len(self.d) == 1:
            return
        fits = len(self.d)
        ctrls = fits + 2
        knots = ctrls + 4
        self.xfit += 2 * [0.0] # pad with 2 endpoint constraints
        self.yfit += 2 * [0.0]
        self.d += 6 * [0.0] # pad with 3 duplicates at each end
        self.d[fits + 2] = self.d[fits + 1] = self.d[fits] = self.d[fits - 1]

        solmatrix = numpy.zeros((ctrls, ctrls), dtype=float)
        for i in range(fits):
            solmatrix[i, i] = get_matrix(self.d, i, i)
            solmatrix[i, i + 1] = get_matrix(self.d, i, i + 1)
            solmatrix[i, i + 2] = get_matrix(self.d, i, i + 2)
        solmatrix[fits, 0] = self.d[2] / self.d[fits - 1]  # curvature at start = 0
        solmatrix[fits, 1] = -(self.d[1] + self.d[2]) / self.d[fits - 1]
        solmatrix[fits, 2] = self.d[1] / self.d[fits - 1]
        solmatrix[fits + 1, fits - 1] = (self.d[fits - 1] - self.d[fits - 2]) / self.d[fits - 1]  # curvature at end = 0
        solmatrix[fits + 1, fits] = (self.d[fits - 3] + self.d[fits - 2] - 2 * self.d[fits - 1]) / self.d[fits - 1]
        solmatrix[fits + 1, fits + 1] = (self.d[fits - 1] - self.d[fits - 3]) / self.d[fits - 1]
        xctrl = solve(solmatrix, self.xfit)
        yctrl = solve(solmatrix, self.yfit)
        self.handle += 1
        self.dxf_add("  0\nSPLINE\n  5\n%x\n100\nAcDbEntity\n  8\n%s\n 62\n%d\n100\nAcDbSpline\n" % (self.handle, self.layer_ROBO, self.color_ROBO))
        self.dxf_add(" 70\n0\n 71\n3\n 72\n%d\n 73\n%d\n 74\n%d\n" % (knots, ctrls, fits))
        for i in range(knots):
            self.dxf_add(" 40\n%f\n" % self.d[i - 3])
        for i in range(ctrls):
            self.dxf_add(" 10\n%f\n 20\n%f\n 30\n0.0\n" % (xctrl[i], yctrl[i]))
        for i in range(fits):
            self.dxf_add(" 11\n%f\n 21\n%f\n 31\n0.0\n" % (self.xfit[i], self.yfit[i]))

    def process_shape(self, node, mat):
        rgb = (0, 0, 0)
        style = node.get('style')
        if style:
            style = dict(inkex.Style.parse_str(style))
            if 'stroke' in style:
                if style['stroke'] and style['stroke'] != 'none' and style['stroke'][0:3] != 'url':
                    rgb = inkex.Color(style['stroke']).to_rgb()
        hsl = colors.rgb_to_hsl(rgb[0] / 255.0, rgb[1] / 255.0, rgb[2] / 255.0)
        self.color = 7  # default is black
        if hsl[2]:
            self.color = 1 + (int(6 * hsl[0] + 0.5) % 6)  # use 6 hues

        if not isinstance(node, (PathElement, Rectangle, Line, Circle, Ellipse)):
            return

        # Transforming /after/ superpath is more reliable than before
        # because of some issues with arcs in transformations
        for sub in node.path.to_superpath().transform(Transform(mat) * node.transform):
            for i in range(len(sub) - 1):
                s = sub[i]
                e = sub[i + 1]
                if s[1] == s[2] and e[0] == e[1]:
                    if self.options.POLY:
                        self.LWPOLY_line([s[1], e[1]])
                    else:
                        self.dxf_line([s[1], e[1]])
                elif self.options.ROBO:
                    self.ROBO_spline([s[1], s[2], e[0], e[1]])
                else:
                    self.dxf_spline([s[1], s[2], e[0], e[1]])

    def process_clone(self, node):
        """Process a clone node, looking for internal paths"""
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
        refid = node.get('xlink:href')
        refnode = self.svg.getElementById(refid[1:])
        if refnode is not None:
            if isinstance(refnode, Group):
                self.process_group(refnode)
            elif isinstance(refnode, Use):
                self.process_clone(refnode)
            else:
                self.process_shape(refnode, self.groupmat[-1])
        # pop transform
        if trans or x or y:
            self.groupmat.pop()

    def process_group(self, group):
        """Process group elements"""
        if isinstance(group, Layer):
            style = group.style
            if style.get('display', '') == 'none' and self.options.layer_option and self.options.layer_option == 'visible':
                return
            layer = group.label
            if self.options.layer_name and self.options.layer_option == 'name':
                if not layer.lower() in self.options.layer_name:
                    return

            layer = layer.replace(' ', '_')
            if layer in self.layers:
                self.layer = layer
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
        # Warn user if name match field is empty
        if self.options.layer_option and self.options.layer_option == 'name' and not self.options.layer_name:
            return inkex.errormsg("Error: Field 'Layer match name' must be filled when using 'By name match' option")

        # Split user layer data into a list: "layerA,layerb,LAYERC" becomes ["layera", "layerb", "layerc"]
        if self.options.layer_name:
            self.options.layer_name = self.options.layer_name.lower().split(',')

        # References:   Minimum Requirements for Creating a DXF File of a 3D Model By Paul Bourke
        #              NURB Curves: A Guide for the Uninitiated By Philip J. Schneider
        #              The NURBS Book By Les Piegl and Wayne Tiller (Springer, 1995)
        # self.dxf_add("999\nDXF created by Inkscape\n")  # Some programs do not take comments in DXF files (KLayout 0.21.12 for example)
        with open(self.get_resource('dxf14_header.txt'), 'r') as fhl:
            self.dxf_add(fhl.read())
        for node in self.svg.xpath('//svg:g'):
            if isinstance(node, Layer):
                layer = node.label
                self.layernames.append(layer.lower())
                if self.options.layer_name and self.options.layer_option and self.options.layer_option == 'name' and not layer.lower() in self.options.layer_name:
                    continue
                layer = layer.replace(' ', '_')
                if layer and layer not in self.layers:
                    self.layers.append(layer)
        self.dxf_add("  2\nLAYER\n  5\n2\n100\nAcDbSymbolTable\n 70\n%s\n" % len(self.layers))
        for i in range(len(self.layers)):
            self.dxf_add("  0\nLAYER\n  5\n%x\n100\nAcDbSymbolTableRecord\n100\nAcDbLayerTableRecord\n  2\n%s\n 70\n0\n  6\nCONTINUOUS\n" % (i + 80, self.layers[i]))
        with open(self.get_resource('dxf14_style.txt'), 'r') as fhl:
            self.dxf_add(fhl.read())

        scale = eval(self.options.units)
        if not scale:
            scale = 25.4 / 96  # if no scale is specified, assume inch as baseunit
        scale /= self.svg.unittouu('1px')
        h = self.svg.height
        doc = self.document.getroot()
        # process viewBox height attribute to correct page scaling
        viewBox = doc.get('viewBox')
        if viewBox:
            viewBox2 = viewBox.split(',')
            if len(viewBox2) < 4:
                viewBox2 = viewBox.split(' ')
            scale *= h / self.svg.unittouu(self.svg.add_unit(viewBox2[3]))
        self.groupmat = [[[scale, 0.0, 0.0], [0.0, -scale, h * scale]]]
        self.process_group(doc)
        if self.options.ROBO:
            self.ROBO_output()
        if self.options.POLY:
            self.LWPOLY_output()
        with open(self.get_resource('dxf14_footer.txt'), 'r') as fhl:
            self.dxf_add(fhl.read())
        # Warn user if layer data seems wrong
        if self.options.layer_name and self.options.layer_option and self.options.layer_option == 'name':
            for layer in self.options.layer_name:
                if layer not in self.layernames:
                    inkex.errormsg("Warning: Layer '%s' not found!" % layer)


if __name__ == '__main__':
    DxfOutlines().run()
