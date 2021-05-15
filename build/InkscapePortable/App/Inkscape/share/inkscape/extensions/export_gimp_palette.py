#!/usr/bin/env python
# coding=utf-8
#
# Copyright (c) 2009 - Jos Hirth, kaioa.com
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
Export a gimp pallet file (.gpl)
"""

import inkex
from inkex import ShapeElement, ColorIdError, ColorError


class ExportGimpPalette(inkex.OutputExtension):
    """Export all colors in a document to a gimp pallet"""
    select_all = (ShapeElement,)

    def save(self, stream):
        name = self.svg.name.replace('.svg', '')
        stream.write('GIMP Palette\nName: {}\n#\n'.format(name).encode('utf-8'))

        for key, value in sorted(list(set(self.get_colors()))):
            stream.write("{} {}\n".format(key, value).encode('utf-8'))

    def get_colors(self):
        """Get all the colors from the selected elements"""
        for elem in self.svg.selection.filter(ShapeElement).values():
            for color in self.process_element(elem):
                if str(color).upper() == 'NONE':
                    continue
                yield ("{:3d} {:3d} {:3d}".format(*color.to_rgb()), str(color).upper())

    def process_element(self, elem):
        """Recursively process elements for colors"""
        style = elem.fallback_style(move=False)
        for name in inkex.Style.color_props:
            try:
                yield inkex.Color(style.get(name))
            except ColorIdError:
                gradient = self.svg.getElementById(style.get(name))
                for item in self.process_element(gradient):
                    yield item
                if gradient.href is not None:
                    for item in self.process_element(gradient.href):
                        yield item
            except ColorError:
                pass # Bad color

        if elem.href is not None: # Capture second level gradient colors
            for color in self.process_element(elem.href):
                yield color

if __name__ == '__main__':
    ExportGimpPalette().run()
