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

from next_glyph_layer import NextLayer

class PreviousLayer(NextLayer):
    """Like next glyph layer, but for the previous"""
    @staticmethod
    def process_glyphs(glyphs, current):
        glyphs[current].set("style", "display:none")
        glyphs[current-1].set("style", "display:inline")

if __name__ == '__main__':
    PreviousLayer().run()
