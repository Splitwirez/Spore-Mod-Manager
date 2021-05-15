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
"""Extension for converting svg fonts to layers"""

import inkex

class SvgFontToLayers(inkex.EffectExtension):
    """Convert an svg font to layers"""
    def add_arguments(self, pars):
        pars.add_argument("--count", type=int, default=30,\
            help="Stop making layers after this number of glyphs.")

    def flip_cordinate_system(self, elem, emsize, baseline):
        """Scale and translate the element's path, returns the path object"""
        path = elem.path
        path.scale(1, -1, inplace=True)
        path.translate(0, int(emsize) - int(baseline), inplace=True)
        return path

    def effect(self):
        # TODO: detect files with multiple svg fonts declared.
        # Current code only reads the first svgfont instance
        font = self.svg.defs.findone('svg:font')
        if font is None:
            return inkex.errormsg("There are no svg fonts")
        #setwidth = font.get("horiz-adv-x")
        baseline = font.get("horiz-origin-y")
        if baseline is None:
            baseline = 0

        fontface = font.findone('svg:font-face')

        # TODO: where should we save the font family name?
        # fontfamily = fontface.get("font-family")
        emsize = fontface.get("units-per-em")

        # TODO: should we guarantee that <svg:font horiz-adv-x> equals <svg:font-face units-per-em> ?
        caps = int(fontface.get("cap-height", 0))
        xheight = int(fontface.get("x-height", 0))
        ascender = int(fontface.get("ascent", 0))
        descender = int(fontface.get("descent", 0))

        self.svg.set("width", emsize)
        self.svg.namedview.new_guide(baseline, True, "baseline")
        self.svg.namedview.new_guide(baseline + ascender, True, "ascender")
        self.svg.namedview.new_guide(baseline + caps, True, "caps")
        self.svg.namedview.new_guide(baseline + xheight, True, "xheight")
        self.svg.namedview.new_guide(baseline - descender, True, "decender")

        # TODO: missing-glyph
        count = 0
        for glyph in font.findall('svg:glyph'):
            unicode_char = glyph.get("unicode")
            if unicode_char is None:
                continue

            layer = self.svg.add(inkex.Layer.new("GlyphLayer-" + unicode_char))
            # glyph layers (except the first one) are innitially hidden
            if count != 0:
                layer.style['display'] = 'none'

            ############################
            # Option 1:
            # Using clone (svg:use) as childnode of svg:glyph

            # use = self.get_or_create(glyph, inkex.Use())
            # use.href = group
            # TODO: This code creates <use> nodes but they do not render on svg fonts dialog. why?

            ############################
            # Option 2:
            # Using svg:paths as childnodes of svg:glyph
            for elem in glyph.findall('svg:path'):
                new_path = layer.add(inkex.PathElement())
                new_path.path = self.flip_cordinate_system(elem, emsize, baseline)

            ############################
            # Option 3:
            # Using curve description in d attribute of svg:glyph
            path = layer.add(inkex.PathElement())
            path.path = self.flip_cordinate_system(glyph, emsize, baseline)

            count += 1
            if count >= self.options.count:
                break

if __name__ == '__main__':
    SvgFontToLayers().run()
