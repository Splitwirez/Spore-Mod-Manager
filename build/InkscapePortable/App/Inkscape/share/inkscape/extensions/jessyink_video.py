#!/usr/bin/env python
#
# Copyright 2008, 2009 Hannes Hochreiner
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program.  If not, see http://www.gnu.org/licenses/.
#
"""Add a video to the slideshow"""

import re
from copy import deepcopy

import inkex
from jessyink_install import JessyInkMixin, _

class Video(JessyInkMixin, inkex.EffectExtension):
    """Add jessyink video"""
    def add_arguments(self, pars):
        self.arg_parser.add_argument('--tab', dest='what')

    def effect(self):
        self.is_installed()
        # Check version.
        base_view = self.svg.xpath("//sodipodi:namedview[@id='base']")
        if base_view is None:
            raise inkex.AbortExtension(_(
                "Could not obtain the selected layer for inclusion of the video element."))

        layer = self.svg.get_current_layer()
        if layer is None:
            raise inkex.AbortExtension(_(
                "Could not obtain the selected layer for inclusion of the video element.\n\n"))

        template = inkex.load_svg(self.get_resource('jessyink_video.svg'))
        root = template.getroot()

        elem = layer.add(root.getElement("//svg:g[@jessyink:element='core.video']").copy())
        node_dict = find_internal_links(elem, root, {})
        delete_ids(elem)

        for node in node_dict.values():
            elem.insert(0, node)

        for node in node_dict.values():
            node.set_id(self.svg.get_unique_id("jessyink.core.video"), backlinks=True)

def find_internal_links(node, svg, node_dict):
    """Get all clone links and css links"""
    for entry in re.findall(br"url\(#.*\)", node.tostring()):
        entry = entry.decode()
        link_id = entry[5:len(entry) - 1]

        if link_id not in node_dict:
            node_dict[link_id] = deepcopy(svg.getElementById(link_id))
            find_internal_links(node_dict[link_id], svg, node_dict)

    for entry in node.iter():
        if entry.get('xlink:href'):
            link_id = entry.get('xlink:href')
            if link_id not in node_dict:
                node_dict[link_id] = deepcopy(svg.getElementById(link_id))
                find_internal_links(node_dict[link_id], svg, node_dict)

    return node_dict

def delete_ids(node):
    """Delete ids in the given node's children"""
    for entry in node.iter():
        if 'id' in entry.attrib:
            del entry.attrib['id']

if __name__ == '__main__':
    Video().run()
