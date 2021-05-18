#!/usr/bin/env python
# coding=utf-8
#
# Authors:
#   Nicolas Dufour - Association Inkscape-fr
#   Aurelio A. Heckert <aurium(a)gmail.com>
#
# Copyright (C) 2008 Authors
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
This extension allows you to draw crop, registration and other
printing marks in Inkscape.
"""

import math
import inkex
from inkex import Circle, Rectangle, TextElement


class PrintingMarks(inkex.EffectExtension):
    # Default parameters
    stroke_width = 0.25

    def add_arguments(self, pars):
        pars.add_argument("--where", help="Apply crop marks to...")
        pars.add_argument("--crop_marks", type=inkex.Boolean, default=True, help="Draw crop Marks")
        pars.add_argument("--bleed_marks", type=inkex.Boolean, help="Draw Bleed Marks")
        pars.add_argument("--registration_marks", type=inkex.Boolean,\
            dest="reg_marks", default=False, help="Draw Registration Marks?")
        pars.add_argument("--star_target", type=inkex.Boolean, help="Draw Star Target?")
        pars.add_argument("--colour_bars", type=inkex.Boolean, help="Draw Colour Bars?")
        pars.add_argument("--page_info", type=inkex.Boolean, help="Draw Page Information?")
        pars.add_argument("--unit", default="px", help="Draw measurement")
        pars.add_argument("--crop_offset", type=float, default=0.0, help="Offset")
        pars.add_argument("--bleed_top", type=float, default=0.0, help="Bleed Top Size")
        pars.add_argument("--bleed_bottom", type=float, default=0.0, help="Bleed Bottom Size")
        pars.add_argument("--bleed_left", type=float, default=0.0, help="Bleed Left Size")
        pars.add_argument("--bleed_right", type=float, default=0.0, help="Bleed Right Size")
        pars.add_argument("--tab", help="The selected UI-tab when OK was pressed")

    def draw_crop_line(self, x1, y1, x2, y2, name, parent):
        style = {'stroke': '#000000', 'stroke-width': str(self.stroke_width),
                 'fill': 'none'}
        line_attribs = {'style': str(inkex.Style(style)),
                        'id': name,
                        'd': 'M ' + str(x1) + ',' + str(y1) + ' L ' + str(x2) + ',' + str(y2)}
        parent.add(inkex.PathElement(**line_attribs))

    def draw_bleed_line(self, x1, y1, x2, y2, name, parent):
        style = {'stroke': '#000000', 'stroke-width': str(self.stroke_width),
                 'fill': 'none',
                 'stroke-miterlimit': '4', 'stroke-dasharray': '4, 2, 1, 2',
                 'stroke-dashoffset': '0'}
        line_attribs = {'style': str(inkex.Style(style)),
                        'id': name,
                        'd': 'M ' + str(x1) + ',' + str(y1) + ' L ' + str(x2) + ',' + str(y2)}
        parent.add(inkex.PathElement(**line_attribs))

    def draw_reg_circles(self, cx, cy, r, name, colours, parent):
        for i in range(len(colours)):
            style = {'stroke': colours[i], 'stroke-width': str(r / len(colours)),
                     'fill': 'none'}
            circle_attribs = {'style': str(inkex.Style(style)),
                              'inkscape:label': name,
                              'cx': str(cx), 'cy': str(cy),
                              'r': str((r / len(colours)) * (i + 0.5))}
            parent.add(Circle(**circle_attribs))

    def draw_reg_marks(self, cx, cy, rotate, name, parent):
        colours = ['#000000', '#00ffff', '#ff00ff', '#ffff00', '#000000']
        g = parent.add(inkex.Group(id=name))
        for i in range(len(colours)):
            style = {'fill': colours[i], 'fill-opacity': '1', 'stroke': 'none'}
            r = (self.mark_size / 2)
            step = r
            stroke = r / len(colours)
            regoffset = stroke * i
            regmark_attribs = {'style': str(inkex.Style(style)),
                               'd': 'm' +
                                    ' ' + str(-regoffset) + ',' + str(r) +
                                    ' ' + str(-stroke) + ',0' +
                                    ' ' + str(step) + ',' + str(-r) +
                                    ' ' + str(-step) + ',' + str(-r) +
                                    ' ' + str(stroke) + ',0' +
                                    ' ' + str(step) + ',' + str(r) +
                                    ' ' + str(-step) + ',' + str(r) +
                                    ' z',
                               'transform': 'translate(' + str(cx) + ',' + str(cy) +
                                            ') rotate(' + str(rotate) + ')'}
            g.add(inkex.PathElement(**regmark_attribs))

    def draw_star_target(self, cx, cy, name, parent):
        r = (self.mark_size / 2)
        style = {'fill': '#000 device-cmyk(1,1,1,1)', 'fill-opacity': '1', 'stroke': 'none'}
        d = ' M 0,0'
        i = 0
        while i < (2 * math.pi):
            i += math.pi / 16
            d += ' L 0,0 ' + \
                 ' L ' + str(math.sin(i) * r) + ',' + str(math.cos(i) * r) + \
                 ' L ' + str(math.sin(i + 0.09) * r) + ',' + str(math.cos(i + 0.09) * r)
        regmark_attribs = {'style': str(inkex.Style(style)),
                           'inkscape:label': name,
                           'transform': 'translate(' + str(cx) + ',' + str(cy) + ')',
                           'd': d}
        parent.add(inkex.PathElement(**regmark_attribs))

    def draw_coluor_bars(self, cx, cy, rotate, name, parent):
        group = parent.add(inkex.Group(id=name))
        group.transform = inkex.Transform(translate=(cx, cy), rotate=rotate)
        bbox = parent.bounding_box()
        loc = 0
        if bbox:
            loc = min(self.mark_size / 3, max(bbox.width, bbox.height) / 45)
        for bar in [{'c': '*', 'stroke': '#000', 'x': 0, 'y': -(loc + 1)},
                    {'c': 'r', 'stroke': '#0FF', 'x': 0, 'y': 0},
                    {'c': 'g', 'stroke': '#F0F', 'x': (loc * 11) + 1, 'y': -(loc + 1)},
                    {'c': 'b', 'stroke': '#FF0', 'x': (loc * 11) + 1, 'y': 0}
                    ]:
            i = 0
            while i <= 1:
                color = inkex.Color('white')
                if bar['c'] == 'r' or bar['c'] == '*':
                    color.red = 255 * i
                if bar['c'] == 'g' or bar['c'] == '*':
                    color.green = 255 * i
                if bar['c'] == 'b' or bar['c'] == '*':
                    color.blue = 255 * i
                r_att = {'fill': str(color),
                         'stroke': bar['stroke'],
                         'stroke-width': '0.5',
                         'x': str((loc * i * 10) + bar['x']), 'y': str(bar['y']),
                         'width': str(loc), 'height': str(loc)}
                group.add(Rectangle(*r_att))
                i += 0.1

    def effect(self):
        self.mark_size = self.svg.unittouu('1cm')
        self.min_mark_margin = self.svg.unittouu('3mm')

        if self.options.where == 'selection':
            bbox = self.svg.selection.bounding_box()
        else:
            bbox = self.svg.get_page_bbox()

        # Get SVG document dimensions
        # self.width must be replaced by bbox.right. same to others.
        svg = self.document.getroot()

        # Convert parameters to user unit
        offset = self.svg.unittouu(str(self.options.crop_offset) +
                                   self.options.unit)
        bt = self.svg.unittouu(str(self.options.bleed_top) + self.options.unit)
        bb = self.svg.unittouu(str(self.options.bleed_bottom) + self.options.unit)
        bl = self.svg.unittouu(str(self.options.bleed_left) + self.options.unit)
        br = self.svg.unittouu(str(self.options.bleed_right) + self.options.unit)
        # Bleed margin
        if bt < offset:
            bmt = 0
        else:
            bmt = bt - offset
        if bb < offset:
            bmb = 0
        else:
            bmb = bb - offset
        if bl < offset:
            bml = 0
        else:
            bml = bl - offset
        if br < offset:
            bmr = 0
        else:
            bmr = br - offset

        # Define the new document limits
        offset_left = bbox.left - offset
        offset_right = bbox.right + offset
        offset_top = bbox.top - offset
        offset_bottom = bbox.bottom + offset

        # Get middle positions
        middle_vertical = bbox.top + (bbox.height / 2)
        middle_horizontal = bbox.left + (bbox.width / 2)

        # Test if printing-marks layer existis
        layer = self.svg.xpath('//*[@id="printing-marks" and @inkscape:groupmode="layer"]')
        if layer:
            svg.remove(layer[0])  # remove if it existis
        # Create a new layer
        layer = svg.add(inkex.Layer.new("Printing Marks"))
        layer.set('id', 'printing-marks')
        layer.set('sodipodi:insensitive', 'true')

        # Crop Mark
        if self.options.crop_marks:
            # Create a group for Crop Mark
            g_crops = layer.add(inkex.Group(id='CropMarks'))
            g_crops.label = 'CropMarks'

            # Top left Mark
            self.draw_crop_line(bbox.left, offset_top,
                                bbox.left, offset_top - self.mark_size,
                                'cropTL1', g_crops)
            self.draw_crop_line(offset_left, bbox.top,
                                offset_left - self.mark_size, bbox.top,
                                'cropTL2', g_crops)

            # Top right Mark
            self.draw_crop_line(bbox.right, offset_top,
                                bbox.right, offset_top - self.mark_size,
                                'cropTR1', g_crops)
            self.draw_crop_line(offset_right, bbox.top,
                                offset_right + self.mark_size, bbox.top,
                                'cropTR2', g_crops)

            # Bottom left Mark
            self.draw_crop_line(bbox.left, offset_bottom,
                                bbox.left, offset_bottom + self.mark_size,
                                'cropBL1', g_crops)
            self.draw_crop_line(offset_left, bbox.bottom,
                                offset_left - self.mark_size, bbox.bottom,
                                'cropBL2', g_crops)

            # Bottom right Mark
            self.draw_crop_line(bbox.right, offset_bottom,
                                bbox.right, offset_bottom + self.mark_size,
                                'cropBR1', g_crops)
            self.draw_crop_line(offset_right, bbox.bottom,
                                offset_right + self.mark_size, bbox.bottom,
                                'cropBR2', g_crops)

        # Bleed Mark
        if self.options.bleed_marks:
            # Create a group for Bleed Mark
            g_attribs = {'inkscape:label': 'BleedMarks',
                         'id': 'BleedMarks'}
            g_bleed = layer.add(inkex.Group(**g_attribs))

            # Top left Mark
            self.draw_bleed_line(bbox.left - bl, offset_top - bmt,
                                 bbox.left - bl, offset_top - bmt - self.mark_size,
                                 'bleedTL1', g_bleed)
            self.draw_bleed_line(offset_left - bml, bbox.top - bt,
                                 offset_left - bml - self.mark_size, bbox.top - bt,
                                 'bleedTL2', g_bleed)

            # Top right Mark
            self.draw_bleed_line(bbox.right + br, offset_top - bmt,
                                 bbox.right + br, offset_top - bmt - self.mark_size,
                                 'bleedTR1', g_bleed)
            self.draw_bleed_line(offset_right + bmr, bbox.top - bt,
                                 offset_right + bmr + self.mark_size, bbox.top - bt,
                                 'bleedTR2', g_bleed)

            # Bottom left Mark
            self.draw_bleed_line(bbox.left - bl, offset_bottom + bmb,
                                 bbox.left - bl, offset_bottom + bmb + self.mark_size,
                                 'bleedBL1', g_bleed)
            self.draw_bleed_line(offset_left - bml, bbox.bottom + bb,
                                 offset_left - bml - self.mark_size, bbox.bottom + bb,
                                 'bleedBL2', g_bleed)

            # Bottom right Mark
            self.draw_bleed_line(bbox.right + br, offset_bottom + bmb,
                                 bbox.right + br, offset_bottom + bmb + self.mark_size,
                                 'bleedBR1', g_bleed)
            self.draw_bleed_line(offset_right + bmr, bbox.bottom + bb,
                                 offset_right + bmr + self.mark_size, bbox.bottom + bb,
                                 'bleedBR2', g_bleed)

        # Registration Mark
        if self.options.reg_marks:
            # Create a group for Registration Mark
            g_attribs = {'inkscape:label': 'RegistrationMarks',
                         'id': 'RegistrationMarks'}
            g_center = layer.add(inkex.Group(**g_attribs))

            # Left Mark
            cx = max(bml + offset, self.min_mark_margin)
            self.draw_reg_marks(bbox.left - cx - (self.mark_size / 2),
                                middle_vertical - self.mark_size * 1.5,
                                '0', 'regMarkL', g_center)

            # Right Mark
            cx = max(bmr + offset, self.min_mark_margin)
            self.draw_reg_marks(bbox.right + cx + (self.mark_size / 2),
                                middle_vertical - self.mark_size * 1.5,
                                '180', 'regMarkR', g_center)

            # Top Mark
            cy = max(bmt + offset, self.min_mark_margin)
            self.draw_reg_marks(middle_horizontal,
                                bbox.top - cy - (self.mark_size / 2),
                                '90', 'regMarkT', g_center)

            # Bottom Mark
            cy = max(bmb + offset, self.min_mark_margin)
            self.draw_reg_marks(middle_horizontal,
                                bbox.bottom + cy + (self.mark_size / 2),
                                '-90', 'regMarkB', g_center)

        # Star Target
        if self.options.star_target:
            # Create a group for Star Target
            g_attribs = {'inkscape:label': 'StarTarget',
                         'id': 'StarTarget'}
            g_center = layer.add(inkex.Group(**g_attribs))

            if bbox.height < bbox.width:
                # Left Star
                cx = max(bml + offset, self.min_mark_margin)
                self.draw_star_target(bbox.left - cx - (self.mark_size / 2),
                                      middle_vertical,
                                      'starTargetL', g_center)
                # Right Star
                cx = max(bmr + offset, self.min_mark_margin)
                self.draw_star_target(bbox.right + cx + (self.mark_size / 2),
                                      middle_vertical,
                                      'starTargetR', g_center)
            else:
                # Top Star
                cy = max(bmt + offset, self.min_mark_margin)
                self.draw_star_target(middle_horizontal - self.mark_size * 1.5,
                                      bbox.top - cy - (self.mark_size / 2),
                                      'starTargetT', g_center)
                # Bottom Star
                cy = max(bmb + offset, self.min_mark_margin)
                self.draw_star_target(middle_horizontal - self.mark_size * 1.5,
                                      bbox.bottom + cy + (self.mark_size / 2),
                                      'starTargetB', g_center)

        # Colour Bars
        if self.options.colour_bars:
            # Create a group for Colour Bars
            g_attribs = {'inkscape:label': 'ColourBars',
                         'id': 'PrintingColourBars'}
            g_center = layer.add(inkex.Group(**g_attribs))

            if bbox.height > bbox.width:
                # Left Bars
                cx = max(bml + offset, self.min_mark_margin)
                self.draw_coluor_bars(bbox.left - cx - (self.mark_size / 2),
                                      middle_vertical + self.mark_size,
                                      90,
                                      'PrintingColourBarsL', g_center)
                # Right Bars
                cx = max(bmr + offset, self.min_mark_margin)
                self.draw_coluor_bars(bbox.right + cx + (self.mark_size / 2),
                                      middle_vertical + self.mark_size,
                                      90,
                                      'PrintingColourBarsR', g_center)
            else:
                # Top Bars
                cy = max(bmt + offset, self.min_mark_margin)
                self.draw_coluor_bars(middle_horizontal + self.mark_size,
                                      bbox.top - cy - (self.mark_size / 2),
                                      0,
                                      'PrintingColourBarsT', g_center)
                # Bottom Bars
                cy = max(bmb + offset, self.min_mark_margin)
                self.draw_coluor_bars(middle_horizontal + self.mark_size,
                                      bbox.bottom + cy + (self.mark_size / 2),
                                      0,
                                      'PrintingColourBarsB', g_center)

        # Page Information
        if self.options.page_info:
            # Create a group for Page Information
            g_attribs = {'inkscape:label': 'PageInformation',
                         'id': 'PageInformation'}
            g_pag_info = layer.add(inkex.Group(**g_attribs))
            y_margin = max(bmb + offset, self.min_mark_margin)
            txt_attribs = {
                'style': 'font-size:12px;font-style:normal;font-weight:normal;fill:#000000;font-family:Bitstream Vera Sans,sans-serif;text-anchor:middle;text-align:center',
                'x': str(middle_horizontal),
                'y': str(bbox.bottom + y_margin + self.mark_size + 20)
            }
            txt = g_pag_info.add(TextElement(**txt_attribs))
            txt.text = 'Page size: ' + \
                       str(round(self.svg.uutounit(bbox.width, self.options.unit), 2)) + \
                       'x' + \
                       str(round(self.svg.uutounit(bbox.height, self.options.unit), 2)) + \
                       ' ' + self.options.unit


if __name__ == '__main__':
    PrintingMarks().run()
