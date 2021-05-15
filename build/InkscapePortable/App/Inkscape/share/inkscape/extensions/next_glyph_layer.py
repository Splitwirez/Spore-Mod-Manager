#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2011 Felipe Correa da Silva Sanches
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

class NextLayer(inkex.EffectExtension):
    """Show the next glyph layer"""
    def effect(self):
        count = 0
        glyphs = []
        for group in self.svg.findall('svg:g'):
            if "GlyphLayer-" in group.label:
                glyphs.append(group)
                if group.style.get('display', '') == "inline":
                    count += 1
                    current = len(glyphs) - 1

        if count != 1 or len(glyphs) < 2:
            return

        self.process_glyphs(glyphs, current)

    @staticmethod
    def process_glyphs(glyphs, current):
        """Process the glyphs"""
        glyphs[current].set("style", "display:none")
        glyphs[(current+1)%len(glyphs)].set("style", "display:inline")

if __name__ == '__main__':
    NextLayer().run()
