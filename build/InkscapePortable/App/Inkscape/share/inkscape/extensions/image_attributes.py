#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2015, ~suv <suv-sf@users.sf.net>
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
image_attributes.py - adjust image attributes which don't have global
GUI options yet

Tool for Inkscape 0.91 to adjust rendering of drawings with linked
or embedded bitmap images created with older versions of Inkscape
or third-party applications.
"""

import inkex
from inkex import Image

class ImageAttributes(inkex.EffectExtension):
    """Set attributes in images"""
    def effect(self):
        self.options.tab_main()

    def add_arguments(self, pars):
        pars.add_argument("--tab_main", type=self.arg_method(), default=self.method_tab_basic)
        pars.add_argument("--fix_scaling", type=inkex.Boolean, default=True)
        pars.add_argument("--fix_rendering", type=inkex.Boolean, default=False)
        pars.add_argument("--aspect_ratio", default="none",\
            help="Value for attribute 'preserveAspectRatio'")
        pars.add_argument("--aspect_clip", default="unset",\
            help="optional 'meetOrSlice' value")
        pars.add_argument("--aspect_ratio_scope", type=self.arg_method("change"),\
            default="selected_only", help="When to edit 'preserveAspectRatio' attribute")
        pars.add_argument("--image_rendering", default="unset",\
            help="Value for attribute 'image-rendering'")
        pars.add_argument("--image_rendering_scope", type=self.arg_method("change"),\
            default="selected_only", help="When to edit 'image-rendering' attribute")

    def change_attribute(self, node, attribute):
        for key, value in attribute.items():
            if key == 'preserveAspectRatio':
                # set presentation attribute
                if value != "unset":
                    node.set(key, str(value))
                else:
                    if node.get(key):
                        del node.attrib[key]
            elif key == 'image-rendering':
                node_style = dict(inkex.Style.parse_str(node.get('style')))
                if key not in node_style:
                    # set presentation attribute
                    if value != "unset":
                        node.set(key, str(value))
                    else:
                        if node.get(key):
                            del node.attrib[key]
                else:
                    # set style property
                    if value != "unset":
                        node_style[key] = str(value)
                    else:
                        del node_style[key]
                    node.set('style', str(inkex.Style(node_style)))
            else:
                pass

    def change_all_images(self, node, attribute):
        for img in node.xpath('descendant-or-self::svg:image'):
            self.change_attribute(img, attribute)

    def change_selected_only(self, selected, attribute):
        for node in selected.values():
            if isinstance(node, Image):
                self.change_attribute(node, attribute)

    def change_in_selection(self, selected, attribute):
        for node in selected.values():
            self.change_all_images(node, attribute)

    def change_in_document(self, selected, attribute):
        self.change_all_images(self.document.getroot(), attribute)

    def change_on_parent_group(self, selected, attribute):
        for node in selected.values():
            self.change_attribute(node.getparent(), attribute)

    def change_on_root_only(self, selected, attribute):
        self.change_attribute(self.document.getroot(), attribute)

    def method_tab_basic(self):
        """Render all bitmap images like in older Inskcape versions"""
        self.change_in_document(self.svg.selected, {
            'preserveAspectRatio': ("none" if self.options.fix_scaling else "unset"),
            'image-rendering': ("optimizeSpeed" if self.options.fix_rendering else "unset"),
        })

    def method_tab_aspect_ratio(self):
        """Image Aspect Ratio"""
        attr_val = [self.options.aspect_ratio]
        if self.options.aspect_clip != "unset":
            attr_val.append(self.options.aspect_clip)
        self.options.aspect_ratio_scope(self.svg.selected,\
            {'preserveAspectRatio': ' '.join(attr_val)})

    def method_tab_image_rendering(self):
        """Image Rendering Quality"""
        self.options.image_rendering_scope(self.svg.selected,\
            {'image-rendering': self.options.image_rendering})

if __name__ == '__main__':
    ImageAttributes().run()
