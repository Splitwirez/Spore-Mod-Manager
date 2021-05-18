#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2012 Jabiertxo Arraiza, jabier.arraiza@marker.es
# Copyright (C) 2016 su_v, <suv-sf@users.sf.net>
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
"""
Version 0.6 - DPI Switcher

This extension scales a document to fit different SVG DPI -90/96-

Changes since v0.5:
    - transform all top-level containers and graphics elements
    - support scientific notation in SVG lengths
    - fix scaling with existing matrix()
    - support different units for document width, height attributes
    - improve viewBox support (syntax, offset)
    - support common cases of text-put-on-path in SVG root
    - support common cases of <use> references in SVG root
    - examples from http://tavmjong.free.fr/INKSCAPE/UNITS/ tested

TODO:
    - check grids/guides created with 0.91:
      http://tavmjong.free.fr/INKSCAPE/UNITS/units_mm_nv_90dpi.svg
    - check <symbol> instances
    - check more <use> and text-on-path cases (reverse scaling needed?)
    - scale perspective of 3dboxes

"""

import re
import math
import inkex
from inkex import Use, TextElement

# globals
SKIP_CONTAINERS = [
    'defs',
    'glyph',
    'marker',
    'mask',
    'missing-glyph',
    'pattern',
    'symbol',
]
CONTAINER_ELEMENTS = [
    'a',
    'g',
    'switch',
]
GRAPHICS_ELEMENTS = [
    'circle',
    'ellipse',
    'image',
    'line',
    'path',
    'polygon',
    'polyline',
    'rect',
    'text',
    'use',
]


def is_3dbox(element):
    """Check whether element is an Inkscape 3dbox type."""
    return element.get('sodipodi:type') == 'inkscape:box3d'


def is_text_on_path(element):
    """Check whether text element is put on a path."""
    if isinstance(element, TextElement):
        text_path = element.find('svg:textPath')
        if text_path is not None and len(text_path):
            return True
    return False


def is_sibling(element1, element2):
    """Check whether element1 and element2 are siblings of same parent."""
    return element2 in element1.getparent()


def is_in_defs(doc, element):
    """Check whether element is in defs."""
    if element is not None:
        defs = doc.find('defs')
        if defs is not None:
            return element in defs.iterdescendants()
    return False


def check_3dbox(svg, element, scale_x, scale_y):
    """Check transformation for 3dbox element."""
    skip = False
    if skip:
        # 3dbox elements ignore preserved transforms
        # FIXME: manually update geometry of 3dbox?
        pass
    return skip


def check_text_on_path(svg, element, scale_x, scale_y):
    """Check whether to skip scaling a text put on a path."""
    skip = False
    path = element.find('textPath').href
    if not is_in_defs(svg, path):
        if is_sibling(element, path):
            # skip common element scaling if both text and path are siblings
            skip = True
            # scale offset
            if 'transform' in element.attrib:
                element.transform.add_scale(scale_x, scale_y)
            # scale font size
            mat = inkex.Transform('scale({},{})'.format(scale_x, scale_y)).matrix
            det = abs(mat[0][0] * mat[1][1] - mat[0][1] * mat[1][0])
            descrim = math.sqrt(abs(det))
            prop = 'font-size'
            # outer text
            sdict = dict(inkex.Style.parse_str(element.get('style')))
            if prop in sdict:
                sdict[prop] = float(sdict[prop]) * descrim
                element.set('style', str(inkex.Style(sdict)))
            # inner tspans
            for child in element.iterdescendants():
                if isinstance(element, inkex.Tspan):
                    sdict = dict(inkex.Style.parse_str(child.get('style')))
                    if prop in sdict:
                        sdict[prop] = float(sdict[prop]) * descrim
                        child.set('style', str(inkex.Style(sdict)))
    return skip


def check_use(svg, element, scale_x, scale_y):
    """Check whether to skip scaling an instantiated element (<use>)."""
    skip = False
    path = element.href
    if not is_in_defs(svg, path):
        if is_sibling(element, path):
            skip = True
            # scale offset
            if 'transform' in element.attrib:
                element.transform.add_scale(scale_x, scale_y)
    return skip


