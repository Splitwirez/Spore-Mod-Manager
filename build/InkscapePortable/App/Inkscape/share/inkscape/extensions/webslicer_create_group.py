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
"""Create webslicer group"""

import inkex
from inkex.localization import inkex_gettext as _
from webslicer_effect import WebSlicerMixin

class CreateGroup(WebSlicerMixin, inkex.EffectExtension):
    """Create new webslicer group"""
    def add_arguments(self, pars):
        pars.add_argument("--tab")
        pars.add_argument("--html-id", dest="html_id")
        pars.add_argument("--html-class", dest="html_class")
        pars.add_argument("--width-unity", dest="width_unity")
        pars.add_argument("--height-unity", dest="height_unity")
        pars.add_argument("--bg-color", dest="bg_color")

    def effect(self):
        if not self.svg.selected:
            raise inkex.AbortExtension(_('You must to select some "Slicer rectangles" '
                                         'or other "Layout groups".'))

        base_elements = self.get_slicer_layer().descendants()
        for key, node in self.svg.selected.id_dict().items():
            if node not in base_elements:
                raise inkex.AbortExtension(_('The element "{}" is not in the Web Slicer layer'.format(key)))
            g_parent = node.getparent()

        group = g_parent.add(inkex.Group())
        desc = group.add(inkex.Desc())
        desc.text = self.get_conf_text_from_list(
            ['html_id', 'html_class', 'width_unity', 'height_unity', 'bg_color'])

        for node in self.svg.selected.values():
            group.insert(1, node)


if __name__ == '__main__':
    CreateGroup().run()
