#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2008 Jonas Termeau - jonas.termeau **AT** gmail.com
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; version 2 of the License.
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
# Thanks to:
#
# Bernard Gray - bernard.gray **AT** gmail.com (python helping)
# Jamie Heames (english translation issues)
# ~suv (bug report in v2.3)
# http://www.gutenberg.eu.org/publications/ (9x9 margins settings)
#
"""
This basic extension allows you to automatically draw guides in inkscape.
"""

from math import cos, sin, sqrt

import inkex
from inkex import Guide


class GuidesCreator(inkex.EffectExtension):
    """Create a set of guides based on the given options"""
    def add_arguments(self, pars):
        pars.add_argument("--tab", type=self.arg_method('generate'), default="regular_guides",\
            help="Type of guides to create.")
        pars.add_argument('--guides_preset', default='custom', help='Preset')
        pars.add_argument('--vertical_guides', type=int, default=3, help='Vertical guides')
        pars.add_argument('--horizontal_guides', type=int, default=3, help='Horizontal guides')
        pars.add_argument('--start_from_edges', type=inkex.Boolean, help='Start from edges')
        pars.add_argument('--ul', type=inkex.Boolean, default=True, help='Upper left corner')
        pars.add_argument('--ur', type=inkex.Boolean, default=True, help='Upper right corner')
        pars.add_argument('--ll', type=inkex.Boolean, default=True, help='Lower left corner')
        pars.add_argument('--lr', type=inkex.Boolean, default=True, help='Lower right corner')
        pars.add_argument('--margins_preset', default='custom', help='Margins preset')
        pars.add_argument('--vert', type=int, default=0, help='Vert subdivisions')
        pars.add_argument('--horz', type=int, default=0, help='Horz subdivisions')
        pars.add_argument('--header_margin', default=6, help='Header margin')
        pars.add_argument('--footer_margin', default=6, help='Footer margin')
        pars.add_argument('--left_margin', default=6, help='Left margin')
        pars.add_argument('--right_margin', default=6, help='Right margin')
        pars.add_argument('--delete', type=inkex.Boolean, help='Delete existing guides')

    def effect(self):
        # getting the width and height attributes of the canvas
        self.width = float(self.svg.width)
        self.height = float(self.svg.height)

        # getting edges coordinates
        self.h_orientation = '0,' + str(round(self.width, 4))
        self.v_orientation = str(round(self.height, 4)) + ',0'

        if self.options.delete:
            for guide in self.svg.namedview.get_guides():
                guide.delete()

        return self.options.tab()

    def generate_regular_guides(self):
        """Generate a regular set of guides"""
        preset = self.options.guides_preset
        from_edges = self.options.start_from_edges
        if preset == 'custom':
            h_division = self.options.horizontal_guides
            v_division = self.options.vertical_guides
            if from_edges:
                v_division = v_division or 1
                h_division = h_division or 1

            self.draw_guides(v_division, from_edges, vert=True)
            self.draw_guides(h_division, from_edges, vert=False)

        elif preset == 'golden':
            gold = (1 + sqrt(5)) / 2

            # horizontal golden guides
            position1 = '0,' + str(self.height / gold)
            position2 = '0,' + str(self.height - (self.height / gold))

            self.draw_guide(position1, self.h_orientation)
            self.draw_guide(position2, self.h_orientation)

            # vertical golden guides
            position1 = str(self.width / gold) + ',0'
            position2 = str(self.width - (self.width / gold)) + ',0'

            self.draw_guide(position1, self.v_orientation)
            self.draw_guide(position2, self.v_orientation)

            if from_edges:
                # horizontal borders
                self.draw_guide('0,' + str(self.height), self.h_orientation)
                self.draw_guide(str(self.height) + ',0', self.h_orientation)

                # vertical borders
                self.draw_guide('0,' + str(self.width), self.v_orientation)
                self.draw_guide(str(self.width) + ',0', self.v_orientation)

        elif ';' in preset:
            v_division = int(preset.split(';')[0])
            h_division = int(preset.split(';')[1])
            self.draw_guides(v_division, from_edges, vert=True)
            self.draw_guides(h_division, from_edges, vert=False)
        else:
            raise inkex.AbortExtension("Unknown guide guide preset: {}".format(preset))

    def generate_diagonal_guides(self):
        """Generate diagonal guides"""
        # Dimentions
        left, bottom = (0, 0)
        right, top = (self.width, self.height)

        # Diagonal angle
        angle = 45

        if self.options.ul:
            ul_corner = str(top) + ',' + str(left)
            from_ul_to_lr = str(cos(angle)) + ',' + str(cos(angle))
            self.draw_guide(ul_corner, from_ul_to_lr)

        if self.options.ur:
            ur_corner = str(right) + ',' + str(top)
            from_ur_to_ll = str(-sin(angle)) + ',' + str(sin(angle))
            self.draw_guide(ur_corner, from_ur_to_ll)

        if self.options.ll:
            ll_corner = str(bottom) + ',' + str(left)
            from_ll_to_ur = str(-cos(angle)) + ',' + str(cos(angle))
            self.draw_guide(ll_corner, from_ll_to_ur)

        if self.options.lr:
            lr_corner = str(bottom) + ',' + str(right)
            from_lr_to_ul = str(-sin(angle)) + ',' + str(-sin(angle))
            self.draw_guide(lr_corner, from_lr_to_ul)

    def generate_margins(self):
        """Generate margin guides"""
        header_margin = int(self.options.header_margin)
        footer_margin = int(self.options.footer_margin)
        left_margin = int(self.options.left_margin)
        right_margin = int(self.options.right_margin)
        h_subdiv = int(self.options.horz)
        v_subdiv = int(self.options.vert)

        if self.options.start_from_edges:
            # horizontal borders
            self.draw_guide('0,' + str(self.height), self.h_orientation)
            self.draw_guide(str(self.height) + ',0', self.h_orientation)

            # vertical borders
            self.draw_guide('0,' + str(self.width), self.v_orientation)
            self.draw_guide(str(self.width) + ',0', self.v_orientation)

        if self.options.margins_preset == 'custom':
            y_header = self.height
            y_footer = 0
            x_left = 0
            x_right = self.width

            if header_margin != 0:
                y_header = (self.height / header_margin) * (header_margin - 1)
                self.draw_guide('0,' + str(y_header), self.h_orientation)

            if footer_margin != 0:
                y_footer = self.height / footer_margin
                self.draw_guide('0,' + str(y_footer), self.h_orientation)

            if left_margin != 0:
                x_left = self.width / left_margin
                self.draw_guide(str(x_left) + ',0', self.v_orientation)

            if right_margin != 0:
                x_right = (self.width / right_margin) * (right_margin - 1)
                self.draw_guide(str(x_right) + ',0', self.v_orientation)

        elif self.options.margins_preset == 'book_left':
            # 1/9th header
            y_header = (self.height / 9) * 8
            self.draw_guide('0,' + str(y_header), self.h_orientation)

            # 2/9th footer
            y_footer = (self.height / 9) * 2
            self.draw_guide('0,' + str(y_footer), self.h_orientation)

            # 2/9th left margin
            x_left = (self.width / 9) * 2
            self.draw_guide(str(x_left) + ',0', self.v_orientation)

            # 1/9th right margin
            x_right = (self.width / 9) * 8
            self.draw_guide(str(x_right) + ',0', self.v_orientation)

        elif self.options.margins_preset == 'book_right':
            # 1/9th header
            y_header = (self.height / 9) * 8
            self.draw_guide('0,' + str(y_header), self.h_orientation)

            # 2/9th footer
            y_footer = (self.height / 9) * 2
            self.draw_guide('0,' + str(y_footer), self.h_orientation)

            # 2/9th left margin
            x_left = (self.width / 9)
            self.draw_guide(str(x_left) + ',0', self.v_orientation)

            # 1/9th right margin
            x_right = (self.width / 9) * 7
            self.draw_guide(str(x_right) + ',0', self.v_orientation)

        # setting up properties of the rectangle created between guides
        rectangle_height = y_header - y_footer
        rectangle_width = x_right - x_left

        if h_subdiv != 0:
            begin_from = y_footer
            # creating horizontal guides
            self._draw_guides(
                (rectangle_width, rectangle_height), h_subdiv,
                edges=0, shift=begin_from, vert=False)

        if v_subdiv != 0:
            begin_from = x_left
            # creating vertical guides
            self._draw_guides(
                (rectangle_width, rectangle_height), v_subdiv,
                edges=0, shift=begin_from, vert=True)

    def draw_guides(self, division, edges, vert=False):
        """Draw a vertical or horizontal lines"""
        return self._draw_guides((self.width, self.height), division, edges, vert=vert)

    def _draw_guides(self, vector, division, edges, shift=0, vert=False):
        if division <= 0:
            return

        # Vert controls both ort template and vector calculation
        ort = '{},0' if vert else '0,{}'
        var = int(bool(edges))
        for x in range(0, division - 1 + 2 * var):
            div = vector[not bool(vert)] / division
            position = str(round(div + (x - var) * div + shift, 4))
            orientation = str(round(vector[bool(vert)], 4))
            self.draw_guide(ort.format(position), ort.format(orientation))

    def draw_guide(self, position, orientation):
        """Create a guide directly into the namedview"""
        if isinstance(position, tuple):
            x, y = position
        elif isinstance(position, str):
            x, y = position.split(',')
        self.svg.namedview.add(Guide().move_to(float(x), float(y), orientation))

if __name__ == '__main__':
    GuidesCreator().run()
