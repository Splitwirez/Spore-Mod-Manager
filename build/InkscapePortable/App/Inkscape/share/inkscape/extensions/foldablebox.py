#! /usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2009 Aurelio A. Heckert <aurium (a) gmail dot com>
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

__version__ = "0.2"

import inkex

class FoldableBox(inkex.EffectExtension):
    """Foldable Box generation."""
    def add_arguments(self, pars):
        pars.add_argument("--width", type=float, default=10.0, help="The Box Width")
        pars.add_argument("--height", type=float, default=15.0, help="The Box Height")
        pars.add_argument("--depth", type=float, default=3.0, help="The Box Depth (z dimention)")
        pars.add_argument("--unit", default="cm", help="The unit of the box dimensions")
        pars.add_argument("--proportion", type=float, default=0.6, help="Inner tab proportion")
        pars.add_argument("--guide", type=inkex.Boolean, default=False, help="Add guide lines")

    def guide(self, value, orient):
        """Create a guideline conditionally"""
        if self.options.guide:
            self.svg.namedview.new_guide(value, orient)

    def effect(self):
        doc_w = self.svg.unittouu(self.document.getroot().get('width'))
        doc_h = self.svg.unittouu(self.document.getroot().get('height'))

        box_w = self.svg.unittouu(str(self.options.width) + self.options.unit)
        box_h = self.svg.unittouu(str(self.options.height) + self.options.unit)
        box_d = self.svg.unittouu(str(self.options.depth) + self.options.unit)
        tab_h = box_d * self.options.proportion

        box_id = self.svg.get_unique_id('box')
        group = self.svg.get_current_layer().add(inkex.Group(id=box_id))

        line_style = {'stroke': '#000000', 'fill': 'none',
                      'stroke-width': str(self.svg.unittouu('1px'))}

        self.guide(doc_h, True)

        # Inner Close Tab
        line = group.add(inkex.PathElement(id=box_id + '-inner-close-tab'))
        line.path = [
            ['M', [box_w - (tab_h * 0.7), 0]],
            ['C', [box_w - (tab_h * 0.25), 0, box_w, tab_h * 0.3, box_w, tab_h * 0.9]],
            ['L', [box_w, tab_h]],
            ['L', [0, tab_h]],
            ['L', [0, tab_h * 0.9]],
            ['C', [0, tab_h * 0.3, tab_h * 0.25, 0, tab_h * 0.7, 0]],
            ['Z', []]
        ]
        line.style = line_style

        lower_pos = box_d + tab_h
        left_pos = 0

        self.guide(doc_h - tab_h, True)

        # Upper Close Tab
        line = group.add(inkex.PathElement(id=box_id + '-upper-close-tab'))
        line.path = [
            ['M', [left_pos, tab_h]],
            ['L', [left_pos + box_w, tab_h]],
            ['L', [left_pos + box_w, lower_pos]],
            ['L', [left_pos + 0, lower_pos]],
            ['Z', []]
        ]
        line.style = line_style

        left_pos += box_w

        # Upper Right Tab
        side_tab_h = lower_pos - (box_w / 2)
        if side_tab_h < tab_h:
            side_tab_h = tab_h

        line = group.add(inkex.PathElement(id=box_id + '-upper-right-tab'))
        line.path = [
            ['M', [left_pos, side_tab_h]],
            ['L', [left_pos + (box_d * 0.8), side_tab_h]],
            ['L', [left_pos + box_d, ((lower_pos * 3) - side_tab_h) / 3]],
            ['L', [left_pos + box_d, lower_pos]],
            ['L', [left_pos + 0, lower_pos]],
            ['Z', []]
        ]
        line.style = line_style

        left_pos += box_w + box_d

        # Upper Left Tab
        line = group.add(inkex.PathElement(id=box_id + '-upper-left-tab'))
        line.path = [
            ['M', [left_pos + box_d, side_tab_h]],
            ['L', [left_pos + (box_d * 0.2), side_tab_h]],
            ['L', [left_pos, ((lower_pos * 3) - side_tab_h) / 3]],
            ['L', [left_pos, lower_pos]],
            ['L', [left_pos + box_d, lower_pos]],
            ['Z', []]
        ]
        line.style = line_style

        left_pos = 0

        self.guide(doc_h - tab_h - box_d, True)

        # Right Tab
        line = group.add(inkex.PathElement(id=box_id + '-left-tab'))
        line.path = [
            ['M', [left_pos, lower_pos]],
            ['L', [left_pos - (box_d / 2), lower_pos + (box_d / 4)]],
            ['L', [left_pos - (box_d / 2), lower_pos + box_h - (box_d / 4)]],
            ['L', [left_pos, lower_pos + box_h]],
            ['Z', []]
        ]
        line.style = line_style

        # Front
        line = group.add(inkex.PathElement(id=box_id + '-front'))
        line.path = [
            ['M', [left_pos, lower_pos]],
            ['L', [left_pos + box_w, lower_pos]],
            ['L', [left_pos + box_w, lower_pos + box_h]],
            ['L', [left_pos, lower_pos + box_h]],
            ['Z', []]
        ]
        line.style = line_style

        left_pos += box_w

        # Right
        line = group.add(inkex.PathElement(id=box_id + '-right'))
        line.path = [
            ['M', [left_pos, lower_pos]],
            ['L', [left_pos + box_d, lower_pos]],
            ['L', [left_pos + box_d, lower_pos + box_h]],
            ['L', [left_pos, lower_pos + box_h]],
            ['Z', []]
        ]
        line.style = line_style

        left_pos += box_d

        # Back
        line = group.add(inkex.PathElement(id=box_id + '-back'))
        line.path = [
            ['M', [left_pos, lower_pos]],
            ['L', [left_pos + box_w, lower_pos]],
            ['L', [left_pos + box_w, lower_pos + box_h]],
            ['L', [left_pos, lower_pos + box_h]],
            ['Z', []]
        ]
        line.style = line_style

        left_pos += box_w

        # Left
        line = group.add(inkex.PathElement(id=box_id + '-line'))
        line.path = [
            ['M', [left_pos, lower_pos]],
            ['L', [left_pos + box_d, lower_pos]],
            ['L', [left_pos + box_d, lower_pos + box_h]],
            ['L', [left_pos, lower_pos + box_h]],
            ['Z', []]
        ]
        line.style = line_style

        lower_pos += box_h
        left_pos = 0
        b_tab = lower_pos + box_d
        if b_tab > box_w / 2.5:
            b_tab = box_w / 2.5

        # Bottom Front Tab
        line = group.add(inkex.PathElement(id=box_id + '-bottom-front-tab'))
        line.path = [
            ['M', [left_pos, lower_pos]],
            ['L', [left_pos, lower_pos + (box_d / 2)]],
            ['L', [left_pos + box_w, lower_pos + (box_d / 2)]],
            ['L', [left_pos + box_w, lower_pos]],
            ['Z', []]
        ]
        line.style = line_style

        left_pos += box_w

        # Bottom Right Tab
        line = group.add(inkex.PathElement(id=box_id + '-bottom-right-tab'))
        line.path = [
            ['M', [left_pos, lower_pos]],
            ['L', [left_pos, lower_pos + b_tab]],
            ['L', [left_pos + box_d, lower_pos + b_tab]],
            ['L', [left_pos + box_d, lower_pos]],
            ['Z', []]
        ]
        line.style = line_style

        left_pos += box_d

        # Bottom Back Tab
        line = group.add(inkex.PathElement(id=box_id + '-bottom-back-tab'))
        line.path = [
            ['M', [left_pos, lower_pos]],
            ['L', [left_pos, lower_pos + (box_d / 2)]],
            ['L', [left_pos + box_w, lower_pos + (box_d / 2)]],
            ['L', [left_pos + box_w, lower_pos]],
            ['Z', []]
        ]
        line.style = line_style

        left_pos += box_w

        # Bottom Left Tab
        line = group.add(inkex.PathElement(id=box_id + '-bottom-left-tab'))
        line.path = [
            ['M', [left_pos, lower_pos]],
            ['L', [left_pos, lower_pos + b_tab]],
            ['L', [left_pos + box_d, lower_pos + b_tab]],
            ['L', [left_pos + box_d, lower_pos]],
            ['Z', []]
        ]
        line.style = line_style

        left_pos += box_d
        lower_pos += b_tab

        group.transform = inkex.Transform(translate=((doc_w - left_pos) / 2, (doc_h - lower_pos) / 2))


if __name__ == '__main__':  # pragma: no cover
    FoldableBox().run()
