#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2011 Felipe Correa da Silva Sanches <juca@members.fsf.org>
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
"""Convert known layer structures to svg font glyphs"""

import inkex
from inkex import SVGfont, FontFace, Glyph

class LayersToSvgFont(inkex.EffectExtension):
    """Convert layers to an svg font"""
    def guideline_value(self, label, index):
        for guide in self.svg.namedview.get_guides():
            if guide.label == label:
                return guide.point[index]
        return 0

    def flip_cordinate_system(self, path, emsize, baseline):
        path = path.copy()
        path.transform.add_scale(1, -1)
        path.transform.add_translate(0, int(emsize) - int(baseline))
        path.apply_transform()
        return str(path.path)

    def effect(self):
        emsize = int(float(self.svg.get("width")))
        baseline = self.guideline_value("baseline", 1)
        ascender = self.guideline_value("ascender", 1) - baseline
        caps = self.guideline_value("caps", 1) - baseline
        xheight = self.guideline_value("xheight", 1) - baseline
        descender = baseline - self.guideline_value("descender", 1)

        font = self.svg.defs.get_or_create('svg:font', SVGfont)
        font.set("horiz-adv-x", str(emsize))
        font.set("horiz-origin-y", str(baseline))

        fontface = font.get_or_create('font-face', FontFace)
        fontface.set("font-family", "SVGFont")
        fontface.set("units-per-em", str(emsize))
        fontface.set("cap-height", str(caps))
        fontface.set("x-height", str(xheight))
        fontface.set("ascent", str(ascender))
        fontface.set("descent", str(descender))

        for group in self.svg.findall('svg:g'):
            label = group.label
            if "GlyphLayer-" in label:
                unicode_char = label.split("GlyphLayer-")[1]
                glyph = font.get_or_create("svg:glyph[@unicode='{}']".format(unicode_char), Glyph)
                glyph.set("unicode", unicode_char)

                ############################
                # Option 1:
                # Using clone (svg:use) as childnode of svg:glyph

                # use = glyph.get_or_create('svg:use', UseElement)
                # use.set('xlink:href', "#"+group.get("id"))
                # TODO: This code creates <use> nodes but they do not render on svg fonts dialog. why?

                ############################
                # Option 2:
                # Using svg:paths as childnodes of svg:glyph

                # for p in group.findall('svg:path'):
                #    d = p.get("d")
                #    d = self.flip_cordinate_system(d, emsize, baseline)
                #    path = glyph.add(PathElement())
                #    path.set("d", d)

                ############################
                # Option 3:
                # Using curve description in d attribute of svg:glyph

                path_d = ""
                for path in group.findall('svg:path'):
                    path_d += " " + self.flip_cordinate_system(path, emsize, baseline)
                glyph.set("d", path_d)

if __name__ == '__main__':
    LayersToSvgFont().run()