class DPISwitcher(inkex.EffectExtension):
    multi_inx = True
    factor_a = 90.0 / 96.0
    factor_b = 96.0 / 90.0
    units = "px"

    def add_arguments(self, pars):
        pars.add_argument("--switcher", type=str, default="0",
                          help="Select the DPI switch you want")

    # dictionaries of unit to user unit conversion factors
    __uuconvLegacy = {
        'in': 90.0,
        'pt': 1.25,
        'px': 1.0,
        'mm': 3.5433070866,
        'cm': 35.433070866,
        'm': 3543.3070866,
        'km': 3543307.0866,
        'pc': 15.0,
        'yd': 3240.0,
        'ft': 1080.0,
    }
    __uuconv = {
        'in': 96.0,
        'pt': 1.33333333333,
        'px': 1.0,
        'mm': 3.77952755913,
        'cm': 37.7952755913,
        'm': 3779.52755913,
        'km': 3779527.55913,
        'pc': 16.0,
        'yd': 3456.0,
        'ft': 1152.0,
    }

    def parse_length(self, length, percent=False):
        """Parse SVG length."""
        if self.options.switcher == "0":  # dpi90to96
            known_units = list(self.__uuconvLegacy)
        else:  # dpi96to90
            known_units = list(self.__uuconv)
        if percent:
            unitmatch = re.compile('(%s)$' % '|'.join(known_units + ['%']))
        else:
            unitmatch = re.compile('(%s)$' % '|'.join(known_units))
        param = re.compile(r'(([-+]?[0-9]+(\.[0-9]*)?|[-+]?\.[0-9]+)([eE][-+]?[0-9]+)?)')
        p = param.match(length)
        u = unitmatch.search(length)
        val = 100  # fallback: assume default length of 100
        unit = 'px'  # fallback: assume 'px' unit
        if p:
            val = float(p.string[p.start():p.end()])
        if u:
            unit = u.string[u.start():u.end()]
        return val, unit

    def convert_length(self, val, unit):
        """Convert length to self.units if unit differs."""
        doc_unit = self.units or 'px'
        if unit != doc_unit:
            if self.options.switcher == "0":  # dpi90to96
                val_px = val * self.__uuconvLegacy[unit]
                val = val_px / (self.__uuconvLegacy[doc_unit] / self.__uuconvLegacy['px'])
                unit = doc_unit
            else:  # dpi96to90
                val_px = val * self.__uuconv[unit]
                val = val_px / (self.__uuconv[doc_unit] / self.__uuconv['px'])
                unit = doc_unit
        return val, unit

    def check_attr_unit(self, element, attr, unit_list):
        """Check unit of attribute value, match to units in *unit_list*."""
        if attr in element.attrib:
            unit = self.parse_length(element.get(attr), percent=True)[1]
            return unit in unit_list

    def scale_attr_val(self, element, attr, unit_list, factor):
        """Scale attribute value if unit matches one in *unit_list*."""
        if attr in element.attrib:
            val, unit = self.parse_length(element.get(attr), percent=True)
            if unit in unit_list:
                element.set(attr, '{}{}'.format(val * factor, unit))

    def scale_root(self, unit_exponent=1.0):
        """Scale all top-level elements in SVG root."""

        # update viewport
        width_num = self.parse_length(self.svg.get('width'))[0]
        height_num = self.convert_length(*self.parse_length(self.svg.get('height')))[0]
        width_doc = width_num * self.factor_a * unit_exponent
        height_doc = height_num * self.factor_a * unit_exponent

        svg = self.svg
        if svg.get('height'):
            svg.set('height', str(height_doc))
        if svg.get('width'):
            svg.set('width', str(width_doc))

        # update viewBox
        if svg.get('viewBox'):
            viewboxstring = re.sub(' +|, +|,', ' ', svg.get('viewBox'))
            viewboxlist = [float(i) for i in viewboxstring.strip().split(' ', 4)]
            svg.set('viewBox', '{} {} {} {}'.format(*[(val * self.factor_a) for val in viewboxlist]))

        # update guides, grids
        if self.options.switcher == "1":
            # FIXME: dpi96to90 only?
            self.scale_guides()
            self.scale_grid()

        for element in svg:  # iterate all top-level elements of SVGRoot

            # init variables
            tag = element.TAG
            width_scale = self.factor_a
            height_scale = self.factor_a

            if tag in GRAPHICS_ELEMENTS or tag in CONTAINER_ELEMENTS:

                # test for specific elements to skip from scaling
                if is_3dbox(element):
                    if check_3dbox(svg, element, width_scale, height_scale):
                        continue
                if is_text_on_path(element):
                    if check_text_on_path(svg, element, width_scale, height_scale):
                        continue
                if isinstance(element, Use):
                    if check_use(svg, element, width_scale, height_scale):
                        continue

                # relative units ('%') in presentation attributes
                for attr in ['width', 'height']:
                    self.scale_attr_val(element, attr, ['%'], 1.0 / self.factor_a)
                for attr in ['x', 'y']:
                    self.scale_attr_val(element, attr, ['%'], 1.0 / self.factor_a)

                # set preserved transforms on top-level elements
                if width_scale != 1.0 and height_scale != 1.0:
                    element.transform.add_scale(width_scale, height_scale)

    def scale_element(self, elem):
        pass  # TODO: optionally scale graphics elements only?

    def scale_guides(self):
        """Scale the guidelines"""
        for guide in self.svg.namedview.get_guides():
            point = guide.get("position").split(",")
            guide.set("position", str(float(point[0].strip()) * self.factor_a) + "," + str(float(point[1].strip()) * self.factor_a))

    def scale_grid(self):
        """Scale the inkscape grid"""
        grids = self.svg.xpath('//inkscape:grid')
        for grid in grids:
            grid.set("units", "px")
            if grid.get("spacingx"):
                spacingx = str(float(re.sub("[a-zA-Z]", "", grid.get("spacingx"))) * self.factor_a) + "px"
                grid.set("spacingx", str(spacingx))
            if grid.get("spacingy"):
                spacingy = str(float(re.sub("[a-zA-Z]", "", grid.get("spacingy"))) * self.factor_a) + "px"
                grid.set("spacingy", str(spacingy))
            if grid.get("originx"):
                originx = str(float(re.sub("[a-zA-Z]", "", grid.get("originx"))) * self.factor_a) + "px"
                grid.set("originx", str(originx))
            if grid.get("originy"):
                originy = str(float(re.sub("[a-zA-Z]", "", grid.get("originy"))) * self.factor_a) + "px"
                grid.set("originy", str(originy))

    def effect(self):
        svg = self.svg
        if self.options.switcher == "0":
            self.factor_a = 96.0 / 90.0
            self.factor_b = 90.0 / 96.0
        svg.namedview.set('inkscape:document-units', "px")
        self.units = self.parse_length(svg.get('width'))[1]
        unit_exponent = 1.0
        if self.units and self.units != "px" and self.units != "" and self.units != "%":
            if self.options.switcher == "0":
                unit_exponent = 1.0 / (self.factor_a / self.__uuconv[self.units])
            else:
                unit_exponent = 1.0 / (self.factor_a / self.__uuconvLegacy[self.units])
        self.scale_root(unit_exponent)


if __name__ == '__main__':
    DPISwitcher().run()
