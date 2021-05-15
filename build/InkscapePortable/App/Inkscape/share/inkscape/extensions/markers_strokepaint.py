#!/usr/bin/env python
# coding=utf-8
# coding=utf-8
#
# Copyright (C) 2006 Aaron Spike, aaron@ekips.org
# Copyright (C) 2010 Nicolas Dufour, nicoduf@yahoo.fr (color options)
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

import inkex
from inkex.localization import inkex_gettext as _

MARKERS = ['marker', 'marker-start', 'marker-mid', 'marker-end']

class MarkersStrokePaint(inkex.EffectExtension):
    """Add marker stroke to outline markers on selected objects."""
    def add_arguments(self, pars):
        pars.add_argument("--modify", type=inkex.Boolean, default=False,
                          help="Do not create a copy, modify the markers")
        pars.add_argument("--type", dest="fill_type", default="stroke",
                          help="Replace the markers' fill with the object stroke or fill color")
        pars.add_argument("--alpha", type=inkex.Boolean, dest="assign_alpha", default=True,
                          help="Assign the object fill and stroke alpha to the markers")
        pars.add_argument("--invert", type=inkex.Boolean, default=False,
                          help="Invert fill and stroke colors")
        pars.add_argument("--assign_fill", type=inkex.Boolean, default=True,
                          help="Assign a fill color to the markers")
        pars.add_argument("--fill_color", type=inkex.Color, default=inkex.Color(1364325887),
                          help="Choose a custom fill color")
        pars.add_argument("--assign_stroke", type=inkex.Boolean, default=True,
                          help="Assign a stroke color to the markers")
        pars.add_argument("--stroke_color", type=inkex.Color, default=inkex.Color(1364325887),
                          help="Choose a custom fill color")
        pars.add_argument("--tab", type=self.arg_method('method'), default=self.method_custom,
                          help="The selected UI-tab when OK was pressed")
        pars.add_argument("--colortab", help="The selected custom color tab when OK was pressed")

    def method_custom(self, _):
        """Choose custom colors"""
        fill = self.options.fill_color if self.options.assign_fill else None
        stroke = self.options.stroke_color if self.options.assign_stroke else None
        return fill, stroke

    def method_object(self, style):
        """Use object colors"""
        fill = style.get_color('fill')
        stroke = style.get_color('stroke')

        if self.options.fill_type == "solid":
            fill = stroke
        elif self.options.fill_type == "filled":
            stroke = None
        elif self.options.invert:
            fill, stroke = stroke, fill

        if not self.options.assign_alpha:
            # Remove alpha values
            fill = fill.to_rgb()
            stroke = stroke.to_rgb()

        return fill, stroke

    def effect(self):
        for node in self.svg.selected.values():
            fill, stroke = self.options.tab(node.style)

            for attr in MARKERS:
                if not node.style.get(attr, '').startswith('url(#'):
                    continue

                marker_id = node.style[attr][5:-1]
                marker_node = self.svg.getElement('/svg:svg//svg:marker[@id="%s"]' % marker_id)

                if marker_node is None:
                    inkex.errormsg(_("unable to locate marker: %s") % marker_id)
                    continue

                if not self.options.modify:
                    marker_node = marker_node.copy()
                    self.svg.defs.append(marker_node)
                    marker_id = self.svg.get_unique_id(marker_id)

                node.style[attr] = "url(#%s)" % marker_id
                marker_node.set('id', marker_id)
                marker_node.set('inkscape:stockid', marker_id)

                for child in marker_node:
                    if stroke is not None:
                        child.style.set_color(stroke, 'stroke')
                    if fill is not None:
                        child.style.set_color(fill, 'fill')

if __name__ == '__main__':
    MarkersStrokePaint().run()
