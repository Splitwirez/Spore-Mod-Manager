#!/usr/bin/env python
#
# Copyright (C) 2010 Aurelio A. Heckert, aurium (a) gmail dot com
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
Common elements between webslicer extensions
"""

import inkex
from inkex import Group

def is_empty(val):
    return val in ('', None)


class WebSlicerMixin(object):
    def get_slicer_layer(self, force_creation=False):
        # Test if webslicer-layer layer existis
        layer = self.svg.getElement(
            '//*[@id="webslicer-layer" and @inkscape:groupmode="layer"]')
        if layer is None:
            if force_creation:
                # Create a new layer
                layer = Group(id='webslicer-layer')
                layer.set('inkscape:label', 'Web Slicer')
                layer.set('inkscape:groupmode', 'layer')
                self.document.getroot().append(layer)
            else:
                layer = None
        return layer

    def get_conf_text_from_list(self, conf_atts):
        conf_list = []
        for att in conf_atts:
            if not is_empty(getattr(self.options, att)):
                conf_list.append(
                        att.replace('_', '-') + ': ' + str(getattr(self.options, att))
                )
        return "\n".join(conf_list)
